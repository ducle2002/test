using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.MultiTenancy;
using YOOTEK.EntityDb;
using YOOTEK.EntityDb.IMAX.DichVu.DigitalServices;
using YOOTEK.Yootek.Services;
using YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.Dto;

namespace Yootek.Services
{
    public interface IAdminSocialDigitalServicePaymentAppService : IApplicationService
    {
    }

    public class AdminSocialDigitalServicePaymentAppService : YootekAppServiceBase, IAdminSocialDigitalServicePaymentAppService
    {
        private readonly IRepository<DigitalServicePayment, long> _digitalServicePaymentRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<ThirdPartyPayment, int> _thirdPartyPaymentRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<EPaymentBalanceTenant, long> _epaymentBanlanceRepository;
        private readonly IRepository<OnepayMerchant, int> _onepayMerchantRepository;
        private readonly IRepository<DigitalServiceOrder, long> _digitalServiceOrderRepository;
        private readonly IRepository<DigitalServices, long> _digitalServiceRepository;
        private readonly IRepository<Citizen, long> _citizenRepository;

        public AdminSocialDigitalServicePaymentAppService(
            IRepository<DigitalServicePayment, long> digitalServicePaymentRepo,
            IRepository<User, long> userRepos,
            IRepository<ThirdPartyPayment, int> thirdPartyPaymentRepository,
            IRepository<Tenant, int> tenantRepository,
            IRepository<EPaymentBalanceTenant, long> epaymentBanlanceRepository,
            IRepository<OnepayMerchant, int> onepayMerchantRepository,
            IRepository<DigitalServiceOrder, long> digitalServiceOrderRepository,
            IRepository<DigitalServices, long> digitalServiceRepository,
            IRepository<Citizen, long> citizenRepository
            )

        {
            _digitalServicePaymentRepo = digitalServicePaymentRepo;
            _userRepos = userRepos;
            _thirdPartyPaymentRepository = thirdPartyPaymentRepository;
            _tenantRepository = tenantRepository;
            _epaymentBanlanceRepository = epaymentBanlanceRepository;
            _onepayMerchantRepository = onepayMerchantRepository;
            _digitalServiceOrderRepository = digitalServiceOrderRepository;
            _digitalServiceRepository = digitalServiceRepository;
            _citizenRepository = citizenRepository;
        }

        public async Task<object> GetTenantPayment(GetAlltenantPaymentInput input)
        {
            try
            {
                var query = _tenantRepository.GetAll().Select(x => new TenantPaymentDto
                {
                    Id = x.Id,
                    TenantName = x.Name,
                    TenantType = x.TenantType
                }).Where(x => x.TenantType == TenantType.IOC).AsQueryable();

                var result = await query.PageBy(input).ToListAsync();

                var queryE = (from pm in _thirdPartyPaymentRepository.GetAll()
                              select new ThirdPartyPaymentDto()
                              {
                                  Id = pm.Id,
                                  Amount = pm.Amount,
                                  Method = pm.Method,
                                  Status = pm.Status,
                                  TenantId = pm.TenantId,
                                  Type = pm.Type
                              })
                             .Where(x => x.Type == EPaymentType.DigitalService);

                var queryR = from payment in _digitalServicePaymentRepo.GetAll()
                             select new AdminDigitalServicePaymentDto()
                             {
                                 Id = payment.Id,
                                 Method = payment.Method,
                                 TenantId = payment.TenantId,
                                 Status = payment.Status,
                                 Amount = payment.Amount,
                                 ApartmentCode = payment.ApartmentCode,
                                 OrderId = payment.OrderId,
                             };

                foreach (var item in result)
                {
                    using (CurrentUnitOfWork.SetTenantId(item.Id))
                    {
                        item.NumberEPayment = queryR
                       .Where(x => x.Method == DigitalServicePaymentMethod.ONEPAY || x.Method == DigitalServicePaymentMethod.MOMO)
                       .Where(x => x.TenantId == item.Id).Count();

                        item.NumberRPayment = queryR
                            .Where(x => x.TenantId == item.Id).Count();

                        item.TotalAmountEpay = (double)queryR
                       .Where(x => x.Method == DigitalServicePaymentMethod.ONEPAY || x.Method == DigitalServicePaymentMethod.MOMO)
                       .Where(x => x.TenantId == item.Id).Sum(x => x.Amount);

                        item.TotalAmountRpay = (double)queryR
                            .Where(x => x.TenantId == item.Id).Sum(x => x.Amount);
                    }

                    var balance = _epaymentBanlanceRepository.GetAll()
                        .Where(x => x.EbalancePaymentType == EbalancePaymentType.DigitalService)
                        .Where(x => x.TenantId == item.Id && x.EBalanceAction == EBalanceAction.Add)
                        .Sum(x => x.BalanceRemaining);
                    var subBalance = _epaymentBanlanceRepository.GetAll()
                        .Where(x => x.EbalancePaymentType == EbalancePaymentType.DigitalService)
                        .Where(x => x.TenantId == item.Id && x.EBalanceAction == EBalanceAction.Sub)
                        .Sum(x => x.BalanceRemaining);

                    item.TotalBalance = balance - subBalance;

                    item.TotalPaymentForTenant = subBalance;
                }

                return DataResult.ResultSuccess(result, "Get  success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        protected IQueryable<ThirdPartyDigitalServicePaymentDto> QueryThirdPartyPayment(GetAllPaymentInput input)
        {
            DateTime fromDay = new DateTime(), toDay = new DateTime();
            if (input.FromDay.HasValue)
            {
                fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

            }

            if (input.ToDay.HasValue)
            {
                toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

            }

            var query = (from pm in _thirdPartyPaymentRepository.GetAll()
                         join mc in _onepayMerchantRepository.GetAll() on pm.MerchantId equals mc.Id into tb_mc
                         from mc in tb_mc.DefaultIfEmpty()
                         select new ThirdPartyDigitalServicePaymentDto()
                         {
                             Id = pm.Id,
                             Amount = pm.Amount,
                             CreatedAt = pm.CreatedAt,
                             CreatedById = pm.CreatedById,
                             Currency = pm.Currency,
                             Description = pm.Description,
                             Method = pm.Method,
                             Properties = pm.Properties,
                             Status = pm.Status,
                             TenantId = pm.TenantId,
                             TransactionId = pm.TransactionId,
                             TransactionProperties = pm.TransactionProperties,
                             Type = pm.Type,
                             MerchantId = pm.MerchantId,
                             MerchantName = mc.Name,
                             InternalState = pm.InternalState,
                             IsAutoVerified = pm.IsAutoVerified,
                             IsManuallyVerified = pm.IsManuallyVerified,

                         })
                         .Where(x => x.Type == EPaymentType.DigitalService)
                         .WhereIf(input.MerchantId.HasValue, x => x.MerchantId == input.MerchantId)
                         .WhereIf(input.Period.HasValue, x => x.CreatedAt.Month == input.Period.Value.Month && x.CreatedAt.Year == input.Period.Value.Year)
                         .WhereIf(input.FromDay.HasValue, u => u.CreatedAt >= fromDay)
                         .WhereIf(input.ToDay.HasValue, u => u.CreatedAt <= toDay)
                         .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId)
                         .WhereIf(input.Method.HasValue, x => x.Method == (EPaymentMethod)input.Method)
                         .WhereIf(input.Status.HasValue, x => x.Status == (EPaymentStatus)input.Status)
                         .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Id.ToString() == input.Keyword || x.Amount.ToString() == input.Keyword)
                         .OrderByDescending(x => x.CreatedAt)
                         .AsQueryable();
            return query;
        }

        public async Task<object> GetAllThirdPartyPayments(GetAllPaymentInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {

                    var tenants = _tenantRepository.GetAllList();
                    var query = QueryThirdPartyPayment(input);
                    var result = await query.PageBy(input).ToListAsync();
                    foreach (var item in result)
                    {
                        item.TenantName = tenants.Where(x => x.Id == item.TenantId).Select(x => x.Name).FirstOrDefault();
                        item.FullName = await GetUserFullName(item.CreatedById ?? 0, item.TenantId);
                        if(!item.TransactionId.IsNullOrEmpty())
                        {
                            long orderId = Int64.Parse(item.TransactionId);
                            item.Order = await _digitalServiceOrderRepository.FirstOrDefaultAsync(x => x.Id == orderId);
                            if (item.Order != null) item.ServiceName = await _digitalServiceRepository.GetAll().Where(x => x.Id == item.Order.ServiceId).Select(x => x.Title).FirstOrDefaultAsync();
                        }
                    }

                    return DataResult.ResultSuccess(result, "", query.Count());
                }
                // return;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        protected async Task<string> GetUserFullName(long id, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                return (await _userRepos.FirstOrDefaultAsync(id))?.FullName;
            }
        }

        public async Task<object> GetCountThirdPartyPayments(GetAllPaymentInput input)
        {
            try
            {
                var query = QueryThirdPartyPayment(input);
                var result = new CountThirdPartyPaymentDto()
                {
                    NumberPayment = query.Count(),
                    TotalAmount = await query.SumAsync(x => x.Amount),
                    TenantName = _tenantRepository.GetAll().Where(x => x.Id == input.TenantId).Select(x => x.Name).FirstOrDefault()
                };

                return DataResult.ResultSuccess(result, "");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private IQueryable<DigitalServicePaymentDto> QueryDigitalServicePayment()
        {
            return (from pm in _digitalServicePaymentRepo.GetAll()
                    join sv in _digitalServiceRepository.GetAll() on pm.ServiceId equals sv.Id into tb_sv
                    from sv in tb_sv.DefaultIfEmpty()
                    join od in _digitalServiceOrderRepository.GetAll() on pm.OrderId equals od.Id into tb_od
                    from od in tb_od.DefaultIfEmpty()
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
                        ApartmentCode = pm.ApartmentCode,
                        Address = od.Address,
                        ServicesText = sv.Title,
                        ServiceDetails = od.ServiceDetails,
                        CustomerName = _citizenRepository.GetAll().Where(x => x.AccountId == od.CreatorUserId).Select(x => x.FullName).FirstOrDefault(),
                    }).AsQueryable();
        }

        public async Task<DataResult> GetAllTenantPaymentAsync(GetAllDigitalServicePaymentInput input)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    IQueryable<DigitalServicePaymentDto> query = QueryDigitalServicePayment()
                                                       .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                                                       x => x.ApartmentCode.ToLower().Contains(input.Keyword.ToLower())
                                                       || x.Code.ToLower().Contains(input.Keyword.ToLower())
                                                       || x.Note.ToLower().Contains(input.Keyword.ToLower()))
                                                       .WhereIf(input.UrbanId > 0, x => x.UrbanId == input.UrbanId)
                                                       .WhereIf(input.BuildingId > 0, x => x.BuildingId == input.BuildingId);

                    var data = await query.PageBy(input).ToListAsync();
                    return DataResult.ResultSuccess(data, "", query.Count());
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> GetOrderById(long id, int tenantId)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var item = await _digitalServiceOrderRepository.GetAsync(id);
                    var data = ObjectMapper.Map<DigitalServiceOrderViewDto>(item);
                    data.CreatorCitizen = await _citizenRepository.FirstOrDefaultAsync(x => x.AccountId == item.CreatorUserId);
                    data.ArrServiceDetails = !string.IsNullOrEmpty(item.ServiceDetails) ? JsonConvert.DeserializeObject<List<DigitalServiceDetailsGridDto>>(item.ServiceDetails) : new List<DigitalServiceDetailsGridDto>();

                    return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

    }
}
