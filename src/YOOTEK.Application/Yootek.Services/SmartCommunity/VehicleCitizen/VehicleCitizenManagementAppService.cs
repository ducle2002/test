

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Configuration;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using Yootek.Yootek.Services.Yootek.SmartCommunity.VehicleCitizen;

namespace Yootek.Services
{

    public interface IVehicleCitizenManagementAppService : IApplicationService
    {

    }

    [AbpAuthorize]
    public class VehicleCitizenManagementAppService : YootekAppServiceBase, IVehicleCitizenManagementAppService
    {
        private readonly IRepository<CarCard, long> _carCardRepository;
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepos;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly ICitizenVehicleExcelExporter _excelExporter;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepos;
        private readonly IRepository<CitizenParking, long> _parkingRepos;
        private readonly IRepository<BillConfig, long> _billConfigRepos;
        private readonly IRepository<Apartment, long> _apartmentRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly ApartmentHistoryAppService _apartmentHistoryAppSerivce;



        public VehicleCitizenManagementAppService(
             IRepository<CarCard, long> carCardRepository,
        IRepository<CitizenVehicle, long> citizenVehicleRepos,
            IRepository<CitizenTemp, long> citizenTempRepos,
            ICitizenVehicleExcelExporter excelExporter,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepos,
            IRepository<CitizenParking, long> parkingRepos,
            IRepository<BillConfig, long> billConfigRepos,
            IRepository<Apartment, long> apartmentRepos,
            IRepository<User, long> userRepos,
             ApartmentHistoryAppService apartmentHistoryAppSerivce
            )
        {
            _carCardRepository = carCardRepository;
            _citizenVehicleRepos = citizenVehicleRepos;
            _citizenTempRepos = citizenTempRepos;
            _excelExporter = excelExporter;
            _appOrganizationUnitRepos = appOrganizationUnitRepos;
            _parkingRepos = parkingRepos;
            _billConfigRepos = billConfigRepos;
            _apartmentRepos = apartmentRepos;
            _userRepos = userRepos;
            _apartmentHistoryAppSerivce = apartmentHistoryAppSerivce;
        }

        public async Task<object> GetAllVehicleByApartmentAsync(GetAllVehicleByApartmentInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from vh in _citizenVehicleRepos.GetAll()
                                 select new CitizenVehicleDto()
                                 {
                                     Id = vh.Id,
                                     ApartmentCode = vh.ApartmentCode,
                                     BuildingId = vh.BuildingId,
                                     CitizenTempId = vh.CitizenTempId,
                                     CreationTime = vh.CreationTime,
                                     CreatorUserId = vh.CreatorUserId,
                                     Description = vh.Description,
                                     UrbanId = vh.UrbanId,
                                     VehicleCode = vh.VehicleCode,
                                     VehicleName = vh.VehicleName,
                                     VehicleType = vh.VehicleType,
                                     TenantId = vh.TenantId,
                                     ParkingId = vh.ParkingId,
                                     OwnerName = vh.OwnerName,
                                     CardNumber = vh.CardNumber,
                                     State = vh.State,
                                     RegistrationDate = vh.RegistrationDate,
                                     ExpirationDate = vh.ExpirationDate,
                                     BillConfigId = vh.BillConfigId,
                                     Cost = vh.Cost,
                                     Level = vh.Level,
                                     ImageUrl = vh.ImageUrl
                                 })
                                 .WhereIf(input.VehicleType.HasValue, x => x.VehicleType == input.VehicleType)
                                 .Where(x => x.State != CitizenVehicleState.WAITING)
                                 .WhereIf(input.State.HasValue, x => x.State == input.State)
                                 .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                 .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                 .WhereIf(input.ApartmentCode != null, x => x.ApartmentCode == input.ApartmentCode)
                                 .ApplySearchFilter(input.Keyword, x => x.ApartmentCode, x => x.VehicleCode, x => x.VehicleName)
                                 .AsQueryable();
                    var dirs = query.ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByVehicleByApartment.VEHICLE_CODE).AsEnumerable().GroupBy(x => x.ApartmentCode).Select(x => new
                    {
                        ApartmentCode = x.Key,
                        Vehicles = x.ToList(),
                    }).ToDictionary(x => x.ApartmentCode, y => y.Vehicles);


                    var paginatedData = dirs.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

                    return DataResult.ResultSuccess(paginatedData, "Get success!", dirs.Count());
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetCountVehicleAsync(GetAllVehicleByApartmentInput input)
        {
            try
            {
                var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = _citizenVehicleRepos.GetAll()
                                 // .Where(x => x.State != CitizenVehicleState.WAITING)
                                 .WhereIf(input.State.HasValue, x => x.State == input.State)
                                 .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                 .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                 .WhereIf(input.VehicleType.HasValue, x => x.VehicleType == input.VehicleType)
                                 .ApplySearchFilter(input.Keyword, x => x.ApartmentCode, x => x.VehicleCode, x => x.VehicleName)
                                 .ApplySort(input.OrderBy, input.SortBy)
                                 .ApplySort(OrderByVehicleByApartment.VEHICLE_CODE)
                                 .AsQueryable();

                    var result = new GetCountVehicleOuput();

                    var queryWaiting = query.Where(x => x.State == CitizenVehicleState.WAITING);
                    result.WaitingCarNumber = await queryWaiting.Where(x => x.VehicleType == VehicleType.Car).CountAsync();
                    result.WaitingMotorNumber = await queryWaiting.Where(x => x.VehicleType == VehicleType.Motorbike).CountAsync();
                    result.WaitingBikeNumber = await queryWaiting.Where(x => x.VehicleType == VehicleType.Bicycle).CountAsync();
                    result.WaitingOtherNumber = await queryWaiting.Where(x => x.VehicleType == VehicleType.Other).CountAsync();
                    result.WaitingECarNumber = await queryWaiting.Where(x => x.VehicleType == VehicleType.ElectricCar).CountAsync();
                    result.WaitingEMotorNumber = await queryWaiting.Where(x => x.VehicleType == VehicleType.ElectricMotor).CountAsync();
                    result.WaitingEBikeNumber = await queryWaiting.Where(x => x.VehicleType == VehicleType.ElectricBike).CountAsync();

                    var queryActive = query.Where(x => x.State == CitizenVehicleState.ACCEPTED && (x.ExpirationDate == null || x.ExpirationDate.Value >= currentPeriod));
                    result.ActiveCarNumber = await queryActive.Where(x => x.VehicleType == VehicleType.Car).CountAsync();
                    result.ActiveMotorNumber = await queryActive.Where(x => x.VehicleType == VehicleType.Motorbike).CountAsync();
                    result.ActiveBikeNumber = await queryActive.Where(x => x.VehicleType == VehicleType.Bicycle).CountAsync();
                    result.ActiveOtherNumber = await queryActive.Where(x => x.VehicleType == VehicleType.Other).CountAsync();
                    result.ActiveECarNumber = await queryActive.Where(x => x.VehicleType == VehicleType.ElectricCar).CountAsync();
                    result.ActiveEMotorNumber = await queryActive.Where(x => x.VehicleType == VehicleType.ElectricMotor).CountAsync();
                    result.ActiveEBikeNumber = await queryActive.Where(x => x.VehicleType == VehicleType.ElectricBike).CountAsync();

                    var queryInactive = query.Where(x => x.State == CitizenVehicleState.REJECTED);
                    result.InactiveCarNumber = await queryInactive.Where(x => x.VehicleType == VehicleType.Car).CountAsync();
                    result.InactiveMotorNumber = await queryInactive.Where(x => x.VehicleType == VehicleType.Motorbike).CountAsync();
                    result.InactiveBikeNumber = await queryInactive.Where(x => x.VehicleType == VehicleType.Bicycle).CountAsync();
                    result.InactiveOtherNumber = await queryInactive.Where(x => x.VehicleType == VehicleType.Other).CountAsync();
                    result.InactiveECarNumber = await queryInactive.Where(x => x.VehicleType == VehicleType.ElectricCar).CountAsync();
                    result.InactiveEMotorNumber = await queryInactive.Where(x => x.VehicleType == VehicleType.ElectricMotor).CountAsync();
                    result.InactiveEBikeNumber = await queryInactive.Where(x => x.VehicleType == VehicleType.ElectricBike).CountAsync();

                    var queryExpire = query.Where(x => x.State == CitizenVehicleState.OVERDUE || (x.ExpirationDate.HasValue && x.ExpirationDate.Value < currentPeriod));
                    result.ExpireCarNumber = await queryExpire.Where(x => x.VehicleType == VehicleType.Car).CountAsync();
                    result.ExpireMotorNumber = await queryExpire.Where(x => x.VehicleType == VehicleType.Motorbike).CountAsync();
                    result.ExpireBikeNumber = await queryExpire.Where(x => x.VehicleType == VehicleType.Bicycle).CountAsync();
                    result.ExpireOtherNumber = await queryExpire.Where(x => x.VehicleType == VehicleType.Other).CountAsync();
                    result.ExpireECarNumber = await queryExpire.Where(x => x.VehicleType == VehicleType.ElectricCar).CountAsync();
                    result.ExpireEMotorNumber = await queryExpire.Where(x => x.VehicleType == VehicleType.ElectricMotor).CountAsync();
                    result.ExpireEBikeNumber = await queryExpire.Where(x => x.VehicleType == VehicleType.ElectricBike).CountAsync();

                    return DataResult.ResultSuccess(result, "Get success!");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> GetVehicleByApartmentAsync(GetAllCitizenVehicleInput input)
        {
            try
            {
                var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var result = _citizenVehicleRepos.GetAllList(x => x.ApartmentCode == input.ApartmentCode && x.State == CitizenVehicleState.ACCEPTED && (x.ExpirationDate == null || x.ExpirationDate.Value >= currentPeriod));
                return DataResult.ResultSuccess(result.MapTo<List<CitizenVehicleDto>>(), "Get success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllVehicleByApartmentCodeAsync(GetAllCitizenVehicleInput input)
        {
            try
            {
                var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var result = _citizenVehicleRepos.GetAllList(x => x.ApartmentCode == input.ApartmentCode && (x.State == CitizenVehicleState.ACCEPTED || x.State == CitizenVehicleState.REJECTED) && (x.ExpirationDate == null || x.ExpirationDate.Value >= currentPeriod));
                return DataResult.ResultSuccess(result.MapTo<List<CitizenVehicleByApartmentDto>>(), "Get success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateCostVehicleApartment(UpdateCostVehicleDto data)
        {
            try
            {
                if (data == null) DataResult.ResultFail("data is null");

                await HandleUpdateCostVehicle(data);
                return DataResult.ResultSuccess("Update success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        //public async Task<object> UpdateAllCostVehicleTenant()
        //{
        //    try
        //    {

        //    }catch
        //    {

        //    }
        //}

        private async Task HandleUpdateCostVehicle(UpdateCostVehicleDto item)
        {
            var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var vehicles = await _citizenVehicleRepos.GetAll()
                .Where(x => x.ApartmentCode == item.ApartmentCode && x.State == CitizenVehicleState.ACCEPTED && (x.ExpirationDate == null || x.ExpirationDate.Value >= currentPeriod))
                .Where(x => x.BuildingId == item.BuildingId)
                .Where(x => x.UrbanId == item.UrbanId)
                .ToListAsync();
            var typeParkingBill = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.UserBillConfig.ParkingBillType, AbpSession.TenantId.Value);

            if (typeParkingBill == (int)BillConfigPricesType.ParkingLevel)
            {
                var priceP = _billConfigRepos.GetAllList(x => x.BillType == BillType.Parking);
                var carPrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Car && x.BuildingId == item.BuildingId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Car && x.UrbanId == item.UrbanId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Car);
                var motorPrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Motorbike && x.BuildingId == item.BuildingId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Motorbike && x.UrbanId == item.UrbanId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Motorbike);
                var bikePrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Bicycle && x.BuildingId == item.BuildingId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Bicycle && x.UrbanId == item.UrbanId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Bicycle);
                var otherPrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Other && x.BuildingId == item.BuildingId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Other && x.UrbanId == item.UrbanId && x.IsPrivate == true) ??
                    priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Other);

                var carProperties = new BillConfigPropertiesDto();
                var bikeProperties = new BillConfigPropertiesDto();
                var motorProperties = new BillConfigPropertiesDto();
                var otherProperties = new BillConfigPropertiesDto();
                var maxCar = 1;
                var maxMotor = 1;
                var maxBike = 1;
                var maxOther = 1;

                try
                {
                    carProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(carPrice.Properties);
                    maxCar = carProperties.Prices.Max(x => x.From).Value;
                }
                catch
                {
                    carProperties = null;
                }

                try
                {
                    bikeProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(bikePrice.Properties);
                    maxBike = bikeProperties.Prices.Max(x => x.From).Value;
                }
                catch
                {
                    bikeProperties = null;
                }

                try
                {
                    motorProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(motorPrice.Properties);
                    maxMotor = motorProperties.Prices.Max(x => x.From).Value;
                }
                catch
                {
                    motorProperties = null;
                }

                try
                {
                    otherProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherPrice.Properties);
                    maxOther = otherProperties.Prices.Max(x => x.From).Value;
                }
                catch
                {
                    otherProperties = null;
                }

                var iCar = 0;
                var iMotor = 0;
                var iBike = 0;
                var iOther = 0;
                var cCar = 0;
                var cMotor = 0;
                var cBike = 0;
                var cOther = 0;
                foreach (var vehicle in vehicles)
                {
                    switch (vehicle.VehicleType)
                    {
                        case VehicleType.Car:
                            if (carProperties != null)
                            {
                                try
                                {
                                    var carPkgPros = carProperties;

                                    if (vehicle.ParkingId > 0)
                                    {
                                        var pkgPrice = priceP.Where(x => x.ParkingId == vehicle.ParkingId).FirstOrDefault();
                                        try
                                        {
                                            carPkgPros = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(pkgPrice.Properties);
                                            maxCar = carPkgPros.Prices.Max(x => x.From).Value;
                                        }
                                        catch
                                        {
                                            pkgPrice = null;
                                        }
                                    }


                                    if (iCar < maxCar)
                                    {
                                        var p = carPkgPros.Prices.FirstOrDefault(x => x.From == iCar);
                                        cCar = (int)p.Value;

                                    }
                                    else
                                    {
                                        var p = carPkgPros.Prices.FirstOrDefault(x => x.From == maxCar);
                                        cCar = (int)p.Value;
                                    }
                                    iCar++;
                                }
                                catch { }
                                vehicle.Cost = cCar;
                                vehicle.Level = iCar;
                            }
                            else vehicle.Cost = 0;
                            break;
                        case VehicleType.Motorbike:
                            if (motorProperties != null)
                            {
                                if (iMotor < maxMotor)
                                {
                                    var p = motorProperties.Prices.FirstOrDefault(x => x.From == iMotor);
                                    if (p != null)
                                    {
                                        cMotor = (int)p.Value;
                                    }

                                }
                                else
                                {
                                    var p = motorProperties.Prices.FirstOrDefault(x => x.From == maxMotor);
                                    if (p != null)
                                    {
                                        cMotor = (int)p.Value;
                                    }
                                }
                                iMotor++;
                                vehicle.Cost = cMotor;
                                vehicle.Level = iMotor;
                            }
                            else vehicle.Cost = 0;
                            break;
                        case VehicleType.Bicycle:
                            if (bikeProperties != null)
                            {
                                if (iBike < maxBike)
                                {
                                    var p = bikeProperties.Prices.FirstOrDefault(x => x.From == iBike);
                                    if (p != null)
                                    {
                                        cBike = (int)p.Value;
                                    }

                                }
                                else
                                {
                                    var p = bikeProperties.Prices.FirstOrDefault(x => x.From == maxBike);
                                    if (p != null)
                                    {
                                        cBike = (int)p.Value;
                                    }
                                }
                                iBike++;
                                vehicle.Cost = cBike;
                                vehicle.Level = iBike;
                            }
                            else vehicle.Cost = 0;
                            break;
                        case VehicleType.Other:
                            if (otherProperties != null)
                            {
                                if (iOther < maxOther)
                                {
                                    var p = otherProperties.Prices.FirstOrDefault(x => x.From == iOther);
                                    if (p != null)
                                    {
                                        cOther = (int)p.Value;
                                    }

                                }
                                else
                                {
                                    var p = otherProperties.Prices.FirstOrDefault(x => x.From == maxOther);
                                    if (p != null)
                                    {
                                        cOther = (int)p.Value;
                                    }
                                }
                                iOther++;
                                vehicle.Cost = cOther;
                                vehicle.Level = iOther;
                            }
                            else vehicle.Cost = 0;
                            break;
                        default:
                            break;
                    }

                    await _citizenVehicleRepos.UpdateAsync(vehicle);
                }

            }
            else if (typeParkingBill == (int)BillConfigPricesType.Parking)
            {
                BillConfig config = null;
                if (item.BillConfigId.HasValue)
                {
                    config = _billConfigRepos.FirstOrDefault(item.BillConfigId.Value);
                }

                if (config == null)
                {

                    config = _billConfigRepos.GetAll().Where(x => x.BillType == BillType.Parking)
                        .WhereIf(typeParkingBill > 0, x => x.PricesType == (BillConfigPricesType)typeParkingBill)
                        .WhereIf(item.BuildingId.HasValue, x => x.BuildingId == item.BuildingId)
                        .WhereIf(item.UrbanId.HasValue, x => x.UrbanId == item.UrbanId)
                        .FirstOrDefault();
                    if (config == null) config = _billConfigRepos.FirstOrDefault(x => x.BillType == BillType.Parking);
                }
                if (config == null) return;


                var properties = JsonConvert.DeserializeObject<BillPropertiesDto>(config.Properties);
                foreach (var vh in vehicles)
                {
                    switch (vh.VehicleType)
                    {
                        case VehicleType.Car:
                            vh.Cost = (double)properties.Prices[0].Value;
                            break;
                        case VehicleType.Motorbike:
                            vh.Cost = (double)properties.Prices[1].Value;
                            break;
                        case VehicleType.Bicycle:
                            vh.Cost = (double)properties.Prices[2].Value;
                            break;
                        case VehicleType.Other:
                            vh.Cost = (double)properties.Prices[3].Value;
                            break;
                        default:
                            break;
                    }
                    await _citizenVehicleRepos.UpdateAsync(vh);
                }
            }
        }

        public async Task<object> GetAllVehicleAsync(GetAllCitizenVehicleInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from vh in _citizenVehicleRepos.GetAll()
                                 join cz in _citizenTempRepos.GetAll() on vh.CitizenTempId equals cz.Id into tb_cz
                                 from cz in tb_cz.DefaultIfEmpty()
                                 select new CitizenVehicleDto()
                                 {
                                     Id = vh.Id,
                                     ApartmentCode = vh.ApartmentCode,
                                     BuildingId = vh.BuildingId,
                                     CitizenTempId = vh.CitizenTempId,
                                     CreationTime = vh.CreationTime,
                                     CreatorUserId = vh.CreatorUserId,
                                     Description = vh.Description,
                                     UrbanId = vh.UrbanId,
                                     VehicleCode = vh.VehicleCode,
                                     VehicleName = vh.VehicleName,
                                     VehicleType = vh.VehicleType,
                                     TenantId = vh.TenantId,
                                     ParkingId = vh.ParkingId,
                                     OwnerName = vh.OwnerName,
                                     CardNumber = vh.CardNumber,
                                     State = vh.State,
                                     RegistrationDate = vh.RegistrationDate,
                                     ExpirationDate = vh.ExpirationDate,
                                     BillConfigId = vh.BillConfigId,
                                     Cost = vh.Cost,
                                     Level = vh.Level,
                                     ImageUrl = vh.ImageUrl,
                                     CitizenName = cz.FullName,

                                 })
                                 .WhereIf(input.VehicleType.HasValue, x => x.VehicleType == input.VehicleType)
                                 .WhereIf(input.State.HasValue, x => x.State == input.State)
                                 .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                 .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                 .WhereIf(input.ApartmentCode != null, x => x.ApartmentCode == input.ApartmentCode)
                                 .ApplySearchFilter(input.Keyword, x => x.ApartmentCode, x => x.VehicleCode, x => x.VehicleName)
                                 .AsQueryable();
                    var paginatedData = await query.PageBy(input).ToListAsync();

                    return DataResult.ResultSuccess(paginatedData, "Get success!", query.Count());
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetById(long id)
        {
            try
            {
                var data = await _citizenVehicleRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> CreateOrUpdateVehicleByApartment(CreateOrUpdateVehicleByApartmentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                if (input.DeleteList != null && input.DeleteList.Count > 0)
                {
                    await _citizenVehicleRepos.DeleteAsync(x => input.DeleteList.Contains(x.Id));
                }

                if (input.Value != null && input.Value.Count > 0)
                {
                    foreach (CitizenVehicleDto detail in input.Value)
                    {
                        var existingItem = await _citizenVehicleRepos.FirstOrDefaultAsync(x => x.Id == detail.Id);

                        if (existingItem == null)
                        {
                            // Tạo một item mới
                            var newItem = detail.MapTo<CitizenVehicle>();
                            // Đặt các thuộc tính bổ sung
                            newItem.VehicleCode = detail.VehicleCode;
                            newItem.ApartmentCode = input.ApartmentCode;
                            newItem.BuildingId = input.BuildingId;
                            newItem.UrbanId = input.UrbanId;
                            newItem.Description = input.Description;
                            newItem.OwnerName = input.OwnerName;
                            newItem.TenantId = AbpSession.TenantId;
                            newItem.RegistrationDate = detail.RegistrationDate;
                            newItem.ExpirationDate = detail.ExpirationDate;
                            newItem.ImageUrl = detail.ImageUrl;

                            // Đặt trạng thái dựa trên ngày hết hạn
                            if (newItem.ExpirationDate != null && newItem.ExpirationDate <= DateTime.Now)
                            {
                                if (newItem.State != CitizenVehicleState.REJECTED)
                                {
                                    newItem.State = CitizenVehicleState.OVERDUE;
                                }
                            }
                            else
                            {
                                newItem.State = newItem.State ?? CitizenVehicleState.ACCEPTED;
                            }

                            await _citizenVehicleRepos.InsertAndGetIdAsync(newItem);
                        }
                        else
                        {
                            // Cập nhật item hiện tại
                            detail.MapTo(existingItem);
                            existingItem.VehicleCode = detail.VehicleCode;
                            existingItem.ApartmentCode = input.ApartmentCode;
                            existingItem.BuildingId = input.BuildingId;
                            existingItem.UrbanId = input.UrbanId;
                            existingItem.Description = input.Description;
                            existingItem.OwnerName = input.OwnerName;
                            existingItem.TenantId = AbpSession.TenantId;
                            existingItem.RegistrationDate = detail.RegistrationDate;
                            existingItem.ExpirationDate = detail.ExpirationDate;
                            existingItem.ImageUrl = detail.ImageUrl;

                            // Đặt trạng thái dựa trên ngày hết hạn
                            if (existingItem.ExpirationDate != null && existingItem.ExpirationDate <= DateTime.Now)
                            {
                                if (existingItem.State != CitizenVehicleState.REJECTED)
                                {
                                    existingItem.State = CitizenVehicleState.OVERDUE;
                                }
                            }
                            else
                            {
                                existingItem.State = existingItem.State ?? CitizenVehicleState.ACCEPTED;
                            }

                            await _citizenVehicleRepos.UpdateAsync(existingItem);
                        }
                    }
                }


                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess("Cập nhật thành công");
            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());

                var data = DataResult.ResultError(e.ToString(), "Lỗi ngoại lệ!");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> CreateOrUpdateVehicle(CitizenVehicleDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateData = await _citizenVehicleRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        await _citizenVehicleRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_vehicle");

                    var data = DataResult.ResultSuccess(updateData, "Update success!");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CitizenVehicle>();

                    long id = await _citizenVehicleRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;

                    mb.statisticMetris(t1, 0, "is_vehicle");

                    var data = DataResult.ResultSuccess(insertInput, "Insert success!");
                    return data;
                }

            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());

                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteByApartment(string apartmentCode)
        {
            try
            {
                await _citizenVehicleRepos.DeleteAsync(x => x.ApartmentCode == apartmentCode);
                return DataResult.ResultSuccess("Delete success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMultipleByApartment([FromBody] List<string> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                await _citizenVehicleRepos.DeleteAsync(x => ids.Contains(x.ApartmentCode));
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Delete(long id)
        {
            try
            {
                var deleteData = await _citizenVehicleRepos.GetAsync(id);
                if (deleteData != null)
                {
                    await _citizenVehicleRepos.DeleteAsync(deleteData);
                }
                return DataResult.ResultSuccess("Deleted!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMultiple([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = Delete(id);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> ApproveCitizenVehicleInsert(ApproveCitizenVehicleInsertDto input)
        {
            try
            {
                var vehicle = await _citizenVehicleRepos.GetAsync(input.Id);
                vehicle.State = input.State;
                if (vehicle.State == CitizenVehicleState.ACCEPTED)
                {
                    var apartment = await _apartmentRepos.FirstOrDefaultAsync(x => x.ApartmentCode == vehicle.ApartmentCode);
                    if (apartment != null)
                    {
                        var newHistory = new CreateApartmentHistoryDto();
                        newHistory.TenantId = AbpSession.TenantId;
                        newHistory.ImageUrls = new List<string> { vehicle.ImageUrl };
                        newHistory.ApartmentId = apartment.Id;
                        newHistory.Title = $"Đăng ký xe {vehicle.VehicleName} biển số {vehicle.VehicleCode}";
                        newHistory.Type = EApartmentHistoryType.Vehicle;
                        var user = _userRepos.FirstOrDefault(AbpSession.UserId ?? 0);
                        newHistory.ExecutorName = user.FullName ?? "";
                        newHistory.DateStart = vehicle.RegistrationDate ?? vehicle.CreationTime;
                        newHistory.DateEnd = vehicle.ExpirationDate ?? DateTime.Now;
                        newHistory.Cost = (long?)vehicle.Cost ?? 0;
                        newHistory.Description = vehicle.Description;
                        await _apartmentHistoryAppSerivce.CreateApartmentHistoryAsync(newHistory);

                    }
                }
                await _citizenVehicleRepos.UpdateAsync(vehicle);
                return DataResult.ResultSuccess(vehicle, "Update success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        //[Obsolete]
        public async Task<object> CreateListAsync(RegisterCitizenVehicleInput input)
        {
            try
            {
                //var insertList = new List<CitizenVehicle>();
                foreach (var vh in input.ListVehicle)
                {
                    var insertData = vh.MapTo<CitizenVehicle>();
                    //insertData.State = CitizenVehicleState.WAITING;
                    insertData.CitizenTempId = input.CitizenTempId;
                    insertData.ApartmentCode = input.ApartmentCode;
                    insertData.BuildingId = input.BuildingId;
                    insertData.UrbanId = input.UrbanId;
                    insertData.TenantId = AbpSession.TenantId;
                    insertData.Description = input.Description;
                    long id = await _citizenVehicleRepos.InsertAndGetIdAsync(insertData);
                    insertData.Id = id;
                    insertData.RegistrationDate = vh.RegistrationDate;
                    insertData.ExpirationDate = vh.ExpirationDate;

                    // Kiểm tra xem ExpirationDate có vượt quá ngày hiện tại không
                    if (insertData.ExpirationDate != null && insertData.ExpirationDate <= DateTime.Now)
                    {
                        // Nếu State là REJECTED thì không cần cập nhật
                        if (insertData.State != CitizenVehicleState.REJECTED)
                        {
                            insertData.State = CitizenVehicleState.OVERDUE;
                        }
                    }
                    else
                    {
                        insertData.State = CitizenVehicleState.ACCEPTED;
                    }
                }
                var data = DataResult.ResultSuccess(input.ListVehicle, "Success!", input.ListVehicle.Count);
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> ExportToExcel(ExportVehicleToExcelInput input)
        {
            try
            {
                var query = (from vh in _citizenVehicleRepos.GetAll()
                             join pk in _parkingRepos.GetAll() on vh.ParkingId equals pk.Id into tbl_prk
                             from pk in tbl_prk
                             join ub in _appOrganizationUnitRepos.GetAll() on vh.UrbanId equals ub.Id into tbl_urban
                             from ub in tbl_urban
                             join bu in _appOrganizationUnitRepos.GetAll() on vh.BuildingId equals bu.Id into tbl_building
                             from bu in tbl_building
                             select new CitizenVehicleExcelOutputDto()
                             {
                                 Id = vh.Id,
                                 ApartmentCode = vh.ApartmentCode,
                                 BuildingId = vh.BuildingId,
                                 CitizenTempId = vh.CitizenTempId,
                                 CreationTime = vh.CreationTime,
                                 CreatorUserId = vh.CreatorUserId,
                                 Description = vh.Description,
                                 UrbanId = vh.UrbanId,
                                 VehicleCode = vh.VehicleCode,
                                 VehicleName = vh.VehicleName,
                                 VehicleType = vh.VehicleType,
                                 TenantId = vh.TenantId,
                                 ParkingId = vh.ParkingId,
                                 State = vh.State,
                                 UrbanCode = ub.ProjectCode,
                                 BuildingCode = bu.ProjectCode,
                                 ParkingName = pk.ParkingName,
                                 OwnerName = vh.OwnerName,
                                 CardNumber = vh.CardNumber,
                             })
                             .Where(x => x.State == CitizenVehicleState.ACCEPTED)
                             .WhereIf(input.ApartmentCodes != null, x => input.ApartmentCodes.Contains(x.ApartmentCode))
                             .OrderBy(x => x.ApartmentCode).AsQueryable();
                var data = await query.ToListAsync();
                var result = _excelExporter.ExportCitizenVehicleToFile(data);
                return DataResult.ResultSuccess(result, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        protected VehicleType GetVehicleTypeNumber(string type)
        {
            if (type.ToLower().Contains("electric car")
               || type.ToLower().Contains("ô tô điện")
               || type.ToLower().Contains("전기차".ToLower())) return VehicleType.ElectricCar;
            if (type.ToLower().Contains("electric motorcycle")
                || type.ToLower().Contains("electric motorbike")
                || type.ToLower().Contains("xe máy điện")
                || type.ToLower().Contains("전기 오토바이".ToLower())) return VehicleType.ElectricMotor;
            if (type.ToLower().Contains("Electric Bicycle")
                || type.ToLower().Contains("electric bike")
                || type.ToLower().Contains("xe đạp điện")
                || type.ToLower().Contains("전기 자전거".ToLower())) return VehicleType.ElectricBike;
            if (type.ToLower().Contains("car")
                || type.ToLower().Contains("ô tô")
                || type.ToLower().Contains("자동차".ToLower())) return VehicleType.Car;
            if (type.ToLower().Contains("motorbike")
                || type.ToLower().Contains("motorcycle")
                || type.ToLower().Contains("xe máy")
                || type.ToLower().Contains("오토바이".ToLower())) return VehicleType.Motorbike;
            if (type.ToLower().Contains("Bicycle")
                || type.ToLower().Contains("bike")
                || type.ToLower().Contains("xe đạp")
                || type.ToLower().Contains("자전거".ToLower())) return VehicleType.Bicycle;
            return VehicleType.Other;
        }

        public async Task<object> ImportVehicleFromExcel([FromForm] ImportVehicleInput input)
        {
            const int COL_URBAN_CODE = 1;
            const int COL_BUILDING_CODE = 2;
            const int COL_APARTMENT_CODE = 3;
            const int COL_CUSTOMER_NAME = 4;
            const int COL_CARD_NUMBER = 5;
            const int COL_VEHICLE_TYPE = 6;
            const int COL_VEHICLE_CODE = 7;
            const int COL_VEHICLE_NAME = 8;
            const int COL_PARKING_LOT_CODE = 9;
            const int COL_DESCRIPTION = 10;
            const int COL_LEVEL = 11;
            const int COL_COST = 12;
            const int COL_REGISTER = 13;
            const int COL_EXPIRES = 14;
            const int COL_BILLCONFIG_CODE = 15;

            var file = input.Form;
            var fileName = file.FileName;
            var fileExt = Path.GetExtension(fileName);
            if (fileExt != ".xlsx" && fileExt != ".xls")
            {
                return DataResult.ResultError("File not support", "Error");
            }

            var filePath = Path.GetRandomFileName() + fileExt;
            var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            try
            {
                var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();

                var rowCount = worksheet.Dimension.End.Row;
                var colCount = worksheet.Dimension.End.Column;
                var listVehicles = new List<CitizenVehicle>();

                for (var row = 2; row <= rowCount; row++)
                {
                    var citizenVehicle = new CitizenVehicle();

                    if (worksheet.Cells[row, COL_VEHICLE_CODE].Value != null)
                        citizenVehicle.VehicleCode = worksheet.Cells[row, COL_VEHICLE_CODE].Value.ToString().Trim();

                    if ((await _citizenVehicleRepos.FirstOrDefaultAsync(x => x.VehicleCode == citizenVehicle.VehicleCode)) != null) continue;

                    if (worksheet.Cells[row, COL_APARTMENT_CODE].Value != null)
                    {
                        citizenVehicle.ApartmentCode = worksheet.Cells[row, COL_APARTMENT_CODE].Value.ToString().Trim();
                    }
                    else
                    {
                        continue;
                    }

                    if (worksheet.Cells[row, COL_URBAN_CODE].Value != null)
                    {
                        var ubIDstr = worksheet.Cells[row, COL_URBAN_CODE].Value.ToString().Trim();
                        var ubObj = await _appOrganizationUnitRepos.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == ubIDstr.ToLower());
                        if (ubObj != null) { citizenVehicle.UrbanId = ubObj.Id; }
                        else
                        {
                            continue;
                        }
                    }
                    else continue;

                    if (worksheet.Cells[row, COL_BUILDING_CODE].Value != null)
                    {
                        var buildIDStr = worksheet.Cells[row, COL_BUILDING_CODE].Value.ToString().Trim();
                        var buildObj = await _appOrganizationUnitRepos.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == buildIDStr.ToLower() && x.ParentId != null);
                        if (buildObj != null)
                        {
                            citizenVehicle.BuildingId = buildObj.Id;
                        }
                        else continue;
                    }
                    else continue;

                    if (worksheet.Cells[row, COL_VEHICLE_NAME].Value != null)
                    {
                        citizenVehicle.VehicleName = worksheet.Cells[row, COL_VEHICLE_NAME].Value.ToString().Trim();
                    }
                    else continue;
                    if (worksheet.Cells[row, COL_VEHICLE_TYPE].Value != null)
                        citizenVehicle.VehicleType = GetVehicleTypeNumber(worksheet.Cells[row, COL_VEHICLE_TYPE].Value.ToString().Trim());
                    else citizenVehicle.VehicleType = VehicleType.Other;

                    if (worksheet.Cells[row, COL_PARKING_LOT_CODE].Value != null)
                    {
                        var parkingIDstr = worksheet.Cells[row, COL_PARKING_LOT_CODE].Value.ToString().Trim();

                        var checkParkingId = (await _parkingRepos.FirstOrDefaultAsync(x => x.ParkingCode == parkingIDstr));
                        if (checkParkingId != null)
                        {
                            citizenVehicle.ParkingId = checkParkingId.Id;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else continue;

                    if (worksheet.Cells[row, COL_CARD_NUMBER].Value != null)
                    {
                        var cardNumber = worksheet.Cells[row, COL_CARD_NUMBER].Value.ToString().Trim();

                        var checkCarCard = await _citizenVehicleRepos.FirstOrDefaultAsync(x => x.CardNumber.ToLower() == cardNumber.ToLower());


                        if (checkCarCard != null)
                        {
                            continue;
                        }

                        citizenVehicle.CardNumber = cardNumber;
                    }
                    else
                    {
                        continue;
                    }



                    if (worksheet.Cells[row, COL_CUSTOMER_NAME].Value != null)
                    {
                        citizenVehicle.OwnerName = worksheet.Cells[row, COL_CUSTOMER_NAME].Value.ToString().Trim();
                    }

                    if (worksheet.Cells[row, COL_DESCRIPTION].Value != null)
                        citizenVehicle.Description = worksheet.Cells[row, COL_DESCRIPTION].Value.ToString().Trim();

                    if (worksheet.Cells[row, COL_LEVEL].Value != null)
                    {
                        citizenVehicle.Level = int.Parse(worksheet.Cells[row, COL_LEVEL].Value.ToString().Trim());
                    }
                    else continue;

                    if (worksheet.Cells[row, COL_COST].Value != null)
                    {
                        citizenVehicle.Cost = double.Parse(worksheet.Cells[row, COL_COST].Value.ToString().Trim());
                    }
                    else
                    {
                        citizenVehicle.Cost = 0;
                    }

                    if (worksheet.Cells[row, COL_REGISTER].Value != null)
                    {


                        citizenVehicle.RegistrationDate = DateTime.ParseExact(worksheet.Cells[row, COL_REGISTER].Value.ToString().Trim(), "dd/MM/yyyy",
                              CultureInfo.InvariantCulture);
                    }
                    else continue;
                    if (worksheet.Cells[row, COL_EXPIRES].Value != null)
                    {
                        citizenVehicle.ExpirationDate = DateTime.ParseExact(worksheet.Cells[row, COL_EXPIRES].Value.ToString().Trim(), "dd/MM/yyyy",
                              CultureInfo.InvariantCulture);
                    }
                    else continue;

                    citizenVehicle.State = CitizenVehicleState.ACCEPTED;
                    citizenVehicle.TenantId = AbpSession.TenantId;

                    if (worksheet.Cells[row, COL_BILLCONFIG_CODE].Value != null)
                    {
                        var code = worksheet.Cells[row, COL_BILLCONFIG_CODE].Value.ToString().Trim();

                        var config = await _billConfigRepos.FirstOrDefaultAsync(x => x.Code == code && x.BillType == BillType.Parking && (x.PricesType == BillConfigPricesType.ParkingLevel || x.PricesType == BillConfigPricesType.Parking));

                        if (config != null)
                        {
                            citizenVehicle.BillConfigId = config.Id;
                        }
                        else
                        {
                            citizenVehicle.BillConfigId = null;
                        }


                    }

                    listVehicles.Add(citizenVehicle);

                }

                await CreateListVehicleAsync(listVehicles);
                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);

                return DataResult.ResultSuccess(listVehicles, "Success");
            }
            catch (Exception e)
            {
                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);
                Logger.Fatal(e.Message);
                throw;
            }
        }

        private async Task CreateListVehicleAsync(List<CitizenVehicle> input)
        {
            try
            {
                if (input == null || !input.Any())
                {
                    return;
                }
                var groupedVehicles = input.GroupBy(v => new { v.ApartmentCode, v.BuildingId, v.UrbanId, v.BillConfigId })
                                    .ToDictionary(g => $"{g.Key.ApartmentCode}/{g.Key.BuildingId}/{g.Key.UrbanId}/{g.Key.BillConfigId}", g => g.ToList());
                foreach (var group in groupedVehicles)
                {


                    var totalVehicles = await _citizenVehicleRepos.CountAsync(x =>
             x.ApartmentCode == group.Value[0].ApartmentCode &&
             x.BuildingId == group.Value[0].BuildingId &&
             x.UrbanId == group.Value[0].UrbanId &&
             x.VehicleType == group.Value[0].VehicleType &&
              x.BillConfigId == group.Value[0].BillConfigId &&
             x.State == CitizenVehicleState.ACCEPTED);

                    foreach (var vehicle in group.Value)
                    {
                        totalVehicles++;
                        await ProcessVehicleAsync(vehicle, totalVehicles);
                    }
                }


            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }
        public async Task ProcessVehicleAsync(CitizenVehicle vh, int totalVehicles)
        {

            var checkCarCard = _carCardRepository.FirstOrDefault(x => x.VehicleCardCode == vh.CardNumber);
            if (checkCarCard == null)
            {
                var carCard = new CreateCarCardDto();
                carCard.VehicleCardCode = vh.CardNumber;
                carCard.ParkingId = vh.ParkingId;
                var carCardNew = carCard.MapTo<CarCard>();
                carCardNew.TenantId = AbpSession.TenantId;
                await _carCardRepository.InsertAsync(carCardNew);
            }
            if (vh.BillConfigId == null)
            {

                await _citizenVehicleRepos.InsertAsync(vh);
            }
            else
            {

                var carPropertiesInput = _billConfigRepos.FirstOrDefault(x => x.Id == vh.BillConfigId);
                var carProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(carPropertiesInput.Properties);
                if (carPropertiesInput.PricesType == BillConfigPricesType.Parking)
                {
                    switch (vh.VehicleType)
                    {
                        case VehicleType.Car:
                            vh.Cost = (double)carProperties.Prices[0].Value;
                            break;
                        case VehicleType.Motorbike:
                            vh.Cost = (double)carProperties.Prices[1].Value;
                            break;
                        case VehicleType.Bicycle:
                            vh.Cost = (double)carProperties.Prices[2].Value;
                            break;
                        case VehicleType.ElectricCar:
                            vh.Cost = (double)carProperties.Prices[2].Value;
                            break;
                        case VehicleType.ElectricMotor:
                            vh.Cost = (double)carProperties.Prices[2].Value;
                            break;
                        case VehicleType.ElectricBike:
                            vh.Cost = (double)carProperties.Prices[2].Value;
                            break;
                        case VehicleType.Other:
                            vh.Cost = (double)carProperties.Prices[3].Value;
                            break;
                        default:
                            break;
                    }
                    await _citizenVehicleRepos.InsertAsync(vh);
                }
                else
                {
                    foreach (var price in carProperties.Prices)
                    {
                        if (price.From == totalVehicles)
                        {
                            vh.Cost = price.Value;
                        }
                        else if (price.From < totalVehicles)
                        {
                            vh.Cost = price.Value;
                        }
                    }
                    await _citizenVehicleRepos.InsertAsync(vh);
                }

            }

        }
        public async Task<object> GetVehicleById(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var data = _citizenVehicleRepos.FirstOrDefault(x => x.Id == id);
                mb.statisticMetris(t1, 0, "GetVehicleById");
                return DataResult.ResultSuccess(data, "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> UpdateVehicleRegistrationApproval(UpdateVehicleApproval input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var checkCarCard = _carCardRepository.FirstOrDefault(x => x.VehicleCardCode == input.CardNumber);
                if (checkCarCard == null)
                {

                    var carCard = new CreateCarCardDto();
                    carCard.VehicleCardCode = input.CardNumber;
                    carCard.ParkingId = input.ParkingId;
                    var carCardNew = carCard.MapTo<CarCard>();
                    carCardNew.TenantId = AbpSession.TenantId;
                    await _carCardRepository.InsertAsync(carCardNew);
                }
                var data = _citizenVehicleRepos.FirstOrDefault(x => x.Id == input.Id);
                data.CardNumber = input.CardNumber;
                data.RegistrationDate = input.RegistrationDate;
                data.ExpirationDate = input.ExpirationDate;
                data.State = CitizenVehicleState.ACCEPTED;
                data.ParkingId = input.ParkingId;
                data.Cost = input.Cost;
                data = await _citizenVehicleRepos.UpdateAsync(data);
                mb.statisticMetris(t1, 0, "UpdateVehicleRegistrationApproval");
                return DataResult.ResultSuccess(data, "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetTotalVehiclesApartment(TotalVehiclesApartment input)
        {
            try
            {
                long startTime = TimeUtils.GetNanoseconds();

                var query = await _citizenVehicleRepos.GetAll()
                    .Where(x =>
                        x.ApartmentCode == input.ApartmentCode &&
                        x.BuildingId == input.BuildingId &&
                        x.UrbanId == input.UrbanId &&
                        x.VehicleType == input.VehicleType && x.State == CitizenVehicleState.ACCEPTED)
                    .ToListAsync();

                var totalVehicles = query.Count + 1;

                mb.statisticMetris(startTime, totalVehicles, "GetTotalVehiclesApartment");

                return DataResult.ResultSuccess(totalVehicles, "Get success");

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> GetAllParkingPrices(GetAllParkingPrices input)
        {
            try
            {
                long startTime = TimeUtils.GetNanoseconds();
                var query = _billConfigRepos.GetAll();
                if (input.ParkingId.HasValue)
                {
                    query = query.Where(x => x.ParkingId == input.ParkingId || x.ParkingId == null && x.BillType == BillType.Parking);
                }
                else
                {
                    query = query.Where(x => x.BillType == BillType.Parking && x.ParkingId == null);
                }

                var result = await query.ToListAsync();
                mb.statisticMetris(startTime, 0, "GetTotalVehiclesApartment");

                return DataResult.ResultSuccess(result, "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


    }
}
