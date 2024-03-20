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
using Yootek.Lib.CrudBase;
using Yootek.Extensions;
using Abp.Collections.Extensions;

namespace Yootek.Services
{
    public interface IUserBillPaymentAppService : IApplicationService
    {
        Task<DataResult> HandlePaymentForThirdParty(HandPaymentForThirdPartyInput input);
        Task<UserBillPaymentValidation> RequestValidationUserBillPayment(RequestValidationPaymentDto input);
    }

    public class UserBillPaymentAppService : YootekAppServiceBase, IUserBillPaymentAppService
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<UserBillPaymentValidation, long> _userBillPaymentValidationRepo;
        private readonly IRepository<ThirdPartyPayment, int> _thirdPartyPaymentRepo;
        private readonly IRepository<EPaymentBalanceTenant, long> _epaymentBanlanceRepository;
        private readonly HandlePaymentUtilAppService _handlePaymentUtilAppService;
        private readonly HttpClient _httpClient;

        public UserBillPaymentAppService(
            IConfiguration configuration,
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<UserBill, long> userBillRepository,
            IRepository<UserBillPaymentValidation, long> userBillPaymentValidationRepo,
            IRepository<ThirdPartyPayment, int> thirdPartyPaymentRepo,
            HandlePaymentUtilAppService handlePaymentUtilAppService,
            IRepository<EPaymentBalanceTenant, long> epaymentBanlanceRepository,
            YootekHttpClient yootekHttpClient
        )
        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userBillRepository = userBillRepository;
            _handlePaymentUtilAppService = handlePaymentUtilAppService;
            _userBillPaymentValidationRepo = userBillPaymentValidationRepo;
            _thirdPartyPaymentRepo = thirdPartyPaymentRepo;
            _epaymentBanlanceRepository = epaymentBanlanceRepository;
            _httpClient = yootekHttpClient.GetHttpClient(configuration["ApiSettings:Payments"]);
        }

        public async Task<UserBillPaymentValidation> RequestValidationUserBillPayment(RequestValidationPaymentDto request)
        {
            var payment = await _handlePaymentUtilAppService.RequestValidationPaymentByApartment(request.TransactionProperties, request.TenantId);
            return payment;
        }

        public async Task RequestValidationUserBillPaymentOnSuccess(RequestValidationInput input)
        {
            try
            {
                var tenantId = AbpSession.TenantId;
                //var validate = await _userBillPaymentValidationRepo.FirstOrDefaultAsync(input.TransactionId);

                //if (!input.ReturnUrl.IsNullOrEmpty())
                //{
                //    if (input.ReturnUrl.Contains("Approved"))
                //    {
                //        try
                //        {
                //            var url = input.ReturnUrl.Split("yoolife://onepay")[1];
                //            var res = await _httpClient.SendAsync<PaymentDto>("api/payments/onepay-ipn" + url, HttpMethod.Get);
                //        }
                //        catch
                //        {

                //        }
                //    }
                   
                   
                //}

               // await _userBillPaymentValidationRepo.UpdateAsync(validate);
                return;
            }
            catch
            {
                return;
            }

        }

        public async Task<DataResult> HandlePaymentForThirdParty(HandPaymentForThirdPartyInput input)
        {
            try
            {
                var paymentTransaction = _thirdPartyPaymentRepo.FirstOrDefault(x => x.Id == input.Id);
                if (paymentTransaction == null) throw new Exception();
                var transaction = JsonConvert.DeserializeObject<PayMonthlyUserBillsInput>(paymentTransaction.TransactionProperties);
                var requestPayment = new
                {
                    id = input.Id,
                    internalState = 2,
                    isManuallyVerified = true
                };

                switch (input.Status)
                {
                    case EPrepaymentStatus.SUCCESS:
                        transaction.Status = UserBillPaymentStatus.Success;
                        transaction.Method = (UserBillPaymentMethod)paymentTransaction.Method;
                        var pm = await _handlePaymentUtilAppService.PayMonthlyUserBillByApartment(transaction, input.Id);  

                        var res =  await _httpClient.SendAsync<PaymentDto>("/api/payments/change-bill-payment-status", HttpMethod.Post, requestPayment);
                        await CreateEPaymentBalance(pm.Id, input.Id, paymentTransaction.Amount, pm.Title, pm.TenantId, pm.Method);
                        return DataResult.ResultSuccess(pm.Id, "");
                    case EPrepaymentStatus.FAILED:
                        await _httpClient.SendAsync<PaymentDto>("/api/payments/change-bill-payment-status", HttpMethod.Post, requestPayment);
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
