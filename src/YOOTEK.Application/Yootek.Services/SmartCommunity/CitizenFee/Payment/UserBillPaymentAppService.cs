using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.RealTime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications.UserBillNotification;
using Yootek.Notifications;
using Yootek.Services.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment;
using Microsoft.EntityFrameworkCore;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Common.DataResult;
using YOOTEK.EntityDb;
using Abp.Extensions;
using System.ComponentModel.DataAnnotations;
using Abp.Json;
using Abp.Domain.Uow;
using Yootek.Services.SmartSocial.Ecofarm;
using Abp.Runtime.Session;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Yootek.Services
{
    public interface IUserBillPaymentAppService : IApplicationService
    {
        Task<DataResult> HandlePaymentForThirdParty(HandPaymentForThirdPartyInput input);
        Task<UserBillPaymentValidation> RequestValidationUserBillPayment(CreatePaymentDto input);
    }

    public class UserBillPaymentAppService : YootekAppServiceBase, IUserBillPaymentAppService
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<UserBillPaymentValidation, long> _userBillPaymentValidationRepo;
        private readonly IRepository<ThirdPartyPayment, int> _thirdPartyPaymentRepo;
        private readonly IRepository<EPaymentBalanceTenant, long> _epaymentBanlanceRepository;
        private readonly HandlePaymentUtilAppService _handlePaymentUtilAppService;
        private readonly BaseHttpClient _httpClient;

        public UserBillPaymentAppService(
            IAbpSession abpSession,
            IConfiguration configuration,
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<UserBill, long> userBillRepository,
            IRepository<UserBillPaymentValidation, long> userBillPaymentValidationRepo,
            IRepository<ThirdPartyPayment, int> thirdPartyPaymentRepo,
            HandlePaymentUtilAppService handlePaymentUtilAppService,
            IRepository<EPaymentBalanceTenant, long> epaymentBanlanceRepository
        )
        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userBillRepository = userBillRepository;
            _handlePaymentUtilAppService = handlePaymentUtilAppService;
            _userBillPaymentValidationRepo = userBillPaymentValidationRepo;
            _thirdPartyPaymentRepo = thirdPartyPaymentRepo;
            _epaymentBanlanceRepository = epaymentBanlanceRepository;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Payments"]);
        }

        [RemoteService(false)]
        public async Task<UserBillPaymentValidation> RequestValidationUserBillPayment(CreatePaymentDto request)
        {
           
            var payment = await _handlePaymentUtilAppService.RequestValidationPaymentByApartment(request.TransactionProperties);
            return payment;
        }

        public async Task RequestValidationUserBillPaymentOnSuccess(RequestValidationInput input)
        {
            var tenantId = AbpSession.TenantId;
            var validate = await _userBillPaymentValidationRepo.FirstOrDefaultAsync(input.TransactionId);
            if (validate != null && !validate.UserBillIds.IsNullOrEmpty())
            {
                await UpdatePaymentPendingUserBills(validate.UserBillIds, true, UserBillStatus.Pending, tenantId);
            }

            if (validate != null && !validate.UserBillDebtIds.IsNullOrEmpty())
            {
                await UpdatePaymentPendingUserBills(validate.UserBillDebtIds, true, UserBillStatus.Debt, tenantId);
            }

        }

        public async Task<DataResult> HandlePaymentForThirdParty(HandPaymentForThirdPartyInput input)
        {
            try
            {
                var paymentTransaction = _thirdPartyPaymentRepo.FirstOrDefault(x => x.Id == input.Id);
                if (paymentTransaction == null) throw new Exception();
                var transaction = JsonConvert.DeserializeObject<PayMonthlyUserBillsInput>(JsonConvert.DeserializeObject<string>(paymentTransaction.TransactionProperties));
                switch (input.Status)
                {
                    case EPrepaymentStatus.SUCCESS:
                        transaction.Status = UserBillPaymentStatus.Success;
                        transaction.Method = (UserBillPaymentMethod)paymentTransaction.Method;
                        var pm = await _handlePaymentUtilAppService.PayMonthlyUserBillByApartment(transaction);

                        var requestPayment = new
                        {
                            id = input.Id,
                            internalState = 2,
                            isManuallyVerified = true
                        };
                        var res =  await _httpClient.SendSync<PaymentDto>("/api/payments/change-bill-payment-status", HttpMethod.Post, requestPayment);
                        await CreateEPaymentBalance(pm.Id, input.Id, paymentTransaction.Amount, pm.Title, pm.TenantId, pm.Method);
                        return DataResult.ResultSuccess(pm.Id, "");
                    case EPrepaymentStatus.FAILED:
                        if (transaction.UserBills != null)
                        {
                            var ids = string.Join(",", transaction.UserBills.Select(x => x.Id).OrderBy(x => x));
                            await UpdatePaymentPendingUserBills(ids, false, UserBillStatus.Pending, input.TenantId);
                        }

                        if (transaction.UserBillDebts != null)
                        {
                            var ids = string.Join(",", transaction.UserBillDebts.Select(x => x.Id).OrderBy(x => x));
                            await UpdatePaymentPendingUserBills(ids, false, UserBillStatus.Debt, input.TenantId);
                        }
                        paymentTransaction.IsManuallyVerified = true;
                        paymentTransaction.InternalState = EInternalStateChangeStatus.Success;
                        await _thirdPartyPaymentRepo.UpdateAsync(paymentTransaction);
                        break;
                    default:
                        throw new Exception();
                }
              
                return DataResult.ResultSuccess("");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToJsonString());
                throw;
            }
        }

        protected async Task CreateEPaymentBalance(long billPaymentId, int epaymentId, double amount, string title, int? tenantId, UserBillPaymentMethod method)
        {
            var payment = new EPaymentBalanceTenant()
            {
                BalanceRemaining = amount,
                BillPaymentId = billPaymentId,
                EBalanceAction = EBalanceAction.Add,
                EPaymentId = epaymentId,
                Method = method,
                Title = title,  
                TenantId = tenantId 
            };

            await _epaymentBanlanceRepository.InsertAsync(payment);
        }
        private async Task UpdatePaymentPendingUserBills(string billIds, bool isPaymentPending, UserBillStatus status, int? tenantId)
        {
            using(CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var ids = billIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                var bills = await _userBillRepository.GetAll().Where(x => ids.Contains(x.Id) && x.Status == status).ToListAsync();
                foreach (var bill in bills)
                {
                    bill.IsPaymentPending = isPaymentPending;
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

        }

    }
}
