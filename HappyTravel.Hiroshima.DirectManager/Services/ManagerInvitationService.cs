using CSharpFunctionalExtensions;
using HappyTravel.Hiroshima.Common.Infrastructure;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Data;
using HappyTravel.Hiroshima.Data.Extensions;
using HappyTravel.Hiroshima.DirectContracts.Extensions.FunctionalExtensions;
using HappyTravel.Hiroshima.DirectManager.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HappyTravel.Hiroshima.DirectManager.Services
{
    public class ManagerInvitationService : IManagerInvitationService
    {
        public ManagerInvitationService(IManagerContextService managerContextService, INotificationService notificationService, 
            DirectContractsDbContext dbContext, ILogger<ManagerInvitationService> logger, IOptions<ManagerInvitationOptions> options,
            IDateTimeProvider dateTimeProvider)
        {
            _managerContext = managerContextService;
            _notificationService = notificationService;
            _dbContext = dbContext;
            _logger = logger;
            _options = options;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<Result> Send(Models.Requests.ManagerInvitationInfo managerInvitationInfo)
        {
            return await _managerContext.GetManagerRelation()
                .Ensure(managerRelation => HasManagerInvitationManagerPermission(managerRelation).Value, "The manager does not have enough rights")
                .BindWithTransaction(_dbContext, managerRelation => CreateInvitation(managerInvitationInfo, managerRelation)
                    .Bind(SaveInvitation)
                    .Bind(SendInvitationMail))
                .Tap(managerInvitation => LogInvitationCreated(managerInvitation.Email));
        }


        public async Task<Result<string>> Create(Models.Requests.ManagerInvitationInfo managerInvitationInfo)
        {
            return await _managerContext.GetManagerRelation()
                .Ensure(managerRelation => HasManagerInvitationManagerPermission(managerRelation).Value, "The manager does not have enough rights")
                .Bind(managerRelation => CreateInvitation(managerInvitationInfo, managerRelation))
                .Bind(SaveInvitation)
                .Tap(managerInvitation => LogInvitationCreated(managerInvitation.Email))
                .Bind(GetInvitationCode);

            static Result<string> GetInvitationCode(ManagerInvitation managerInvitation)
                => managerInvitation.InvitationCode;
        }


        public async Task<Result> Resend(string invitationCode)
        {
            return await _managerContext.GetManagerRelation()
                .Ensure(managerRelation => HasManagerInvitationManagerPermission(managerRelation).Value, "The manager does not have enough rights")
                .Bind(managerRelation => GetExistingInvitation(invitationCode))
                .BindWithTransaction(_dbContext, managerInvitation => DisableExistingInvitation(managerInvitation)
                    .Bind(CreateNewInvitation)
                    .Bind(SaveInvitation)
                    .Bind(SendInvitationMail))
                .Tap(managerInvitation => LogInvitationCreated(managerInvitation.Email));


            async Task<Result<ManagerInvitation>> GetExistingInvitation(string invitationCode)
            {
                var invitation = await _dbContext.ManagerInvitations.SingleOrDefaultAsync(i => i.InvitationCode == invitationCode);

                return invitation ?? Result.Failure<ManagerInvitation>($"Invitation with Code {invitationCode} not found");
            }


            async Task<Result<ManagerInvitation>> DisableExistingInvitation(ManagerInvitation existingInvitation)
            {
                existingInvitation.IsResent = true;
                await _dbContext.SaveChangesAsync();
                
                return existingInvitation;
            }


            Result<ManagerInvitation> CreateNewInvitation(ManagerInvitation existingInvitation)
            {
                return new ManagerInvitation
                {
                    InvitationCode = GenerateInvitationCode(),
                    FirstName = existingInvitation.FirstName,
                    LastName = existingInvitation.LastName,
                    Title = existingInvitation.Title,
                    Position = existingInvitation.Position,
                    Email = existingInvitation.Email,
                    ManagerId = existingInvitation.ManagerId,
                    ServiceSupplierId = existingInvitation.ServiceSupplierId,
                    Created = _dateTimeProvider.UtcNow(),
                    IsAccepted = false,
                    IsResent = false
                };
            }
        }


        public async Task Accept(string invitationCode)
        {
            var invitationMaybe = await GetInvitation(invitationCode);
            if (invitationMaybe.HasValue)
            {
                var managerInvitation = invitationMaybe.Value;
                managerInvitation.IsAccepted = true;

                _dbContext.Update(managerInvitation);
                await _dbContext.SaveChangesAsync();
            }
        }


        public Task<Result<Models.Responses.ManagerInvitation>> GetPendingInvitation(string invitationCode)
        {
            return GetInvitation(invitationCode).ToResult("Could not find invitation")
                .Ensure(IsNotAccepted, "Invitation already accepted")
                .Ensure(IsNotResent, "Invitation already resent")
                .Ensure(InvitationIsActual, "Invitation expired")
                .Map(Build);


            static bool IsNotAccepted(ManagerInvitation managerInvitation) => !managerInvitation.IsAccepted;


            static bool IsNotResent(ManagerInvitation managerInvitation) => !managerInvitation.IsResent;


            bool InvitationIsActual(ManagerInvitation managerInvitation) 
                => managerInvitation.Created + _options.Value.InvitationExpirationPeriod > _dateTimeProvider.UtcNow();
        }


        public async Task<Result<List<Models.Responses.ManagerInvitation>>> GetServiceSupplierInvitations()
        {
            return await _managerContext.GetManagerRelation()
                .Ensure(managerRelation => HasManagerInvitationManagerPermission(managerRelation).Value, "The manager does not have enough rights")
                .Map(managerRelation => GetNotAcceptedInvitations(managerRelation.ServiceSupplierId))
                .Map(Build);


            async Task<List<ManagerInvitation>> GetNotAcceptedInvitations(int serviceSupplierId)
            {
                return await _dbContext.ManagerInvitations
                    .Where(invitation => invitation.ServiceSupplierId == serviceSupplierId && invitation.IsAccepted == false && invitation.IsResent == false)
                    .ToListAsync();
            }
        }


        private static Result<bool> HasManagerInvitationManagerPermission(ManagerServiceSupplierRelation managerRelation)
            => (managerRelation.ManagerPermissions & Common.Models.Enums.ManagerPermissions.ManagerInvitation) == Common.Models.Enums.ManagerPermissions.ManagerInvitation;


        private Result<ManagerInvitation> CreateInvitation(Models.Requests.ManagerInvitationInfo managerInvitationInfo, ManagerServiceSupplierRelation managerRelation)
        {
            return new ManagerInvitation
            {
                InvitationCode = GenerateInvitationCode(),
                FirstName = managerInvitationInfo.FirstName,
                LastName = managerInvitationInfo.LastName,
                Title = managerInvitationInfo.Title,
                Position = managerInvitationInfo.Position,
                Email = managerInvitationInfo.Email,
                ManagerId = managerRelation.ManagerId,
                ServiceSupplierId = managerRelation.ServiceSupplierId,
                Created = _dateTimeProvider.UtcNow(),
                IsAccepted = false,
                IsResent = false
            };
        }


        private string GenerateInvitationCode()
        {
            using var provider = new RNGCryptoServiceProvider();

            var byteArray = new byte[64];
            provider.GetBytes(byteArray);

            return Base64UrlEncoder.Encode(byteArray);
        }


        private async Task<Maybe<ManagerInvitation>> GetInvitation(string invitationCode)
        {
            var managerInvitation = await _dbContext.ManagerInvitations.SingleOrDefaultAsync(c => c.InvitationCode == invitationCode);

            return managerInvitation ?? Maybe<ManagerInvitation>.None;
        }


        private async Task<Result<ManagerInvitation>> SaveInvitation(ManagerInvitation managerInvitation)
        {
            var entry = _dbContext.ManagerInvitations.Add(managerInvitation);
            await _dbContext.SaveChangesAsync();
            _dbContext.DetachEntry(entry.Entity);

            return entry.Entity;
        }


        private async Task<Result<ManagerInvitation>> SendInvitationMail(ManagerInvitation managerInvitation)
        {
            var serviceSupplier = await _managerContext.GetServiceSupplier();
            if (serviceSupplier.IsFailure)
                return Result.Failure<ManagerInvitation>(serviceSupplier.Error);

            var sendingResult = await _notificationService.SendInvitation(managerInvitation, serviceSupplier.Value.Name);
            if (sendingResult.IsFailure)
                return Result.Failure<ManagerInvitation>(sendingResult.Error);

            return managerInvitation;
        }


        private void LogInvitationCreated(string email)
        {
            _logger.LogInvitationCreated($"The invitation created for the manager with email '{email}'");
        }


        private static List<Models.Responses.ManagerInvitation> Build(List<ManagerInvitation> managerInvitations)
            => managerInvitations.Select(Build).ToList();


        private static Models.Responses.ManagerInvitation Build(ManagerInvitation managerInvitation)
        {
            return new Models.Responses.ManagerInvitation(managerInvitation.FirstName,
                managerInvitation.LastName,
                managerInvitation.Title,
                managerInvitation.Position,
                managerInvitation.Email,
                managerInvitation.ManagerId,
                managerInvitation.ServiceSupplierId);
        }


        private readonly IManagerContextService _managerContext;
        private readonly INotificationService _notificationService;
        private readonly DirectContractsDbContext _dbContext;
        private readonly ILogger<ManagerInvitationService> _logger;
        private readonly IOptions<ManagerInvitationOptions> _options;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}