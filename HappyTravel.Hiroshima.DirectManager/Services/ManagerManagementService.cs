﻿using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Hiroshima.Data;
using HappyTravel.Hiroshima.Data.Extensions;
using HappyTravel.Hiroshima.DirectManager.Infrastructure.Extensions;
using HappyTravel.Hiroshima.DirectManager.RequestValidators;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Hiroshima.DirectManager.Services
{
    public class ManagerManagementService : IManagerManagementService
    {
        public ManagerManagementService(IManagerContextService managerContextService, DirectContractsDbContext dbContext)
        {
            _managerContext = managerContextService;
            _dbContext = dbContext;
        }


        public Task<Result<Models.Responses.Manager>> Get()
            => _managerContext.GetManager()
                .Map(Build);
        
        
        public Task<Result<Models.Responses.Manager>> Register(Models.Requests.Manager managerRequest, string email)
        {
           return Result.Success()
                .Ensure(IdentityHashNotEmpty, "Failed to get the sub claim")
                .Ensure(DoesManagerNotExist, "Contract manager has already been registered")
                .Bind(() => IsRequestValid(managerRequest))
                .Map(CreateCompany)
                .Map(AddCompany)
                .Map(Create)
                .Map(Add)
                .Map(Build);

            
            bool IdentityHashNotEmpty() => !string.IsNullOrEmpty(_managerContext.GetIdentityHash());
            
            
            async Task<bool> DoesManagerNotExist() => !await _managerContext.DoesManagerExist();


            Common.Models.Company CreateCompany()
            {
                var utcNowDate = DateTime.UtcNow;
                return new Common.Models.Company
                {
                    Name = string.Empty,
                    Address = string.Empty,
                    PostalCode = string.Empty,
                    Phone = string.Empty,
                    Website = string.Empty,
                    Created = utcNowDate,
                    Modified = utcNowDate
                };
            }


            async Task<Common.Models.Company> AddCompany(Common.Models.Company company)
            {
                var entry = _dbContext.Companies.Add(company);
                await _dbContext.SaveChangesAsync();
                _dbContext.DetachEntry(entry.Entity);

                return entry.Entity;
            }


            Common.Models.Manager Create(Common.Models.Company company)
            {
                var utcNowDate = DateTime.UtcNow;
                return new Common.Models.Manager
                {
                    IdentityHash = _managerContext.GetIdentityHash(),
                    Email = email,
                    FirstName = managerRequest.FirstName,
                    LastName = managerRequest.LastName,
                    Title = managerRequest.Title,
                    Position = managerRequest.Position,
                    Phone = managerRequest.Phone,
                    Fax = managerRequest.Fax,
                    Created = utcNowDate,
                    Updated = utcNowDate,
                    IsActive = true
                };
            }


            async Task<Common.Models.Manager> Add(Common.Models.Manager manager)
            {
                var entry = _dbContext.Managers.Add(manager);
                await _dbContext.SaveChangesAsync();
                _dbContext.DetachEntry(entry.Entity);

                return entry.Entity;
            }
        }


        public Task<Result<Models.Responses.Company>> RegisterCompany(Models.Requests.Company companyRequest)
        {
            return _managerContext.GetManager()
                .Ensure(manager => manager.IsMaster, "Manager has no rights to register the company")
                .GetCompany(_dbContext)
                .Check(company => IsRequestValid(companyRequest))
                .Map(ModifyCompany)
                .Map(Update)
                .Map(Build);


            Common.Models.Company ModifyCompany(Common.Models.Company company)
            {
                company.Name = companyRequest.Name;
                company.Address = companyRequest.Address;
                company.PostalCode = companyRequest.PostalCode;
                company.Phone = companyRequest.Phone;
                company.Website = companyRequest.Website;
                company.Modified = DateTime.UtcNow;

                return company;
            }


            async Task<Common.Models.Company> Update(Common.Models.Company company)
            {
                var entry = _dbContext.Companies.Update(company);
                await _dbContext.SaveChangesAsync();
                _dbContext.DetachEntry(entry.Entity);

                return entry.Entity;
            }
        }


        public Task<Result<Models.Responses.Manager>> Modify(Models.Requests.Manager managerRequest)
        {
            return GetManager()
                .Check(manager => IsRequestValid(managerRequest))
                .Map(ModifyManager)
                .Map(Update)
                .Map(Build);

            
            Task<Result<Common.Models.Manager>> GetManager() 
                => _managerContext.GetManager();
            
            
            Common.Models.Manager ModifyManager(Common.Models.Manager manager)
            {
                manager.FirstName = managerRequest.FirstName;
                manager.LastName = managerRequest.LastName;
                manager.Title = managerRequest.Title;
                manager.Position = managerRequest.Position;
                manager.Phone = managerRequest.Phone;
                manager.Fax = managerRequest.Fax;
                manager.Updated = DateTime.UtcNow;

                return manager;
            }
            
            
            async Task<Common.Models.Manager> Update(Common.Models.Manager manager)
            {
                var entry = _dbContext.Managers.Update(manager);
                await _dbContext.SaveChangesAsync();
                _dbContext.DetachEntry(entry.Entity);
                
                return entry.Entity;
            }
        }


        private Models.Responses.Manager Build(Common.Models.Manager manager) 
            => new Models.Responses.Manager(manager.FirstName, 
                manager.LastName, 
                manager.Title, 
                manager.Position,
                manager.Email,
                manager.Phone,
                manager.Fax,
                Common.Models.Enums.ManagerPermissions.All, // TODO: Need add ManagerPermissions and IsMaster in next task
                true);


        private Models.Responses.Company Build(Common.Models.Company company)
            => new Models.Responses.Company(company.Name,
                company.Address,
                company.PostalCode,
                company.Phone,
                company.Website);


        private Result IsRequestValid(Models.Requests.Company companyRequest)
            => ValidationHelper.Validate(companyRequest, new CompanyRegisterRequestValidator());


        private Result IsRequestValid(Models.Requests.Manager managerRequest)
            => ValidationHelper.Validate(managerRequest, new ManagerRegisterRequestValidator());


        private readonly IManagerContextService _managerContext;
        private readonly DirectContractsDbContext _dbContext;
    }
}