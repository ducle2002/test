using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.UI;
using Castle.Core.Internal;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment;
using Yootek.Notifications;
using Yootek.Notifications.UserBillNotification;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IUserBillAppService : IApplicationService
    {
        Task<object> GetBillEmailTemplateAsync(GetAllBillEmailDto input);
        Task<object> GetAllBillConfigs(GetAllBillConfigInputDto input);
        Task<object> GetAllUserBills(GetAllUserBillsInputDto input);
        Task<object> GetPaymentHistories(GetPaymentHistoriesInputDto input);
        Task<object> GetAllUserBillByMonth(UserGetBillByMonthInput input);
        Task<object> HandleUserBillDirect(HandlePayUserBillDirectInputDto input);
        Task HandlePaymentForMomo(HandlePayUserBillInputDto input);
        Task HandlePaymentForVNPay(HandlePayUserBillInputDto input);
    }

    [AbpAuthorize]
    public class UserBillAppService : YootekAppServiceBase, IUserBillAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<BillEmailHistory, long> _billEmailRepos;
        private readonly IRepository<Booking, long> _bookingRepos;
        private readonly IRepository<BillConfig, long> _billConfigRepo;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<Citizen, long> _citizenRepo;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly BillUtilAppService _billUtilAppService;
        private readonly IUserBillRealtimeNotifier _userBillRealtimeNotifier;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IBillEmailUtil _billEmailUtil;
        private readonly IAppNotifier _appNotifier;
        private readonly HandlePaymentUtilAppService _handlePaymentUtilAppService;

        public UserBillAppService(
            IBillEmailUtil billEmailUtil,
            IRepository<UserBill, long> userBillRepo,
            IRepository<Booking, long> bookingRepo,
            IRepository<BillConfig, long> billConfigRepo,
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<Citizen, long> citizenRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IRepository<BillEmailHistory, long> billEmailRepos,
            BillUtilAppService billUtilAppService,
            IUserBillRealtimeNotifier userBillRealtimeNotifier,
            IOnlineClientManager onlineClientManager,
            IAppNotifier appNotifier,
            HandlePaymentUtilAppService handlePaymentUtilAppService
        )
        {
            _billEmailUtil = billEmailUtil;
            _userBillRepo = userBillRepo;
            _bookingRepos = bookingRepo;
            _billConfigRepo = billConfigRepo;
            _userBillPaymentRepo = userBillPaymentRepo;
            _citizenRepo = citizenRepo;
            _billUtilAppService = billUtilAppService;
            _userBillRealtimeNotifier = userBillRealtimeNotifier;
            _onlineClientManager = onlineClientManager;
            _appNotifier = appNotifier;
            _billDebtRepo = billDebtRepo;
            _billEmailRepos = billEmailRepos;
            _handlePaymentUtilAppService = handlePaymentUtilAppService;
        }

        public async Task<object> GetBillEmailTemplateAsync(GetAllBillEmailDto input)
        {
            try
            {       

                var template = await _billEmailUtil.GetTemplateByApartment(input.ApartmentCode, input.Period);
                var result = new BillEmailHistory()
                {
                    ApartmentCode = input.ApartmentCode,
                    Period = input.Period,
                    EmailTemplate = template.ToString().Replace("OCTYPE html>", "")
                };
                var data = DataResult.ResultSuccess(result, "Get success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllBillConfigs(GetAllBillConfigInputDto input)
        {
            try
            {
                var result = await _billConfigRepo.GetAll()
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Title.Contains(input.Keyword))
                    .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType)
                    .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId)
                    .WhereIf(input.Level.HasValue, x => x.Level == input.Level)
                    .ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllUserBillByMonth(UserGetBillByMonthInput input)
        {

            try
            {
                var query = _userBillRepo.GetAll()
                     .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType)
                     .Where(x => x.ApartmentCode == input.ApartmentCode);

                switch (input.FormId)
                {
                    case UserBillFormId.Pending_WaitForConfirm:
                        query = query.Where(x =>
                            x.Status == UserBillStatus.Pending || x.Status == UserBillStatus.WaitForConfirm || x.IsPaymentPending == true);
                        break;
                    case UserBillFormId.Paid:
                        query = query.Where(x => x.Status == UserBillStatus.Paid && x.IsPaymentPending != true);
                        break;
                    case UserBillFormId.Debt:
                        query = query.Where(x => x.Status == UserBillStatus.Debt);
                        break;
                    default:
                        break;
                }

                var result = query
                    .AsQueryable().AsEnumerable().GroupBy(x => new
                        { x.Period.Value.Month, x.Period.Value.Year, x.ApartmentCode })
                    .Select(x => new
                    {
                        Key = string.Format("{0}/{1}", x.Key.Year, x.Key.Month < 10 ? "0" + x.Key.Month : x.Key.Month),
                        Value = x.ToList()
                    }).ToDictionary(x => x.Key, y => y.Value)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToList();

                foreach(var item in result)
                {
                    foreach(var bill in item.Value)
                    {
                        if (bill.IsPaymentPending == true) bill.Status = UserBillStatus.WaitForConfirm;
                    }
                }

                var data = DataResult.ResultSuccess(result, "Get success", result.Count());
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllUserBills(GetAllUserBillsInputDto input)
        {
            try
            {
                // var billConfigIds = new List<long>();
                // if (input.BillType.HasValue)
                // {
                //     billConfigIds = await _billConfigRepo.GetAll()
                //         .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType)
                //         .Select(x => x.Id).ToListAsync();
                // }

                var apartmentCodes = GetDepartmentCodeByUser();
                if (!apartmentCodes.Any())
                {
                    return DataResult.ResultSuccess(null, "Căn hộ không tồn tại hoặc chưa xác minh !");
                }

                var query = _userBillRepo.GetAll()
                    .WhereIf(input.Ids != null && input.Ids.Length > 0, x => input.Ids.Contains(x.Id))
                    .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType)
                    .WhereIf(apartmentCodes != null, x => apartmentCodes.Contains(x.ApartmentCode))
                    .WhereIf(input.ApartmentCode != null && input.ApartmentCode.Length > 0,
                        x => x.ApartmentCode.Contains(input.ApartmentCode))
                    .WhereIf(!input.Keyword.IsNullOrEmpty(),
                        x => x.Title.Contains(input.Keyword) || x.Code.Contains(input.Keyword))
                    .WhereIf(input.PeriodFrom.HasValue, x => x.Period >= input.PeriodFrom)
                    .WhereIf(input.PeriodTo.HasValue, x => x.Period <= input.PeriodTo)
                    //.WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                    .WhereIf(input.DueDateFrom.HasValue, x => x.DueDate >= input.DueDateFrom)
                    .WhereIf(input.DueDateTo.HasValue, x => x.DueDate <= input.DueDateTo);

                switch (input.FormId)
                {
                    case UserBillFormId.Pending_WaitForConfirm:
                        query = query.Where(x =>
                            x.Status == UserBillStatus.Pending || x.Status == UserBillStatus.WaitForConfirm);
                        break;
                    case UserBillFormId.Paid:
                        query = query.Where(x => x.Status == UserBillStatus.Paid);
                        break;
                    case UserBillFormId.Debt:
                        query = query.Where(x => x.Status == UserBillStatus.Debt);
                        break;
                    default:
                        break;
                }

                query = query.OrderByDescending(x => x.Period);

                var result = await query.PageBy(input).ToListAsync();

                // Tính tiền từ server
                // foreach (var item in result)
                // {
                //     var costResult = await _billUtilAppService.CalculateUserBill(item);
                //     item.Cost = costResult.Cost;
                //     item.LastCost = costResult.LastCost;
                //     item.Surcharges = costResult.Surcharges;
                // }

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                return null;
            }
        }

        public async Task<object> GetUserBillByIdAsync(long id)
        {
            try
            {
                var data = await _userBillRepo.GetAsync(id);
                var ub = data.MapTo<UserBillDto>();
                return DataResult.ResultSuccess(ub, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> GetMonthPrepayment(GetMonthPrepaymentDto input)
        {
            try
            {
                var data = await _userBillRepo.GetAll()
                    .Where(x => x.ApartmentCode == input.ApartmentCode && x.UrbanId == input.UrbanId
                    && x.BuildingId == input.BuildingId && x.BillType == input.BillType 
                    && x.Status == UserBillStatus.Paid && x.IsPrepayment == true && x.IsPaymentPending != true)
                    .Select(x => x.Period.Value).OrderByDescending(x => x).FirstOrDefaultAsync();
              
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetPaymentHistories(GetPaymentHistoriesInputDto input)
        {
            try
            {
                var query = _userBillPaymentRepo.GetAll()
                    .Where(x => x.CreatorUserId == AbpSession.UserId)
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
                    .WhereIf(input.Keyword != null && input.Keyword.Length > 0, x => x.Title.Contains(input.Keyword))
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                    .WhereIf(input.PaymentDateFrom.HasValue, x => x.CreationTime >= input.PaymentDateFrom)
                    .WhereIf(input.PaymentDateTo.HasValue, x => x.CreationTime <= input.PaymentDateTo);
                var paymentHistories = await query.PageBy(input).ToListAsync();

                foreach (var p in paymentHistories)
                {
                    if (p.UserBillIds.Any())
                    {
                        var userBillIds = p.UserBillIds.Split(',').Select(x => long.Parse(x)).ToList();
                        var userBills = await _userBillRepo.GetAll().Where(x => userBillIds.Contains(x.Id))
                            .ToArrayAsync();
                        p.UserBills = userBills;
                    }
                }

                var data = DataResult.ResultSuccess(paymentHistories, "Get success", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                return null;
            }
        }

        public async Task<object> PaymentUserBillMonthly(PayMonthlyUserBillsInput input)
        {
            try
            {
                input.Status = UserBillPaymentStatus.Pending;
                await _handlePaymentUtilAppService.PayMonthlyUserBillByApartment(input);

                var data = DataResult.ResultSuccess("Admin payment success");
                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> HandleUserBillDirect(HandlePayUserBillDirectInputDto input)
        {
            try
            {
                var request = input.MapTo<MappingRequestPaymentDto>();

                if (input.TypePayment == TypePayment.DebtBill)
                {

                    var billdebts = await _billDebtRepo.GetAllListAsync(x => input.BillDebtIds.Contains(x.Id));

                    if (billdebts.Any(x => x.State == DebtState.WAITFORCONFIRM)) throw new Exception("Bill debt is wait for confirm !");
                    var dt = await RequestPaymentBillDebt(request);
                    foreach (var billdebt in billdebts)
                    {
                        billdebt.State = DebtState.WAITFORCONFIRM;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    return dt;
                }


                if (input.UserBillIds != null && input.UserBillIds.Count() > 0)
                {
                    var userBills = _userBillRepo.GetAll()
                        .Where(x => input.UserBillIds.Contains(x.Id))
                        .Where(x => x.Status == UserBillStatus.Pending)
                        .ToList();
                    foreach (var userBill in userBills)
                    {
                        userBill.Status = UserBillStatus.WaitForConfirm;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                if (input.UserBillDebtIds != null && input.UserBillDebtIds.Count() > 0)
                {
                    var userBills = _userBillRepo.GetAll()
                        .Where(x => input.UserBillDebtIds.Contains(x.Id))
                        .Where(x => x.Status == UserBillStatus.Debt)
                        .ToList();
                    foreach (var userBill in userBills)
                    {
                        userBill.Status = UserBillStatus.WaitForConfirm;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                var result = await RequestPaymentUserBill(request);


                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [RemoteService(false)]
        public async Task HandlePaymentForMomo(HandlePayUserBillInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    // Check status of request payment
                    var userBillPayment = await _userBillPaymentRepo.FirstOrDefaultAsync(x => x.Id == input.PaymentId);

                    // Make csv of input.userBillIds
                    var csvUserBillIds = string.Join(",", input.UserBillIds.OrderBy(x => x));
                    if (userBillPayment is not { Status: UserBillPaymentStatus.Pending } ||
                        userBillPayment.UserBillIds != csvUserBillIds)
                    {
                        throw new Exception("Payment is not pending");
                    }

                    if (input.ResultCode != 0)
                    {
                        userBillPayment.Status = UserBillPaymentStatus.Fail;
                        await CurrentUnitOfWork.SaveChangesAsync();

                        throw new Exception("Momo payment failed");
                    }

                    if (input.TypePayment == TypePayment.BookingLocalService)
                    {
                        await HanlePaymentBookingLocalService(input, userBillPayment);
                    }
                    else if (input.TypePayment == TypePayment.DebtBill)
                    {
                        await HandlePaymentBillDebt(input, userBillPayment);
                    }
                    else
                    {
                        await HandlePaymentUserBill(input, userBillPayment);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [RemoteService(false)]
        public async Task HandlePaymentForVNPay(HandlePayUserBillInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    // Check status of request payment
                    var userBillPayment = await _userBillPaymentRepo.FirstOrDefaultAsync(x => x.Id == input.PaymentId);

                    // Check VNPay exceptions
                    if (userBillPayment == null)
                    {
                        throw new Exception(VnPayLib.ORDER_NOT_FOUND_CODE);
                    }

                    var newProperties =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(input.Properties!);
                    var oldProperties =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(userBillPayment.Properties!);

                    var secureHash1 = newProperties![VnPayLib.VNP_SECURE_HASH_KEY].ToString();
                    newProperties.Remove(VnPayLib.VNP_SECURE_HASH_KEY);
                    // Remove all key with null value of newProperties
                    newProperties = newProperties.Where(x => x.Value != null)
                        .ToDictionary(x => x.Key, x => x.Value);

                    var secretKey = oldProperties![VnPayLib.VNP_SECRET_KEY_KEY].ToString();
                    var queryString = string.Join("&",
                            newProperties.OrderBy(x => x.Key)
                                .Select(x => $"{x.Key}={x.Value}"))
                        .Replace("{", "%7B")
                        .Replace("}", "%7D")
                        .Replace(" ", "%22")
                        .Replace(":", "%3A")
                        .Replace("[", "%5B")
                        .Replace("]", "%5D")
                        .Replace("\"", "%22")
                        .Replace("/", "%2F")
                        .Replace(",", "%2C");
                    var secureHash2 = VnPayUtils.HmacSHA512(secretKey, queryString);
                    if (secureHash1 != secureHash2)
                    {
                        throw new Exception(VnPayLib.INVALID_SIGNATURE_CODE);
                    }

                    // Make csv of input.userBillIds
                    var csvUserBillIds = string.Join(",", input.UserBillIds.OrderBy(x => x));


                    if (newProperties!["vnp_TxnRef"].ToString() != oldProperties!["vnp_TxnRef"].ToString())
                    {
                        throw new Exception(VnPayLib.ORDER_NOT_FOUND_CODE);
                    }


                    if (newProperties["vnp_ResponseCode"].ToString() != "00" ||
                        newProperties["vnp_TransactionStatus"].ToString() != "00")
                    {
                        userBillPayment.Status = UserBillPaymentStatus.Fail;
                        await _userBillPaymentRepo.UpdateAsync(userBillPayment);

                        await CurrentUnitOfWork.SaveChangesAsync();
                    }

                    // Get all userBills from input.userBillIds
                    var userBills = await _userBillRepo.GetAll()
                        .Where(x => input.UserBillIds.Contains(x.Id))
                        .ToListAsync();

                    // Check cost (sum of amount) of bills
                    var totalCost = userBills.Sum(x => x.LastCost);
                    if (totalCost == null)
                    {
                        throw new Exception(VnPayLib.INVALID_AMOUNT_CODE);
                    }

                    if (userBillPayment is not { Status: UserBillPaymentStatus.Pending } ||
                        userBillPayment.UserBillIds != csvUserBillIds ||
                        userBills.Any(x => x.Status != UserBillStatus.Pending))
                    {
                        throw new Exception(VnPayLib.ORDER_ALREADY_CONFIRMED_CODE);
                    }


                    // Check bills are valid?
                    if (userBills.Count != input.UserBillIds.Length || !userBills.Any())
                    {
                        throw new Exception(VnPayLib.UNKNOWN_ERROR_CODE);
                    }

                    var nowTime = DateTime.Now;
                    userBillPayment.Title = userBills.Count == 1
                        ? userBills[0].Title
                        : "Hoá đơn ngày " + nowTime.ToString("dd/MM/yyyyy");
                    userBillPayment.Status = UserBillPaymentStatus.Success;
                    await _userBillPaymentRepo.UpdateAsync(userBillPayment);

                    try
                    {
                        await NotifierBillPaymentSuccess(input);
                    }
                    catch
                    {
                    }

                    if (totalCost != input.Amount)
                    {
                        await HandleUserBillDebt(userBills, input.Amount, userBillPayment.Id);
                    }
                    else
                    {
                        foreach (var userBill in userBills)
                        {
                            userBill.Status = UserBillStatus.Paid;
                        }

                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        #region Xu ly hoa don

        private async Task<object> RequestPaymentUserBill(MappingRequestPaymentDto input)
        {
            string csvUserBillIds = null;
            string userBillDebtIds = null;
            if (input.UserBill == null && input.UserBillIds != null && input.UserBillIds.Count() > 0)
            {
                input.UserBill = await _userBillRepo.GetAll()
                    .Where(x => input.UserBillIds.Contains(x.Id))
                    .FirstOrDefaultAsync();
                csvUserBillIds = string.Join(",", input.UserBillIds.OrderBy(x => x));

            }
            else if (input.UserBill == null && input.UserBillDebtIds != null && input.UserBillDebtIds.Count() > 0)
            {
                input.UserBill = await _userBillRepo.GetAll()
                    .Where(x => input.UserBillDebtIds.Contains(x.Id))
                    .FirstOrDefaultAsync();
                userBillDebtIds = string.Join(",", input.UserBillDebtIds.OrderBy(x => x));
            }

            var result = new UserBillPayment
            {
                TenantId = CurrentUnitOfWork.GetTenantId(),
                Title = input.Title ?? "Thanh toán hoá đơn ngày " + DateTime.Now.ToString("dd/MM/yyyyy"),
                UserBillIds = csvUserBillIds,
                UserBillDebtIds = userBillDebtIds,
                Method = input.Method > 0 ? input.Method: UserBillPaymentMethod.Banking,
                Status = UserBillPaymentStatus.Pending,
                Properties = input.Properties,
                Period = input.Period,
                Amount = input.Amount,
                TypePayment = input.TypePayment,
                ImageUrl = input.ImgUrls,
                FileUrl = input.FileUrls,
                Description = input.Description,
                OrganizationUnitId = input.OrganizationUnitId,
                ApartmentCode = input.UserBill.ApartmentCode,
                UrbanId = input.UserBill.UrbanId,
                BuildingId = input.UserBill.BuildingId
            };
            await _userBillPaymentRepo.InsertAndGetIdAsync(result);
            //await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }

        private async Task HandlePaymentUserBill(HandlePayUserBillInputDto input, UserBillPayment payment)
        {
            // Get all userBills from input.userBillIds
            var userBills = await _userBillRepo.GetAll()
                .Where(x => input.UserBillIds.Contains(x.Id))
                .ToListAsync();

            // Check bills are valid?
            if (userBills.Count != input.UserBillIds.Length || !userBills.Any())
            {
                throw new Exception("Bill not found");
            }

            // Check cost (sum of amount) of bills
            var totalCost = userBills.Sum(x => x.LastCost);
            if (totalCost == null)
            {
                throw new Exception("Invalid amount");
            }

            var nowTime = DateTime.Now;
            payment.Title = userBills.Count == 1
                ? userBills[0].Title
                : "Hoá đơn ngày " + nowTime.ToString("dd/MM/yyyyy");

            payment.Status = UserBillPaymentStatus.Success;
            await _userBillPaymentRepo.UpdateAsync(payment);

            try
            {
                await NotifierBillPaymentSuccess(input);
            }
            catch
            {
            }

            if (totalCost != input.Amount)
            {
                await HandleUserBillDebt(userBills, input.Amount, payment.Id);
            }
            else
            {
                // Update status of bills to paid
                foreach (var userBill in userBills)
                {
                    userBill.Status = UserBillStatus.Paid;
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        #endregion


        #region Xu ly cong no

        private async Task<object> RequestPaymentBillDebt(MappingRequestPaymentDto input)
        {
            // Get all userBills from input.userBillIds
            if (input.BillDebtIds == null || input.BillDebtIds.Length == 0) throw new Exception("Bill debt not found");
            var billdebts = await _billDebtRepo.GetAllListAsync(x => input.BillDebtIds.Contains(x.Id));
            if (billdebts == null || billdebts.Count() == 0)
            {
                throw new Exception("Bill debt not found");
            }

            var userBillIds = billdebts.SelectMany(x => x.UserBillIds.Split(",").Select(x => long.Parse(x)).ToList())
                .Distinct().ToList();
            var userBills = _userBillRepo.GetAll()
                .Where(x => userBillIds.Contains(x.Id))
                .Where(x => x.Status == UserBillStatus.Debt)
                .ToList();

            var total = billdebts.Sum(x => x.DebtTotal);
            if ((int)input.Amount != (int)total) throw new Exception("Invalid amount");

            var result = new UserBillPayment
            {
                TenantId = CurrentUnitOfWork.GetTenantId(),
                Title = input.Title,
                BillDebtIds = string.Join(",", input.BillDebtIds),
                Method = input.Method,
                Status = UserBillPaymentStatus.Pending,
                Properties = input.Properties,
                Period = DateTime.Now,
                Amount = input.Amount,
                TypePayment = input.TypePayment,
                UserBillIds = string.Join(",", userBillIds),
                ImageUrl = input.ImgUrls,
                FileUrl = input.FileUrls,
                Description = input.Description,
                OrganizationUnitId = input.OrganizationUnitId,
                ApartmentCode = billdebts[0].ApartmentCode,
                UrbanId = userBills[0].UrbanId,
                BuildingId = userBills[0].BuildingId
            };
            var id = await _userBillPaymentRepo.InsertAndGetIdAsync(result);
            await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }

        private async Task HandlePaymentBillDebt(HandlePayUserBillInputDto input, UserBillPayment payment)
        {
            if (input.BillDebtIds == null || input.BillDebtIds.Length == 0) throw new Exception("Bill debt not found");
            var billdebts = await _billDebtRepo.GetAllListAsync(x => input.BillDebtIds.Contains(x.Id));
            if (billdebts == null || billdebts.Count() == 0)
            {
                throw new Exception("Bill debt not found");
            }

            var total = billdebts.Sum(x => x.DebtTotal);

            if ((int)input.Amount != (int)total) throw new Exception("Invalid amount");

            var userBillIds = billdebts.SelectMany(x => x.UserBillIds.Split(",").Select(x => long.Parse(x)).ToList())
                .Distinct().ToList();
            var userBills = _userBillRepo.GetAll()
                .Where(x => userBillIds.Contains(x.Id))
                .Where(x => x.Status == UserBillStatus.Debt)
                .ToList();

            //// Check bills are valid?
            //if (userBills.Count != input.UserBillIds.Length || !userBills.Any())
            //{
            //    throw new Exception("Bill not found");
            //}

            // Check cost (sum of amount) of bills

            var nowTime = DateTime.Now;
            payment.Title = "Thanh toán công nợ ngày " + nowTime.ToString("dd/MM/yyyyy");

            payment.Status = UserBillPaymentStatus.Success;
            await _userBillPaymentRepo.UpdateAsync(payment);

            try
            {
                await NotifierBillPaymentSuccess(input);
            }
            catch
            {
            }

            foreach (var userBill in userBills)
            {
                userBill.Status = UserBillStatus.Paid;
            }

            foreach (var billdebt in billdebts)
            {
                billdebt.State = DebtState.PAIED;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }


        private async Task HandleUserBillDebt(List<UserBill> bills, double amount, long paymnentId)
        {
            try
            {
                var userBillIds = bills.Select(x => x.Id).ToList();
                if (bills == null || bills.Count == 0)
                {
                    return;
                }

                var totalCost = bills.Sum(x => x.LastCost);
                if (totalCost == 0)
                {
                    return;
                }

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

        #endregion

        #region Xu ly tien ich noi khu

        private async Task HanlePaymentBookingLocalService(HandlePayUserBillInputDto input,
            UserBillPayment userBillPayment)
        {
            //Get all booking local service from input.userBillIds
            var bookingLocalServices = await _bookingRepos.GetAll()
                .Where(x => input.UserBillIds.Contains(x.Id))
                .Where(x => x.State == StateBooking.Requesting)
                .ToListAsync();

            //Check booking are valids
            if (bookingLocalServices.Count != input.UserBillIds.Length || !bookingLocalServices.Any())
            {
                throw new Exception("Booking not found");
            }

            //Check cost (sum of amount) of bills 
            var totalAmount = bookingLocalServices.Sum(x => x.Amount);
            if (totalAmount == null || Math.Abs((double)totalAmount - input.Amount) > 0.0001)
            {
                throw new Exception("Invalid Amount");
            }

            await _userBillPaymentRepo.UpdateAsync(userBillPayment);

            try
            {
                await NotifierBillPaymentSuccess(input);
            }
            catch
            {
            }

            // Update status of bills to paid
            foreach (var bookingLocalService in bookingLocalServices)
            {
                bookingLocalService.State = StateBooking.Accepted;
                bookingLocalService.BookingCode = GetUniqueKey(4) + bookingLocalService.Id;
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Common

        protected List<string> GetDepartmentCodeByUser()
        {
            // var result = new List<string>();
            var result = (from cz in _citizenRepo.GetAll()
                where cz.AccountId == AbpSession.UserId && cz.State == STATE_CITIZEN.ACCEPTED
                select cz.ApartmentCode).AsQueryable().ToList();

            return result;
        }


        private async Task NotifierBillPaymentSuccess(HandlePayUserBillInputDto bill)
        {
            var method = L(nameof(bill.PaymentMethod));
            var detailUrlApp = $"yoolife://app/receipt?apartmentCode={bill.ApartmentCode}&formId=2";
            var detailUrlWA = $"/monthly?apartmentCode={bill.ApartmentCode}&formId=2";
            var messageSuccess = new UserMessageNotificationDataBase(
                AppNotificationAction.BillPaymentSuccess,
                AppNotificationIcon.BillPaymentSuccessIcon,
                TypeAction.Detail,
                $"Thanh toán hóa đơn {method} thành công ! Tổng số tiền đã thanh toán : {bill.Amount} VNĐ",
                detailUrlApp,
                detailUrlWA
            );

            await _appNotifier.SendMessageNotificationInternalAsync(
                $"Thông báo thanh toán hóa đơn {method}!",
                $"Bạn đã thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {bill.Amount} VNĐ",
                detailUrlApp,
                detailUrlWA,
                new UserIdentifier[] { new UserIdentifier(bill.TenantId, bill.UserId) },
                messageSuccess,
                AppType.USER
                );

            // await _appNotifier.SendUserMessageNotifyFireBaseAsync(
            //     $"Thông báo thanh toán hóa đơn {method}!",
            //     $"Bạn đã thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {bill.Amount} VNĐ",
            //     detailUrlApp,
            //     detailUrlWA,
            //     new UserIdentifier[] { new UserIdentifier(bill.TenantId, bill.UserId) },
            //     messageSuccess);
        }

        private async Task<object> BookingServicePaymentHandle(MappingRequestPaymentDto input)
        {
            try
            {
                // Get all userBills from input.userBillIds
                var bookingServices = await _bookingRepos.GetAll()
                    .Where(x => input.UserBillIds.Contains(x.Id))
                    .ToListAsync();

                // Check if userBills is valid
                if (!bookingServices.Any())
                {
                    throw new Exception("Booking Service not found");
                }

                // Check if userBills is valid
                if (bookingServices.Any(x => x.State != StateBooking.Requesting))
                {
                    throw new Exception("Booking Service is invalid");
                }

                // make csv of input.userBillIds
                var csvUserBillIds = string.Join(",", input.UserBillIds.OrderBy(x => x));
                // Check if the payment is pending
                var userBillPayments = await _userBillPaymentRepo.GetAll()
                    .Where(x => x.UserBillIds == csvUserBillIds)
                    .ToListAsync();
                if (userBillPayments.Any(x
                        => x.Status == UserBillPaymentStatus.Pending))
                {
                    return userBillPayments[0];
                }

                var result = new UserBillPayment
                {
                    TenantId = CurrentUnitOfWork.GetTenantId(),
                    Title = input.Title.IsNullOrEmpty() ? input.Title : "Thanh toán dịch vụ",
                    UserBillIds = csvUserBillIds,
                    Method = input.Method,
                    Status = UserBillPaymentStatus.Pending,
                    Properties = input.Properties,

                    Period = DateTime.Now,
                    Amount = input.Amount,
                    TypePayment = input.TypePayment,
                    ImageUrl = input.ImgUrls,
                    FileUrl = input.FileUrls,
                    Description = input.Description,
                    OrganizationUnitId = input.OrganizationUnitId,
                    ApartmentCode = bookingServices[0].BookingCode
                };
                await _userBillPaymentRepo.InsertAndGetIdAsync(result);
                await CurrentUnitOfWork.SaveChangesAsync();


                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message, e);
                throw new Exception(e.Message);
            }
        }

        #endregion
    }
}