
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
using Newtonsoft.Json;
using System.Net.Mail;
using Yootek.Services.SmartCommunity.BillingInvoice;
using Abp.Net.Mail;
using Yootek.Application.Configuration.Tenant;

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
        private readonly IBillInvoiceAppService _billInvoiceAppService;
        private readonly IRepository<BillEmailHistory, long> _billEmailHistoryRepos;
        private readonly IEmailSender _emailSender;
        private readonly ITenantSettingsAppService _tenantSetting;


        public UserBillEmailer(
             IBillEmailUtil billEmailUtil,
             IBackgroundJobManager backgroundJobManager,
             IRepository<CitizenTemp, long> citizenTempRepository,
             IRepository<Citizen, long> citizenRepository,
             IRepository<Apartment, long> apartmentRepository,
             IAppNotifier appNotifier,
             IAppConfigurationAccessor configurationAccessor,
             IBillInvoiceAppService billInvoiceAppService,
             IRepository<BillEmailHistory, long> billEmailHistoryRepos,
             IEmailSender emailSender,
             ITenantSettingsAppService tenantSetting


            )
        {

            _backgroundJobManager = backgroundJobManager;
            _billEmailUtil = billEmailUtil;
            _apartmentRepository = apartmentRepository;
            _citizenTempRepository = citizenTempRepository;
            _appNotifier = appNotifier;
            _citizenRepository = citizenRepository;
            _appConfiguration = configurationAccessor.Configuration;
            _billInvoiceAppService = billInvoiceAppService;
            _billEmailHistoryRepos = billEmailHistoryRepos;
            _emailSender = emailSender;
            _tenantSetting = tenantSetting;

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
                        await NotificationUserBill(bill.ApartmentCode, bill.Period, citizens, AbpSession.TenantId);
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
        public async Task SendEmailToEmailAdminSenderAsync(long id, DateTime? tim)
        {
            try
            {
                if (tim == null) return;
                var time = tim.Value;
                var emailTemplate = _billInvoiceAppService.GetPaymentVoucher(id); //phiếu thu

                //var citizenTemp = _citizenTempRepository.FirstOrDefault(x => x.AccountId == AbpSession.UserId && x.RelationShip == RELATIONSHIP.Contractor && x.IsStayed == true);
                //if (citizenTemp == null)
                //{
                //    var citizenTemp0 = _citizenTempRepository.GetAll().Where(x => x.AccountId == AbpSession.UserId && x.RelationShip == RELATIONSHIP.Contractor).OrderByDescending(x => x.OwnerGeneration).FirstOrDefault();
                //    if (citizenTemp0 != null)
                //    {
                //        var apartmentCode = citizenTemp0.ApartmentCode;

                //        var history = new BillEmailHistory()
                //        {
                //            ApartmentCode = apartmentCode,
                //            CitizenTempId = citizenTemp0 != null ? citizenTemp0.Id : null,
                //            Period = time,
                //            EmailTemplate = emailTemplate.ToString(),
                //            TenantId = AbpSession.TenantId

                //        };

                //        await _billEmailHistoryRepos.InsertAsync(history);
                //    }
                //}

                var currentPeriod = string.Format("{0:MM/yyyy}", time);
                var emailSender = await _tenantSetting.GetEmailSettingsAsync();
                if (emailSender.DefaultFromAddress != null && emailSender.DefaultFromAddress != "") {
                    await _emailSender.SendAsync(new MailMessage
                    {
                        To = { emailSender.DefaultFromAddress },
                        Subject = $"Thông báo hóa đơn dịch vụ tháng {currentPeriod}",
                        Body = emailTemplate.ToString().Replace("OCTYPE html>", ""),
                        IsBodyHtml = true
                    });
                }

            }
            catch (Exception e)
            {
                Logger.Fatal("Send email to email sender: " + e.Message);
                Logger.Fatal(JsonConvert.SerializeObject(e));
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

        private async Task NotificationUserBill(string apartmentCode, DateTime period, List<long> userIds, int? tenantId)
        {
            var users = userIds.Select(x => new UserIdentifier(tenantId, x)).ToArray();
            var detailUrlApp = $"yoolife://app/receipt?apartmentCode={apartmentCode}&formId=1";
            var detailUrlWA = $"/monthly?apartmentCode={apartmentCode}&formId=1";
            var messageSuccess = new UserMessageNotificationDataBase(
                               AppNotificationAction.UserBill,
                               AppNotificationIcon.UserBill,
                               TypeAction.Detail,
                               $"Bạn có hóa đơn tháng {period.Month}/{period.Year} của căn hộ {apartmentCode} !",
                               detailUrlApp,
                               detailUrlWA
                               );
            await _appNotifier.SendMessageNotificationInternalAsync(
                $"Thông báo hóa đơn mới!",
                $"Bạn có hóa đơn tháng {period.Month}/{period.Year} của căn hộ {apartmentCode} !",
                detailUrlApp,
                detailUrlWA,
                users,
                messageSuccess,
                AppType.USER
                );
            // await _appNotifier.SendUserMessageNotifyFireBaseAsync(
            //      $"Thông báo hóa đơn mới!",
            //      $"Bạn có hóa đơn tháng {period.Month}/{period.Year} của căn hộ {apartmentCode} !",
            //      detailUrlApp,
            //      detailUrlWA,
            //      users,
            //      messageSuccess);
            return;
        }

    }
}
