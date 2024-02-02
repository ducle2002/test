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
                var carCard = await _carCardRepository.FirstOrDefaultAsync(x => x.ParkingId == input.ParkingId && x.VehicleCardCode == input.VehicleCardCode);
                if (carCard != null)
                {
                    throw new UserFriendlyException("Creation failed");
                }

                carCard = input.MapTo<CarCard>();
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
                                                       CreationTime = u.CreationTime,
                                                       State = creator.State,
                                                       VehicleCardCode = u.VehicleCardCode,
                                                       BuildingId = creator.BuildingId,
                                                       UrbanId = creator.UrbanId,
                                                       BuildingName = _organizationUnit.GetAll().Where(x => x.Id == creator.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                                                       UrbanName = _organizationUnit.GetAll().Where(x => x.Id == creator.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                                                       ApartmentCode = creator.ApartmentCode,
                                                       VehicleName = creator.VehicleName,
                                                       VehicleCode = creator.VehicleCode,
                                                       OwnerName = creator.OwnerName,
                                                       VehicleType = creator.VehicleType,
                                                       ParkingId = u.ParkingId,
                                                       RegistrationDate = creator.RegistrationDate,
                                                       ExpirationDate = creator.ExpirationDate,
                                                       Cost = creator.Cost,
                                                       ParkingName = _citizenParkingRepository.GetAll().Where(x => x.Id == u.ParkingId).Select(x => x.ParkingName).FirstOrDefault()

                                                   })
          .WhereIf(input.State != default, u => u.State == input.State)
          .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
          .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
          .WhereIf(input.Status == 5, x => x.State == null)
          .WhereIf(input.ToDay.HasValue, x => x.CreationTime.Date == input.ToDay.Value.Date)
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
                                                       State = creator.State,
                                                   });

                int countCardNotHasBeenUsed = query.Count(x => x.ApartmentCode == null);
                int countCardHasBeenUsed = query.Count(x => x.ApartmentCode != null);

                int countCardAccepted = query.Count(x => x.ApartmentCode != null && x.State == CitizenVehicleState.ACCEPTED);
                int countCardReject = query.Count(x => x.ApartmentCode != null && x.State == CitizenVehicleState.REJECTED);
                int countCardOverdue = query.Count(x => x.ApartmentCode != null && x.State == CitizenVehicleState.OVERDUE);
                int countCardWatting = query.Count(x => x.ApartmentCode != null && x.State == CitizenVehicleState.WAITING);

                int countByVehicleTypeOtherAccepted = query.Count(x => x.VehicleType == VehicleType.Other && x.State == CitizenVehicleState.ACCEPTED);
                int countByVehicleTypeCarAccepted = query.Count(x => x.VehicleType == VehicleType.Car && x.State == CitizenVehicleState.ACCEPTED);
                int countByVehicleTypeMotorbikeAccepted = query.Count(x => x.VehicleType == VehicleType.Motorbike && x.State == CitizenVehicleState.ACCEPTED);
                int countByVehicleTypeBicycleAccepted = query.Count(x => x.VehicleType == VehicleType.Bicycle && x.State == CitizenVehicleState.ACCEPTED);
                int countByVehicleTypeElectricCarAccepted = query.Count(x => x.VehicleType == VehicleType.ElectricCar && x.State == CitizenVehicleState.ACCEPTED);
                int countByVehicleTypeElectricMotorAccepted = query.Count(x => x.VehicleType == VehicleType.ElectricMotor && x.State == CitizenVehicleState.ACCEPTED);
                int countByVehicleTypeElectricBikeAccepted = query.Count(x => x.VehicleType == VehicleType.ElectricBike && x.State == CitizenVehicleState.ACCEPTED);

                int countByVehicleTypeOtherWaiting = query.Count(x => x.VehicleType == VehicleType.Other && x.State == CitizenVehicleState.WAITING);
                int countByVehicleTypeCarWaiting = query.Count(x => x.VehicleType == VehicleType.Car && x.State == CitizenVehicleState.WAITING);
                int countByVehicleTypeMotorbikeWaiting = query.Count(x => x.VehicleType == VehicleType.Motorbike && x.State == CitizenVehicleState.WAITING);
                int countByVehicleTypeBicycleWaiting = query.Count(x => x.VehicleType == VehicleType.Bicycle && x.State == CitizenVehicleState.WAITING);
                int countByVehicleTypeElectricCarWaiting = query.Count(x => x.VehicleType == VehicleType.ElectricCar && x.State == CitizenVehicleState.WAITING);
                int countByVehicleTypeElectricMotorWaiting = query.Count(x => x.VehicleType == VehicleType.ElectricMotor && x.State == CitizenVehicleState.WAITING);
                int countByVehicleTypeElectricBikeWaiting = query.Count(x => x.VehicleType == VehicleType.ElectricBike && x.State == CitizenVehicleState.WAITING);

                int countByVehicleTypeOtherReject = query.Count(x => x.VehicleType == VehicleType.Other && x.State == CitizenVehicleState.REJECTED);
                int countByVehicleTypeCarReject = query.Count(x => x.VehicleType == VehicleType.Car && x.State == CitizenVehicleState.REJECTED);
                int countByVehicleTypeMotorbikeReject = query.Count(x => x.VehicleType == VehicleType.Motorbike && x.State == CitizenVehicleState.REJECTED);
                int countByVehicleTypeBicycleReject = query.Count(x => x.VehicleType == VehicleType.Bicycle && x.State == CitizenVehicleState.REJECTED);
                int countByVehicleTypeElectricCarReject = query.Count(x => x.VehicleType == VehicleType.ElectricCar && x.State == CitizenVehicleState.REJECTED);
                int countByVehicleTypeElectricMotorReject = query.Count(x => x.VehicleType == VehicleType.ElectricMotor && x.State == CitizenVehicleState.REJECTED);
                int countByVehicleTypeElectricBikeReject = query.Count(x => x.VehicleType == VehicleType.ElectricBike && x.State == CitizenVehicleState.REJECTED);

                int countByVehicleTypeOtherOverdue = query.Count(x => x.VehicleType == VehicleType.Other && x.State == CitizenVehicleState.OVERDUE);
                int countByVehicleTypeCarOverdue = query.Count(x => x.VehicleType == VehicleType.Car && x.State == CitizenVehicleState.OVERDUE);
                int countByVehicleTypeMotorbikeOverdue = query.Count(x => x.VehicleType == VehicleType.Motorbike && x.State == CitizenVehicleState.OVERDUE);
                int countByVehicleTypeBicycleOverdue = query.Count(x => x.VehicleType == VehicleType.Bicycle && x.State == CitizenVehicleState.OVERDUE);
                int countByVehicleTypeElectricCarOverdue = query.Count(x => x.VehicleType == VehicleType.ElectricCar && x.State == CitizenVehicleState.OVERDUE);
                int countByVehicleTypeElectricMotorOverdue = query.Count(x => x.VehicleType == VehicleType.ElectricMotor && x.State == CitizenVehicleState.OVERDUE);
                int countByVehicleTypeElectricBikeOverdue = query.Count(x => x.VehicleType == VehicleType.ElectricBike && x.State == CitizenVehicleState.OVERDUE);

                var result = new
                {
                    TotalcountCardHasBeenUsed = countCardHasBeenUsed,
                    TotalCountCardNotHasBeenUsed = countCardNotHasBeenUsed,
                    TotalCountCardAccepted = countCardAccepted,
                    TotalCountCardReject = countCardReject,
                    TotalCountCardOverdue = countCardOverdue,
                    TotalCountCardWatting = countCardWatting,
                    TotalCountByVehicleTypeOtherAccepted = countByVehicleTypeOtherAccepted,
                    TotalCountByVehicleTypeCarAccepted = countByVehicleTypeCarAccepted,
                    TotalCountByVehicleTypeMotorbikeAccepted = countByVehicleTypeMotorbikeAccepted,
                    TotalCountByVehicleTypeBicycleAccepted = countByVehicleTypeBicycleAccepted,
                    TotalCountByVehicleTypeElectricCarAccepted = countByVehicleTypeElectricCarAccepted,
                    TotalCountByVehicleTypeElectricMotorAccepted = countByVehicleTypeElectricMotorAccepted,
                    TotalCountByVehicleTypeElectricBikeAccepted = countByVehicleTypeElectricBikeAccepted,
                    TotalCountByVehicleTypeOtherWaiting = countByVehicleTypeOtherWaiting,
                    TotalCountByVehicleTypeCarWaiting = countByVehicleTypeCarWaiting,
                    TotalCountByVehicleTypeMotorbikeWaiting = countByVehicleTypeMotorbikeWaiting,
                    TotalCountByVehicleTypeBicycleWaiting = countByVehicleTypeBicycleWaiting,
                    TotalCountByVehicleTypeElectricCarWaiting = countByVehicleTypeElectricCarWaiting,
                    TotalCountByVehicleTypeElectricMotorWaiting = countByVehicleTypeElectricMotorWaiting,
                    TotalCountByVehicleTypeElectricBikeWaiting = countByVehicleTypeElectricBikeWaiting,
                    TotalCountByVehicleTypeOtherReject = countByVehicleTypeOtherReject,
                    TotalCountByVehicleTypeCarReject = countByVehicleTypeCarReject,
                    TotalCountByVehicleTypeMotorbikeReject = countByVehicleTypeMotorbikeReject,
                    TotalCountByVehicleTypeBicycleReject = countByVehicleTypeBicycleReject,
                    TotalCountByVehicleTypeElectricCarReject = countByVehicleTypeElectricCarReject,
                    TotalCountByVehicleTypeElectricMotorReject = countByVehicleTypeElectricMotorReject,
                    TotalCountByVehicleTypeElectricBikeReject = countByVehicleTypeElectricBikeReject,
                    TotalCountByVehicleTypeOtherOverdue = countByVehicleTypeOtherOverdue,
                    TotalCountByVehicleTypeCarOverdue = countByVehicleTypeCarOverdue,
                    TotalCountByVehicleTypeMotorbikeOverdue = countByVehicleTypeMotorbikeOverdue,
                    TotalCountByVehicleTypeBicycleOverdue = countByVehicleTypeBicycleOverdue,
                    TotalCountByVehicleTypeElectricCarOverdue = countByVehicleTypeElectricCarOverdue,
                    TotalCountByVehicleTypeElectricMotorOverdue = countByVehicleTypeElectricMotorOverdue,
                    TotalCountByVehicleTypeElectricBikeOverdue = countByVehicleTypeElectricBikeOverdue,
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
        public async Task<object> UpdateRejectCarCard(UpdateStatusCarCardInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var carCard = _citizenVehicleRepos.FirstOrDefault(x => x.CardNumber == input.VehicleCardCode);
                if (carCard == null)
                {
                    return DataResult.ResultCode(null, "CarCard not found", 404);
                }


                carCard.State = input.State;
                await _citizenVehicleRepos.UpdateAsync(carCard);

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