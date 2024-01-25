using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Organizations;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Building;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Count.Dto;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Count
{
    public interface ICount : IApplicationService
    {
        Task<object> GetCitizenCount();
        Task<object> GetCitizenTempCount();
        Task<object> GetReflectCount();
        Task<object> GetCityNotificationsCount();
        Task<object> GetCityVoteCount();
        Task<object> GetAdministrativeCount();
        Task<object> GetUserBillCount();
    }
    [AbpAuthorize]
    public class Count : YootekAppServiceBase, ICount
    {
        private readonly IRepository<Citizen, long> _citizenRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;
        private readonly IRepository<CitizenReflect, long> _citizenReflectRepos;
        private readonly IRepository<CityNotification, long> _cityNotificationRepos;
        private readonly IRepository<Administrative, long> _adminIsTrativeRepository;
        private readonly IRepository<CityVote, long> _cityVoteRepos;
        private readonly IRepository<UserBill, long> _userBillRepository;


        public Count(
            IRepository<Citizen, long> citizenRespository,
            IRepository<CitizenReflect, long> citizenReflectRepos,
            IRepository<CitizenTemp, long> citizenTempRepository,
            IRepository<CityNotification, long> cityNotificationRepos,
            IRepository<Administrative, long> adminIsTrativeRepository,
            IRepository<CityVote, long> cityVoteRepos,
            IRepository<UserBill, long> userBillRepository

        )
        {
            _citizenRepository = citizenRespository;
            _citizenReflectRepos = citizenReflectRepos;
            _citizenTempRepository = citizenTempRepository;
            _cityNotificationRepos = cityNotificationRepos;
            _adminIsTrativeRepository = adminIsTrativeRepository;
            _cityVoteRepos = cityVoteRepos;
            _userBillRepository = userBillRepository;
        }

        public async Task<object> GetCitizenCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _citizenRepository.GetAll().CountAsync();
                    return DataResult.ResultSuccess(count, "Get success");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetCitizenTempCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _citizenTempRepository.GetAll().CountAsync();
                    return DataResult.ResultSuccess(count, "Get success");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetReflectCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _citizenReflectRepos.GetAll().CountAsync();
                    return DataResult.ResultSuccess(count, "Get success!");
                }
                    
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetCityNotificationsCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _cityNotificationRepos.GetAll().CountAsync();
                    return DataResult.ResultSuccess(count, "Get success!");
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAdministrativeCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _adminIsTrativeRepository.GetAll().Where(a => a.State == AdministrativeState.Requesting).CountAsync();
                    return DataResult.ResultSuccess(count, "Get success!");
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetCityVoteCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _cityVoteRepos.GetAll().Where(a => a.Status == STATUS_VOTE.FINISH).CountAsync();
                    return DataResult.ResultSuccess(count, "Get success");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetUserBillCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var count = await _userBillRepository.GetAll().CountAsync();
                    return DataResult.ResultSuccess(count, "Get success");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
    }
   
}
