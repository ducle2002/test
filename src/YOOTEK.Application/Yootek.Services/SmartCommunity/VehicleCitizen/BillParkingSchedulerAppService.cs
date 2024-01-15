using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Yootek.Common.Enum;
using Yootek.Configuration;
using Yootek.EntityDb;
using Yootek.MultiTenancy;
using Yootek.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IBillParkingSchedulerAppService: IApplicationService
    {
        Task CreateBillParkingMonthly();
    }

    public class BillParkingSchedulerAppService: YootekAppServiceBase, IBillParkingSchedulerAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<BillConfig, long> _billConfigRepository;
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepos;
        private readonly IRepository<Tenant, int> _tenantRepos;
        private readonly IRepository<BillDebt, long> _billDebtRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<Citizen, long> _citizenRepos;

        public BillParkingSchedulerAppService(
            IRepository<UserBill, long> userBillRepository,
            IRepository<CitizenVehicle, long> citizenVehicleRepos,
            IRepository<Tenant, int> tenantRepos,
            IRepository<BillDebt, long> billDebtRepos,
            IAppNotifier appNotifier,
            IRepository<Citizen, long> citizenRepos,
            IRepository<BillConfig, long> billConfigRepository
            )
        {
            _userBillRepository = userBillRepository;
            _citizenVehicleRepos = citizenVehicleRepos;
            _tenantRepos = tenantRepos;
            _billDebtRepos = billDebtRepos;
            _appNotifier = appNotifier;
            _citizenRepos = citizenRepos;
            _billConfigRepository = billConfigRepository;
        }

        [AbpAllowAnonymous]
        [RemoteService(true)]
        public async Task CreateBillParkingMonthly()
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenant = _tenantRepos.GetAllList();
                foreach (var tenantItem in tenant)
                {
                    using (CurrentUnitOfWork.SetTenantId(tenantItem.Id))
                    {

                        try
                        {
                            var today = DateTime.Now;
                            var preMonth = today.AddMonths(-1);

                            var timeCheck = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.ParkingCreateDay, tenantItem.Id);

                            if(timeCheck > 0 && timeCheck == today.Day)
                            {
                                var vehicles = _citizenVehicleRepos.GetAllList(x => x.State == CitizenVehicleState.ACCEPTED);
                                if (vehicles != null && vehicles.Count > 0)
                                {
                                    var apartmentVehicles = vehicles.GroupBy(x => new
                                    {
                                        x.ApartmentCode
                                    }).Select(x => new
                                    {
                                        Key = x.Key.ApartmentCode,
                                        Value = x.ToList()
                                    }).ToDictionary(x => x.Key, y => y.Value);

                                    if(tenantItem.Id == 62)
                                    {
                                       await CreateParkingBillMonthHTS(apartmentVehicles);
                                    }
                                }
                            }
                         
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                }

            });

        }


        private async Task CreateParkingBillMonthHTS(Dictionary<string, List<CitizenVehicle>> vehicles)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(62))
                {
                    var prices = _billConfigRepository.GetAllList(x => x.BillType == BillType.Parking);
                    foreach(var vehicle in vehicles)
                    {
                        var apartmentCode = vehicle.Key;
                        var listvh = vehicle.Value.ToList();
                    }
                }
            }catch { }
        }
    }
}
