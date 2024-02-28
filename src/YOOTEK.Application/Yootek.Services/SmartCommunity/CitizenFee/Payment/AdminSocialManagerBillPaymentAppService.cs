﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using YOOTEK.Yootek.Services.SmartCommunity.CitizenFee.Dto;
using Abp.Domain.Uow;
using Yootek.Services.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment;
using YOOTEK.EntityDb;
using NPOI.SS.Formula.Functions;
using Abp.MultiTenancy;
using Yootek.MultiTenancy;

namespace Yootek.Yootek.Services.SmartCommunity.Phidichvu
{
    public interface IAdminSocialManagerBillPaymentAppService : IApplicationService
    {
        Task<object> GetAllUserBillPayments(GetAllBillPaymentByAdminSocialDto input);
        Task<object> GetCountAmountUserBillPayments(GetAllBillPaymentByAdminSocialDto input);
        Task<object> GetAllThirdPartyPayments(GetAllPaymentInput input);
    }

    public class AdminSocialManagerBillPaymentAppService : YootekAppServiceBase, IAdminSocialManagerBillPaymentAppService
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<ThirdPartyPayment, int> _thirdPartyPaymentRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;

        public AdminSocialManagerBillPaymentAppService(
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<User, long> userRepos, IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IRepository<ThirdPartyPayment, int> thirdPartyPaymentRepository,
            IRepository<Tenant, int> tenantRepository
            )

        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userRepos = userRepos;
            _userBillRepo = userBillRepo;
            _billDebtRepo = billDebtRepo;
            _thirdPartyPaymentRepository = thirdPartyPaymentRepository; 
            _tenantRepository = tenantRepository;   
        }

        protected IQueryable<ThirdPartyPaymentDto> QueryThirdPartyPayment(GetAllPaymentInput input)
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
                         select new ThirdPartyPaymentDto()
                         {
                             Id = pm.Id,
                             Amount = pm.Amount,
                             CreatedAt = pm.CreatedAt,
                             Currency = pm.Currency,
                             Description = pm.Description,
                             Method = pm.Method,
                             Properties = pm.Properties,
                             Status = pm.Status,
                             TenantId = pm.TenantId,
                             TransactionId = pm.TransactionId,
                             TransactionProperties = pm.TransactionProperties,
                             Type = pm.Type
                         })
                         .Where(x => x.Type == EPaymentType.Invoice)
                         .WhereIf(input.Period.HasValue, x => x.CreatedAt.Month == input.Period.Value.Month && x.CreatedAt.Year ==  input.Period.Value.Year)
                         .WhereIf(input.FromDay.HasValue, u => u.CreatedAt >= fromDay)
                         .WhereIf(input.ToDay.HasValue, u => u.CreatedAt <= toDay)
                         .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId)
                         .WhereIf(input.Method.HasValue, x => x.Method == (EPaymentMethod)input.Method)
                         .WhereIf(input.Status.HasValue, x => x.Status == (EPaymentStatus)input.Status)
                         .AsQueryable();
            return query;
        }

        public async Task<object> GetAllThirdPartyPayments(GetAllPaymentInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {

                    var tenants = _tenantRepository.GetAllList();
                    var query = QueryThirdPartyPayment(input);
                    var result = await query.PageBy(input).ToListAsync();
                    foreach(var item in result)
                    {
                        item.TenantName = tenants.Where(x => x.Id == item.TenantId).Select(x => x.Name).FirstOrDefault();
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

        protected IQueryable<AdminUserBillPaymentOutputDto> QueryUserBillPayments(GetAllBillPaymentByAdminSocialDto input)
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

            var query = from payment in _userBillPaymentRepo.GetAll()
                        join us in _userRepos.GetAll() on payment.CreatorUserId equals us.Id into tbl_p_u
                        from user in tbl_p_u.DefaultIfEmpty()
                        select new AdminUserBillPaymentOutputDto()
                        {
                            Id = payment.Id,
                            Title = payment.Title,
                            UserBillIds = payment.UserBillIds,
                            Method = payment.Method,
                            Properties = payment.Properties,
                            TenantId = payment.TenantId,
                            Status = payment.Status,
                            FullName = user.FullName,
                            CreationTime = payment.CreationTime,
                            CreatorUserId = payment.CreatorUserId,
                            DeleterUserId = payment.DeleterUserId,
                            LastModificationTime = payment.LastModificationTime,
                            LastModifierUserId = payment.LastModifierUserId,
                            ImageUrl = payment.ImageUrl,
                            FileUrl = payment.FileUrl,
                            Amount = payment.Amount,
                            BillDebtIds = payment.BillDebtIds,
                            TypePayment = payment.TypePayment,
                            ApartmentCode = payment.ApartmentCode,
                            OrganizationUnitId = payment.OrganizationUnitId,
                            PaymentCode = payment.PaymentCode,
                            UserBillDebtIds = payment.UserBillDebtIds,
                            UserBillPrepaymentIds = payment.UserBillPrepaymentIds,
                            CustomerName = payment.CustomerName,
                            BillPaymentInfo = payment.BillPaymentInfo,
                            BuildingId = payment.BuildingId,
                            UrbanId = payment.UrbanId,
                            IsDeleted = payment.IsDeleted,
                            DeletionTime = payment.DeletionTime,
                        };
            query = query
                .WhereIf(!(input.IsDeleted.HasValue && input.IsDeleted.Value), x => x.IsDeleted == false)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(input.InDay.HasValue, x => x.CreationTime.Day == input.InDay.Value.Day && x.CreationTime.Month == input.InDay.Value.Month && x.CreationTime.Year == input.InDay.Value.Year)
                .WhereIf(input.Period.HasValue, x => x.CreationTime.Month == input.Period.Value.Month && x.CreationTime.Year == input.Period.Value.Year)
                .WhereIf(input.Method.HasValue, x => x.Method == input.Method)
                .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode == input.ApartmentCode)
                .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId)
                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .WhereIf(input.FromDay.HasValue, u => u.CreationTime >= fromDay)
                .WhereIf(input.ToDay.HasValue, u => u.CreationTime <= toDay)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.PaymentCode.Contains(input.Keyword) || x.ApartmentCode.Contains(input.Keyword))
                .AsQueryable();
            return query;
        }

        public async Task<object> GetAllUserBillPayments(GetAllBillPaymentByAdminSocialDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete, AbpDataFilters.MayHaveTenant))
                    {
                        var query = QueryUserBillPayments(input);
                        var result = await query.PageBy(input).ToListAsync();

                        foreach (var bill in result)
                        {
                            double totalPrice = 0;

                            if (!bill.BillPaymentInfo.IsNullOrEmpty())
                            {
                                try
                                {
                                    var infos = JsonConvert.DeserializeObject<BillPaymentInfo>(bill.BillPaymentInfo);
                                    bill.BillList = infos.BillList;
                                    bill.BillListDebt = infos.BillListDebt;
                                    bill.BillListPrepayment = infos.BillListPrepayment;
                                }
                                catch { }
                            }
                            else
                            {
                                if (!bill.UserBillIds.IsNullOrWhiteSpace())
                                {
                                    bill.BillList = await SplitBills(bill.UserBillIds);
                                    foreach (var b in bill.BillList)
                                    {
                                        totalPrice += b.LastCost.Value;
                                    }

                                }

                                if (!bill.UserBillDebtIds.IsNullOrEmpty())
                                {
                                    bill.BillListDebt = await SplitBills(bill.UserBillDebtIds);

                                }

                                if (!bill.UserBillPrepaymentIds.IsNullOrEmpty())
                                {
                                    bill.BillListPrepayment = await SplitBills(bill.UserBillPrepaymentIds);

                                }

                            }

                            if (bill.TypePayment == TypePayment.DebtBill && !bill.BillDebtIds.IsNullOrWhiteSpace())
                            {
                                try
                                {
                                    var ids = bill.BillDebtIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                                    bill.DebtList = await _billDebtRepo.GetAll().Where(x => ids.Contains(x.Id)).ToListAsync();
                                    if (bill.DebtList != null && bill.DebtList.Count > 0 && totalPrice == 0)
                                    {
                                        foreach (var debt in bill.DebtList)
                                        {
                                            totalPrice += debt.DebtTotal.Value;
                                        }
                                    }

                                    if (bill.ApartmentCode.IsNullOrEmpty() && bill.DebtList.Count > 0) bill.ApartmentCode = bill.DebtList[0].ApartmentCode;
                                }
                                catch { }
                            }
                            bill.TotalPayment = totalPrice;

                            if (bill.ApartmentCode.IsNullOrEmpty())
                            {
                                bill.ApartmentCode = bill.BillList?.Count > 0 ? bill.BillList[0].ApartmentCode : bill.BillListDebt?.Count > 0 ? bill.BillListDebt[0].ApartmentCode : bill.BillListPrepayment?.Count > 0 ? bill.BillListPrepayment[0].ApartmentCode : "";
                            }


                        }
                        var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                        return data;
                    }

                }
             

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        protected Task<List<BillPaidDto>> SplitBills(string input)
        {
            var billIds = input.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
            return _userBillRepo.GetAll()
                .Select(x => new BillPaidDto()
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    ApartmentCode = x.ApartmentCode,
                    BillType = x.BillType,
                    Code = x.Code,
                    LastCost = x.LastCost,
                    Period = x.Period,
                    Status = x.Status,
                    Title = x.Title,
                    IndexEndPeriod = x.IndexEndPeriod,
                    IndexHeadPeriod = x.IndexHeadPeriod,
                    TotalIndex = x.TotalIndex,
                    DebtTotal = x.DebtTotal,
                    PayAmount = x.DebtTotal > 0 ? (double)x.DebtTotal : x.LastCost
                })
                .Where(x => billIds.Contains(x.Id)).ToListAsync();
        }
        public async Task<object> GetCountAmountUserBillPayments(GetAllBillPaymentByAdminSocialDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete, AbpDataFilters.MayHaveTenant))
                    {

                        var query = QueryUserBillPayments(input);
                        var total = await query.SumAsync(x => x.Amount);
                        var result = new CountPaymentSocialResult()
                        {
                            TotalAmount = total ?? 0,
                            NumberPayment = query.Count(),
                            TenantName = _tenantRepository.GetAll().Where(x => x.Id == input.TenantId).Select(x => x.Name).FirstOrDefault()
                        };
                        return DataResult.ResultSuccess(result, "Get success");
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

      
    }
}