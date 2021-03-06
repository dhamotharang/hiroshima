using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Hiroshima.Common.Models;

namespace HappyTravel.Hiroshima.DirectManager.Services
{
    public interface IManagerContextService
    {
        Task<Result<Manager>> GetManager();

        string GetHash();

        Task<bool> DoesManagerExist();

        Task<Result<ServiceSupplier>> GetServiceSupplier();

        Task<Result<ManagerServiceSupplierRelation>> GetManagerRelation();

        Task<Result<ManagerContext>> GetManagerContext();

        Task<Result<Manager>> GetMasterManager(int serviceSupplierId);
    }
}