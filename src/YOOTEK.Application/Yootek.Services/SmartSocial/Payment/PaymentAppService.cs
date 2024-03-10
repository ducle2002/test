using Abp.Application.Services;
using Abp.Web.Models;
using System;
using System.Linq;
using Yootek.Services;
using Yootek.Services.Dto;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Abp.Domain.Uow;
using Abp.RealTime;
using Yootek.MultiTenancy;
using Abp.Extensions;
using Abp.Configuration;
using Yootek.Configuration;
using Abp;

namespace Yootek.Yootek.Services.Yootek.DichVu.Payment
{
    public interface IPaymentAppService : IApplicationService
    {
        Task<object> MockPayUserBill(MockPayUserBillDto input);
        Task RemindUserBill();
    }

    public class PaymentAppService : YootekAppServiceBase, IPaymentAppService
    {
        private readonly UserBillAppService _userBillAppService;
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly ICloudMessagingManager _cloudMessagingManager;
        private readonly IRepository<Tenant, int> _tenantRepos;
        private readonly IRepository<BillDebt,  long> _billDebtRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<Citizen, long> _citizenRepos;

        public PaymentAppService(UserBillAppService userBillAppService,
            IRepository<UserBill, long> userBillRepository,
            ICloudMessagingManager cloudMessagingManager,
            IRepository<Tenant, int> tenantRepos,
            IRepository<BillDebt, long> billDebtRepos,
            IAppNotifier appNotifier,
            IRepository<Citizen, long> citizenRepos
            )
        {
            _userBillAppService = userBillAppService;
            _userBillRepository = userBillRepository;
            _cloudMessagingManager = cloudMessagingManager;
            _tenantRepos = tenantRepos;
            _billDebtRepos = billDebtRepos;
            _appNotifier = appNotifier;
            _citizenRepos = citizenRepos;
        }


        public async Task<object> MockPayUserBill(MockPayUserBillDto input)
        {
            try
            {
                // Fake payment change userBill Status
                var userBills = await _userBillRepository.GetAll().Where(x => input.BillIds.Contains(x.Id))
                    .ToListAsync();

                foreach (var userBill in userBills)
                {
                    userBill.Status = UserBillStatus.Paid;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                return userBills;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [AbpAllowAnonymous]
        [RemoteService(true)]
        public async Task RemindUserBill()
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

                            var notiTime1 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime1, tenantItem.Id);
                            var notiTime2 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime1, tenantItem.Id);
                            var notiTime3 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime1, tenantItem.Id);

                            var prepareToPayBills = _userBillRepository.GetAll()
                               .Where(x => x.Status == UserBillStatus.Pending)
                               .Where(x => (x.Period.Value.Month == today.Month && x.Period.Value.Year == today.Year)
                               || x.Period.Value.Month == preMonth.Month && x.Period.Value.Year == preMonth.Year)
                               .ToList();
                            var prepareToPayBillsNeedNotifies = prepareToPayBills.Select(x => new
                            {
                                x.TenantId,
                                x.ApartmentCode,
                                x.Period
                            }).GroupBy(x => new { x.TenantId, x.ApartmentCode, x.Period }).Select(x => x.First()).ToList();




                            if ((notiTime1 > 0 && notiTime1 == today.Day) || (notiTime2 > 0 && notiTime2 == today.Day) || (notiTime3 > 0 && notiTime3 == today.Day))
                            {

                                foreach (var x in prepareToPayBillsNeedNotifies)
                                {
 
                                    var users = await _citizenRepos.GetAllListAsync(c => c.ApartmentCode == x.ApartmentCode && c.AccountId.HasValue);
                                    if (users != null & users.Count > 0)
                                    {
                                        var userIds = users.Select(x => new UserIdentifier(x.TenantId, x.AccountId.Value)).ToList();
                                        await NotificationUserBill(x.ApartmentCode, x.Period.Value, userIds.ToArray());
                                    }
                                }
                            }

                            // user prepare to pay before 3 days

                            // user late to pay
                            var lateToPayBills = _userBillRepository.GetAll()
                                .Where(x => x.Status == UserBillStatus.Pending)
                                .Where(x => x.DueDate <= DateTime.Now.Date)
                                .ToList();

                            //await UpdateUserBillStatus(lateToPayBills);

                            var lateToPayBillsNeedNotifies = lateToPayBills.Select(x => new
                            {
                                x.TenantId,
                                x.ApartmentCode,
                            }).GroupBy(x => new { x.TenantId, x.ApartmentCode }).Select(x => x.First()).ToList();


                            var billDebts = lateToPayBills.Where(x => !x.ApartmentCode.IsNullOrWhiteSpace())
                                                            .GroupBy(x => new { x.Period.Value.Month, x.Period.Value.Year, x.ApartmentCode, x.TenantId })
                                                            .Select(x => new
                                                            {
                                                                Key = $"{x.Key.ApartmentCode}.{x.Key.Month}.{x.Key.Year}.{x.Key.TenantId}",
                                                                Bills = x.ToList(),
                                                            }).ToDictionary(x => x.Key, y => y.Bills);



                            foreach (var x in billDebts)
                            {
                                await HandleUserBillDebt(x.Value, tenantItem.Id);
                            }

                            try
                            {
                                foreach (var x in lateToPayBillsNeedNotifies)
                                {
                                    await _cloudMessagingManager.FcmSendToMultiDevice(new FcmMultiSendToDeviceInput()
                                    {
                                        Title = L("BillNotification"),
                                        Body = L("BillNotificationDueBody"),
                                        Data = JsonConvert.SerializeObject(new
                                        {
                                            action = "payment"
                                        }),
                                        GroupName = string.Format("apartment_{0}_{1}", x.TenantId, x.ApartmentCode)
                                    });
                                }
                            }
                            catch { }
                        }
                        catch (Exception e)
                        {
                            throw;
                        }

                    }

                }

            });

        }

        [DontWrapResult]
        public async Task<object> IPNPaymentMomo(IPNPaymentMomoInputDto input)
        {
            try
            {
                // decode base64
                var sExtraData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(input.ExtraData));
                Logger.Info("ExtraData : " + sExtraData);
                var extraData = JsonConvert.DeserializeObject<PaymentExtraDataDto>(sExtraData);

                // Handle with response from Momo here
                if (extraData != null)
                {
                    await _userBillAppService.HandlePaymentForMomo(new HandlePayUserBillInputDto
                    {
                        ResultCode = input.ResultCode,
                        UserBillIds = extraData.UserBillIds,
                        PaymentId = extraData.PaymentId,
                        TenantId = extraData.TenantId,
                        Amount = input.Amount,
                        PaymentMethod = UserBillPaymentMethod.Momo,
                        UserId = extraData.UserId,
                        TypePayment = extraData.TypePayment,
                        BillDebtIds = extraData.BillDebtIds
                    });
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }

        [ServiceFilter(typeof(IpBlockActionFilter))]
        [DontWrapResult]
        [HttpGet]
        public async Task<IPNPaymentVNPayResponseDto> IPNPaymentVNPay(IPNPaymentVNPayInputDto input)
        {
            try
            {
                var extraData = JsonConvert.DeserializeObject<PaymentExtraDataDto>(input.vnp_OrderInfo);

                if (extraData == null)
                {
                    throw new Exception("ExtraData is null");
                }

                await _userBillAppService.HandlePaymentForVNPay(new HandlePayUserBillInputDto
                {
                    UserBillIds = extraData.UserBillIds,
                    PaymentId = extraData.PaymentId,
                    TenantId = extraData.TenantId,
                    Amount = input.vnp_Amount / 100,
                    PaymentMethod = UserBillPaymentMethod.OnePay,
                    UserId = extraData.UserId,
                    Properties = JsonConvert.SerializeObject(input)
                });

                var result = new IPNPaymentVNPayResponseDto
                {
                    RspCode = VnPayLib.CONFIRM_SUCCESS_CODE,
                    Message = VnPayLib.CONFIRM_SUCCESS_MESSAGE,
                };
                return result;
            }
            catch (Exception e)
            {
                if (e.Message == VnPayLib.ORDER_NOT_FOUND_CODE)
                {
                    return new IPNPaymentVNPayResponseDto
                    {
                        RspCode = VnPayLib.ORDER_NOT_FOUND_CODE,
                        Message = VnPayLib.ORDER_NOT_FOUND_MESSAGE,
                    };
                }
                else if (e.Message == VnPayLib.ORDER_ALREADY_CONFIRMED_CODE)
                {
                    return new IPNPaymentVNPayResponseDto
                    {
                        RspCode = VnPayLib.ORDER_ALREADY_CONFIRMED_CODE,
                        Message = VnPayLib.ORDER_ALREADY_CONFIRMED_MESSAGE,
                    };
                }
                else if (e.Message == VnPayLib.INVALID_AMOUNT_CODE)
                {
                    return new IPNPaymentVNPayResponseDto
                    {
                        RspCode = VnPayLib.INVALID_AMOUNT_CODE,
                        Message = VnPayLib.INVALID_AMOUNT_MESSAGE,
                    };
                }
                else if (e.Message == VnPayLib.INVALID_SIGNATURE_CODE)
                {
                    return new IPNPaymentVNPayResponseDto
                    {
                        RspCode = VnPayLib.INVALID_SIGNATURE_CODE,
                        Message = VnPayLib.INVALID_SIGNATURE_MESSAGE,
                    };
                }
                else
                {
                    return new IPNPaymentVNPayResponseDto
                    {
                        RspCode = VnPayLib.UNKNOWN_ERROR_CODE,
                        Message = VnPayLib.UNKNOWN_ERROR_MESSAGE,
                    };
                }
            }
        }

        private async Task HandleUserBillDebt(List<UserBill> bills, int? tenantId)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var userBillIds = bills.Select(x => x.Id).ToList();
                    if (bills == null || bills.Count == 0) { return; }
                    var totalCost = bills.Sum(x => x.LastCost);
                    if (totalCost == 0) { return; }

                    var debt = new BillDebt()
                    {
                        ApartmentCode = bills[0].ApartmentCode,
                        CitizenTempId = bills[0].CitizenTempId,
                        DebtTotal = totalCost,
                        UserBillIds = string.Join(",", userBillIds),
                        PaidAmount = 0,
                        Period = DateTime.Now,
                        State = DebtState.DEBT,
                        TenantId = bills[0].TenantId,
                        Title = $"Công nợ hóa đơn tháng {bills[0].Period.Value.Month}/{bills[0].Period.Value.Year}"
                    };
                   // await _billDebtRepos.InsertAsync(debt);

                    foreach (var userBill in bills)
                    {
                        var bill = _userBillRepository.FirstOrDefault(userBill.Id);
                        if(bill != null)
                        {
                            bill.Status = UserBillStatus.Debt;
                            bill.DebtTotal = bill.LastCost.HasValue ? (decimal)bill.LastCost : 0;
                            await _userBillRepository.UpdateAsync(bill);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private async Task NotificationUserBill(string apartmentCode, DateTime period, UserIdentifier[] users)
        {
            var detailUrlApp = $"yoolife://app/receipt?apartmentCode={apartmentCode}&formId=1";
            var detailUrlWA = $"/monthly?apartmentCode={apartmentCode}&formId=1";

            var messageSuccess = new UserMessageNotificationDataBase(
                             AppNotificationAction.UserBill,
                             AppNotificationIcon.UserBill,
                             TypeAction.Detail,
                             $"Bạn có hóa đơn tháng {period.Month}/{period.Year} của căn hộ {apartmentCode}. Nhấn để xem chi tiết !",
                             detailUrlApp,
                             detailUrlWA
                             );
            await _appNotifier.SendMessageNotificationInternalAsync(
                $"Yoolife thông báo hóa đơn!",
                $"Bạn có hóa đơn tháng {period.Month}/{period.Year} của căn hộ {apartmentCode}. Nhấn để xem chi tiết !",
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