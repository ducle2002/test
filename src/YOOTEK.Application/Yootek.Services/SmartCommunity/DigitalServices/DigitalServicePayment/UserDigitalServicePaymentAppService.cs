using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using AutoMapper.Internal.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek;
using YOOTEK.EntityDb.IMAX.DichVu.DigitalServices;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.Enum;
using Yootek.Services;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment;
using YOOTEK.EntityDb;
using System.Net.Http;
using Yootek.Lib.CrudBase;
using Yootek.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace YOOTEK.Yootek.Services
{
    public interface IUserDigitalServicePaymentAppService : IApplicationService
    {

    }

    public class UserDigitalServicePaymentAppService : YootekAppServiceBase, IUserDigitalServicePaymentAppService
    {
        private readonly IRepository<DigitalServicePayment, long> _digitalServicePaymentRepository;
        private readonly IRepository<DigitalServiceOrder, long> _digitalServiceOrderRepository;
        private readonly IRepository<ThirdPartyPayment, int> _thirdPartyPaymentRepo;
        private readonly DigitalServicePaymentUtil _digitalServicePaymentUtil;
        private readonly HttpClient _httpClient;
        private readonly IRepository<EPaymentBalanceTenant, long> _epaymentBanlanceRepository;

        public UserDigitalServicePaymentAppService(

             IRepository<DigitalServicePayment, long> digitalServicePaymentRepository,
             IRepository<DigitalServiceOrder, long> digitalServiceOrderRepository,
             IRepository<ThirdPartyPayment, int> thirdPartyPaymentRepo,
             DigitalServicePaymentUtil digitalServicePaymentUtil,
             YootekHttpClient yootekHttpClient,
             IConfiguration configuration
            )
        {
            _digitalServicePaymentRepository = digitalServicePaymentRepository;
            _digitalServiceOrderRepository = digitalServiceOrderRepository;
            _thirdPartyPaymentRepo = thirdPartyPaymentRepo;
            _digitalServicePaymentUtil = digitalServicePaymentUtil;
            _httpClient = yootekHttpClient.GetHttpClient(configuration["ApiSettings:Payments"]);
        }

        public async Task<DataResult> GetAllAsync(UserGetAllDigitalServicePaymentInput input)
        {
            try
            {
                IQueryable<DigitalServicePaymentDto> query = (from pm in _digitalServicePaymentRepository.GetAll()
                                                              select new DigitalServicePaymentDto
                                                              {
                                                                  Id = pm.Id,
                                                                  Amount = pm.Amount,
                                                                  Code = pm.Code,
                                                                  UrbanId = pm.UrbanId,
                                                                  BuildingId = pm.BuildingId,
                                                                  Method = pm.Method,
                                                                  Note = pm.Note,
                                                                  OrderId = pm.OrderId,
                                                                  Properties = pm.Properties,
                                                                  ServiceId = pm.ServiceId,
                                                                  Status = pm.Status,
                                                                  CreationTime = pm.CreationTime,
                                                                  TenantId = pm.TenantId,
                                                                  ApartmentCode = pm.ApartmentCode

                                                              })
                                                        .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                                                        x => x.ApartmentCode.ToLower().Contains(input.Keyword.ToLower())
                                                        || x.Code.ToLower().Contains(input.Keyword.ToLower())
                                                        || x.Note.ToLower().Contains(input.Keyword.ToLower()))
                                                        .AsQueryable();

                var data = await query.PageBy(input).ToListAsync();
                return DataResult.ResultSuccess(data, "", query.Count());
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> CreateRequestPaymentAsync(CreateDigitalServicePaymentDto dto)
        {
            try
            {
                var insertData = ObjectMapper.Map<DigitalServicePayment>(dto);
                await _digitalServicePaymentRepository.InsertAndGetIdAsync(insertData);

                return DataResult.ResultSuccess(insertData, "Insert success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<DataResult> RequestValidationDigitalServicePayment(RequestPaymentDigitalServiceInput request)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(request.TenantId))
                {
                    var all = await _digitalServiceOrderRepository.GetAllListAsync();
                    var session = AbpSession;
                    if (request.Amount == 0) throw new UserFriendlyException("Amount is invalid");
                    var order = await _digitalServiceOrderRepository.FirstOrDefaultAsync(x => x.Id == request.TransactionId 
                    && (x.PaymentState == DigitalServicePaymentState.Pending || x.PaymentState == DigitalServicePaymentState.Debt));
                    if (order == null) throw new UserFriendlyException("Data not found");

                    return DataResult.ResultSuccess("");
                }
            }catch(Exception e)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<DataResult> HandlePaymentForThirdParty(HandPaymentDigitalServiceForThirdPartyInput input)
        {
            try
            {
                var paymentTransaction = _thirdPartyPaymentRepo.FirstOrDefault(x => x.Id == input.Id);
                if (paymentTransaction == null) throw new UserFriendlyException("Data not found");

                var requestPayment = new
                {
                    id = input.Id,
                    internalState = 2,
                    isManuallyVerified = true
                };

                switch (input.Status)
                {
                    case EPrepaymentStatus.SUCCESS:
                        var pm = await _digitalServicePaymentUtil.HandlePaymentSuccess(
                            Int64.Parse(paymentTransaction.TransactionId),
                            DecimalRoudingUp(paymentTransaction.Amount),
                            (DigitalServicePaymentMethod)paymentTransaction.Method,
                            "",
                            "",
                            DigitalServicePaymentStatus.SUCCESS
                            );

                        var res = await _httpClient.SendAsync<PaymentDto>("/api/payments/change-bill-payment-status", HttpMethod.Post, requestPayment);
                        await CreateEPaymentBalance(pm.Id, input.Id, paymentTransaction.Amount, pm.TenantId, (UserBillPaymentMethod)pm.Method);
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
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private async Task CreateEPaymentBalance(long billPaymentId, int epaymentId, double amount, int? tenantId, UserBillPaymentMethod method)
        {
            var payment = new EPaymentBalanceTenant()
            {
                BalanceRemaining = amount,
                BillPaymentId = billPaymentId,
                EBalanceAction = EBalanceAction.Add,
                EPaymentId = epaymentId,
                Method = method,
                TenantId = tenantId,
                EbalancePaymentType = EbalancePaymentType.DigitalService
            };

            await _epaymentBanlanceRepository.InsertAsync(payment);
        }

    }
}
