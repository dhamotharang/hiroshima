﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Hiroshima.Common.Models.Seasons;
using HappyTravel.Hiroshima.Data;
using HappyTravel.Hiroshima.Data.Extensions;
using HappyTravel.Hiroshima.DirectContracts.Services.Management;
using HappyTravel.Hiroshima.DirectManager.Infrastructure.Extensions;
using HappyTravel.Hiroshima.DirectManager.RequestValidators;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Hiroshima.DirectManager.Services
{
    public class SeasonManagementService : ISeasonManagementService
    {
        public SeasonManagementService(DirectContractsDbContext dbContext, IContractManagerContextService contractManagerContext,
            IContractManagementRepository contractManagementRepository)
        {
            _dbContext = dbContext;
            _contractManagerContext = contractManagerContext;
            _contractManagementRepository = contractManagementRepository;
        }


        public Task<Result<List<Models.Responses.Season>>> Add(int contractId, List<string> names)
        {
            return _contractManagerContext.GetContractManager()
                .EnsureContractBelongsToContractManager(_dbContext, contractId)
                .Map(contractManager => AddSeasonNames())
                .Map(Build);


             async Task<List<Season>> AddSeasonNames()
             {
                 var seasons = names.Select(name => new Season
                     {
                         ContractId = contractId,
                         Name = name
                     })
                     .ToList();
                 _dbContext.Seasons.AddRange(seasons);
                 await _dbContext.SaveChangesAsync();
                 
                 _dbContext.DetachEntries(seasons);

                 return seasons;
             }
        }


        public Task<Result<List<Models.Responses.Season>>> Get(int contractId, int skip, int top)
        {
            return _contractManagerContext.GetContractManager()
                .EnsureContractBelongsToContractManager(_dbContext, contractId)
                .Map(contractManager => GetSeasons())
                .Map(Build);


            Task<List<Season>> GetSeasons()
                => _dbContext.Seasons.Where(season => season.ContractId == contractId)
                    .OrderBy(season => season.Id)
                    .Skip(skip).Take(top).ToListAsync();
        }


        public async Task<Result> Remove(int contractId, int seasonId)
        {
            return await _contractManagerContext.GetContractManager()
                .EnsureContractBelongsToContractManager(_dbContext, contractId)
                .Bind(contractManager => GetSeason())
                .Ensure(CheckIfSeasonDoesntHaveAnySeasonRanges,
                    $"Season with {nameof(seasonId)} '{seasonId}' have an associated date range")
                .Tap(Remove);
            

            async Task<Result<Season>> GetSeason()
            {
                var season = await _dbContext.Seasons.SingleOrDefaultAsync(season => season.Id == seasonId && season.ContractId == contractId);

                return season != null
                    ? Result.Success(season)
                    : Result.Failure<Season>($"Failed to get the {nameof(season)} '{seasonId}'");
            }
            
            
           async Task<bool> CheckIfSeasonDoesntHaveAnySeasonRanges(Season season)
           {
                var seasonRanges = await _dbContext.SeasonRanges
                    .Where(seasonRange => seasonRange.SeasonId == season.Id)
                    .ToListAsync();

                return !seasonRanges.Any();
           }
            
            
            async Task Remove(Season season)
            {
                _dbContext.Seasons.Remove(season);
                await _dbContext.SaveChangesAsync();
            }
        }


        public Task<Result<List<Models.Responses.SeasonRange>>> SetSeasonRanges(int contractId, List<Models.Requests.SeasonRange> seasonRanges)
        {
            return _contractManagerContext.GetContractManager()
                .Bind(contractManager => Validate(contractManager.Id, contractId, seasonRanges))
                .Map(async () => await ReplaceSeasonRanges())
                .Map(Build);

            
            async Task RemovePreviousSeasonRanges()
            {
                var seasonRangesToDelete = (await _dbContext.GetSeasons().Where(season => season.ContractId == contractId).ToListAsync())
                    .SelectMany(season => season.SeasonRanges).ToList();
                
                if (!seasonRangesToDelete.Any())
                    return;
                
                _dbContext.SeasonRanges.RemoveRange(seasonRangesToDelete);
            }
            

            async Task<List<SeasonRange>> ReplaceSeasonRanges()
            {
                await RemovePreviousSeasonRanges();
                var newSeasonRanges = AddSeasonRanges();
                await _dbContext.SaveChangesAsync();
                _dbContext.DetachEntries(newSeasonRanges);

                return newSeasonRanges;
            }
            
            
            List<SeasonRange> AddSeasonRanges()
            {
                var newSeasonRanges = Create(seasonRanges);
                _dbContext.SeasonRanges.AddRange(newSeasonRanges);
                
                return newSeasonRanges;
            }


            List<SeasonRange> Create(List<Models.Requests.SeasonRange> seasonRanges) => seasonRanges.Select(CreateSeasonRange).ToList();
            
            
            SeasonRange CreateSeasonRange(Models.Requests.SeasonRange seasonRange) 
                => new SeasonRange
            {
                SeasonId = seasonRange.SeasonId,
                StartDate = seasonRange.StartDate,
                EndDate = seasonRange.EndDate
            };
        }


        public Task<Result<List<Models.Responses.SeasonRange>>> GetSeasonRanges(int contractId, int skip, int top)
        {
            return _contractManagerContext.GetContractManager()
                .EnsureContractBelongsToContractManager(_dbContext, contractId)
                .Map(contractManager => GetOrderedSeasonRanges(season => season.ContractId == contractId, skip, top)) 
                .Map(Build);
        }


        public Task<Result<List<Models.Responses.SeasonRange>>> GetSeasonRanges(int contractId, int seasonId, int skip, int top)
        {
            return _contractManagerContext.GetContractManager()
                .EnsureContractBelongsToContractManager(_dbContext, contractId)
                .Map(contractManager => GetOrderedSeasonRanges(season => season.ContractId == contractId && season.Id == seasonId, skip, top))
                .Map(Build);
        }


        private async Task<Result> Validate(int contractManagerId, int contractId, List<Models.Requests.SeasonRange> seasonRanges)
        {
            var contract = await _contractManagementRepository.GetContract(contractId, contractManagerId);
            if (contract == null)
                return Result.Failure($"Contract '{contractId}' doesn't belong to the contract manager");

            var dateRanges = GetSortedDateRanges();

            return Result.Combine(ValidationHelper.Validate(seasonRanges, new SeasonRangeValidator()), CheckIfRangesInAscendingOrder(), CheckIfRangesCoverContractedPeriod(), await CheckIfSeasonsBelongToContract());

            
            Result CheckIfRangesInAscendingOrder()
            {
                var previousDateRange = dateRanges.First();
                foreach (var dateRange in dateRanges.Skip(1))
                {
                    var daysBetweenSeasons = (dateRange.startDate.Date - previousDateRange.endDate.Date).Days;
                    if (Math.Abs(daysBetweenSeasons) != 1)
                        return Result.Failure(
                            $"Incorrect interval between date ranges: '{previousDateRange.startDate} - {previousDateRange.endDate}' and '{dateRange.startDate} - {dateRange.endDate}'");

                    previousDateRange = dateRange;
                }

                return Result.Success();
            }


            Result CheckIfRangesCoverContractedPeriod()
            {
                var contractStartDate = contract.ValidFrom;
                var contractEndDate = contract.ValidTo;
                var firstSeason = dateRanges.First();
                var lastSeason = dateRanges.Last();

                return contractStartDate.Date == firstSeason.startDate.Date && contractEndDate.Date == lastSeason.endDate.Date
                    ? Result.Success()
                    : Result.Failure($"Contracted period '{contract.ValidFrom} - {contract.ValidTo}' must be fully covered");
            }


            async Task<Result> CheckIfSeasonsBelongToContract()
            {
               var requestSeasonIds = seasonRanges.Select(seasonRange => seasonRange.SeasonId).ToList();
               var validRequestSeasonIds = await _dbContext.Seasons
                   .Where(season => requestSeasonIds.Contains(season.Id) && season.ContractId == contractId)
                   .Select(season => season.Id)
                   .ToListAsync();

               var invalidRequestIds = requestSeasonIds.Except(validRequestSeasonIds).ToList();
               if (invalidRequestIds.Any())
                   return Result.Failure($"Incorrect season ids '{string.Join(", ", invalidRequestIds)}'");

               return Result.Success();
            }
            

            List<(DateTime startDate, DateTime endDate)> GetSortedDateRanges()
                => seasonRanges.Select(seasonRange => (seasonRange.StartDate, seasonRange.EndDate)).OrderBy(dateRange => dateRange.StartDate).ToList();
        }


        private async Task<List<SeasonRange>> GetOrderedSeasonRanges(Expression<Func<Season, bool>> expression, int skip, int top)
            => await _dbContext.Seasons.Where(expression)
                .Join(_dbContext.SeasonRanges, season => season.Id, seasonRange => seasonRange.SeasonId, (season, seasonRange) => seasonRange)
                .OrderBy(seasonRange => seasonRange.Id)
                .Skip(skip).Take(top)
                .ToListAsync();
            
        
        private List<Models.Responses.SeasonRange> Build(List<SeasonRange> seasonRanges)
            => seasonRanges.Select(BuildSeasonRange).ToList();


        private Models.Responses.SeasonRange BuildSeasonRange(SeasonRange seasonRange) 
            => new Models.Responses.SeasonRange(seasonRange.Id, seasonRange.SeasonId, seasonRange.StartDate, seasonRange.EndDate);
        

        private List<Models.Responses.Season> Build(List<Season> seasons) 
            => seasons.Select(BuildSeason).ToList();


        private Models.Responses.Season BuildSeason(Season season) 
            => new Models.Responses.Season(season.Id, season.Name);
        
        
        private readonly IContractManagementRepository _contractManagementRepository;
        private readonly IContractManagerContextService _contractManagerContext;
        private readonly DirectContractsDbContext _dbContext;
    }
}