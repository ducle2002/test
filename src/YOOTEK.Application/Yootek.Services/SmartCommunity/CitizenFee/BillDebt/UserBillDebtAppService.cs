using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IUserBillDebtAppService : IApplicationService
    {
    }

    //  [AbpAuthorize(PermissionNames.Pages_SmartCommunity_Fees)]
    [AbpAuthorize]
    public class UserBillDebtAppService : YootekAppServiceBase, IUserBillDebtAppService
    {
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<HomeMember, long> _homeMemberRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepo;


        public UserBillDebtAppService(
            IRepository<BillDebt, long> billDebtRepo,
            IRepository<HomeMember, long> homeMemberRepo,
            IRepository<CitizenTemp, long> citizenTempRepo,
            IRepository<UserBill, long> userBillRepo
            )
        {
            _billDebtRepo = billDebtRepo;
            _homeMemberRepo = homeMemberRepo;
            _citizenTempRepo = citizenTempRepo;
            _userBillRepo = userBillRepo;
        }

        public async Task<object> GetAllUserBillDebtAsync(UserGetBillDebtInputDto input)
        {
            try
            {
                var mb = _homeMemberRepo.FirstOrDefault(x => x.UserId == AbpSession.UserId);
                var query = _billDebtRepo.GetAll().Where(x => x.ApartmentCode == input.ApartmentCode && (x.State == DebtState.DEBT || x.State == DebtState.WAITFORCONFIRM))
                    .WhereIf(input.Period.HasValue, x => x.Period.Month == input.Period.Value.Month && x.Period.Year == input.Period.Value.Year)
                    .AsQueryable();

                var data = query.PageBy(input).ToList();
                var result = data.MapTo<List<UserBillDebtDto>>();
                foreach (var item in result)
                {
                    item.BillList = await SplitBillAsync(item.UserBillIds);
                }
                return DataResult.ResultSuccess(result, "Get success", query.Count()); ;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        protected Task<List<UserBill>> SplitBillAsync(string input)
        {
            try
            {
                var billIds = input.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                return _userBillRepo.GetAll().Where(x => billIds.Contains(x.Id)).ToListAsync();

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}