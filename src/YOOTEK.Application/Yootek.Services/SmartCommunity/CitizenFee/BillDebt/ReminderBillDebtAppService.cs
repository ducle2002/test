using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Yootek.Configuration;
using Yootek.EntityDb;
using Yootek.MultiTenancy;
using Yootek.Notifications;
using Yootek.Services.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IReminderBillDebtAppService : IApplicationService
    {
        Task ReminderUserBillDebtAsync();
    }
    public class ReminderBillDebtAppService : YootekAppServiceBase, IReminderBillDebtAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly ICloudMessagingManager _cloudMessagingManager;
        private readonly IAppNotifyBusiness _appNotifyBusiness;
        public ReminderBillDebtAppService(
            IRepository<UserBill, long> userBillRepo,
            ICloudMessagingManager cloudMessagingManager,
            IRepository<Tenant, int> tenantRepository,
            IAppNotifier appNotifier,
            IAppNotifyBusiness appNotifyBusiness,
            IRepository<Citizen, long> citizenRepos)
        {
            _userBillRepo = userBillRepo;
            _tenantRepository = tenantRepository;
            _citizenRepos = citizenRepos;
            _cloudMessagingManager = cloudMessagingManager;
            _appNotifyBusiness = appNotifyBusiness;
            _appNotifier = appNotifier;
        }
        [AbpAllowAnonymous]
        [RemoteService(true)]
        public async Task ReminderUserBillDebtAsync()
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                List<Tenant> listTenants = _tenantRepository.GetAllList();
                foreach (Tenant tenant in listTenants)
                {
                    using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                    {
                        try
                        {
                            DateTime dateTimeNow = DateTime.Now;
                            DateTime dateTimePreMonth = DateTime.Now.AddMonths(-1);
                            int notiTime1 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime1, tenant.Id);
                            int notiTime2 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime2, tenant.Id);
                            int notiTime3 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime3, tenant.Id);
                            List<UserBill> billDebtPreMonths = _userBillRepo.GetAll()
                                .Where(x => x.Status == UserBillStatus.Debt)
                                .Where(x => x.Period.Value.Year == dateTimePreMonth.Year && x.Period.Value.Month == dateTimePreMonth.Month)
                                .ToList();
                            var listBillDebtApartment = billDebtPreMonths.Select(x => new
                            {
                                x.TenantId,
                                x.ApartmentCode,
                            }).Distinct().ToList();
                            if (listBillDebtApartment == null || listBillDebtApartment.Count == 0) continue;
                            if ((notiTime1 > 0 && notiTime1 == dateTimeNow.Day)
                                || (notiTime2 > 0 && notiTime2 == dateTimeNow.Day)
                                || (notiTime3 > 0 && notiTime3 == dateTimeNow.Day))
                            {
                                foreach (var billDebtApartment in listBillDebtApartment)
                                {
                                    try
                                    {
                                      
                                        List<Citizen> users = await _citizenRepos.GetAllListAsync(c => c.ApartmentCode == billDebtApartment.ApartmentCode && c.AccountId.HasValue);
                                        if (users != null & users.Count > 0)
                                        {
                                            List<long> userIds = users.Select(x => x.AccountId.Value).ToList();
                                            await NotificationUserBill(billDebtApartment.ApartmentCode, dateTimePreMonth, userIds, billDebtApartment.TenantId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        continue;
                                    }
                                }
                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal(ex.Message, ex);
                            throw;
                        }
                    }
                }
            });
        }
        private async Task NotificationUserBill(string apartmentCode, DateTime period, List<long> userIds, int? tenantId)
        {
            var users = userIds.Select(x => new UserIdentifier(tenantId, x)).ToArray();
            var detailUrlApp = $"yoolife://app/receipt?apartmentCode={apartmentCode}&formId=3";
            var detailUrlWA = $"/debts?apartmentCode={apartmentCode}&formId=3";
            var messageSuccess = new UserMessageNotificationDataBase(
                               AppNotificationAction.UserBill,
                               AppNotificationIcon.UserBill,
                               TypeAction.Detail,
                               $"Bạn có hóa đơn công nợ tháng {period.Month}/{period.Year} của căn hộ {apartmentCode} !",
                               detailUrlApp,
                               detailUrlWA
                               );
            await _appNotifier.SendMessageNotificationInternalAsync(
                $"Thông báo hóa đơn công nợ!",
                $"Bạn có hóa đơn công nợ tháng {period.Month}/{period.Year} của căn hộ {apartmentCode} !",
                detailUrlApp,
                detailUrlWA,
                users,
                messageSuccess,
                AppType.USER
                );
            return;
        }
    }
}
