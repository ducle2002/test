using Abp.Runtime.Session;
using Abp;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Yootek.App.ServiceHttpClient;
using Microsoft.EntityFrameworkCore;
using Abp.Linq.Extensions;
using Abp.AutoMapper;
using Abp.UI;

namespace Yootek.Services
{
    public interface IUserVehicleAppService
    {
        Task<object> GetAllVehicleAsync(GetAllVehicleInput input);
        Task<object> CreateUserVehicleAsync(CreateUserVehicleDto input);
        Task<object> UpdateVehicleAsync(UpdateUserVehicleDto input);
        Task<object> DeleteVehicleAsync(long id);
    }

    public class UserVehicleAppService : YootekAppServiceBase, IUserVehicleAppService
    {
        private readonly IRepository<Parking, long> _parkingRepository;
        private readonly IRepository<UserVehicle, long> _userVehicleRepository;
        private readonly IHttpQRCodeService _httpQRCodeService;
        private readonly IRepository<HomeMember, long> _homeMemberRepository;
        private readonly IRepository<Citizen, long> _citizenRepository;

        public UserVehicleAppService(
            IRepository<Parking, long> parkingRepository,
            IHttpQRCodeService httpQRCodeService,
            IRepository<UserVehicle, long> userVehicleRepository,
            IRepository<HomeMember, long> homeMemberRepository,
            IRepository<Citizen, long> citizenRepository)
        {
            _parkingRepository = parkingRepository;
            _httpQRCodeService = httpQRCodeService;
            _userVehicleRepository = userVehicleRepository;
            _homeMemberRepository = homeMemberRepository;
            _citizenRepository = citizenRepository;
        }

        public async Task<object> GetAllVehicleAsync(GetAllVehicleInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var home = _citizenRepository.FirstOrDefault(x => x.AccountId == AbpSession.UserId);
                string apartmentCode = null;
                if (home != null) apartmentCode = home.ApartmentCode;

                var data = await _userVehicleRepository.GetAllListAsync(x => x.CreatorUserId == AbpSession.UserId
                || (!string.IsNullOrEmpty(input.ApartmentCode) && x.ApartmentCode == input.ApartmentCode)
                || (!string.IsNullOrEmpty(apartmentCode) && x.ApartmentCode == apartmentCode));

                mb.statisticMetris(t1, 0, "VehicleService.GetListVehicleAsync");

                return DataResult.ResultSuccess(data, "Get list success", data.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetVehicleByIdAsync(long id)
        {
            try
            {
                var data = await _userVehicleRepository.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreateUserVehicleAsync(CreateUserVehicleDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var vehicle = input.MapTo<UserVehicle>();

                vehicle.TenantId = AbpSession.TenantId;

                vehicle.Id = await _userVehicleRepository.InsertAndGetIdAsync(vehicle);

                if(vehicle.IsDefault)
                {
                    var vehicleDefaults = _userVehicleRepository.GetAllList(x => x.CreatorUserId == AbpSession.UserId && x.Id != vehicle.Id);
                    if (vehicleDefaults != null)
                    {
                        foreach (var v in vehicleDefaults)
                        {
                            v.IsDefault = false;
                        }
                    }

                }
                await CurrentUnitOfWork.SaveChangesAsync();
                mb.statisticMetris(t1, 0, "VehicleService.CreateUserVehicleAsync");

                return DataResult.ResultSuccess(vehicle, "Create success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> SetDefaultVehicleAsync(long vehicleId)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var vh = _userVehicleRepository.Get(vehicleId);
                if (vh == null) throw new UserFriendlyException("Vehicle not found !");
                vh.IsDefault = true;
                _userVehicleRepository.Update(vh);
                var vehicleDefaults = _userVehicleRepository.GetAllList(x => x.CreatorUserId == AbpSession.UserId && x.Id != vh.Id);
                if(vehicleDefaults != null)
                {
                    foreach( var v in vehicleDefaults)
                    {
                        v.IsDefault = false;
                    }
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                mb.statisticMetris(t1, 0, "VehicleService.UpdateVehicleAsync");

                return DataResult.ResultSuccess(true, "Update success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UnSetDefaultVehicleAsync(long vehicleId)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var vh = _userVehicleRepository.Get(vehicleId);
                if (vh == null) throw new UserFriendlyException("Vehicle not found !");
                vh.IsDefault = false;
                _userVehicleRepository.Update(vh);
               

                await CurrentUnitOfWork.SaveChangesAsync();

                mb.statisticMetris(t1, 0, "VehicleService.UpdateVehicleAsync");

                return DataResult.ResultSuccess(true, "Update success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> UpdateVehicleAsync(UpdateUserVehicleDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var vh = _userVehicleRepository.FirstOrDefault(input.Id);
                if (vh == null) throw new UserFriendlyException("Vehicle not found !");
                input.MapTo(vh);

                vh.TenantId = AbpSession.TenantId;

                await _userVehicleRepository.UpdateAsync(vh);
                mb.statisticMetris(t1, 0, "VehicleService.UpdateVehicleAsync");

                return DataResult.ResultSuccess(true, "Update success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteVehicleAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                await _userVehicleRepository.DeleteAsync(id);

                mb.statisticMetris(t1, 0, "VehicleService.DeleteVehicleAsync");

                return DataResult.ResultSuccess(true, "Delete success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
