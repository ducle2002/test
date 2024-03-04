using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Yootek.Authorization.Users;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Notifications;
using Yootek.Services;
using Newtonsoft.Json;
using Abp.Application.Services;
using Abp.Domain.Entities;
using YOOTEK.EntityDb;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Payment
{
    public class HandlePaymentUtilAppService : YootekAppServiceBase
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<UserBillPaymentValidation, long> _userBillPaymentValidationRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IRepository<UserBillPaymentHistory, long> _billPaymentHistoryRepos;
        private readonly IRepository<BillStatistic, long> _billStatisticRepos;
        private readonly IRepository<Apartment, long> _apartmentRepos;
        private readonly IRepository<ApartmentBalance, long> _apartmentBalanceRepos;
        private readonly ApartmentHistoryAppService _apartmentHistoryAppSerivce;

        public HandlePaymentUtilAppService(
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<UserBillPaymentValidation, long> userBillPaymentValidationRepo,
            IRepository<User, long> userRepos, IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IAppNotifier appNotifier,
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<UserBillPaymentHistory, long> billPaymentHistoryRepos,
            IRepository<BillStatistic, long> billStatisticRepos,
            ApartmentHistoryAppService apartmentHistoryAppSerivce,
            IRepository<Apartment, long> apartmentRepos,
            IRepository<ApartmentBalance, long> apartmentBalanceRepos
            )

        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userRepos = userRepos;
            _userBillRepo = userBillRepo;
            _appNotifier = appNotifier;
            _citizenTempRepos = citizenTempRepos;
            _billDebtRepo = billDebtRepo;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _billStatisticRepos = billStatisticRepos;
            _billPaymentHistoryRepos = billPaymentHistoryRepos;
            _apartmentHistoryAppSerivce = apartmentHistoryAppSerivce;
            _apartmentRepos = apartmentRepos;
            _userBillPaymentValidationRepo = userBillPaymentValidationRepo;
            _apartmentBalanceRepos = apartmentBalanceRepos;
        }

        [RemoteService(false)]
        public async Task<UserBillPayment> PayMonthlyUserBillByApartment(PayMonthlyUserBillsInput input, int? epaymentId = null)
        {
            using(CurrentUnitOfWork.SetTenantId(input.UserBill.TenantId))
            {
                try
                {
                    if ((input.UserBills == null || input.UserBills.Count() == 0)
                        && (input.UserBillDebts == null || input.UserBillDebts.Count() == 0)
                        && (input.PrepaymentBills == null || input.PrepaymentBills.Count() == 0)) throw new Exception("Input user bill is null");
                    if(epaymentId.HasValue)
                    {
                        var checkPayment = await _userBillPaymentRepo.FirstOrDefaultAsync(x => x.EPaymentId == epaymentId);
                        if (checkPayment != null) return checkPayment;
                    }

                    var payment = new UserBillPayment()
                    {
                        Amount = input.Amount,
                        ApartmentCode = input.ApartmentCode,
                        Method = CheckMethod(input.Method),
                        Status = input.Status ?? UserBillPaymentStatus.Pending,
                        TypePayment = TypePayment.Bill,
                        Period = input.Period,
                        Title = "Thanh toán hóa đơn tháng " + input.Period.ToString("MM/yyyyy"),
                        TenantId = input.UserBill.TenantId,
                        Description = input.Description,
                        BuildingId = input.UserBill.BuildingId,
                        UrbanId = input.UserBill.UrbanId,
                        FileUrl = input.FileUrl,
                        ImageUrl = input.ImageUrl,
                        CreationTime = input.CreationTime,
                        EPaymentId = epaymentId
                    };

                    bool isPaymentDebt = true;

                    var billPaymentInfo = new BillPaymentInfo();

                    // Handle billDebt
                    var listBills = new List<BillPaidInfoDto>();

                    // List bill balance
                    var billBalances = new List<BillPaymentBalanceDto>();
                    if(input.BalanceAmount > 0)
                    {
                        var balance = new BillPaymentBalanceDto()
                        {
                            Amount = input.BalanceAmount,
                            ApartmentCode = input.UserBill.ApartmentCode,
                            BuildingId = input.UserBill.BuildingId,
                            CitizenTempId = input.UserBill.CitizenTempId,
                            EBalanceAction = EBalanceAction.Add,
                            TenantId = input.UserBill.TenantId,
                            UrbanId = input.UserBill.UrbanId,
                            UserBillId = input.UserBill.Id
                        };
                        billBalances.Add(balance);
                    }

                    if (input.UserBillDebts != null && input.UserBillDebts.Count() > 0)
                    {
                        var res = await HandlePayUserBillDebts(input.UserBillDebts, payment);
                        billPaymentInfo.BillListDebt = res.Item1;
                        listBills.AddRange(res.Item2);
                        payment.UserBillDebtIds = string.Join(",", res.Item1.Select(x => x.Id).OrderBy(x => x));

                        billBalances = billBalances.Concat(res.Item3).ToList();
                        isPaymentDebt = true;
                    }

                    // Handle Userbill
                    if (input.UserBills != null && input.UserBills.Count() > 0)
                    {
                        var res = await HandlePayUserBillPendings(input.UserBills, payment);
                        billPaymentInfo.BillList = res.Item1;
                        listBills.AddRange(res.Item2);
                        payment.UserBillIds = string.Join(",", res.Item1.Select(x => x.Id).OrderBy(x => x));
                        billBalances = billBalances.Concat(res.Item3).ToList();
                        isPaymentDebt = false;
                    }

                    // Handle prepayment
                    if (input.PrepaymentBills != null && input.PrepaymentBills.Count > 0)
                    {
                        var res = await HandlePrepaymentVerifyPayment(input.PrepaymentBills, input.UserBill, payment);
                        billPaymentInfo.BillListPrepayment = res.Item1;
                        listBills.AddRange(res.Item2);
                        payment.UserBillPrepaymentIds = string.Join(",", res.Item1.Select(x => x.Id).OrderBy(x => x));
                    }

                    payment.BillPaymentInfo = JsonConvert.SerializeObject(billPaymentInfo);
                    if (!input.UserBill.Properties.IsNullOrEmpty())
                    {
                        try
                        {
                            var obj = JsonConvert.DeserializeObject<dynamic>(input.UserBill.Properties);
                            payment.CustomerName = obj.customerName;
                        }
                        catch { }
                    }
                    
                    
                    if(billBalances.Count() > 0)
                    {
                        await CreateApartmentBalances(billBalances);
                    }

                    if (isPaymentDebt) payment.TypePayment = TypePayment.DebtBill;
                    var id = await _userBillPaymentRepo.InsertAndGetIdAsync(payment);
                    payment.PaymentCode = "PM-" + id + "-" + GetUniqueKey(6);
                    await  CurrentUnitOfWork.SaveChangesAsync();


                    try
                    {
                        foreach (var item in listBills)
                        {
                            await CreateBillPaymentHistory(item, payment);
                        }
                        if (payment.Status == UserBillPaymentStatus.Success)
                        {
                            await NotifierBillPaymentSuccess(payment, (int)payment.Amount, payment.CreatorUserId.Value);
                            await CreateApartmentHistory(payment, input.UserBill);
                        }

                    }
                    catch
                    {
                    }

                    return payment;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        [RemoteService(false)]
        public async Task<UserBillPaymentValidation> RequestValidationPaymentByApartment(PayMonthlyUserBillsInput transactionProperties, int? tenantId)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var input = transactionProperties;
                    if ((input.UserBills == null || input.UserBills.Count() == 0)
                        && (input.UserBillDebts == null || input.UserBillDebts.Count() == 0)
                        && (input.PrepaymentBills == null || input.PrepaymentBills.Count() == 0)) throw new Exception("Input user bill is null");
                    var payment = new UserBillPaymentValidation()
                    {
                        Amount = input.Amount,
                        ApartmentCode = input.ApartmentCode,
                        Method = input.Method,
                        Status = UserBillPaymentStatus.RequestingThirdParty,
                        TypePayment = TypePayment.Bill,
                        Period = input.Period,
                        Title = "Thanh toán hóa đơn tháng " + input.Period.ToString("MM/yyyyy"),
                        TenantId = input.UserBill.TenantId,
                        BuildingId = input.UserBill.BuildingId,
                        UrbanId = input.UserBill.UrbanId,
                        TransactionProperties = JsonConvert.SerializeObject(transactionProperties)
                    };

                    bool isPaymentDebt = true;

                    var billPaymentInfo = new BillPaymentInfo();

                    // Handle billDebt
                    var listBills = new List<BillPaidInfoDto>();

                    if (input.UserBillDebts != null && input.UserBillDebts.Count() > 0)
                    {
                        var res = await ValidatePayUserBillDebt(input.UserBillDebts);
                        billPaymentInfo.BillListDebt = res.Item1;
                        listBills.AddRange(res.Item2);
                        payment.UserBillDebtIds = string.Join(",", res.Item1.Select(x => x.Id).OrderBy(x => x));
                        isPaymentDebt = true;
                    }

                    // Handle Userbill
                    if (input.UserBills != null && input.UserBills.Count() > 0)
                    {
                        var res = await ValidatePayUserBillPendings(input.UserBills);
                        billPaymentInfo.BillList = res.Item1;
                        listBills.AddRange(res.Item2);
                        payment.UserBillIds = string.Join(",", res.Item1.Select(x => x.Id).OrderBy(x => x));
                        isPaymentDebt = false;
                    }

                    if (isPaymentDebt) payment.TypePayment = TypePayment.DebtBill;
                    await _userBillPaymentValidationRepo.InsertAndGetIdAsync(payment);
                    return payment;

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [RemoteService(false)]
        public async Task UpdatePaymentSuccess(UserBillPayment payment)
        {

            var nowTime = DateTime.Now;
            payment.Status = UserBillPaymentStatus.Success;
            await _userBillPaymentRepo.UpdateAsync(payment);

            // Handle Userbill
            if (payment.UserBillIds != null)
            {
                var billIds = payment.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                var userBills = _userBillRepo.GetAll().Where(x => billIds.Contains(x.Id))
                  .ToList();

                foreach (var userBill in userBills)
                {
                    userBill.IsPaymentPending = false;
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
                    userBill.IsPaymentPending = false;
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }

            //Handle prepayment
            if (!payment.UserBillPrepaymentIds.IsNullOrEmpty())
            {
                var ids = payment.UserBillPrepaymentIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                var userBills = _userBillRepo.GetAll().Where(x => ids.Contains(x.Id))
                  .ToList();

                foreach (var userBill in userBills)
                {
                    userBill.IsPaymentPending = false;
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            try
            {
                await NotifierBillPaymentSuccess(payment, (int)payment.Amount, payment.CreatorUserId.Value);
            }
            catch
            {
            }
        }

        [RemoteService(false)]
        public async Task CancelPaymentUserBill(UserBillPayment payment)
        {

            var nowTime = DateTime.Now;
            payment.Status = UserBillPaymentStatus.Cancel;
            await _userBillPaymentRepo.UpdateAsync(payment);
            await HandleUserBillRecoverPayment(payment);

            try
            {
                await NotifierBillPaymentCancel(payment);
            }
            catch
            {
            }

        }
        
        [RemoteService(false)]
        public async Task HandleUserBillRecoverPayment(UserBillPayment payment)
        {
            if (payment.TypePayment == TypePayment.Bill || payment.TypePayment == TypePayment.DebtBill)
            {
                var billPaymentInfo = new BillPaymentInfo();
                if (!payment.BillPaymentInfo.IsNullOrEmpty())
                {
                    try
                    {
                        billPaymentInfo = JsonConvert.DeserializeObject<BillPaymentInfo>(payment.BillPaymentInfo);
                    }
                    catch { }
                }


                if (!payment.UserBillIds.IsNullOrEmpty())
                {

                    if (billPaymentInfo.BillList != null)
                    {
                        foreach (var bill in billPaymentInfo.BillList)
                        {
                            var userBill = _userBillRepo.FirstOrDefault(bill.Id);
                            if (userBill == null) continue;
                            userBill.Status = bill.Status;
                            userBill.DebtTotal = bill.DebtTotal;
                            userBill.IsPaymentPending = false;

                        }
                    }
                    else
                    {
                        var billIds = payment.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                        var bills = await _userBillRepo.GetAllListAsync(x => billIds.Contains(x.Id));
                        foreach (var bill in bills)
                        {
                            bill.Status = UserBillStatus.Pending;
                            bill.IsPaymentPending = false;
                        }
                    }
                }

                if (!payment.UserBillDebtIds.IsNullOrEmpty())
                {
                    if (billPaymentInfo.BillListDebt != null)
                    {
                        foreach (var bill in billPaymentInfo.BillListDebt)
                        {
                            var userBill = _userBillRepo.FirstOrDefault(bill.Id);
                            if (userBill == null) continue;
                            userBill.Status = bill.Status;
                            userBill.DebtTotal = bill.DebtTotal;
                            userBill.IsPaymentPending = false;
                        }
                    }
                    else
                    {
                        var ids = payment.UserBillDebtIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                        var debts = await _userBillRepo.GetAllListAsync(x => ids.Contains(x.Id));
                        foreach (var bill in debts)
                        {
                            bill.Status = UserBillStatus.Debt;
                            bill.IsPaymentPending = false;
                        }
                    }

                }

                if (!payment.UserBillPrepaymentIds.IsNullOrEmpty())
                {
                    var ids = payment.UserBillPrepaymentIds.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
                    await _userBillRepo.DeleteAsync(x => ids.Contains(x.Id));

                }

                try
                {
                    await _billPaymentHistoryRepos.DeleteAsync(x => x.PaymentId == payment.Id);
                }
                catch { }
                await CurrentUnitOfWork.SaveChangesAsync();

            }

        }     

        private async Task CreateApartmentHistory(UserBillPayment payment, UserBill userBill)
        {
            //tạo lịch sử căn hộ
            using(CurrentUnitOfWork.SetTenantId(payment.TenantId))
            {
                var newHistory = new CreateApartmentHistoryDto();
                newHistory.TenantId = AbpSession.TenantId;
                newHistory.ImageUrls = new List<string> { payment.ImageUrl };
                newHistory.ApartmentId = _apartmentRepos.FirstOrDefault(x => x.ApartmentCode == payment.ApartmentCode && x.UrbanId == payment.UrbanId && x.BuildingId == payment.BuildingId)?.Id ?? 0;
                newHistory.Title = $"Thanh toán hoá đơn mã {userBill.Code} tháng {userBill.Period.Value.ToString("MM/yyyy")}";
                newHistory.Type = EApartmentHistoryType.Service;
                var user = _userRepos.FirstOrDefault(AbpSession.UserId ?? 0);
                newHistory.ExecutorName = user.FullName;
                newHistory.DateStart = (DateTime)userBill.Period.Value;
                newHistory.DateEnd = (DateTime)payment.Period;
                newHistory.Cost = (long?)payment.Amount;
                await _apartmentHistoryAppSerivce.CreateApartmentHistoryAsync(newHistory);
            }
        }

        private async Task<Tuple<List<BillPaidDto>, List<BillPaidInfoDto>>> ValidatePayUserBillPendings(List<PayUserBillDto> userBills)
        {
            var listBills = new List<BillPaidDto>();
            var bills = new List<BillPaidInfoDto>();
            foreach (var bd in userBills)
            {
                var bill = await _userBillRepo.GetAsync(bd.Id);
                if (bill == null) throw new Exception("BillDebt is not found !");
                var billPaid = bill.MapTo<BillPaidDto>();
                var billInfo = bill.MapTo<BillPaidInfoDto>();
                billPaid.PayAmount = bd.PayAmount;
                if ((int)billInfo.LastCost == (int)(bd.PayAmount))
                {
                    billInfo.Status = UserBillStatus.Paid;
                    billInfo.DebtTotal = 0;
                }
                else if ((int)billInfo.LastCost > (int)(bd.PayAmount))
                {
                    billInfo.DebtTotal = (int)billInfo.LastCost - (int)bd.PayAmount;
                    billInfo.Status = UserBillStatus.Debt;
                }
              
                billInfo.PayAmount = billPaid.PayAmount;
                listBills.Add(billPaid);
                bills.Add(billInfo);
            }

            return new Tuple<List<BillPaidDto>, List<BillPaidInfoDto>>(listBills, bills);
        }

        private async Task<Tuple<List<BillPaidDto>, List<BillPaidInfoDto>>> ValidatePayUserBillDebt(List<PayUserBillDto> userBills)
        {
            var listBills = new List<BillPaidDto>();
            var bills = new List<BillPaidInfoDto>();
            foreach (var bd in userBills)
            {
                var bill = await _userBillRepo.GetAsync(bd.Id);
                if (bill == null) throw new Exception("BillDebt is not found !");
                var billPaid = bill.MapTo<BillPaidDto>();
                var billInfo = bill.MapTo<BillPaidInfoDto>();
                billPaid.PayAmount = bd.PayAmount;
                if (billInfo.DebtTotal == null || billInfo.DebtTotal == 0) billInfo.DebtTotal = (decimal)billInfo.LastCost;
                if ((int)billInfo.DebtTotal == (int)(bd.PayAmount))
                {
                    billInfo.Status = UserBillStatus.Paid;
                    billInfo.DebtTotal = 0;
                }
                else if ((int)billInfo.DebtTotal > (int)(bd.PayAmount))
                {
                    billInfo.DebtTotal = (int)billInfo.DebtTotal - (int)bd.PayAmount;
                    billInfo.Status = UserBillStatus.Debt;
                }
             

             
                billInfo.PayAmount = billPaid.PayAmount;
                listBills.Add(billPaid);
                bills.Add(billInfo);
            }

            return new Tuple<List<BillPaidDto>, List<BillPaidInfoDto>>(listBills, bills);
        }

        private async Task<Tuple<List<BillPaidDto>, List<BillPaidInfoDto>, List<BillPaymentBalanceDto>>> HandlePayUserBillPendings(List<PayUserBillDto> userBills, UserBillPayment payment)
        {
            var listBills = new List<BillPaidDto>();
            var bills = new List<BillPaidInfoDto>();
            var balances = new List<BillPaymentBalanceDto>();

            foreach (var bd in userBills)
            {
                var bill = await _userBillRepo.GetAsync(bd.Id);
                if (bill == null) throw new Exception("BillDebt is not found !");

                var billPaid = ObjectMapper.Map<BillPaidDto>(bill);
                billPaid.PayAmount = bd.PayAmount;
                var lastCost = DecimalRoudingUp(bill.LastCost.Value);
                var payAmount = DecimalRoudingUp(bd.PayAmount);

                if (lastCost == payAmount)
                {
                    bill.Status = UserBillStatus.Paid;
                    bill.DebtTotal = 0;
                }
                else if (lastCost > payAmount)
                {
                    bill.DebtTotal = (int)bill.LastCost - (int)bd.PayAmount;
                    bill.Status = UserBillStatus.Debt;
                }
                else
                {
                    var balance = new BillPaymentBalanceDto()
                    {
                        Amount = payAmount - lastCost,
                        ApartmentCode = bill.ApartmentCode,
                        BillType = bill.BillType,
                        BuildingId = bill.BuildingId,
                        CitizenTempId = bill.CitizenTempId,
                        EBalanceAction = EBalanceAction.Add,
                        TenantId = bill.TenantId,
                        UrbanId = bill.UrbanId,
                        UserBillId = bill.Id
                    };
                    balances.Add(balance);
                }

                if (payment.Status == UserBillPaymentStatus.Pending)
                {
                    bill.IsPaymentPending = true;
                }else
                {
                    bill.IsPaymentPending = false;
                }
                var billInfo = ObjectMapper.Map<BillPaidInfoDto>(bill);
                billInfo.PayAmount = billPaid.PayAmount;
                listBills.Add(billPaid);
                bills.Add(billInfo);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return new Tuple<List<BillPaidDto>, List<BillPaidInfoDto>, List<BillPaymentBalanceDto>>(listBills, bills, balances);
        }

        private async Task<Tuple<List<BillPaidDto>, List<BillPaidInfoDto>, List<BillPaymentBalanceDto>>> HandlePayUserBillDebts(List<PayUserBillDto> userBillDebts, UserBillPayment payment)
        {
            var listBills = new List<BillPaidDto>();
            var bills = new List<BillPaidInfoDto>();

            var balances = new List<BillPaymentBalanceDto>();

            foreach (var bd in userBillDebts)
            {
                var bill = await _userBillRepo.GetAsync(bd.Id);
                if (bill == null) throw new Exception("BillDebt is not found !");

                var billPaid = ObjectMapper.Map<BillPaidDto>(bill);
                billPaid.PayAmount = bd.PayAmount;
                if (bill.DebtTotal == null || bill.DebtTotal == 0) bill.DebtTotal = (decimal)bill.LastCost;

                var debtTotal = DecimalRoudingUp(bill.DebtTotal.Value);
                var payAmount = DecimalRoudingUp(bd.PayAmount);

                if (debtTotal == payAmount)
                {
                    bill.Status = UserBillStatus.Paid;
                    bill.DebtTotal = 0;
                }
                else if (debtTotal > payAmount)
                {
                    bill.DebtTotal = (int)bill.DebtTotal - (int)bd.PayAmount;
                    bill.Status = UserBillStatus.Debt;
                }
                else
                {
                    var balance = new BillPaymentBalanceDto()
                    {
                        Amount = payAmount - debtTotal,
                        ApartmentCode = bill.ApartmentCode,
                        BillType = bill.BillType,
                        BuildingId = bill.BuildingId,
                        CitizenTempId = bill.CitizenTempId,
                        EBalanceAction = EBalanceAction.Add,
                        TenantId = bill.TenantId,
                        UrbanId = bill.UrbanId,
                        UserBillId = bill.Id
                    };
                    balances.Add(balance);
                }

                if (payment.Status == UserBillPaymentStatus.Pending)
                {
                    bill.IsPaymentPending = true;
                }
                else
                {
                    bill.IsPaymentPending = false;
                }
                var billInfo = ObjectMapper.Map<BillPaidInfoDto>(bill);
                billInfo.PayAmount = billPaid.PayAmount;
                listBills.Add(billPaid);
                bills.Add(billInfo);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return new Tuple<List<BillPaidDto>, List<BillPaidInfoDto>, List<BillPaymentBalanceDto>>(listBills, bills, balances);
        }

        private async Task<Tuple<List<BillPaidDto>, List<BillPaidInfoDto>>> HandlePrepaymentVerifyPayment(List<PrepaymentBillDto> prepaymentBillDtos, UserBill userBill, UserBillPayment payment)
        {
            var properties = JsonConvert.DeserializeObject<dynamic>(userBill.Properties);
            var period = DateTime.Now;
            var listBills = new List<BillPaidDto>();
            var bills = new List<BillPaidInfoDto>();

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
                        userBill1.BillType = prepaymentBill.BillType;
                        userBill1.Status = UserBillStatus.Paid;
                        userBill1.CarNumber = userBill.CarNumber;
                        userBill1.MotorbikeNumber = userBill.MotorbikeNumber;
                        userBill1.BicycleNumber = userBill.BicycleNumber;
                        userBill1.OtherVehicleNumber = userBill.OtherVehicleNumber;
                        userBill1.IsPrepayment = true;

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
                        if (payment.Status == UserBillPaymentStatus.Pending)
                        {
                            userBill1.IsPaymentPending = true;
                        }
                        var id = await _userBillRepo.InsertAndGetIdAsync(userBill1);

                        userBill1.Code = "HD" + userBill1.Id + (billPeriod.Month < 10 ? "0" + billPeriod.Month : billPeriod.Month) + "" + billPeriod.Year;
                        var billPaid = userBill1.MapTo<BillPaidDto>();
                        var billInfo = userBill1.MapTo<BillPaidInfoDto>();
                        billPaid.PayAmount = userBill1.LastCost;
                        billInfo.PayAmount = billPaid.PayAmount;
                        listBills.Add(billPaid);
                        bills.Add(billInfo);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

            }

            return new Tuple<List<BillPaidDto>, List<BillPaidInfoDto>>(listBills, bills);
        }

        private async Task CreateBillPaymentHistory(BillPaidInfoDto bill, UserBillPayment payment)
        {
            try
            {
                var billHis = ObjectMapper.Map<UserBillPaymentHistory>(bill);
                billHis.Id = 0;
                billHis.UserBillId = bill.Id;
                billHis.PaymentId = payment.Id;
                if (billHis.BillType == BillType.Parking)
                {
                    try
                    {
                        string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(bill.Properties)?.vehicles?.ToString() ?? null;
                        if (listVehiclesString != null)
                        {
                            var vehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);

                            foreach (var vehicle in vehicles)
                            {
                                switch (vehicle.vehicleType)
                                {
                                    case VehicleType.Car:
                                        billHis.CarPrice = (billHis.CarPrice ?? 0) + (double)vehicle.cost;
                                        billHis.CarNumber += 1;
                                        break;
                                    case VehicleType.Motorbike:
                                        billHis.MotorPrice = (billHis.MotorPrice ?? 0) + (double)vehicle.cost;
                                        billHis.MotorbikeNumber += 1;
                                        break;
                                    case VehicleType.Bicycle:
                                        billHis.BikePrice = (billHis.BikePrice ?? 0) + (double)vehicle.cost;
                                        billHis.BicycleNumber += 1;
                                        break;
                                    case VehicleType.Other:
                                        billHis.OtherVehiclePrice = (billHis.OtherVehiclePrice ?? 0) + (double)vehicle.cost;
                                        billHis.OtherVehicleNumber += 1;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }

                await _billPaymentHistoryRepos.InsertAsync(billHis);
            }
            catch
            {
            }
        }

        private async Task CreateApartmentBalances(List<BillPaymentBalanceDto> balances)
        {
            foreach(var balance in balances)
            {
                var data = ObjectMapper.Map<ApartmentBalance>(balance);
                await _apartmentBalanceRepos.InsertAsync(data);
            }
        }
        #region Notification
        private async Task NotifierBillPaymentSuccess(UserBillPayment bill, int amount, long userId)
        {
            var method = L(nameof(bill.Method));
            var detailUrlApp = $"yoolife://app/receipt?apartmentCode={bill.ApartmentCode}&formId=2";
            var detailUrlWA = $"/monthly?apartmentCode={bill.ApartmentCode}&formId=2";
            var messageSuccess = new UserMessageNotificationDataBase(
                               AppNotificationAction.BillPaymentSuccess,
                               AppNotificationIcon.BillPaymentSuccessIcon,
                               TypeAction.Detail,
                               $"Thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {string.Format("{0:#,#.##}", amount)} VNĐ",
                               detailUrlApp,
                               detailUrlWA
                               );
            await _appNotifier.SendMessageNotificationInternalAsync(
                    $"Thông báo thanh toán hóa đơn !",
                    $"Bạn đã thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {string.Format("{0:#,#.##}", amount)} VNĐ",
                    detailUrlApp,
                    detailUrlWA,
                    new UserIdentifier[] { new UserIdentifier(bill.TenantId, userId) },
                    messageSuccess,
                    AppType.USER
                );
            // await _appNotifier.SendUserMessageNotifyFireBaseAsync(
            //      $"Thông báo thanh toán hóa đơn !",
            //      $"Bạn đã thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {string.Format("{0:#,#.##}", amount)} VNĐ",
            //      detailUrlApp,
            //      detailUrlWA,
            //      new UserIdentifier[] { new UserIdentifier(bill.TenantId, userId) },
            //      messageSuccess);
        }
        private async Task NotifieRequestUserBillPayment(UserBillPayment bill, int amount, long userId)
        {
            var method = L(nameof(bill.Method));
            var detailUrlApp = $"yoolife://app/receipt?apartmentCode={bill.ApartmentCode}&formId=2";
            var detailUrlWA = $"/monthly?apartmentCode={bill.ApartmentCode}&formId=2";
            var messageSuccess = new UserMessageNotificationDataBase(
                               AppNotificationAction.BillPaymentSuccess,
                               AppNotificationIcon.BillPaymentSuccessIcon,
                               TypeAction.Detail,
                               $"Yêu cầu thanh toán hóa đơn thành công ! Tổng số tiền : {string.Format("{0:#,#.##}", amount)} VNĐ",
                               detailUrlApp,
                               detailUrlWA
                               );
            await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                 $"Thông báo thanh toán hóa đơn !",
                 $"Yêu cầu thanh toán hóa đơn thành công ! Tổng số tiền : {string.Format("{0:#,#.##}", amount)} VNĐ",
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
    }
}
