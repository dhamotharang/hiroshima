using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Data;
using HappyTravel.Hiroshima.DirectContracts.Services;
using HappyTravel.Hiroshima.DirectManager.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Hiroshima.DirectManager.Services
{
    public class ManagerContextService : IManagerContextService
    {
        public ManagerContextService(DirectContractsDbContext dbContext, ITokenInfoAccessor tokenInfoAccessor, ISha256HashGenerator sha256HashGenerator)
        {
            _dbContext = dbContext;
            _tokenInfoAccessor = tokenInfoAccessor;
            _sha256HashGenerator = sha256HashGenerator;
        }


        public async Task<Result<Manager>> GetManager()
        {
            var hash = GetHash();

            var manager = await _dbContext.Managers.SingleOrDefaultAsync(manager => manager.IsActive && manager.IdentityHash == hash);

            return manager is null
                ? Result.Failure<Manager>("Failed to retrieve a contract manager") 
                : Result.Success(manager);
        }


        public async Task<bool> DoesManagerExist()
            => await _dbContext.Managers.SingleOrDefaultAsync(manager => manager.IdentityHash == GetHash()) != null;


        public string GetHash() => _sha256HashGenerator.Generate(_tokenInfoAccessor.GetIdentityHash());


        public async Task<Result<ServiceSupplier>> GetServiceSupplier()
        {
            var manager = await GetManager();
            if (manager.IsFailure)
                return Result.Failure<ServiceSupplier>(manager.Error);

            // TODO: now we find only one service supplier. Need change in next tasks
            var managerRelation = await _dbContext.ManagerServiceSupplierRelations.SingleOrDefaultAsync(relation => relation.ManagerId == manager.Value.Id);
            if (managerRelation is null)
                return Result.Failure<ServiceSupplier>("Manager has no relations with service suppliers");

            var serviceSupplier = await _dbContext.ServiceSuppliers.SingleOrDefaultAsync(serviceSupplier => serviceSupplier.Id == managerRelation.ServiceSupplierId);
            return serviceSupplier is null
                ? Result.Failure<ServiceSupplier>("Failed to retrieve a service supplier")
                : Result.Success(serviceSupplier);
        }


        public async Task<Result<ManagerServiceSupplierRelation>> GetManagerRelation()
        {
            var manager = GetManager();
            if (manager.Result.IsFailure)
                return Result.Failure<ManagerServiceSupplierRelation>(manager.Result.Error);

            var serviceSupplier = GetServiceSupplier();
            if (serviceSupplier.Result.IsFailure)
                return Result.Failure<ManagerServiceSupplierRelation>(serviceSupplier.Result.Error);

            var managerRelation = await _dbContext.ManagerServiceSupplierRelations
                .SingleOrDefaultAsync(relation => relation.ManagerId == manager.Result.Value.Id && relation.ServiceSupplierId == serviceSupplier.Result.Value.Id);
            
            return managerRelation is null
                ? Result.Failure<ManagerServiceSupplierRelation>("Manager has no relation with service supplier")
                : Result.Success(managerRelation);
        }


        public async Task<Result<ManagerContext>> GetManagerContext()
        {
            var manager = GetManager();
            if (manager.Result.IsFailure)
                return Result.Failure<ManagerContext>(manager.Result.Error);

            var serviceSupplier = GetServiceSupplier();
            if (serviceSupplier.Result.IsFailure)
                return Result.Failure<ManagerContext>(serviceSupplier.Result.Error);

            var managerRelation = await _dbContext.ManagerServiceSupplierRelations
                .SingleOrDefaultAsync(relation => relation.ManagerId == manager.Result.Value.Id && relation.ServiceSupplierId == serviceSupplier.Result.Value.Id);
            if (managerRelation is null)
                return Result.Failure<ManagerContext>("Manager has no relation with service supplier");

            var managerContext = new ManagerContext
            {
                Id = manager.Result.Value.Id,
                FirstName = manager.Result.Value.FirstName,
                LastName = manager.Result.Value.LastName,
                Title = manager.Result.Value.Title,
                Position = manager.Result.Value.Position,
                Email = manager.Result.Value.Email,
                Phone = manager.Result.Value.Phone,
                Fax = manager.Result.Value.Fax,
                ServiceSupplierId = managerRelation.ServiceSupplierId,
                ManagerPermissions = managerRelation.ManagerPermissions,
                IsMaster = managerRelation.IsMaster
            };

            return Result.Success(managerContext);
        }


        public async Task<Result<Manager>> GetMasterManager(int serviceSupplierId)
        {
            var managerRelation = await _dbContext.ManagerServiceSupplierRelations
                .SingleOrDefaultAsync(relation => relation.ServiceSupplierId == serviceSupplierId && relation.IsMaster == true && relation.IsActive == true);

            var manager = await _dbContext.Managers.SingleOrDefaultAsync(manager => manager.Id == managerRelation.ManagerId);

            return manager is null
                ? Result.Failure<Manager>("Master manager does not exist")
                : Result.Success(manager);
        }


        private readonly DirectContractsDbContext _dbContext;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly ISha256HashGenerator _sha256HashGenerator;
    }
}