using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Exception = System.Exception;

namespace Yootek.Services
{
    public interface ICarCardAppService
    {
        Task<object> CreateCarCard(CreateCarCardDto input);
        Task<object> GetAllCarCard(CarCardInput input);
        Task<object> GetCarCardById(long id);
        Task<object> UpdateCarCard(UpdateCarCardInput input);
        Task<object> DeleteCarCard(long id);
        Task<object> GetCountCarCard();

    }




    public class CarCardAppService(IRepository<CarCard, long> carCardRepository, IRepository<AppOrganizationUnit, long> organizationUnit, IRepository<CitizenVehicle, long> citizenVehicleRepos, IRepository<CitizenParking, long> citizenParkingRepository) : YootekAppServiceBase, ICarCardAppService
    {
        private readonly IRepository<CarCard, long> _carCardRepository = carCardRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnit = organizationUnit;
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepos = citizenVehicleRepos;
        private readonly IRepository<CitizenParking, long> _citizenParkingRepository = citizenParkingRepository;
        public async Task<object> CreateCarCard(CreateCarCardDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var carCard = await _carCardRepository.FirstOrDefaultAsync(x => x.BuildingId == input.BuildingId && x.UrbanId == input.UrbanId && x.VehicleCardCode == input.VehicleCardCode);
                if (carCard != null)
                {
                    throw new UserFriendlyException("Creation failed");
                }

                carCard = input.MapTo<CarCard>();
                carCard.Status = CarCardType.WaitingActivation;
                carCard.TenantId = AbpSession.TenantId;

                var data = await _carCardRepository.InsertAsync(carCard);

                mb.statisticMetris(t1, 0, "CarCardService.CreateCarCardAsync");
                return DataResult.ResultSuccess(data, "Insert success");
            }
            catch (UserFriendlyException e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Code, e.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllCarCard(CarCardInput input)
        {
            try
            {
                IQueryable<GetAllCarCard> query = (from u in _carCardRepository.GetAll()
                                                   join citizenVehicle in _citizenVehicleRepos.GetAll() on u.VehicleCardCode equals citizenVehicle.CardNumber into tb_citizenVehicle
                                                   from creator in tb_citizenVehicle.DefaultIfEmpty()
                                                   select new GetAllCarCard()
                                                   {
                                                       Id = u.Id,
                                                       Status = u.Status,
                                                       VehicleCardCode = u.VehicleCardCode,
                                                       BuildingId = u.BuildingId,
                                                       UrbanId = u.UrbanId,
                                                       BuildingName = _organizationUnit.GetAll().Where(x => x.Id == u.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                                                       UrbanName = _organizationUnit.GetAll().Where(x => x.Id == u.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                                                       ApartmentCode = creator.ApartmentCode,
                                                       VehicleName = creator.VehicleName,
                                                       VehicleCode = creator.VehicleCode,
                                                       OwnerName = creator.OwnerName,
                                                       VehicleType = creator.VehicleType,
                                                       ParkingId = creator.ParkingId,
                                                       RegistrationDate = creator.RegistrationDate,
                                                       ExpirationDate = creator.ExpirationDate,
                                                       Cost = creator.Cost,
                                                       ParkingName = _citizenParkingRepository.GetAll().Where(x => x.Id == creator.ParkingId).Select(x => x.ParkingName).FirstOrDefault()

                                                   })
          .WhereIf(input.Status != default, u => u.Status == input.Status)
          .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
          .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
         .ApplySearchFilter(input.Keyword, u => u.VehicleCardCode, u => u.BuildingName, u => u.UrbanName)
         .AsQueryable();
                var result = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());

                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetCountCarCard()
        {
            try
            {
                IQueryable<GetAllCarCard> query = (from u in _carCardRepository.GetAll()
                                                   join citizenVehicle in _citizenVehicleRepos.GetAll() on u.VehicleCardCode equals citizenVehicle.CardNumber into tb_citizenVehicle
                                                   from creator in tb_citizenVehicle.DefaultIfEmpty()
                                                   select new GetAllCarCard()
                                                   {
                                                       Id = u.Id,
                                                       ApartmentCode = creator.ApartmentCode,
                                                       VehicleType = creator.VehicleType,
                                                       Status = u.Status,
                                                   });

                int countWithNullApartmentCode = query.Count(x => x.ApartmentCode == null && x.Status == CarCardType.Activated);
                int countWithNotNullApartmentCode = query.Count(x => x.ApartmentCode != null && x.Status == CarCardType.Activated);
                int countPendingApproval = query.Count(x => x.Status == CarCardType.WaitingActivation);
                int countByVehicleTypeOther = query.Count(x => x.VehicleType == VehicleType.Other);
                int countByVehicleTypeCar = query.Count(x => x.VehicleType == VehicleType.Car);
                int countByVehicleTypeMotorbike = query.Count(x => x.VehicleType == VehicleType.Motorbike);
                int countByVehicleTypeBicycle = query.Count(x => x.VehicleType == VehicleType.Bicycle);
                int countByVehicleTypeElectricCar = query.Count(x => x.VehicleType == VehicleType.ElectricCar);
                int countByVehicleTypeElectricMotor = query.Count(x => x.VehicleType == VehicleType.ElectricMotor);
                int countByVehicleTypeElectricBike = query.Count(x => x.VehicleType == VehicleType.ElectricBike);

                var result = new
                {
                    TotalRecordsWithPendingApproval = countPendingApproval,
                    TotalRecordsWithNullApartmentCode = countWithNullApartmentCode,
                    TotalRecordsWithNotNullApartmentCode = countWithNotNullApartmentCode,
                    TotalRecordsByVehicleTypeOther = countByVehicleTypeOther,
                    TotalRecordsByVehicleTypeCar = countByVehicleTypeCar,
                    TotalRecordsByVehicleTypeMotorbike = countByVehicleTypeMotorbike,
                    TotalRecordsByVehicleTypeBicycle = countByVehicleTypeBicycle,
                    TotalRecordsByVehicleTypeElectricCar = countByVehicleTypeElectricCar,
                    TotalRecordsByVehicleTypeElectricMotor = countByVehicleTypeElectricMotor,
                    TotalRecordsByVehicleTypeElectricBike = countByVehicleTypeElectricBike
                };
                var data = DataResult.ResultSuccess(result, "Get success");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }



        public async Task<object> GetCarCardById(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var data = _carCardRepository.FirstOrDefault(x => x.Id == id);

                return DataResult.ResultSuccess(data, "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateCarCard(UpdateCarCardInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var carCard = _carCardRepository.FirstOrDefault(x => x.Id == input.Id);
                if (carCard == null)
                {
                    return DataResult.ResultCode(null, "CarCard not found", 404);
                }

                var updateData = input.MapTo(carCard);

                carCard = await _carCardRepository.UpdateAsync(updateData);

                mb.statisticMetris(t1, 0, "CarCardService.UpdateCarCardAsync");

                return DataResult.ResultSuccess(carCard, "Update success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteCarCard(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var carCard = _carCardRepository.FirstOrDefault(x => x.Id == id);
                if (carCard == null)
                {
                    return DataResult.ResultCode(null, "Parking not found", 404);
                }

                await _carCardRepository.DeleteAsync(carCard);

                mb.statisticMetris(t1, 0, "CarCardService.DeleteCarCardAsync");

                return DataResult.ResultSuccess(null, "Delete success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> UpdateStatusCarCard(UpdateStatusCarCardInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var carCard = _carCardRepository.FirstOrDefault(x => x.Id == input.Id);
                if (carCard == null)
                {
                    return DataResult.ResultCode(null, "CarCard not found", 404);
                }

                carCard.Status = input.Status;
                await _carCardRepository.UpdateAsync(carCard);

                mb.statisticMetris(t1, 0, "CarCardService.UpdateStatusCarCardAsync");

                return DataResult.ResultSuccess(carCard, "Update status success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}