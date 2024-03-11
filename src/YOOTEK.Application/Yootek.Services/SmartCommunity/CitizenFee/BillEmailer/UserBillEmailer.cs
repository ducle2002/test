
using Abp.UI;
using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Yootek.EntityDb;
using System.Linq;
using System.Collections.Generic;
using Yootek.Notifications;
using Abp;
using NPOI.SS.Formula.Functions;
using Microsoft.EntityFrameworkCore;
using Yootek.Configuration;
using Microsoft.Extensions.Configuration;
using Abp.Notifications;

namespace Yootek.Services.BillEmailer
{
    public interface IUserBillEmailer : IApplicationService
    {
        Task SendEmailUserBillMonthlyAsync(string apartmentCode, DateTime? tim, int? tenantId);
        Task SendListEmailUserBillMonthlyAsync(SendEmailUserBillJobArgs input);
    }

    public class UserBillEmailer : YootekAppServiceBase, IUserBillEmailer
    {

        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IBillEmailUtil _billEmailUtil;
        private readonly IAppNotifier _appNotifier;

        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;
        private readonly IRepository<Citizen, long> _citizenRepository;

        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IConfigurationRoot _appConfiguration;

        public UserBillEmailer(
             IBillEmailUtil billEmailUtil,
             IBackgroundJobManager backgroundJobManager,
             IRepository<CitizenTemp, long> citizenTempRepository,
             IRepository<Citizen, long> citizenRepository,
             IRepository<Apartment, long> apartmentRepository,
             IAppNotifier appNotifier,
             IAppConfigurationAccessor configurationAccessor
            )
        {

            _backgroundJobManager = backgroundJobManager;
            _billEmailUtil = billEmailUtil;
            _apartmentRepository = apartmentRepository;
            _citizenTempRepository = citizenTempRepository;
            _appNotifier = appNotifier;
            _citizenRepository = citizenRepository;
            _appConfiguration = configurationAccessor.Configuration;
        }

        public async Task SendEmailAndBNotificationAllApartment(List<SendUserBillNotificationInput> input)
        {
            try
            {
                if (input != null && input.Count <= 1)
                {
                    foreach (var bill in input)
                    {
                        try
                        {
                            await _billEmailUtil.SendEmailToApartmentAsync(bill.ApartmentCode, bill.Period, AbpSession.TenantId);
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    //We enqueue a background job since distributing may get a long time
                    await _backgroundJobManager
                        .EnqueueAsync<SendEmailUserBillJobs, SendEmailUserBillJobArgs>(
                            new SendEmailUserBillJobArgs(
                                input.Select(x => x.ApartmentCode).ToList(),
                                input[0].Period,
                                AbpSession.TenantId
                            ),
                            BackgroundJobPriority.High
                        );
                }

                foreach (var bill in input)
                {
                    try
                    {
                        var citizens = await _citizenRepository.GetAll().Where(x => x.ApartmentCode == bill.ApartmentCode && x.State == STATE_CITIZEN.ACCEPTED && x.AccountId.HasValue).Select(x => x.AccountId.Value).ToListAsync();
                        await NotificationUserBill(bill, citizens, AbpSession.TenantId);
                    }
                    catch { }
                }


            }
            catch (Exception e)
            {
                Logger.Fatal("send mail bills :" + e.Message);
                throw;
            }
        }

        public async Task SendListEmailUserBillMonthlyAsync(SendEmailUserBillJobArgs input)
        {
            if (input.ApartmentCodes == null || input.ApartmentCodes.Count == 0) return;
            var period = input.Period != null ? input.Period.Value : DateTime.Now;
            foreach (string code in input.ApartmentCodes)
            {
                try
                {
                    await _billEmailUtil.SendEmailToApartmentAsync(code, period, AbpSession.TenantId);
                }
                catch
                {

                }

            }
        }

        public async Task SendEmailUserBillMonthlyAsync(string apartmentCode, DateTime? tim, int? tenantId)
        {
            try
            {
                await _billEmailUtil.SendEmailToApartmentAsync(apartmentCode, tim, tenantId);
            }
            catch (Exception exception)
            {
                throw new UserFriendlyException(exception.Message);
            }
        }

        private async Task NotificationUserBill(SendUserBillNotificationInput bill, List<long> userIds, int? tenantId)
        {
            var users = userIds.Select(x => new UserIdentifier(tenantId, x)).ToArray();
            var formId = 1;
            switch (bill.Status)
            {
                case UserBillStatus.Pending: formId = 1; break;
                case UserBillStatus.Paid: formId = 2; break;
                case UserBillStatus.Debt: formId = 3; break;
                default: break;
            }
            var detailUrlApp = $"yoolife://app/receipt?apartmentCode={bill.ApartmentCode}&formId={formId}";
            var detailUrlWA = $"/monthly?apartmentCode={bill.ApartmentCode}&formId={formId}";
            var messageSuccess = new UserMessageNotificationDataBase(
                               AppNotificationAction.UserBill,
                               AppNotificationIcon.UserBill,
                               TypeAction.Detail,
                               $"Bạn có hóa đơn tháng {bill.Period.Month}/{bill.Period.Year} của căn hộ {bill.ApartmentCode}. Nhấn để xem chi tiết !",
                               detailUrlApp,
                               detailUrlWA
                               );
            await _appNotifier.SendMessageNotificationInternalAsync(
                $"Yoolife thông báo hóa đơn!",
                $"Bạn có hóa đơn tháng {bill.Period.Month}/{bill.Period.Year} của căn hộ {bill.ApartmentCode}. Nhấn để xem chi tiết !",
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
