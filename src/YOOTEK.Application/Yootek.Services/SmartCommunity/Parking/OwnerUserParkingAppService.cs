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
    public interface IOwnerUserParkingAppService
    {
        Task<object> GetListOwnerParkingAsync(GetListOwnerParkingInput input);
        Task<object> CreateOwnerUserParkingAsync(CreateOwnerUserParkingDto input);
        Task<object> UpdateOwnerParkingAsync(UpdateOwnerUserParkingDto input);
        Task<object> DeleteOwnerParkingAsync(long id);
    }

    public class OwnerUserParkingAppService : YootekAppServiceBase, IOwnerUserParkingAppService
    {
        private readonly IRepository<UserVehicle, long> _vehicleRepository;
        private readonly IRepository<Parking, long> _parkingRepository;
        private readonly IRepository<UserParking, long> _userParkingRepository;
        private readonly IHttpQRCodeService _httpQRCodeService;

        public OwnerUserParkingAppService(
            IRepository<Parking, long> parkingRepository,
             IRepository<UserVehicle, long> vehicleRepository,
            IHttpQRCodeService httpQRCodeService,
            IRepository<UserParking, long> userParkingRepository)
        {
            _parkingRepository = parkingRepository;
            _httpQRCodeService = httpQRCodeService;
            _userParkingRepository = userParkingRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<object> GetListOwnerParkingAsync(GetListOwnerParkingInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var data = (from upk in _userParkingRepository.GetAll()
                            join pk in _parkingRepository.GetAll() on upk.ParkingId equals pk.Id into tb_pk
                            from pk in tb_pk.DefaultIfEmpty()
                            join vh in _vehicleRepository.GetAll() on upk.VehicleId equals vh.Id into tb_vh
                            from vh in tb_vh.DefaultIfEmpty()
                            select new OwnerUserParkingDto()
                            {
                                UserId = upk.UserId,
                                ParkingId = upk.ParkingId,
                                Properties = upk.Properties,
                                ImageUrls = upk.ImageUrls,
                                CreationTime = upk.CreationTime,
                                ParkingInfo = pk,
                                VehicleId = upk.VehicleId,
                                VehicleName = vh.VehicleName,
                                VehicleCode = vh.VehicleCode,
                                CreatorUserId = upk.CreatorUserId,
                                ApartmentCode = vh.ApartmentCode,
                                Id = upk.Id
                            })
                            .Where(x => x.UserId == AbpSession.UserId || (!string.IsNullOrEmpty(input.ApartmentCode) && x.ApartmentCode == input.ApartmentCode))
                            .OrderByDescending(x => x.CreationTime)
                            .AsNoTracking().AsQueryable();
                var result = data.PageBy(input).ToList();
                mb.statisticMetris(t1, 0, "ParkingService.GetListOwnerParkingAsync");

                return DataResult.ResultSuccess(result, "Get list success", data.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreateOwnerUserParkingAsync(CreateOwnerUserParkingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var parking = _parkingRepository.FirstOrDefault(x => x.Id == input.ParkingId && x.Status != ParkingStatus.Occupied);

                if (parking == null)
                {
                    return DataResult.ResultCode(null, "Parking not found", 404);
                }

                if (parking.Status == ParkingStatus.Occupied)
                {
                    return DataResult.ResultCode(null, "Parking is occupied", 404);
                }

                var insertData = new UserParking
                {
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId.Value,
                    ParkingId = input.ParkingId,
                    ListImageUrl = input.ListImageUrl,
                    Properties = input.Properties,
                    VehicleId = input.VehicleId
                };

                await _userParkingRepository.InsertAsync(insertData);

                mb.statisticMetris(t1, 0, "ParkingService.CreateUserParkingAsync");

                return DataResult.ResultSuccess(null, "Create success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateOwnerParkingAsync(UpdateOwnerUserParkingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var userParking = _userParkingRepository.GetAll().FirstOrDefault(x => x.Id == input.Id);

                if (userParking == null)
                {
                    return DataResult.ResultCode(null, "User parking not found", 404);
                }

                if (userParking.UserId != AbpSession.UserId)
                {
                    return DataResult.ResultCode(null, "User parking not found", 404);
                }

                var parking = _parkingRepository.GetAll().FirstOrDefault(x =>
                    x.Id == userParking.ParkingId && x.Status == ParkingStatus.Occupied);
                if (parking == null)
                {
                    return DataResult.ResultCode(null, "Parking not found", 404);
                }

                input.MapTo(userParking);
                await _userParkingRepository.UpdateAsync(userParking);

                mb.statisticMetris(t1, 0, "ParkingService.UpdateOwnerParkingAsync");

                return DataResult.ResultSuccess(true, "Update success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteOwnerParkingAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var userParking = _userParkingRepository.GetAll().FirstOrDefault(x => x.Id == id);

                if (userParking == null)
                {
                    return DataResult.ResultCode(null, "User parking not found", 404);
                }

                if (userParking.UserId != AbpSession.UserId)
                {
                    return DataResult.ResultCode(null, "User parking not found", 404);
                }

                var parking = _parkingRepository.GetAll().FirstOrDefault(x =>
                    x.Id == userParking.ParkingId && x.Status == ParkingStatus.Occupied);
                if (parking == null)
                {
                    return DataResult.ResultCode(null, "Parking not found", 404);
                }

                parking.Status = ParkingStatus.Free;
                await _parkingRepository.UpdateAsync(parking);

                await _userParkingRepository.DeleteAsync(userParking);

                mb.statisticMetris(t1, 0, "ParkingService.DeleteOwnerParkingAsync");

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
