using Abp.Application.Services;
using Abp.Domain.Repositories;
using Yootek.EntityDb;
using Yootek.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Abp;
using Abp.Authorization;
using Yootek.Notifications;
using Abp.RealTime;
using Abp.Domain.Uow;
using Yootek.MultiTenancy;
using Yootek.Configuration;
using Abp.Configuration;
using Yootek.Common.Enum;
using Microsoft.EntityFrameworkCore;
using Abp.Extensions;
using Newtonsoft.Json;

namespace Yootek.Services
{
    public interface IBillUtilAppService : IApplicationService
    {
        Task SchedulerCreateBillMonthly();
    }

    public class BillUtilAppService : YootekAppServiceBase, IBillUtilAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<BillConfig, long> _billConfigRepo;
        private readonly IRepository<Citizen, long> _citizenRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepo;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IRepository<HomeMember, long> _homeMemberRepo;
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<UserBillVehicleInfo, long> _billVehicleInfoRepository;
        // private readonly IUserBillRealtimeNotifier _userBillRealtimeNotifier;

        public BillUtilAppService(
            IRepository<UserBill, long> userBillRepo,
            IRepository<BillConfig, long> billConfigRepo,
            IRepository<Citizen, long> citizenRepo,
            IRepository<HomeMember, long> homeMemberRepo,
            IRepository<Tenant> tenantRepository,
            IRepository<CitizenVehicle, long> citizenVehicleRepository,
            IRepository<Apartment, long> apartmentRepository,
            IRepository<CitizenTemp, long> citizenTempRepo,
             IRepository<UserBillVehicleInfo, long> billVehicleInfoRepository,
        //  IUserBillRealtimeNotifier userBillRealtimeNotifier,
            IAppNotifier appNotifier,
            IOnlineClientManager onlineClientManager
        )
        {
            _userBillRepo = userBillRepo;
            _billConfigRepo = billConfigRepo;
            _tenantRepository = tenantRepository;
            _citizenRepo = citizenRepo;
            _homeMemberRepo = homeMemberRepo;
            _citizenTempRepo = citizenTempRepo;
            _billVehicleInfoRepository = billVehicleInfoRepository;
            //  _userBillRealtimeNotifier = userBillRealtimeNotifier;
            _appNotifier = appNotifier;
            _onlineClientManager = onlineClientManager;
            _citizenVehicleRepository = citizenVehicleRepository;   
            _apartmentRepository = apartmentRepository; 
        }

        private double CalculateDependOnLevel(PriceDto[] levels, double amount)
        {
            string[] keys = { "start" };
            Array.Sort(keys, levels);

            var levelIndex = 0;
            for (var i = 0; i < levels.Count(); i++)
            {
                if (amount >= levels[i].From)
                {
                    levelIndex = i;
                }
            }

            var result = 0.0;
            for (var i = 0; i <= levelIndex; i++)
            {
                var level = levels[i];
                if (i != 0) level.From = level.From - 1;

                if (amount < level.To)
                {
                    result += (double)(level.Value * (amount - level.From));
                    break;
                }

                if (i == levelIndex)
                {
                    result += (double)(level.Value * (amount - level.From));
                    break;
                }

                result += (double)(level.Value * (level.To - level.From));
            }

            return result;
        }

        #region Schedule

        [RemoteService(false)]
        [AbpAllowAnonymous]
        public Task SendUserBillToClient(UserBill[] bills, bool isMembersSend = true, long? userId = null)
        {
            // var costResult = CalculateUserBill(bills).Result;
            // bills.Cost = costResult.Cost;
            // bills.LastCost = costResult.LastCost;
            // bills.Surcharges = costResult.Surcharges;

            if (isMembersSend)
            {
                SendBillMessageToMembers(bills);
            }
            else
            {
                SendBillMessageToUser(bills, userId.Value);
            }

            return Task.CompletedTask;
        }

        protected async Task SendBillMessageToMembers(UserBill[] bills)
        {
            var apartmentCodes = bills.Select(x => x.ApartmentCode).ToList();
            var members = _homeMemberRepo.GetAllList(x => apartmentCodes.Any(y => y == x.ApartmentCode))
                .Select(x => x.UserId)
                .ToList().ToHashSet().ToList();
            if (members == null)
            {
                return;
            }

            var users = new List<UserIdentifier>();
            foreach (var usId in members)
            {
                var us = new UserIdentifier(bills[0].TenantId, usId.Value);
                users.Add(us);
                var clients = (await _onlineClientManager.GetAllClientsAsync())
                    .Where(c => c.UserId == usId)
                    .ToImmutableList();
                //    _userBillRealtimeNotifier.NotifyUpdateStateBill(clients, item);
            }

            var totalCost = bills.Sum(x => x.LastCost);

            if (totalCost != null)
            {
                string message = GetMessageText("hóa đơn", bills[0].ApartmentCode, (double)totalCost);
                await _appNotifier.MultiSendMessageAsync("UserBillMessage", users.ToArray(), message);
            }

        }

        protected async Task SendBillMessageToUser(UserBill[] bills, long userId)
        {
            var user = new UserIdentifier(bills[0].TenantId, userId);
            var clients = (await _onlineClientManager.GetAllClientsAsync())
                .Where(c => bills.Any(b => b.CreatorUserId == userId))
                .ToImmutableList();
            var totalCost = bills.Sum(x => x.LastCost);
            if (totalCost != null)
            {
                string message = GetMessageText("hóa đơn", bills[0].ApartmentCode, (double)totalCost);
                //    _userBillRealtimeNotifier.NotifyUpdateStateBill(clients, item);
                await _appNotifier.MultiSendMessageAsync("UserBillMessage", new UserIdentifier[] { user }, message);
            }

        }

        private string GetMessageText(string name, string apartmentCode, double cost)
        {
            return $"Thông báo {name} căn hộ {apartmentCode}: {cost} VND ";
        }

        [RemoteService(false)]
        [AbpAllowAnonymous]
        public async Task SchedulerCreateBillMonthly()
        {
            var period =  new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var peperiod = period.AddMonths(-1);
            var lastMonth = new DateTime(period.Year, period.Month, period.TotalDaysInMonth());
       
            await UnitOfWorkManager.WithUnitOfWorkAsync(async ()=>
            {
                List<Tenant> listTenants = _tenantRepository.GetAllList();
                foreach (Tenant tenant in listTenants)
                {
                    using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                    {
                        var citizens = _citizenTempRepo.GetAll().Where(x => x.IsStayed == true && x.RelationShip == RELATIONSHIP.Contractor).ToList();
                        #region  parking bill
                        var isEnableCreateP = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateP, tenant.Id);
                        var monthNumberP = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberP, tenant.Id);
                        var parkingType = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.UserBillConfig.ParkingBillType, tenant.Id);
                        var dayCreateP = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.ParkingCreateDay, tenant.Id);
                        if (isEnableCreateP && dayCreateP == DateTime.Now.Day)
                        {
                            try
                            {
                                var billconfigs = await _billConfigRepo.GetAll().Where(x => x.BillType == BillType.Parking)                                                                                .Where(x => x.PricesType == (BillConfigPricesType)parkingType).ToListAsync();

                                var defaultConfig = billconfigs.FirstOrDefault(x => x.IsDefault == true && x.IsPrivate != true) ?? billconfigs.FirstOrDefault();
                                var vehicles = await _citizenVehicleRepository.GetAll().Where(x => x.State == CitizenVehicleState.ACCEPTED).ToListAsync();

                                if(vehicles.Any())
                                {
                                    var vehicleApartments = vehicles.GroupBy(x => new
                                    {
                                        x.ApartmentCode,
                                        x.UrbanId,
                                        x.BuildingId
                                    }).Select(x => new
                                    {
                                        Key =  x.Key,
                                        Value = x
                                    }).ToDictionary(x => x.Key, y => y.Value).ToList();

                                    foreach (var item in vehicleApartments)
                                    {
                                        //var properties = JsonConvert.DeserializeObject<dynamic>(config.Properties);
                                        if (monthNumberP > 1)
                                        {
                                            var checkBillMonth = await _userBillRepo.FirstOrDefaultAsync(x =>
                                                    x.ApartmentCode == item.Key.ApartmentCode
                                                    && x.BuildingId == item.Key.BuildingId
                                                    && x.UrbanId == item.Key.UrbanId
                                                    && x.Period.Value.AddMonths(monthNumberP) >= period
                                                    && x.BillType == BillType.Parking);
                                            if (checkBillMonth != null) continue;
                                        };

                                        var checkBill = await _userBillRepo.FirstOrDefaultAsync(x =>
                                        x.ApartmentCode == item.Key.ApartmentCode
                                        && x.BuildingId == item.Key.BuildingId
                                        && x.UrbanId == item.Key.UrbanId
                                        && x.Period.Value.Month == period.Month
                                        && x.Period.Value.Year == period.Year
                                        && x.BillType == BillType.Parking);
                                        if (checkBill != null) continue;
                                       

                                        var customerName = citizens.Where(x => x.ApartmentCode == item.Key.ApartmentCode).Select(x => x.FullName).FirstOrDefault();

                                        var bill = new UserBill()
                                        {
                                            ApartmentCode = item.Key.ApartmentCode,
                                            BuildingId = item.Key.BuildingId,
                                            UrbanId = item.Key.UrbanId,
                                            BillType = BillType.Parking,
                                            Title = $"Hóa đơn gửi xe tháng {period.ToString("MM/yy")}",
                                            Period = period,
                                            DueDate = lastMonth,
                                            DebtTotal = 0,
                                            Status = UserBillStatus.Pending,
                                            TenantId = tenant.Id,
                                            LastCost = 0,
                                            CarNumber = 0,
                                            MotorbikeNumber = 0,
                                            BicycleNumber = 0,
                                            OtherVehicleNumber = 0
                                        };

                                        foreach (var vh in item.Value)
                                        {
                                            bill.LastCost += vh.Cost;
                                            switch (vh.VehicleType)
                                            {
                                                case VehicleType.Car:
                                                    bill.CarNumber += 1;
                                                    break;
                                                case VehicleType.Motorbike:
                                                    bill.MotorbikeNumber += 1;
                                                    break;
                                                case VehicleType.Bicycle:
                                                    bill.BicycleNumber += 1;
                                                    break;
                                                case VehicleType.Other:
                                                    bill.OtherVehicleNumber += 1;
                                                    break;
                                                default:
                                                    break;
                                            }

                                            var info = new UserBillVehicleInfo()
                                            {
                                                CitizenVehicleId = vh.Id,
                                                Cost = vh.Cost ?? 0,
                                                ParkingId = vh.ParkingId,
                                                Period = bill.Period.Value,
                                                TenantId = bill.TenantId,
                                                UserBillId = bill.Id,
                                                VehicleType = vh.VehicleType

                                            };
                                            await _billVehicleInfoRepository.InsertAsync(info);
                                        }

                                        bill.Properties = JsonConvert.SerializeObject(new
                                        {
                                            customerName = customerName,
                                            formulas = new BillConfig[] { },
                                            vehicles = item.Value.ToArray(),
                                            pricesType = parkingType
                                        });

                                        await _userBillRepo.InsertAndGetIdAsync(bill);
                                    }

                                }

                            }
                            catch
                            {
                            }
                        }

                        #endregion

                        #region  management bill

                        var isEnableCreateM = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateM, tenant.Id);
                        var monthNumberM = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberM, tenant.Id);
                        var dayCreateM = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.ManagerCreateDay, tenant.Id);
                        if (isEnableCreateM && dayCreateM == DateTime.Now.Day)
                        {
                            try
                            {
                                var billconfigs = await _billConfigRepo.GetAllListAsync(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport);
                                if (billconfigs == null && billconfigs.Count == 0) continue;

                                var defaultConfig = billconfigs.FirstOrDefault(x => x.IsDefault == true && x.IsPrivate != true) ?? billconfigs.FirstOrDefault();

                                var apartments = await _apartmentRepository.GetAll()
                                .Where(x => x.Area > 0)
                                .Select(x => new ApartmentCreateBillDto()
                                {
                                    Area = x.Area,
                                    ApartmentCode = x.ApartmentCode,
                                    BuildingId = x.BuildingId,
                                    Id = x.Id,  
                                    TypeId = x.TypeId,
                                    UrbanId = x.UrbanId,
                                    BillConfig = x.BillConfig
                                })
                                .ToListAsync();

                                foreach (var apartment in apartments)
                                {
                                    if (monthNumberP > 1)
                                    {
                                        var checkBillMonth = await _userBillRepo.FirstOrDefaultAsync(x =>
                                                x.ApartmentCode == apartment.ApartmentCode
                                                && x.BuildingId == apartment.BuildingId
                                                && x.UrbanId == apartment.UrbanId
                                                && x.BillType == BillType.Manager
                                                && x.Period.Value.AddMonths(monthNumberM) >= period);
                                        if (checkBillMonth != null) continue;
                                    };
                                    var checkBill = await _userBillRepo.FirstOrDefaultAsync(x =>
                                        x.ApartmentCode == apartment.ApartmentCode
                                        && x.BuildingId == apartment.BuildingId
                                        && x.UrbanId == apartment.UrbanId
                                        && x.Period.Value.Month == period.Month
                                        && x.Period.Value.Year == period.Year
                                        && x.BillType == BillType.Manager);
                                    if (checkBill != null) continue;

                                    var config = billconfigs.FirstOrDefault(x => x.ApartmentTypeId == apartment.TypeId && x.IsPrivate == true)
                                           ?? billconfigs.FirstOrDefault(x => x.BuildingId == apartment.BuildingId && x.IsPrivate == true)
                                           ?? billconfigs.FirstOrDefault(x => x.UrbanId == apartment.UrbanId && x.IsPrivate == true) ?? defaultConfig;

                                    var customerName = citizens.Where(x => x.ApartmentCode == apartment.ApartmentCode).Select(x => x.FullName).FirstOrDefault();

                                    if (!string.IsNullOrEmpty(apartment.BillConfig))
                                    {
                                        try
                                        {
                                            var billConfigList = JsonConvert.DeserializeObject<List<BillConfigProperties>>(apartment.BillConfig);
                                            var billConfigMs = billConfigList.Where(x => x.BillType == BillType.Manager).ToList();
                                            if(billConfigMs.Count > 0)
                                            {
                                                var listConfigMs = billConfigMs.SelectMany(x => x.Properties.formulaDetails.ToList()).ToList();
                                                foreach(var cfM in listConfigMs)
                                                {
                                                    await CreateUserBillManager(apartment, cfM, period, tenant.Id, customerName, lastMonth);
                                                }
                                            }
                                            else
                                            {
                                                await CreateUserBillManager(apartment, config, period, tenant.Id, customerName, lastMonth);
                                            }
                                        }
                                        catch
                                        {
                                            await CreateUserBillManager(apartment, config, period, tenant.Id, customerName, lastMonth);
                                        }                                     
                                     
                                    }
                                    else
                                    {
                                        await CreateUserBillManager(apartment, config, period, tenant.Id, customerName, lastMonth);
                                    }
                                  
                                }

                            }
                            catch
                            {
                            }
                        }

                        #endregion


                    }
                }
            });
        }

        private async Task CreateUserBillManager(ApartmentCreateBillDto apartment, BillConfig config, DateTime period, int tenantId, string customerName, DateTime dueDate)
        {
            var billConfigPropertiesM = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(config.Properties);
            var area = apartment.Area ?? 0;
            var bill = new UserBill()
            {
                ApartmentCode = apartment.ApartmentCode,
                BuildingId = apartment.BuildingId,
                UrbanId = apartment.UrbanId,
                BillType = BillType.Manager,
                BillConfigId = config.Id,
                Title = $"Hóa đơn tháng {period.ToString("MM/yy")}",
                TotalIndex = apartment.Area,
                Period = period,
                DueDate = dueDate,
                DebtTotal = 0,
                Status = UserBillStatus.Pending,
                TenantId = tenantId,
                LastCost = billConfigPropertiesM.Prices[0].Value * (double)area
            };
            bill.Properties = JsonConvert.SerializeObject(new
            {
                customerName = customerName,
                formulas = new BillConfig[] { config }
            });
            await _userBillRepo.InsertAsync(bill);
        }

        #endregion
    }
}