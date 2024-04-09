using Abp.Application.Services;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System;
using Yootek;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.EntityDb;
using Yootek.Services.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using YOOTEK.EntityDb;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Yootek.Common.DataResult;
using Microsoft.EntityFrameworkCore;

namespace YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.ApartmentBalanceBill
{
    public interface IApartmentBalanceManagerAppService : IApplicationService
    {

    }

    public class ApartmentBalanceManagerAppService : YootekAppServiceBase, IApartmentBalanceManagerAppService
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<UserBillPaymentHistory, long> _billPaymentHistoryRepos;
        private readonly IRepository<ApartmentBalance, long> _apartmentBalanceRepos;


        public ApartmentBalanceManagerAppService(
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<User, long> userRepos, IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<UserBillPaymentHistory, long> billPaymentHistoryRepos,
            IRepository<ApartmentBalance, long> apartmentBalanceRepos
            )

        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userRepos = userRepos;
            _userBillRepo = userBillRepo;
            _citizenTempRepos = citizenTempRepos;
            _billDebtRepo = billDebtRepo;
            _billPaymentHistoryRepos = billPaymentHistoryRepos;
            _apartmentBalanceRepos = apartmentBalanceRepos;
        }

        protected IQueryable<ApartmentBalanceDto> QueryApartmentBalance(GetApartmentBalanceInput input)
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

            var query = from bl in _apartmentBalanceRepos.GetAll()
                        select new ApartmentBalanceDto()
                        {
                            Id = bl.Id,
                            TenantId = bl.TenantId,
                            Amount = bl.Amount,
                            ApartmentCode = bl.ApartmentCode,
                            CustomerName =  bl.CustomerName,
                            BuildingId = bl.BuildingId,
                            UrbanId = bl.UrbanId,
                            CitizenTempId = bl.CitizenTempId,
                            BillType = bl.BillType,
                            EBalanceAction = bl.EBalanceAction,
                            UserBillId = bl.UserBillId,
                            CreationTime = bl.CreationTime

                        };
            query = query
                .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode == input.ApartmentCode)
                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .WhereIf(input.FromDay.HasValue, u => u.CreationTime >= fromDay)
                .WhereIf(input.ToDay.HasValue, u => u.CreationTime <= toDay)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.CustomerName.Contains(input.Keyword) || x.ApartmentCode.Contains(input.Keyword))
                .AsQueryable();
            return query;
        }

        public async Task<object> GetAll(GetApartmentBalanceInput input)
        {
            try
            {
                var query = QueryApartmentBalance(input);
                var result = await query.PageBy(input).ToListAsync();
            
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetTotalBalanceByApartment(GetTotalApartmentBalanceInput input)
        {
            try
            {
                var request = new GetApartmentBalanceInput()
                {
                    ApartmentCode = input.ApartmentCode,
                    BuildingId = input.BuildingId,
                    UrbanId = input.UrbanId
                };
                var query = QueryApartmentBalance(request);

                var totalAdd = await query
                    .Where(x => x.EBalanceAction == EBalanceAction.Add)
                    .WhereIf(input.IsWhereByType, x => x.BillType == input.BillType)
                    .SumAsync(x => x.Amount);

                var totalSub = await query
                    .Where(x => x.EBalanceAction == EBalanceAction.Sub)
                    .WhereIf(input.IsWhereByType, x => x.BillType == input.BillType)
                    .SumAsync(x => x.Amount);

                var data = DataResult.ResultSuccess(totalAdd - totalSub, "Get success");
                return data;

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetDetailBalanceByApartment(GetTotalApartmentBalanceInput input)
        {
            try
            {
                var request = new GetApartmentBalanceInput()
                {
                    ApartmentCode = input.ApartmentCode,
                    BuildingId = input.BuildingId,
                    UrbanId = input.UrbanId
                };
                var query = QueryApartmentBalance(request);
                var totalAdd = await query.Where(x => x.EBalanceAction == EBalanceAction.Add).SumAsync(x => x.Amount);
                var totalSub = await query.Where(x => x.EBalanceAction == EBalanceAction.Sub).SumAsync(x => x.Amount);

                var data = DataResult.ResultSuccess(totalAdd - totalSub, "Get success");
                return data;

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

    }
}
