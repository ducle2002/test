using Abp;
using Abp.Application.Services;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Castle.Core.Internal;
using IMAX.Application;
using IMAX.Application.BusinessChat.Dto;
using IMAX.Authorization;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Dto;
using IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Payment;
using IMAX.Notifications;
using IMAX.QueriesExtension;
using IMAX.Services;
using IMAX.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.IMAX.Services.SmartCommunity.Phidichvu
{
    public interface IAdminManagerBillPaymentAppService : IApplicationService
    {
        Task<object> GetAllUserBillPayments(GetAllAdminUserBillPaymentDto input);
        Task<object> GetCountAmountUserBillPayments(GetAllAdminUserBillPaymentDto input);
        Task<object> GetUserBillPaymentById(long id);
        Task<object> SetDebtPaymentStatus(SetBillPaymentStatusDto input);
        Task<object> SetUserBillPaymentStatus(SetBillPaymentStatusDto input);
        Task<object> VerifyListPaymentUserBill(List<VerifyPaymentUserBill> input);
        Task<object> VerifyPaymentUserBill(VerifyPaymentUserBill input);
        Task<object> RecoverPaymentUserBill(long id);

    }

    public class AdminManagerBillPaymentAppService : IMAXAppServiceBase, IAdminManagerBillPaymentAppService
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly HandlePaymentUtilAppService _handlePaymentUtilAppService;
        private readonly IPaymentExcelExporter _paymentExcelExporter;
        public AdminManagerBillPaymentAppService(
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<User, long> userRepos, IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IAppNotifier appNotifier,
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            HandlePaymentUtilAppService handlePaymentUtilAppService,
            IPaymentExcelExporter paymentExcelExporter
            )

        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userRepos = userRepos;
            _userBillRepo = userBillRepo;
            _appNotifier = appNotifier;
            _citizenTempRepos = citizenTempRepos;
            _billDebtRepo = billDebtRepo;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _handlePaymentUtilAppService = handlePaymentUtilAppService;
            _paymentExcelExporter = paymentExcelExporter;
        }

        protected IQueryable<AdminUserBillPaymentOutputDto> QueryUserBillPayments(GetAllAdminUserBillPaymentDto input)
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
            
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
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

                        };
            query = query
                .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                .WhereIf(!input.IsAdvanced, x => !(x.Method != UserBillPaymentMethod.Direct && x.Status == UserBillPaymentStatus.Pending) || !(x.Method != UserBillPaymentMethod.Banking && x.Status == UserBillPaymentStatus.Pending))
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(input.InDay.HasValue, x => x.CreationTime.Day == input.InDay.Value.Day && x.CreationTime.Month == input.InDay.Value.Month && x.CreationTime.Year == input.InDay.Value.Year)
                .WhereIf(input.Period.HasValue, x => x.Period.HasValue && (x.Period.Value.Month == input.Period.Value.Month && x.Period.Value.Year == input.Period.Value.Year))
                .WhereIf(input.Method.HasValue, x => x.Method == input.Method)
                .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode == input.ApartmentCode)
                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId ==  input.BuildingId)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .WhereIf(input.FromDay.HasValue, u => u.CreationTime >= fromDay)
                .WhereIf(input.ToDay.HasValue, u => u.CreationTime <= toDay)
                .WhereIf(input.ToDay.HasValue, u => u.CreationTime <= toDay)
                .ApplySearchFilter(input.Keyword, x => x.Title)
                .ApplySort(input.OrderBy, input.SortBy)
                .ApplySort(OrderByBillPayment.CREATION_TIME, SortBy.DESC) // sort default
                .AsQueryable();
            return query;
        }

        public async Task<object> GetAllUserBillPayments(GetAllAdminUserBillPaymentDto input)
        {
            try
            {
                var query = QueryUserBillPayments(input).ApplySearchFilter(input.Keyword, x => x.Title, x => x.ApartmentCode);
                var result = await query.ApplySort(input.OrderBy, input.SortBy).ApplySort(OrderByBillPayment.PAYMENT_CODE).PageBy(input).ToListAsync();

                foreach (var bill in result)
                {
                    double totalPrice = 0;
                    
                    if(!bill.BillPaymentInfo.IsNullOrEmpty())
                    {
                        try
                        {
                            var infos = JsonConvert.DeserializeObject<BillPaymentInfo>(bill.BillPaymentInfo);
                            bill.BillList = infos.BillList;
                            bill.BillListDebt = infos.BillListDebt; 
                            bill.BillListPrepayment = infos.BillListPrepayment;
                        }catch { }
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
                    if (bill.PaymentCode.IsNullOrEmpty())
                    {
                        bill.PaymentCode = await UpdatePaymentCode(bill);
                    }

                }
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<object> GetCountAmountUserBillPayments(GetAllAdminUserBillPaymentDto input)
        {
            try
            {
                var query = QueryUserBillPayments(input).ApplySearchFilter(input.Keyword, x => x.Title, x => x.ApartmentCode);
                var total = await query.SumAsync(x => x.Amount);
                var result = new CountPaymentResult()
                {
                    TotalAmount = total??0,
                    NumberPayment = query.Count()
                };
                return DataResult.ResultSuccess(result, "Get success");

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> GetUserBillPaymentById(long id)
        {
            try
            {
                var data = await _userBillPaymentRepo.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> SetDebtPaymentStatus(SetBillPaymentStatusDto input)
        {
            try
            {
                long[] id_list = Array.ConvertAll(input.BillDebtIds.Split(","), long.Parse);
                var debts = await _billDebtRepo.GetAll().Where(x => id_list.Contains(x.Id)).ToListAsync();

                var userBillPayment = await _userBillPaymentRepo.FirstOrDefaultAsync(input.Id);
                switch (input.Status)
                {
                    case UserBillPaymentStatus.Success:
                        await HandlePaymentDebt(debts, userBillPayment);
                        break;
                    case UserBillPaymentStatus.Fail:
                        userBillPayment.Status = UserBillPaymentStatus.Cancel;
                        foreach (var bill in debts)
                        {
                            bill.State = DebtState.DEBT;
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                        await NotifierBillPaymentCancel(userBillPayment);
                        break;
                    default:
                        break;

                }
                //userBillPayment.Status = input.Status;
                //await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(null, "Update success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> SetUserBillPaymentStatus(SetBillPaymentStatusDto input)
        {
            try
            {
                var userBillPayment = await _userBillPaymentRepo.FirstOrDefaultAsync(input.Id);
                switch (input.Status)
                {
                    case UserBillPaymentStatus.Success:
                        await HandlePaymentUserBill(userBillPayment);
                        break;
                    case UserBillPaymentStatus.Fail:
                        await CancelPaymentUserBill(userBillPayment);

                        break;
                    default:
                        break;

                }

                return DataResult.ResultSuccess("Update success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public DataResult ExportPaymentExcel(GetAllAdminUserBillPaymentDto input)
        {
            try
            {
                var query = QueryUserBillPayments(input);
                var data = query.ToList();
                var result = _paymentExcelExporter.ExportToFile(data);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> VerifyListPaymentUserBill(List<VerifyPaymentUserBill> input)
        {
            try
            {
                if (input == null || input.Count() == 0) throw new Exception("Input payment null");

                foreach (var pm in input)
                {
                    await VerifyPaymentUserBill(pm);
                }

                var data = DataResult.ResultSuccess("Admin payment success");
                return data;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> VerifyPaymentUserBill(VerifyPaymentUserBill input)
        {
            try
            {
                if ((input.UserBillIds == null || input.UserBillIds.Count() == 0)
                    && (input.BillDebtIds == null || input.BillDebtIds.Count() == 0)
                    && (input.PrepaymentBills == null || input.PrepaymentBills.Count() == 0)) throw new Exception("Input user bill is null");

                var payment = new UserBillPayment()
                {
                    Amount = input.Amount,
                    ApartmentCode = input.ApartmentCode,
                    Method = input.Method ?? UserBillPaymentMethod.AdminVerify,
                    Status = UserBillPaymentStatus.Success,
                    TypePayment = TypePayment.Bill,
                    Period = input.Period,
                    Title = "Thanh toán hóa đơn tháng " + input.Period.Value.ToString("MM/yyyyy"),
                    TenantId = AbpSession.TenantId,
                    Description = input.Description,
                    BuildingId = input.UserBill.BuildingId,
                    UrbanId = input.UserBill.UrbanId,
                    FileUrl = input.FileUrl,
                    ImageUrl = input.ImageUrl,
                };

                // Handle Userbill
                if (input.UserBillIds != null && input.UserBillIds.Count() > 0)
                {
                    var userBills = _userBillRepo.GetAll().Where(x => input.UserBillIds.Contains(x.Id))
                      .Where(x => x.Status == UserBillStatus.Pending)
                      .ToList();
                    payment.UserBillIds = string.Join(",", input.UserBillIds.OrderBy(x => x));
                    foreach (var userBill in userBills)
                    {
                        userBill.Status = UserBillStatus.Paid;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();

                }

                // Handle billDebt

                if (input.BillDebtIds != null && input.BillDebtIds.Count() > 0)
                {
                    long[] userBillDebtIds = await HandleBillDebtVerifyPayment(input.BillDebtIds);
                    payment.UserBillDebtIds = string.Join(",", userBillDebtIds.OrderBy(x => x));
                }

                // Handle prepayment
                if (input.PrepaymentBills != null && input.PrepaymentBills.Count > 0)
                {
                    var prepayments = await HandlePrepaymentVerifyPayment(input.PrepaymentBills, input.UserBill);
                    payment.UserBillPrepaymentIds = string.Join(",", prepayments.Select(x => x.Id).OrderBy(x => x));
                }


                // Tạo thanh toán thành công
                await _userBillPaymentRepo.InsertAndGetIdAsync(payment);

                // Kiểm tra và tạo công nợ
                //if (checkDebt)
                //{
                //    var debt = new BillDebt()
                //    {
                //        ApartmentCode = input.ApartmentCode,
                //        BillPaymentId = payment.Id,
                //        CitizenTempId = userBills[0].CitizenTempId,
                //        DebtTotal = totalCost - input.Amount,
                //        UserBillIds = string.Join(",", input.UserBillIds),
                //        PaidAmount = input.Amount,
                //        Period = DateTime.Now,
                //        State = DebtState.DEBT,
                //        TenantId = AbpSession.TenantId,
                //        Title = $"Công nợ hóa đơn tháng {DateTime.Now.Month}/{DateTime.Now.Year}",
                //        BuildingId = userBills[0].BuildingId,
                //        UrbanId = userBills[0].UrbanId
                //    };
                //    await _billDebtRepo.InsertAsync(debt);
                //}

                //Update trạng thái hóa đơn


                // Thông báo đến cư dân
                try
                {
                    await NotifierBillPaymentSuccess(payment, (int)payment.Amount, payment.CreatorUserId.Value);
                }
                catch
                {
                }

                var data = DataResult.ResultSuccess("Admin payment success");
                return data;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> PayMonthlyUserBills(PayMonthlyUserBillsInput input)
        {
            try
            {
                await _handlePaymentUtilAppService.PayMonthlyUserBillByApartment(input);

                var data = DataResult.ResultSuccess("Admin payment success");
                return data;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        private bool CheckDebtVerifyPayment(int totalCost, int amount)
        {
            return false;
        }

        private async Task<long[]> HandleUserBillVerifyPayment(long[] userBillDebts)
        {
            List<long> ids = new List<long>();

            foreach (var id in userBillDebts)
            {
                ids.Add(id);
                var bill = await _userBillRepo.GetAsync(id);
                if (bill != null)
                {
                    bill.Status = UserBillStatus.Paid;

                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            return ids.ToArray();
        }

        private async Task<long[]> HandleBillDebtVerifyPayment(long[] userBillDebts)
        {
            List<long> ids = new List<long>();

            foreach (var id in userBillDebts)
            {
                ids.Add(id);
                var bill = await _userBillRepo.GetAsync(id);
                if (bill != null)
                {
                    bill.Status = UserBillStatus.Paid;

                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            return ids.ToArray();
        }

        private async Task<List<BillPaidDto>> HandlePrepaymentVerifyPayment(List<PrepaymentBillDto> prepaymentBillDtos, UserBill userBill)
        {
            var properties = JsonConvert.DeserializeObject<dynamic>(userBill.Properties);
            var period = DateTime.Now;
            var bills = new List<BillPaidDto>();
            foreach (var prepaymentBill in prepaymentBillDtos)
            {
                if (prepaymentBill.NumberPeriod > 0)
                {

                    for (var i = 1; i <= prepaymentBill.NumberPeriod; i++)
                    {

                        DateTime billPeriod = period.AddMonths(i);
                        var userBill1 = new UserBill();
                        userBill1.Title = $"Hóa đơn tháng {billPeriod.Month}/{billPeriod.Year}";
                        userBill1.UrbanId = userBill.UrbanId ?? null;
                        userBill1.BuildingId = userBill.BuildingId ?? null;
                        userBill1.ApartmentCode = userBill.ApartmentCode;
                        userBill1.Period = billPeriod;
                        userBill1.DueDate = billPeriod;
                        userBill1.TenantId = AbpSession.TenantId;
                        userBill1.CitizenTempId = userBill.CitizenTempId;
                        userBill1.IsDraft = false;
                        userBill1.BillType = prepaymentBill.BillType;
                        userBill1.Status = UserBillStatus.Paid;
                        userBill1.CarNumber = userBill.CarNumber;
                        userBill1.MotorbikeNumber = userBill.MotorbikeNumber;
                        userBill1.BicycleNumber = userBill.BicycleNumber;
                        userBill1.OtherVehicleNumber = userBill.OtherVehicleNumber;


                        userBill1.TotalIndex = userBill.TotalIndex;
                        userBill1.LastCost = prepaymentBill.LastCost;
                        if (!string.IsNullOrEmpty(prepaymentBill.Vehicles))
                        {
                            userBill1.Properties = JsonConvert.SerializeObject(new
                            {
                                customerName = properties?.CustomerName ?? null,
                                formulas = properties?.Formulas ?? null,
                                vehicles = JsonConvert.DeserializeObject<object>(prepaymentBill.Vehicles),
                                pricesType = prepaymentBill.PricesType ?? 5
                            });
                        }
                        else
                        {
                            userBill1.Properties = JsonConvert.SerializeObject(new
                            {
                                customerName = properties?.CustomerName ?? null,
                                formulas = properties?.Formulas ?? null,
                                vehicles = JsonConvert.DeserializeObject<object>("[]"),
                                pricesType = prepaymentBill.PricesType ?? 5
                            });
                        }

                        var id = await _userBillRepo.InsertAndGetIdAsync(userBill1);

                        userBill1.Code = "HD" + userBill1.Id + (billPeriod.Month < 10 ? "0" + billPeriod.Month : billPeriod.Month) + "" + billPeriod.Year;
                        var billPaid = userBill1.MapTo<BillPaidDto>();
                        bills.Add(billPaid);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

            }

            return bills;
        }


        public async Task<object> GetAllRecoverPaymentHistoryAsync(GetAllRecoverPaymentHistoryInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    var query = (from pm in _userBillPaymentRepo.GetAll()
                                 select new AdminUserBillPaymentOutputDto()
                                 {
                                     Amount = pm.Amount,
                                     ApartmentCode = pm.ApartmentCode,
                                     IsDeleted = pm.IsDeleted,
                                     IsRecover = pm.IsRecover,
                                     BillDebtIds = pm.BillDebtIds,
                                     UserBillIds = pm.UserBillIds,
                                     DeleterUserId = pm.DeleterUserId,
                                     DeletionTime = pm.DeletionTime,
                                     Id = pm.Id,
                                     Method = pm.Method,
                                     TypePayment = pm.TypePayment,
                                     PaymentCode = pm.PaymentCode,
                                     Period = pm.Period
                                 })
                                 .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                                 .Where(x => x.IsDeleted && x.IsRecover.HasValue && x.IsRecover.Value)
                                 .ApplySearchFilter(input.Keyword, x => x.ApartmentCode)
                                 .AsQueryable();
                    var result = await query.PageBy(input).ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                    return data;
                }

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "ResultFail");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> RecoverPaymentUserBill(long id)
        {
            try
            {
                var payment = await _userBillPaymentRepo.GetAsync(id);
                if (payment == null) return DataResult.ResultSuccess(null, "Delete success");
                payment.IsRecover = true;

                if (payment.TypePayment == TypePayment.Bill)
                {
                    var billPaymentInfo = new BillPaymentInfo();
                    if(!payment.BillPaymentInfo.IsNullOrEmpty())
                    {
                        try
                        {
                            billPaymentInfo = JsonConvert.DeserializeObject<BillPaymentInfo>(payment.BillPaymentInfo);
                        }
                        catch { }
                    }

                   
                    if(!payment.UserBillIds.IsNullOrEmpty())
                    {
                      
                        if(billPaymentInfo.BillList != null)
                        {
                            foreach (var bill in billPaymentInfo.BillList)
                            {
                                var userBill = _userBillRepo.Get(bill.Id);
                                userBill.Status = bill.Status;
                                userBill.DebtTotal = bill.DebtTotal;
                               
                            }
                        }
                        else
                        {
                            var billIds = payment.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                            var bills = await _userBillRepo.GetAllListAsync(x => billIds.Contains(x.Id));
                            foreach (var bill in bills)
                            {
                                bill.Status = UserBillStatus.Pending;
                            }
                        }
                    }

                    if (!payment.UserBillDebtIds.IsNullOrEmpty())
                    {
                        if (billPaymentInfo.BillListDebt != null)
                        {
                            foreach (var bill in billPaymentInfo.BillListDebt)
                            {
                                var userBill = _userBillRepo.Get(bill.Id);
                                userBill.Status = bill.Status;
                                userBill.DebtTotal = bill.DebtTotal;
                            }
                        }
                        else 
                        {
                            var ids = payment.UserBillDebtIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                            var debts = await _userBillRepo.GetAllListAsync(x => ids.Contains(x.Id));
                            foreach (var bill in debts)
                            {
                                bill.Status = UserBillStatus.Debt;
                            }
                        }
                      
                    }

                    if (!payment.UserBillPrepaymentIds.IsNullOrEmpty())
                    {
                        var ids = payment.UserBillPrepaymentIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                        await _userBillRepo.DeleteAsync(x => ids.Contains(x.Id));

                    }

                    await CurrentUnitOfWork.SaveChangesAsync();

                }
                else if (payment.TypePayment == TypePayment.DebtBill)
                {
                    var debtIds = payment.BillDebtIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                    var debts = await _billDebtRepo.GetAllListAsync(x => debtIds.Contains(x.Id));

                    var userBillIds = new List<long>();
                    foreach (var debt in debts)
                    {
                        debt.State = DebtState.DEBT;
                        if (!debt.UserBillIds.IsNullOrEmpty())
                        {
                            try
                            {
                                var billIds = debt.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToList();
                                userBillIds = userBillIds.Concat(billIds).ToList();

                            }
                            catch { }

                        }
                    }

                    if (userBillIds.Count > 0)
                    {
                        var userbills = await _userBillRepo.GetAllListAsync(x => userBillIds.Contains(x.Id));
                        foreach (var userbill in userbills) userbill.Status = UserBillStatus.Debt;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();

                }

                await _userBillPaymentRepo.DeleteAsync(id);
                var data = DataResult.ResultSuccess(null, "Delete success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
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

        #region Common
        private async Task NotifierBillPaymentSuccess(UserBillPayment bill, int amount, long userId)
        {
            var method = L(nameof(bill.Method));
            string detailUrlApp = $"yoolife://app/receipt?apartmentCode={bill.ApartmentCode}&formId=2";
            string detailUrlWA = $"/monthly?apartmentCode={bill.ApartmentCode}&formId=2";
            var messageSuccess = new UserMessageNotificationDataBase(
                               AppNotificationAction.BillPaymentSuccess,
                               AppNotificationIcon.BillPaymentSuccessIcon,
                               TypeAction.Detail,
                               $"Thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {string.Format("{0:#,#.##}",amount)} VNĐ",
                               detailUrlApp,
                               detailUrlWA,
                               "",
                               "",
                               0
                               );
            await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                 $"Thông báo thanh toán hóa đơn !",
                 $"Bạn đã thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {string.Format("{0:#,#.##}", amount)} VNĐ",
                 detailUrlApp,
                 detailUrlWA,
                 new UserIdentifier[] { new UserIdentifier(bill.TenantId, userId) },
                 messageSuccess);
        }

        private async Task NotifierBillPaymentCancel(UserBillPayment bill)
        {
            try
            {
                var method = L(nameof(bill.Method));
                var detailUrlApp = $"yoolife://app/receipt?apartmentCode={bill.ApartmentCode}&formId=1";
                var detailUrlWA = $"/monthly?apartmentCode={bill.ApartmentCode}&formId=1";
                var messageSuccess = new UserMessageNotificationDataBase(
                                   AppNotificationAction.BillPaymentCancel,
                                   AppNotificationIcon.BillPaymentCancelIcon,
                                   TypeAction.Detail,
                                   $"Yêu cầu thanh toán của bạn đã bị từ chối",
                                   detailUrlApp,
                                   detailUrlWA
                                   );
                await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                     $"Thông báo thanh toán !",
                      $"Yêu cầu thanh toán của bạn đã bị từ chối",
                      detailUrlApp,
                      detailUrlWA,
                     new UserIdentifier[] { new UserIdentifier(bill.TenantId, bill.CreatorUserId.Value) },
                     messageSuccess);
            }
            catch { }
        }
        #endregion

        #region Handle payment

        private async Task HandlePaymentDebt(List<BillDebt> debts, UserBillPayment payment)
        {

            // Check cost (sum of amount) of bills
            var totalCost = debts.Sum(x => x.DebtTotal);
            if (totalCost == null)
            {
                throw new Exception("Invalid amount");
            }

            var nowTime = DateTime.Now;

            payment.Status = UserBillPaymentStatus.Success;
            await _userBillPaymentRepo.UpdateAsync(payment);

            try
            {
                await NotifierBillPaymentSuccess(payment, (int)payment.Amount, payment.CreatorUserId.Value);
            }
            catch
            {
            }

            if (totalCost != payment.Amount)
            {
                throw new Exception("Invalid amount");
            }
            else
            {
                var userBillIds = new List<long>();
                // Update status of bills to paid
                foreach (var debt in debts)
                {
                    debt.State = DebtState.PAIED;
                    if (!debt.UserBillIds.IsNullOrEmpty())
                    {
                        try
                        {
                            var billIds = debt.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToList();
                            userBillIds = userBillIds.Concat(billIds).ToList();

                        }
                        catch { }

                    }
                }

                if (userBillIds.Count > 0)
                {
                    var userbills = await _userBillRepo.GetAllListAsync(x => userBillIds.Contains(x.Id));
                    foreach (var userbill in userbills) userbill.Status = UserBillStatus.Paid;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

            }

        }


        private async Task HandlePaymentUserBill(UserBillPayment payment)
        {

            var nowTime = DateTime.Now;
            payment.Status = UserBillPaymentStatus.Success;
            await _userBillPaymentRepo.UpdateAsync(payment);

            try
            {
                await NotifierBillPaymentSuccess(payment, (int)payment.Amount, payment.CreatorUserId.Value);
            }
            catch
            {
            }

            // Handle Userbill
            if (payment.UserBillIds != null)
            {
                var billIds = payment.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                var userBills = _userBillRepo.GetAll().Where(x => billIds.Contains(x.Id))
                  .ToList();

                foreach (var userBill in userBills)
                {
                    userBill.Status = UserBillStatus.Paid;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

            }

            // Handle billDebt

            if (payment.UserBillDebtIds != null)
            {
                var billIds = payment.UserBillDebtIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                var userBills = _userBillRepo.GetAll().Where(x => billIds.Contains(x.Id))
                  .ToList();

                foreach (var userBill in userBills)
                {
                    userBill.Status = UserBillStatus.Paid;
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }


        private async Task CancelPaymentUserBill(UserBillPayment payment)
        {

            var nowTime = DateTime.Now;
            payment.Status = UserBillPaymentStatus.Cancel;
            await _userBillPaymentRepo.UpdateAsync(payment);

            try
            {
                await NotifierBillPaymentCancel(payment);
            }
            catch
            {
            }

            // Handle Userbill
            if (payment.UserBillIds != null)
            {
                var billIds = payment.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                var userBills = _userBillRepo.GetAll().Where(x => billIds.Contains(x.Id))
                  .ToList();

                foreach (var userBill in userBills)
                {
                    userBill.Status = UserBillStatus.Pending;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

            }

            // Handle billDebt

            if (payment.UserBillDebtIds != null)
            {
                var billIds = payment.UserBillDebtIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                var userBills = _userBillRepo.GetAll().Where(x => billIds.Contains(x.Id))
                  .ToList();

                foreach (var userBill in userBills)
                {
                    userBill.Status = UserBillStatus.Debt;
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }

        }

        private async Task HandleUserBillDebt(List<UserBill> bills, double amount, long paymnentId)
        {
            try
            {
                var userBillIds = bills.Select(x => x.Id).ToList();
                if (bills == null || bills.Count == 0) { return; }
                var totalCost = bills.Sum(x => x.LastCost);
                if (totalCost == 0) { return; }
                //paid 100%
                if (totalCost == amount && userBillIds.Count() == bills.Count())
                {
                    return;
                }
                else if (totalCost > amount)
                {
                    var debt = new BillDebt()
                    {
                        ApartmentCode = bills[0].ApartmentCode,
                        BillPaymentId = paymnentId,
                        CitizenTempId = bills[0].CitizenTempId,
                        DebtTotal = totalCost - amount,
                        UserBillIds = string.Join(",", userBillIds),
                        PaidAmount = amount,
                        Period = DateTime.Now,
                        State = DebtState.DEBT,
                        TenantId = bills[0].TenantId,
                        Title = $"Công nợ hóa đơn tháng {DateTime.Now.Month}/{DateTime.Now.Year}",
                        UrbanId = bills[0].UrbanId,
                        BuildingId = bills[0].BuildingId
                    };
                    await _billDebtRepo.InsertAsync(debt);

                    foreach (var userBill in bills)
                    {
                        userBill.Status = UserBillStatus.Debt;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (totalCost < amount)
                {
                    var debt = new BillDebt()
                    {
                        ApartmentCode = bills[0].ApartmentCode,
                        BillPaymentId = paymnentId,
                        CitizenTempId = bills[0].CitizenTempId,
                        DebtTotal = amount - totalCost,
                        UserBillIds = string.Join(",", userBillIds),
                        Period = DateTime.Now,
                        State = DebtState.REDUNDANT,
                        TenantId = bills[0].TenantId,
                        PaidAmount = amount,
                        Title = $"Công nợ hóa đơn tháng {DateTime.Now.Month}/{DateTime.Now.Year}",
                        UrbanId = bills[0].UrbanId,
                        BuildingId = bills[0].BuildingId
                    };
                    await _billDebtRepo.InsertAsync(debt);

                    foreach (var userBill in bills)
                    {
                        userBill.Status = UserBillStatus.Paid;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                //var ids = new List<long>();

                //if (!string.IsNullOrEmpty(userBillIds)) ids = JsonConvert.DeserializeObject<List<long>>(userBillIds);

                //var billnotPaids = bills.Where(x => !ids.Contains(x.Id)).ToList();
            }
            catch (Exception e)
            {

            }
        }

        private async Task<string> UpdatePaymentCode(AdminUserBillPaymentOutputDto pm)
        {
            try
            {
                var payment = await _userBillPaymentRepo.FirstOrDefaultAsync(pm.Id);
                if (payment == null) return null;
                payment.PaymentCode = "PM-" + payment.Id + "-" + GetUniqueKey(6);
                await CurrentUnitOfWork.SaveChangesAsync();
                return payment.PaymentCode;
            }
            catch
            {
                return null;
            }
        }
     
        #endregion
    }
}
