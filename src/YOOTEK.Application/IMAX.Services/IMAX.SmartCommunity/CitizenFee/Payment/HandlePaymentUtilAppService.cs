using Abp;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Dto;
using IMAX.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.CitizenFee.Payment
{
    public class HandlePaymentUtilAppService: IMAXAppServiceBase
    {
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;

        public HandlePaymentUtilAppService(
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<User, long> userRepos, IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepo,
            IAppNotifier appNotifier,
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository
            )

        {
            _userBillPaymentRepo = userBillPaymentRepo;
            _userRepos = userRepos;
            _userBillRepo = userBillRepo;
            _appNotifier = appNotifier;
            _citizenTempRepos = citizenTempRepos;
            _billDebtRepo = billDebtRepo;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
        }


        public async Task<object> PayMonthlyUserBillByApartment(PayMonthlyUserBillsInput input)
        {
            try
            {
                if ((input.UserBills == null || input.UserBills.Count() == 0)
                    && (input.UserBillDebts == null || input.UserBillDebts.Count() == 0)
                    && (input.PrepaymentBills == null || input.PrepaymentBills.Count() == 0)) throw new Exception("Input user bill is null");
                var payment = new UserBillPayment()
                {
                    Amount = input.Amount,
                    ApartmentCode = input.ApartmentCode,
                    Method = input.Method,
                    Status = UserBillPaymentStatus.Success,
                    TypePayment = TypePayment.Bill,
                    Period = input.Period,
                    Title = "Thanh toán hóa đơn tháng " + input.Period.ToString("MM/yyyyy"),
                    TenantId = AbpSession.TenantId,
                    Description = input.Description,
                    BuildingId = input.UserBill.BuildingId,
                    UrbanId = input.UserBill.UrbanId,
                    FileUrl = input.FileUrl,
                    ImageUrl = input.ImageUrl,
                };

                bool isPaymentDebt = true;

                var billPaymentInfo = new BillPaymentInfo();
                      
                // Handle billDebt

                if (input.UserBillDebts != null && input.UserBillDebts.Count() > 0)
                {
                    var bills = await HandlePayUserBillDebts(input.UserBillDebts);
                    billPaymentInfo.BillListDebt = bills;
                    payment.UserBillDebtIds = string.Join(",", bills.Select(x => x.Id).OrderBy(x => x));
                    isPaymentDebt = true;
                }

                // Handle Userbill
                if (input.UserBills != null && input.UserBills.Count() > 0)
                {
                    var bills = await HandlePayUserBillPendings(input.UserBills);
                    billPaymentInfo.BillList = bills;
                    payment.UserBillIds = string.Join(",", bills.Select(x => x.Id).OrderBy(x => x));
                    isPaymentDebt = false;
                }


                // Handle prepayment
                if (input.PrepaymentBills != null && input.PrepaymentBills.Count > 0)
                {
                    var bills = await HandlePrepaymentVerifyPayment(input.PrepaymentBills, input.UserBill);
                    billPaymentInfo.BillListPrepayment = bills;
                    payment.UserBillPrepaymentIds = string.Join(",", bills.Select(x => x.Id).OrderBy(x => x));
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
                if (isPaymentDebt) payment.TypePayment = TypePayment.DebtBill;
                await _userBillPaymentRepo.InsertAndGetIdAsync(payment);

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


        private async Task<List<BillPaidDto>> HandlePayUserBillPendings(List<PayUserBillDto> userBills)
        {
            var listBills = new List<BillPaidDto>();
            foreach (var bd in userBills)
            {
                var bill = await _userBillRepo.GetAsync(bd.Id);
                if (bill == null) throw new Exception("BillDebt is not found !");
                var billPaid = bill.MapTo<BillPaidDto>();
                billPaid.PayAmount = bd.PayAmount;
                if ((int)bill.LastCost == (int)(bd.PayAmount))
                {
                    bill.Status = UserBillStatus.Paid;
                    bill.DebtTotal = 0;
                }
                else if ((int)bill.LastCost > (int)(bd.PayAmount))
                {
                    bill.DebtTotal = (int)bill.LastCost - (int)bd.PayAmount;
                    bill.Status = UserBillStatus.Debt;
                }
                else
                {
                    throw new Exception("PayAmount is not matching !");
                }
                listBills.Add(billPaid);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return listBills;
        }

        private async Task<List<BillPaidDto>> HandlePayUserBillDebts(List<PayUserBillDto> userBillDebts)
        {
            var listBills = new List<BillPaidDto>();
            foreach (var bd in userBillDebts)
            {
                var bill = await _userBillRepo.GetAsync(bd.Id);
                if (bill == null) throw new Exception("BillDebt is not found !");
                var billPaid = bill.MapTo<BillPaidDto>();
                billPaid.PayAmount = bd.PayAmount;
                if (bill.DebtTotal == null || bill.DebtTotal == 0) bill.DebtTotal = (decimal)bill.LastCost;
                if ((int)bill.DebtTotal == (int)(bd.PayAmount))
                {
                    bill.Status = UserBillStatus.Paid;
                    bill.DebtTotal = 0;
                }
                else if ((int)bill.DebtTotal > (int)(bd.PayAmount))
                {
                    bill.DebtTotal = (int)bill.DebtTotal - (int)bd.PayAmount;
                    bill.Status = UserBillStatus.Debt;
                }
                else
                {
                    throw new Exception("PayAmount is not matching !");
                }
                listBills.Add(billPaid);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return listBills;
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
            await _appNotifier.SendUserMessageNotifyFireBaseAsync(
                 $"Thông báo thanh toán hóa đơn !",
                 $"Bạn đã thanh toán hóa đơn thành công ! Tổng số tiền đã thanh toán : {string.Format("{0:#,#.##}", amount)} VNĐ",
                 detailUrlApp,
                 detailUrlWA,
                 new UserIdentifier[] { new UserIdentifier(bill.TenantId, userId) },
                 messageSuccess);
        }

    }
}
