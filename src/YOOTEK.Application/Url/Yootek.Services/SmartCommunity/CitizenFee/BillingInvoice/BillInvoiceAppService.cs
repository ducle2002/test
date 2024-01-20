using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using Yootek.Authorization.BillInvoices;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Yootek.Services.SmartCommunity.BillingInvoice.Dto;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NumToWords.VietNamese;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.SmartCommunity.BillingInvoice
{
    public interface IBillInvoiceAppService : IApplicationService
    {
        Task<DataResult> GetBillInvoiceApartment(PrintBillInvoiceInput input);
    }
    public class BillInvoiceAppService : YootekAppServiceBase, IBillInvoiceAppService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepository;
        private readonly IRepository<CitizenParking, long> _citizenParkingRepository;
        private readonly IBillInvoiceTemplateProvider _billInvoiceTemplateProvider;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<BillConfig, long> _billConfigRepository;
        private readonly IRepository<UserBillPayment, long> _paymentRepository;
        private readonly ITemplateBillAppService _templateBillAppService;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly UserManager _userManager;


        public BillInvoiceAppService(
            IConfiguration configuration,
            IRepository<BillConfig, long> billConfigRepository,
            IRepository<CitizenVehicle, long> citizenVehicleRepository,
            IRepository<CitizenTemp, long> citizenTempRepository,
            IRepository<Apartment, long> apartmentRepository,
            IRepository<UserBill, long> userBillRepository,
            IBillInvoiceTemplateProvider billInvoiceTemplateProvider,
            ITemplateBillAppService templateBillAppService,
            IRepository<CitizenParking, long> citizenParkingRepository,
            IRepository<UserBillPayment, long> paymentRepository,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            UserManager userManager

            )
        {
            _configuration = configuration;
            _apartmentRepository = apartmentRepository;
            _citizenVehicleRepository = citizenVehicleRepository;
            _citizenTempRepository = citizenTempRepository;
            _userBillRepository = userBillRepository;
            _billInvoiceTemplateProvider = billInvoiceTemplateProvider;
            _templateBillAppService = templateBillAppService;
            _billConfigRepository = billConfigRepository;
            _citizenParkingRepository = citizenParkingRepository;
            _paymentRepository = paymentRepository;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _userManager = userManager;
        }


        public async Task<DataResult> GetReceiptPayment(long id)
        {
            try
            {
                var payment = await _paymentRepository.FirstOrDefaultAsync(id);
                if (payment == null) throw new Exception("Payment not found !");

                var billIds = new List<long>();
                if (!payment.UserBillIds.IsNullOrEmpty())
                {
                    long[] ids = Array.ConvertAll(payment.UserBillIds.Split(","), long.Parse);
                    billIds.AddRange(ids);
                }
                if (!payment.UserBillDebtIds.IsNullOrEmpty())
                {
                    long[] ids = Array.ConvertAll(payment.UserBillDebtIds.Split(","), long.Parse);
                    billIds.AddRange(ids);
                }
                if (!payment.UserBillPrepaymentIds.IsNullOrEmpty())
                {
                    long[] ids = Array.ConvertAll(payment.UserBillPrepaymentIds.Split(","), long.Parse);
                    billIds.AddRange(ids);
                }

                return await GetBillInvoiceApartment(new PrintBillInvoiceInput()
                {
                    ApartmentCode = payment.ApartmentCode,
                    PeriodMonth = payment.Period.HasValue ? payment.Period.Value.Month : DateTime.Now.Month,
                    PeriodYear = payment.Period.HasValue ? payment.Period.Value.Year : DateTime.Now.Year,
                    UserBillIds = billIds,
                    PaymentId = payment.Id,
                    Method = payment.Method,
                    Payment = payment
                });

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> GetPaymentVoucher(long id)
        {
            try
            {
                var payment = await _paymentRepository.FirstOrDefaultAsync(id);
                if (payment == null) throw new Exception("Payment not found !");

                var billIds = new List<long>();
                if (!payment.UserBillIds.IsNullOrEmpty())
                {
                    long[] ids = Array.ConvertAll(payment.UserBillIds.Split(","), long.Parse);
                    billIds.AddRange(ids);
                }
                if (!payment.UserBillDebtIds.IsNullOrEmpty())
                {
                    long[] ids = Array.ConvertAll(payment.UserBillDebtIds.Split(","), long.Parse);
                    billIds.AddRange(ids);
                }
                if (!payment.UserBillPrepaymentIds.IsNullOrEmpty())
                {
                    long[] ids = Array.ConvertAll(payment.UserBillPrepaymentIds.Split(","), long.Parse);
                    billIds.AddRange(ids);
                }

                return await GetBillInvoicePaymentApartment(new PrintBillInvoiceInput()
                {
                    ApartmentCode = payment.ApartmentCode,
                    PeriodMonth = payment.Period.HasValue ? payment.Period.Value.Month : DateTime.Now.Month,
                    PeriodYear = payment.Period.HasValue ? payment.Period.Value.Year : DateTime.Now.Year,
                    UserBillIds = billIds,
                    PaymentId = payment.Id,
                    Method = payment.Method,
                    Payment = payment
                });

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<DataResult> GetBillInvoicePaymentApartment(PrintBillInvoiceInput input)
        {
            try
            {
                int tenantId = (int)AbpSession.TenantId;
                var creator = await _userManager.GetUserByIdAsync(AbpSession.UserId ?? 0);
                StringBuilder billInvoiceTemplate = new(_billInvoiceTemplateProvider.GetBillPaymentTemplate(tenantId));
                /*StringBuilder billInvoiceTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput()
                {
                    TenantId = tenantId,
                    Type = ETemplateBillType.INVOICE,
                }));*/
                var template = new StringBuilder();

                template = await CreateTemplateGeneral(input, billInvoiceTemplate);

                return DataResult.ResultSuccess(template.ToString(), "Get success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<DataResult> GetBillInvoiceApartment(PrintBillInvoiceInput input)
        {
            try
            {
                int tenantId = (int)AbpSession.TenantId;
                StringBuilder billInvoiceTemplate = new(_billInvoiceTemplateProvider.GetBillInvoiceTemplate(tenantId));
                /*StringBuilder billInvoiceTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput()
                {
                    TenantId = tenantId,
                    Type = ETemplateBillType.INVOICE,
                }));*/
                var template = new StringBuilder();
                if (tenantId == _configuration.GetValue<int>("CustomTenant:HudlandTenantId"))
                {
                    template = await CreateTemplateHTS(input, billInvoiceTemplate);
                }
                else if (tenantId == _configuration.GetValue<int>("CustomTenant:KeangnamTenantId"))
                {
                    template = await CreateTemplateKeangnam(input, billInvoiceTemplate);
                }
                else if (tenantId == _configuration.GetValue<int>("CustomTenant:Vina22TenantId"))
                {
                    template = CreateTemplateVina22(input, billInvoiceTemplate);
                }
                //  else if (tenantId == 47)
                // {
                //     template = CreateTemplateGeneral(input, billInvoiceTemplate);
                // }
                else
                {
                    template = await CreateTemplateGeneral(input, billInvoiceTemplate);
                }
                return DataResult.ResultSuccess(template.ToString(), "Get success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        // hudlands
        private async Task<StringBuilder> CreateTemplateHTS(PrintBillInvoiceInput input, StringBuilder billInvoiceTemplate)
        {
            using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
            {

                DateTime period = new DateTime(input.PeriodYear, input.PeriodMonth, 1);
                DateTime currentDate = DateTime.Now;
                string apartmentCode = input.ApartmentCode;
                string periodString = period.ToString("MM/yyyy");

                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                List<UserBill> userBills = GetListUserBillByApartment(apartmentCode, input.UserBillIds);
                CitizenTemp citizenTemp = GetCitizenTempByApartment(apartmentCode);

                List<UserBill> managementBills = userBills.Where(x => x.BillType == BillType.Manager).ToList();
                List<UserBill> electricBills = userBills.Where(x => x.BillType == BillType.Electric).ToList();
                List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();
                List<UserBill> waterBills = userBills.Where(x => x.BillType == BillType.Water).ToList();
                List<UserBill> parkingBills = userBills.Where(x => x.BillType == BillType.Parking).ToList();

                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);

                long? buldingId = input.Payment?.BuildingId ?? managementBill?.BuildingId ?? parkingBill?.BuildingId ?? null;
                long? urbanId = input.Payment?.UrbanId ?? managementBill?.UrbanId ?? parkingBill?.UrbanId ?? null;

                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport, urbanId, buldingId);
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                int numberManagementBill = managementBills.Count;
                int numberParkingBill = parkingBills.Count;

                string customerName = GetCustomerName(userBills, citizenTemp);
                decimal? acreageApartment = GetAcreageApartment(managementBill);

                int managementCost = (int)GetBillCost(managementBill) * numberManagementBill;
                int parkingCost = (int)GetBillCost(parkingBill) * numberParkingBill;
                int waterCost = waterBills.Sum(waterBill => (int)GetBillCost(waterBill));
                int electricCost = electricBills.Sum(electricBill => (int)GetBillCost(electricBill));
                int otherCost = otherBills.Sum(otherBill => (int)GetBillCost(otherBill));

                long totalFee = managementCost + parkingCost + waterCost + otherCost + electricCost;

                //  hoá đơn điện
                StringBuilder electricBillTable = new StringBuilder();
                if (electricBills.Count > 0)
                {
                    electricBillTable.Append("<div style=\"margin-bottom: 12px; font-weight: 600;\">IV. Phí điện</div>");
                    electricBillTable.Append("<table class=\"border-collapse-table\" style=\"margin-bottom: 20px; width: 100%; table-layout: auto; border-collapse: collapse;\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\">");
                    electricBillTable.Append("<thead>");
                    electricBillTable.Append("<tr>");
                    electricBillTable.Append("<th style=\"width: 32px; padding: 10px 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">STT</th>");
                    electricBillTable.Append("<th style=\"padding: 10px 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">Nội dung</th>");
                    electricBillTable.Append("<th style=\"width: 120px; padding: 10px 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">Thành tiền</th>");
                    electricBillTable.Append("</tr>");
                    electricBillTable.Append("</thead>");
                    electricBillTable.Append("<tbody>");

                    foreach (var (electricBill, index) in electricBills.Select((electricBill, index) => (electricBill, index)))
                    {
                        DateTime periodDate = (DateTime)electricBill.Period;
                        int periodMonth = periodDate.Month;
                        int periodYear = periodDate.Year;
                        int electricCostBill = (int)(electricBill.LastCost ?? 0);

                        electricBillTable.Append("<tr>");
                        electricBillTable.Append($"<td style=\"width: 32px; padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{GetStringValue(index + 1)}</td>");
                        electricBillTable.Append($"<td style=\"padding: 6px; text-align: left; white-space: nowrap; border-width: 1px; border-style: solid;\">Phí điện tháng {periodMonth}/{periodYear}</td>");
                        electricBillTable.Append($"<td style=\"padding: 6px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{FormatCost(electricCostBill)}</td>");
                        electricBillTable.Append("</tr>");
                    }

                    electricBillTable.Append("</tbody>");
                    electricBillTable.Append("<tfoot>");
                    electricBillTable.Append("<tr>");
                    electricBillTable.Append("<td colspan=\"2\" style=\"padding: 6px; text-align: center; font-weight: 600; white-space: nowrap; border-width: 1px; border-style: solid;\">Tổng tiền điện</td>");
                    electricBillTable.Append($"<td style=\"padding: 6px; text-align: right; font-weight: 600; white-space: nowrap; border-width: 1px; border-style: solid;\">{FormatCost(electricCost)}</td>");
                    electricBillTable.Append("</tr>");
                    electricBillTable.Append("</tfoot>");
                    electricBillTable.Append("</table>");
                }

                // hoá đơn nước
                string listWaterBill = "";
                if (waterBills != null)
                {
                    foreach (var (waterBill, index) in waterBills.Select((waterBill, index) => (waterBill, index)))
                    {
                        DateTime periodDate = (DateTime)waterBill.Period;
                        int periodMonth = periodDate.Month;
                        int periodYear = periodDate.Year;
                        int waterCostBill = (int)GetBillCost(waterBill);

                        StringBuilder waterRowTemplate = new("<tr>\r\n          <td style=\"width: 32px; padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{INDEX}</td>\r\n          <td style=\"padding: 6px; text-align: left; white-space: nowrap; border-width: 1px; border-style: solid;\">Phí nước tháng {PERIOD_MONTH}/{PERIOD_YEAR}</td>\r\n          <td style=\"padding: 6px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{ELECTRIC_COST}</td>\r\n          </tr>");

                        waterRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{PERIOD_MONTH}", GetStringValue(periodMonth))
                            .Replace("{PERIOD_YEAR}", GetStringValue(periodYear))
                            .Replace("{ELECTRIC_COST}", FormatCost(waterCostBill));
                        listWaterBill += waterRowTemplate.ToString();
                    }
                }

                // hóa đơn khác
                StringBuilder otherBillTable = new StringBuilder();

                if (otherCost != 0)
                {
                    if (electricBills.Count > 0)
                    {
                        otherBillTable.Append("<div style=\"margin-bottom: 12px; font-weight: 600;\">V. Phí khác</div>");
                    }
                    else otherBillTable.Append("<div style=\"margin-bottom: 12px; font-weight: 600;\">IV. Phí khác</div>");
                    otherBillTable.Append("<table class=\"border-collapse-table\" style=\"margin-bottom: 20px; width: 100%; table-layout: auto; border-collapse: collapse;\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\">");
                    otherBillTable.Append("<thead>");
                    otherBillTable.Append("<tr>");
                    otherBillTable.Append("<th style=\"width: 32px; padding: 10px 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">STT</th>");
                    otherBillTable.Append("<th style=\"padding: 10px 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">Nội dung</th>");
                    otherBillTable.Append("<th style=\"width: 120px; padding: 10px 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">Thành tiền</th>");
                    otherBillTable.Append("</tr>");
                    otherBillTable.Append("</thead>");
                    otherBillTable.Append("<tbody>");

                    foreach (var (otherBill, index) in otherBills.Select((otherBill, index) => (otherBill, index)))
                    {
                        DateTime periodDate = (DateTime)otherBill.Period;
                        int periodMonth = periodDate.Month;
                        int periodYear = periodDate.Year;
                        int otherCostBill = (int)(otherBill.LastCost ?? 0);
                        string otherName = GetOtherBillName(otherBill, billConfigs);

                        otherBillTable.Append("<tr>");
                        otherBillTable.Append($"<td style=\"width: 32px; padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{GetStringValue(index + 1)}</td>");
                        otherBillTable.Append($"<td style=\"padding: 6px; text-align: left; white-space: nowrap; border-width: 1px; border-style: solid;\">{otherName} tháng {periodMonth}/{periodYear}</td>");
                        otherBillTable.Append($"<td style=\"padding: 6px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{FormatCost(otherCostBill)}</td>");
                        otherBillTable.Append("</tr>");
                    }

                    otherBillTable.Append("</tbody>");
                    otherBillTable.Append("<tfoot>");
                    otherBillTable.Append("<tr>");
                    otherBillTable.Append("<td colspan=\"2\" style=\"padding: 6px; text-align: center; font-weight: 600; white-space: nowrap; border-width: 1px; border-style: solid;\">Tổng tiền khác</td>");
                    otherBillTable.Append($"<td style=\"padding: 6px; text-align: right; font-weight: 600; white-space: nowrap; border-width: 1px; border-style: solid;\">{FormatCost(otherCost)}</td>");
                    otherBillTable.Append("</tr>");
                    otherBillTable.Append("</tfoot>");
                    otherBillTable.Append("</table>");
                }

                // list vehicle
                List<CitizenVehiclePas> listVehicles = new();
                if (parkingBill?.Properties != null)
                {
                    string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                    if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                }
                string listRowVehicles = "";
                if (listVehicles.Count > 0)
                {
                    foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                    {
                        StringBuilder vehicleRowTemplate = new(GetListVehicle(AbpSession.TenantId));
                        string vehicleName = GetVehicleName(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehiclePrice = vehicle?.cost ?? 0;
                        decimal vehicleCost = vehiclePrice * numberParkingBill;

                        vehicleRowTemplate
                       .Replace("{INDEX}", GetStringValue(index + 1))
                       .Replace("{PERIOD}", periodString)
                       .Replace("{NUMBER_MONTH_PARKING}", GetStringValue(numberParkingBill))
                       .Replace("{VEHICLE_NAME}", $"{vehicleName}")
                       .Replace("{VEHICLE_CODE}", $"{vehicleCode}")
                       .Replace("{PERIOD_MONTH}", $"{period.Month:D2}")
                       .Replace("{PERIOD_YEAR}", $"{period.Year}")
                       .Replace("{UNIT_PARKING_ELEMENT}", FormatCost((double?)vehiclePrice))
                       .Replace("{COST_PARKING_ELEMENT}", FormatCost((double?)vehicleCost));
                        listRowVehicles += vehicleRowTemplate.ToString();
                    }
                }

                // edit template
                billInvoiceTemplate
                    .Replace("{PAYMENT_TYPE}", GetPaymentType(input.Method))
                    .Replace("{PAYMENT_ID}", input.PaymentId > 0 ? GetStringValue(input.PaymentId) : "0001")
                    .Replace("{DAY_NOW}", $"{currentDate.Day:D2}")
                    .Replace("{MONTH_NOW}", $"{currentDate.Month:D2}")
                    .Replace("{YEAR_NOW}", GetStringValue(currentDate.Year))
                    .Replace("{PERIOD}", GetStringValue(periodString))
                    .Replace("{CUSTOMER_NAME}", GetStringValue(customerName))
                    .Replace("{APARTMENT_CODE}", GetStringValue(apartmentCode))
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementCost))
                    .Replace("{LIST_VEHICLE}", listRowVehicles)
                    .Replace("{LIST_WATER_BILL}", listWaterBill)
                    .Replace("{LIST_ELECTRIC_BILL}", electricBillTable.ToString())
                    .Replace("{LIST_BILL_OTHER}", otherBillTable.ToString())
                    .Replace("{NUMBER_MONTH_M}", GetStringValue(numberManagementBill))
                    .Replace("{COST_PARKING}", FormatCost(parkingCost))
                    .Replace("{COST_WATER}", FormatCost(waterCost))
                    .Replace("{COST_OTHER_BILL}", FormatCost(otherCost))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricCost))
                    .Replace("{COST_TOTAL}", FormatCost(totalFee))
                    .Replace("{COST_TOTAL_TEXT}", VietNameseConverter.FormatCurrency(totalFee));

                return billInvoiceTemplate;
            }
        }
        private async Task<StringBuilder> CreateTemplateKeangnam(PrintBillInvoiceInput input, StringBuilder billInvoiceTemplate)
        {
            return new StringBuilder("");
        }

        private StringBuilder CreateTemplateVina22(PrintBillInvoiceInput input, StringBuilder billInvoiceTemplate)
        {
            using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
            {

                DateTime period = new DateTime(input.PeriodYear, input.PeriodMonth, 1);
                DateTime pre_period = new DateTime(input.PeriodYear, input.PeriodMonth, 1).AddMonths(-1);
                DateTime currentDate = DateTime.Now;
                string apartmentCode = input.ApartmentCode;
                string periodString = period.ToString("MM/yyyy");

                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                List<UserBill> userBills = new List<UserBill>();
                CitizenTemp citizenTemp = GetCitizenTempByApartment(apartmentCode);

                List<UserBill> managementBills = new List<UserBill>();
                List<UserBill> electricBills = new List<UserBill>();
                List<UserBill> waterBills = new List<UserBill>();
                List<UserBill> parkingBills = new List<UserBill>();

                List<UserBill> managementBillDebts = new List<UserBill>();
                List<UserBill> electricBillDebts = new List<UserBill>();
                List<UserBill> waterBillDebts = new List<UserBill>();
                List<UserBill> parkingBillDebts = new List<UserBill>();

                if (input.Payment.BillPaymentInfo.IsNullOrEmpty())
                {
                    userBills = GetListUserBillByApartment(apartmentCode, input.UserBillIds);
                    managementBills = userBills.Where(x => x.BillType == BillType.Manager).ToList();
                    electricBills = userBills.Where(x => x.BillType == BillType.Electric).ToList();
                    waterBills = userBills.Where(x => x.BillType == BillType.Water).ToList();
                    parkingBills = userBills.Where(x => x.BillType == BillType.Parking).ToList();
                }
                else
                {
                    try
                    {
                        var billPaymentInfo = JsonConvert.DeserializeObject<BillPaymentInfo>(input.Payment.BillPaymentInfo);
                        if (billPaymentInfo.BillList == null) billPaymentInfo.BillList = new List<BillPaidDto> { };
                        if (billPaymentInfo.BillListPrepayment == null) billPaymentInfo.BillListPrepayment = new List<BillPaidDto>();
                        var allBills = new List<BillPaidDto>().Concat(billPaymentInfo.BillList).Concat(billPaymentInfo.BillListPrepayment).Distinct().ToList();
                        userBills = ObjectMapper.Map<List<UserBill>>(allBills);

                        managementBills = userBills.Where(x => x.BillType == BillType.Manager).ToList();
                        electricBills = userBills.Where(x => x.BillType == BillType.Electric).ToList();
                        waterBills = userBills.Where(x => x.BillType == BillType.Water).ToList();
                        parkingBills = userBills.Where(x => x.BillType == BillType.Parking).ToList();

                        if (billPaymentInfo.BillListDebt != null)
                        {
                            var billDebts = ObjectMapper.Map<List<UserBill>>(billPaymentInfo.BillListDebt);
                            managementBillDebts = billDebts.Where(x => x.BillType == BillType.Manager).ToList();
                            electricBillDebts = billDebts.Where(x => x.BillType == BillType.Electric).ToList();
                            waterBillDebts = billDebts.Where(x => x.BillType == BillType.Water).ToList();
                            parkingBillDebts = billDebts.Where(x => x.BillType == BillType.Parking).ToList();
                        }

                    }
                    catch { }
                }

                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                UserBill parkingBillPaid = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);
                UserBill parkingBill = _userBillRepository.FirstOrDefault(x => x.Id == parkingBillPaid.Id);

                long? buldingId = input.Payment?.BuildingId ?? managementBill?.BuildingId ?? parkingBill?.BuildingId ?? null;
                long? urbanId = input.Payment?.UrbanId ?? managementBill?.UrbanId ?? parkingBill?.UrbanId ?? null;

                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport, urbanId, buldingId);
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                int numberManagementBill = managementBills.Count;
                int numberParkingBill = parkingBills.Count;

                string customerName = GetCustomerName(userBills, citizenTemp);
                decimal? acreageApartment = GetAcreageApartment(managementBill);

                int managementCost = (int)GetBillCost(managementBill) * numberManagementBill;
                int parkingCost = (int)GetBillCost(parkingBill) * numberParkingBill;
                int waterCost = waterBills.Sum(waterBill => (int)GetBillCost(waterBill));
                int electricCost = electricBills.Sum(electricBill => (int)GetBillCost(electricBill));

                var totalIndexE = electricBills.Sum(x => x.TotalIndex);
                var totalIndexW = waterBills.Sum(x => x.TotalIndex);

                int debtM = (int)managementBillDebts.Sum(x => x.DebtTotal);
                int debtW = (int)waterBillDebts.Sum(x => x.DebtTotal);
                int debtE = (int)electricBillDebts.Sum(x => x.DebtTotal);
                int debtP = (int)parkingBillDebts.Sum(x => x.DebtTotal);

                int debtTotal = debtM + debtW + debtE + debtP;

                int lastCostM = managementCost + debtM;
                int lastCostW = waterCost + debtW;
                int lastCostE = electricCost + debtE;
                int lastCostP = parkingCost + debtP;


                long totalFee = managementCost + parkingCost + waterCost + electricCost;

                int lastCostTotal = lastCostM + lastCostE + lastCostW + lastCostP;
                //  hoá đơn điện
                StringBuilder electricBillTable = new StringBuilder();

                if (electricBills.Count > 0)
                {
                    electricBillTable.Append("<div style=\"margin-bottom: 12px; text-align: center; font-weight: 600; text-transform: uppercase;\">BẢNG CHỐT ĐIỆN</div>");
                    electricBillTable.Append("<table style=\"margin-bottom: 24px; width: 100%; table-layout: auto; border-collapse: collapse; font-size: 14px;\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\">");

                    electricBillTable.Append("<thead>");
                    electricBillTable.Append("<tr>");
                    electricBillTable.Append("<th style=\"width: 32px; border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">TT</th>");
                    electricBillTable.Append($"<th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">CS ngày 28/{pre_period.Month:D2}/{pre_period.Year}</th>");
                    electricBillTable.Append($"<th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">CS ngày 28/{period.Month:D2}/{period.Year}</th>");
                    electricBillTable.Append("<th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">Khối lượng tiêu thụ</th>");
                    electricBillTable.Append("</tr>");
                    electricBillTable.Append("</thead>");
                    electricBillTable.Append("<tbody>");

                    foreach (var e in electricBills)
                    {
                        electricBillTable.Append("<tr>");
                        electricBillTable.Append("<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">1</td>");
                        electricBillTable.Append($"<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{e.IndexHeadPeriod ?? 0}</td>");
                        electricBillTable.Append($"<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{e.IndexEndPeriod ?? 0}</td>");
                        electricBillTable.Append($"<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{e.TotalIndex ?? 0}</td>");
                        electricBillTable.Append("</tr>");
                    }

                    electricBillTable.Append("</tbody>");
                    electricBillTable.Append("</table>");
                }

                StringBuilder waterBillTable = new StringBuilder();

                if (waterBills.Count > 0)
                {
                    waterBillTable.Append("<div style=\"margin-bottom: 12px; text-align: center; font-weight: 600; text-transform: uppercase;\">BẢNG CHỐT ĐIỆN</div>");
                    waterBillTable.Append("<table style=\"margin-bottom: 24px; width: 100%; table-layout: auto; border-collapse: collapse; font-size: 14px;\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\">");

                    waterBillTable.Append("<thead>");
                    waterBillTable.Append("<tr>");
                    waterBillTable.Append("<th style=\"width: 32px; border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">TT</th>");
                    waterBillTable.Append($"<th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">CS ngày 28/{pre_period.Month:D2}/{pre_period.Year}</th>");
                    waterBillTable.Append($"<th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">CS ngày 28/{period.Month:D2}/{period.Year}</th>");
                    waterBillTable.Append("<th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">Khối lượng tiêu thụ</th>");
                    waterBillTable.Append("</tr>");
                    waterBillTable.Append("</thead>");
                    waterBillTable.Append("<tbody>");

                    foreach (var e in waterBills)
                    {
                        waterBillTable.Append("<tr>");
                        waterBillTable.Append("<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">1</td>");
                        waterBillTable.Append($"<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{e.IndexHeadPeriod ?? 0}</td>");
                        waterBillTable.Append($"<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{e.IndexEndPeriod ?? 0}</td>");
                        waterBillTable.Append($"<td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{e.TotalIndex ?? 0}</td>");
                        waterBillTable.Append("</tr>");
                    }

                    waterBillTable.Append("</tbody>");
                    waterBillTable.Append("</table>");
                }

                // list vehicle

                StringBuilder vehicleBillTable = new StringBuilder();
                if (parkingBill != null)
                {
                    vehicleBillTable.Append("<div style=\"margin-bottom: 12px; text-align: center; font-weight: 600; text-transform: uppercase;\">BẢNG GIA HẠN XE</div>");
                    vehicleBillTable.Append("<table style=\"margin-bottom: 24px; width: 100%; table-layout: auto; border-collapse: collapse; white-space: nowrap; font-size: 14px\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\">");
                    vehicleBillTable.Append("<thead>");
                    vehicleBillTable.Append("<tr>");
                    vehicleBillTable.Append("<th style=\"width: 32px; border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">TT</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">Loại xe</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">BKS</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">Số tiền</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">Ngày gia<br>hạn</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">Ngày hết<br>hạn</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding: 12px 4px\">Số tháng</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding-top: 12px; padding-bottom: 12px;\">Tổng cộng</th>\r\n" +
                        "          <th style=\"border-width: 1px; border-style: solid; padding: 12px 4px;\">Phôi thẻ</th>");
                    vehicleBillTable.Append("</tr>");
                    vehicleBillTable.Append("</thead>");
                    vehicleBillTable.Append("<tbody>");

                    List<CitizenVehiclePas> listVehicles = new();
                    if (parkingBill?.Properties != null)
                    {
                        string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                        if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                    }

                    if (listVehicles.Count > 0)
                    {
                        foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                        {
                            string vehicleName = GetVehicleTypeName(vehicle);
                            string vehicleCode = vehicle?.vehicleCode ?? "";
                            decimal vehiclePrice = vehicle?.cost ?? 0;
                            decimal vehicleCost = vehiclePrice * numberParkingBill;
                            string registrationDate = vehicle.registrationDate != null ? vehicle.registrationDate.Value.ToString("dd/MM/yyyy") : "";
                            string expireDate = vehicle.expirationDate != null ? vehicle.expirationDate.Value.ToString("dd/MM/yyyy") : "";


                            vehicleBillTable.Append($"<tr>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{GetStringValue(index + 1)}</td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{vehicleName}</td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">{vehicleCode}</td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: right;\">{FormatCost((double?)vehiclePrice)}</td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: right;\">{registrationDate}</td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: right;\">{expireDate}</td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: center;\">\r\n" +
                                $"            <span style=\"color: #ef4444\">{numberParkingBill}</span>\r\n" +
                                $"          </td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: right;\">{FormatCost((double?)vehicleCost)}</td>\r\n" +
                                $"          <td style=\"border-width: 1px; border-style: solid; padding: 10px; text-align: right;\"></td>\r\n" +
                                $"        </tr>");

                        }
                    }
                    vehicleBillTable.Append($"<tr>\r\n" +
                        $"          <td style=\"border-width: 1px; border-style: solid; padding: 12px 10px; text-align: center;\"></td>\r\n" +
                        $"          <td colspan=\"6\" style=\"border-width: 1px; border-style: solid; padding: 12px 10px; text-align: center; font-weight: 600;\">Tổng tiền</td>\r\n" +
                        $"          <td style=\"border-width: 1px; border-style: solid; padding: 12px 10px; text-align: right; font-weight: 600;\">{FormatCost(parkingCost)}</td>\r\n" +
                        $"          <td style=\"border-width: 1px; border-style: solid; padding: 12px 10px; text-align: right; font-weight: 600;\">-</td>\r\n" +
                        $"        </tr>");

                    vehicleBillTable.Append("</tbody>");
                    vehicleBillTable.Append("</table>");
                }

                // edit template
                billInvoiceTemplate
                    .Replace("{DAY_NOW}", $"{currentDate.Day:D2}")
                    .Replace("{MONTH_NOW}", $"{currentDate.Month:D2}")
                    .Replace("{MONTH_PERIOD}", $"{period.Month:D2}")
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{QUARTER}", GetQuarter(period.Month))
                    .Replace("{YEAR_NOW}", GetStringValue(currentDate.Year))
                    .Replace("{PERIOD}", GetStringValue(periodString))
                    .Replace("{CUSTOMER_NAME}", GetStringValue(customerName))
                    .Replace("{APARTMENT_CODE}", GetStringValue(apartmentCode))
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementCost))
                    .Replace("{P_TEMPLATE}", vehicleBillTable.ToString())
                    .Replace("{W_TEMPLATE}", waterBillTable.ToString())
                    .Replace("{E_TEMPLATE}", electricBillTable.ToString())
                    .Replace("{NUMBER_MONTH_M}", GetStringValue(numberManagementBill))
                    .Replace("{TOTAL_INDEX_W}", GetStringValue(totalIndexW))
                    .Replace("{TOTAL_INDEX_E}", GetStringValue(totalIndexE))
                    .Replace("{COST_PARKING}", FormatCost(parkingCost))
                    .Replace("{COST_WATER}", FormatCost(waterCost))
                    .Replace("{COST_E}", FormatCost(electricCost))
                    .Replace("{COST_TOTAL}", FormatCost(totalFee))

                    .Replace("{DEBT_MANAGEMENT}", FormatCost(debtM))
                    .Replace("{DEBT_WATER}", FormatCost(debtW))
                    .Replace("{DEBT_E}", FormatCost(debtE))
                    .Replace("{DEBT_PARKING}", FormatCost(debtP))
                    .Replace("{DEBT_PARKING}", FormatCost(debtTotal))

                    .Replace("{LAST_COST_MANAGEMENT}", FormatCost(lastCostM))
                    .Replace("{LAST_COST_WATER}", FormatCost(lastCostW))
                    .Replace("{LAST_COST_E}", FormatCost(lastCostE))
                    .Replace("{LAST_COST_PARKING}", FormatCost(lastCostP))
                    .Replace("{LAST_COST_TOTAL}", FormatCost(lastCostTotal));

                return billInvoiceTemplate;
            }
        }

        private async Task<StringBuilder> CreateTemplateGeneral(PrintBillInvoiceInput input, StringBuilder billInvoiceTemplate)
        {
            DateTime period = new DateTime(input.PeriodYear, input.PeriodMonth, 1).AddMonths(1);
            DateTime currentDate = DateTime.Now;
            string apartmentCode = input.ApartmentCode;
            string periodString = period.ToString("MM/yyyy");

            List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
            List<UserBill> userBills = GetListUserBillByApartment(apartmentCode, input.UserBillIds);
            CitizenTemp citizenTemp = GetCitizenTempByApartment(apartmentCode);

            List<UserBill> managementBills = userBills.Where(x => x.BillType == BillType.Manager).ToList();
            UserBill electricBill = userBills.Where(x => x.BillType == BillType.Electric).FirstOrDefault();
            List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();
            UserBill waterBill = userBills.Where(x => x.BillType == BillType.Water).FirstOrDefault();
            List<UserBill> parkingBills = userBills.Where(x => x.BillType == BillType.Parking).ToList();

            UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
            UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);

            long? buldingId = input.Payment?.BuildingId ?? managementBill?.BuildingId ?? parkingBill?.BuildingId ?? null;
            long? urbanId = input.Payment?.UrbanId ?? managementBill?.UrbanId ?? parkingBill?.UrbanId ?? null;

            #region bill config
            // BillConfig (unit price) for each type 
            BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking, urbanId, buldingId);
            BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport, urbanId, buldingId);
            BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level, urbanId, buldingId);
            BillConfig priceElectricConfig = GetBillConfigPrice(billConfigs, BillType.Electric, BillConfigPricesType.Level, urbanId, buldingId);
            BillConfig priceLightConfig = GetBillConfigPrice(billConfigs, BillType.Lighting, BillConfigPricesType.Normal, urbanId, buldingId);

            // Management
            BillConfigPropertiesDto billManagementConfigProperties = new();
            if (priceManagementConfig?.Properties != null)
            {
                billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
            }

            // Water 
            BillConfigPropertiesDto billWaterConfigProperties = new();
            if (priceWaterConfig?.Properties != null)
            {
                billWaterConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceWaterConfig.Properties);
            }

            // Electric 
            BillConfigPropertiesDto billElectricConfigProperties = new();
            if (priceElectricConfig?.Properties != null)
            {
                billElectricConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceElectricConfig.Properties);
            }

            // Parking 
            BillConfigPropertiesDto billParkingConfigProperties = new();
            if (priceParkingConfig?.Properties != null)
            {
                billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
            }
            #endregion

            // number bill for each type
            int numberManagementBill = managementBills.Count;
            int numberParkingBill = parkingBills.Count;
            int numberOtherBill = otherBills.Count;

            string customerName = GetCustomerName(userBills, citizenTemp);
            string customerPhoneNumber = citizenTemp?.PhoneNumber;

            //payment
            var creator = await _userManager.GetUserByIdAsync(AbpSession.UserId ?? 0);
            var creatorName = creator.FullName ?? "";
            var addressCreator = creator.AddressOfBirth ?? "";

            // cost unpaid
            long managementCostUnpaid = (long)GetBillCost(managementBill) * numberManagementBill;
            long parkingCostUnpaid = (long)GetBillCost(parkingBill) * numberParkingBill;
            long waterCostUnpaid = (long)GetBillCost(waterBill);
            long electricCostUnpaid = (long)GetBillCost(electricBill);
            long otherCostUnpaid = otherBills.Sum(otherBill => (long)GetBillCost(otherBill));

            // cost debt
            long managementCostDebt = 0;
            long parkingCostDebt = 0;
            long waterCostDebt = 0;
            long electricCostDebt = 0;
            long otherCostDebt = 0;

            // cost final 
            long managementCost = managementCostUnpaid + managementCostDebt;
            long parkingCost = parkingCostUnpaid + parkingCostDebt;
            long waterCost = waterCostUnpaid + waterCostDebt;
            long electricCost = electricCostUnpaid + electricCostDebt;
            long otherCost = otherCostUnpaid + otherCostDebt;
            long costTotalUnpaid = managementCostUnpaid + parkingCostUnpaid + waterCostUnpaid + electricCostUnpaid + otherCostUnpaid;
            long costTotal = managementCost + parkingCost + waterCost + electricCost + otherCost;

            // electric index, water index, rental area,...
            decimal? acreageApartment = GetAcreageApartment(managementBill);
            decimal indexHeadWater = GetIndexHead(waterBill);
            decimal indexEndWater = GetIndexEnd(waterBill);
            decimal totalIndexWater = GetTotalIndex(waterBill);
            decimal indexHeadElectric = GetIndexHead(electricBill);
            decimal indexEndElectric = GetIndexEnd(electricBill);
            decimal totalIndexElectric = GetTotalIndex(electricBill);

            #region list vehicle
            List<CitizenVehiclePas> listVehicles = new();
            if (parkingBill?.Properties != null)
            {
                string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
            }
            string listRowVehicles = "";
            if (listVehicles.Count > 0)
            {
                foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                {
                    StringBuilder vehicleRowTemplate = new(GetListVehicle(AbpSession.TenantId));
                    string vehicleName = GetVehicleNameAndParking(vehicle);
                    int vehicleNumber = 1;
                    decimal vehiclePrice = vehicle?.cost ?? 0;
                    decimal vehicleCost = vehiclePrice * numberParkingBill;

                    vehicleRowTemplate
                        .Replace("{INDEX}", GetStringValue(index + 1))
                        .Replace("{PERIOD}", periodString)
                        .Replace("{NUMBER_MONTH_PARKING}", GetStringValue(numberParkingBill))
                        .Replace("{VEHICLE_NAME}", $"{vehicleName}")
                        .Replace("{VEHICLE_NUMBER}", $"{vehicleNumber}")
                        .Replace("{UNIT_PARKING_ELEMENT}", FormatCost((double?)vehiclePrice))
                        .Replace("{COST_PARKING_ELEMENT}", FormatCost((double?)vehicleCost));
                    listRowVehicles += vehicleRowTemplate.ToString();
                }
            }
            #endregion

            // list water 
            string listWaterBills = "";
            if (billWaterConfigProperties.Prices != null && billWaterConfigProperties.Prices.Any())
            {
                List<PriceDto> listUnitPrices = billWaterConfigProperties.Prices.ToList();
                foreach (var (unitPrice, index) in listUnitPrices.Select((unitPrice, index) => (unitPrice, index)))
                {
                    // declare
                    int waterConsumptionQuantity;
                    int waterUnitRange;
                    long unitPriceWater;
                    long costWaterUnpaid;
                    string waterConsumptionContent = string.Empty;
                    StringBuilder waterRowTemplate = new(GetListWaterBill(AbpSession.TenantId));

                    // caculate price for each unit and content
                    if (index == 0 && unitPrice.To < totalIndexWater)
                    {
                        waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From);
                        waterUnitRange = waterConsumptionQuantity;
                        unitPriceWater = (long)unitPrice.Value;
                        costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                    }
                    else if (unitPrice.To.HasValue && unitPrice.To < totalIndexWater)
                    {
                        waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From + 1);
                        waterUnitRange = waterConsumptionQuantity;
                        unitPriceWater = (long)unitPrice.Value;
                        costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                    }
                    else
                    {
                        waterConsumptionQuantity = (int)(totalIndexWater - unitPrice.From + 1) <= 0 ? 0 : (int)(totalIndexWater - unitPrice.From + 1);
                        unitPriceWater = (long)unitPrice.Value;
                        waterUnitRange = 0;
                        costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                    }

                    // complete
                    waterRowTemplate
                        .Replace("{INDEX}", $"{index + 1}")
                        .Replace("{WATER_UNIT_RANGE}", GetStringValue(waterUnitRange))
                        .Replace("{WATER_CONSUMPTION_QUANTITY}", $"{waterConsumptionQuantity}")
                        .Replace("{UNIT_PRICE_WATER}", FormatCost(unitPriceWater))
                        .Replace("{COST_WATER_UNPAID}", FormatCost(costWaterUnpaid));

                    // vinasinco 
                    if (index == 0)
                    {
                        waterRowTemplate
                        .Replace("{INDEX_HEAD_WATER}", GetStringValue(indexHeadWater))
                        .Replace("{INDEX_END_WATER}", GetStringValue(indexEndWater))
                        .Replace("{TOTAL_INDEX_WATER}", GetStringValue(totalIndexWater));
                    }
                    else
                    {
                        waterRowTemplate
                        .Replace("{INDEX_HEAD_WATER}", "")
                        .Replace("{INDEX_END_WATER}", "")
                        .Replace("{TOTAL_INDEX_WATER}", "");
                    }

                    listWaterBills += waterRowTemplate.ToString();
                }
            }

            // list electric 
            string listElectricBills = "";
            if (electricBill != null && billElectricConfigProperties.Prices != null && billElectricConfigProperties.Prices.Any())
            {
                List<PriceDto> listUnitPrices = billElectricConfigProperties.Prices.ToList();
                foreach (var (unitPrice, index) in listUnitPrices.Select((unitPrice, index) => (unitPrice, index)))
                {
                    // declare
                    int electricConsumptionQuantity;
                    int electricUnitRange;
                    long unitPriceElectric;
                    long costElectricUnpaid;
                    string electricConsumptionContent = string.Empty;
                    StringBuilder electricRowTemplate = new(GetListWaterBill(AbpSession.TenantId));

                    // caculate price for each unit and content
                    if (index == 0 && unitPrice.To < totalIndexElectric)
                    {
                        electricConsumptionQuantity = (int)(unitPrice.To - unitPrice.From);
                        electricUnitRange = electricConsumptionQuantity;
                        unitPriceElectric = (long)unitPrice.Value;
                        costElectricUnpaid = electricConsumptionQuantity * unitPriceElectric;
                    }
                    else if (unitPrice.To.HasValue && unitPrice.To < totalIndexElectric)
                    {
                        electricConsumptionQuantity = (int)(unitPrice.To - unitPrice.From + 1);
                        electricUnitRange = electricConsumptionQuantity;
                        unitPriceElectric = (long)unitPrice.Value;
                        costElectricUnpaid = electricConsumptionQuantity * unitPriceElectric;
                    }
                    else
                    {
                        electricConsumptionQuantity = (int)(totalIndexElectric - unitPrice.From + 1) <= 0 ? 0 : (int)(totalIndexElectric - unitPrice.From + 1);
                        unitPriceElectric = (long)unitPrice.Value;
                        electricUnitRange = 0;
                        costElectricUnpaid = electricConsumptionQuantity * unitPriceElectric;
                    }

                    // complete
                    electricRowTemplate
                        .Replace("{INDEX}", $"{index + 1}")
                        .Replace("{ELECTRIC_UNIT_RANGE}", GetStringValue(electricUnitRange))
                        .Replace("{ELECTRIC_CONSUMPTION_QUANTITY}", $"{electricConsumptionQuantity}")
                        .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(unitPriceElectric))
                        .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(costElectricUnpaid));

                    // vinasinco 
                    if (index == 0)
                    {
                        electricRowTemplate
                        .Replace("{INDEX_HEAD_ELECTRIC}", GetStringValue(indexHeadElectric))
                        .Replace("{INDEX_END_ELECTRIC}", GetStringValue(indexEndElectric))
                        .Replace("{TOTAL_INDEX_ELECTRIC}", GetStringValue(totalIndexElectric));
                    }
                    else
                    {
                        electricRowTemplate
                        .Replace("{INDEX_HEAD_ELECTRIC}", "")
                        .Replace("{INDEX_END_ELECTRIC}", "")
                        .Replace("{TOTAL_INDEX_ELECTRIC}", "");
                    }

                    listElectricBills += electricRowTemplate.ToString();
                }
            }

            // list other bill
            string listOtherBill = "";
            if (otherBills != null)
            {
                foreach (var (otherBill, index) in otherBills.Select((otherBill, index) => (otherBill, index)))
                {
                    DateTime periodDate = (DateTime)otherBill.Period;
                    int periodMonth = periodDate.Month;
                    int periodYear = periodDate.Year;
                    long otherCostBill = (int)GetBillCost(otherBill);
                    StringBuilder otherRowTemplate = new(GetListOtherBill(AbpSession.TenantId));

                    otherRowTemplate
                        .Replace("{INDEX}", GetStringValue(index + 1))
                        .Replace("{PERIOD_MONTH}", GetStringValue(periodMonth))
                        .Replace("{PERIOD_YEAR}", GetStringValue(periodYear))
                        .Replace("{OTHER_COST}", FormatCost(otherCostBill))
                        .Replace("{NUMBER_MONTH_OTHER}", GetStringValue(1))
                        .Replace("{OTHER_BILL_NAME}", GetOtherBillName(otherBill, billConfigs))
                        .Replace("{COST_OTHER_BILL}", FormatCost(otherCostBill))
                        .Replace("{UNIT_PRICE_OTHER}", FormatCost(otherBill.LastCost));
                    listOtherBill += otherRowTemplate.ToString();
                }
            }

            billInvoiceTemplate
                    .Replace("{PAYMENT_TYPE}", GetPaymentType(input.Method))
                    .Replace("{PAYMENT_ID}", input.PaymentId > 0 ? GetStringValue(input.PaymentId) : "0001")
                    .Replace("{DAY_NOW}", FormatNumberToTwoDigits(currentDate.Day))
                    .Replace("{MONTH_NOW}", FormatNumberToTwoDigits(currentDate.Month))
                    .Replace("{YEAR_NOW}", GetStringValue(currentDate.Year))
                    .Replace("{DAY_PERIOD}", FormatNumberToTwoDigits(period.Day))
                    .Replace("{MONTH_PERIOD}", FormatNumberToTwoDigits(period.Month))
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{PERIOD}", GetStringValue(periodString))
                    .Replace("{CUSTOMER_NAME}", GetStringValue(customerName))
                    .Replace("{CUSTOMER_PHONE_NUMBER}", GetStringValue(customerPhoneNumber))
                    .Replace("{APARTMENT_CODE}", GetStringValue(apartmentCode))

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{INDEX_HEAD_WATER}", GetStringValue(indexHeadWater))
                    .Replace("{INDEX_END_WATER}", GetStringValue(indexEndWater))
                    .Replace("{TOTAL_INDEX_WATER}", GetStringValue(totalIndexWater))
                    .Replace("{INDEX_HEAD_ELECTRIC}", GetStringValue(indexHeadElectric))
                    .Replace("{INDEX_END_ELECTRIC}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_ELECTRIC}", GetStringValue(totalIndexElectric))

                    // number month
                    .Replace("{NUMBER_MONTH_MANAGEMENT}", GetStringValue(numberManagementBill))
                    .Replace("{NUMBER_MONTH_PARKING}", GetStringValue(numberParkingBill))
                    .Replace("{NUMBER_MONTH_OTHER}", GetStringValue(numberOtherBill))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementCostUnpaid))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementCostDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementCost))

                    // electric
                    .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(electricCostUnpaid))
                    .Replace("{COST_ELECTRIC_DEBT}", FormatCost(electricCostDebt))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricCost))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterCostUnpaid))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterCostDebt))
                    .Replace("{COST_WATER}", FormatCost(waterCost))

                    // parking
                    .Replace("{COST_PARKING_UNPAID}", FormatCost(parkingCostUnpaid))
                    .Replace("{COST_PARKING_DEBT}", FormatCost(parkingCostDebt))
                    .Replace("{COST_PARKING}", FormatCost(parkingCost))

                    // list vehicle, water, electric, other, ...
                    .Replace("{LIST_VEHICLE}", listRowVehicles)
                    .Replace("{LIST_WATER_BILL}", listWaterBills)
                    .Replace("{LIST_ELECTRIC_BILL}", listElectricBills)
                    .Replace("{LIST_BILL_OTHER}", listOtherBill)
                    .Replace("{BUILDING_CODE}", GetBuildingName(buldingId))

                    //aden payment
                    .Replace("{CREATOR_NAME}", creatorName)
                    .Replace("{CREATOR_ADDRESS}", addressCreator)

                    // cost final

                    .Replace("{COST_TOTAL}", FormatCost(costTotal))
                    .Replace("{COST_TOTAL_TEXT}", VietNameseConverter.FormatCurrency(costTotal));
            return billInvoiceTemplate;
        }

        #region helpers method 
        private double GetBillCost(UserBill bill) => bill?.LastCost ?? 0;
        private decimal GetAcreageApartment(UserBill bill) => bill?.TotalIndex ?? 0;
        private string GetCustomerName(List<UserBill> userBills, CitizenTemp? citizenTemp)
        {
            string customerName = string.Empty;
            string customerNameCitizenTemp = citizenTemp?.FullName ?? "";
            foreach (var bill in userBills)
            {
                if (bill == null) continue;
                try
                {
                    BillProperty properties = JsonConvert.DeserializeObject<BillProperty>(bill.Properties);
                    string customerNameBill = properties?.customerName;
                    if (!string.IsNullOrWhiteSpace(customerNameBill))
                    {
                        customerName = customerNameBill;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal($"Exception create template email NamCuong - GetCustomerName: {ex.Message}");
                }
            }
            customerName = string.IsNullOrWhiteSpace(customerName) ? customerNameCitizenTemp : customerName;
            return customerName;
        }
        private BillConfig GetBillConfigPrice(IEnumerable<BillConfig> billConfigs, BillType billType, BillConfigPricesType pricesType, long? urbanId, long? buildingId)
        {
            return billConfigs.FirstOrDefault(x => x.BillType == billType && x.PricesType == pricesType && x.BuildingId == (buildingId ?? 0))
                ?? billConfigs.FirstOrDefault(x => x.BillType == billType && x.PricesType == pricesType && x.UrbanId == (urbanId ?? 0))
                ?? billConfigs.FirstOrDefault(x => x.BillType == billType && x.PricesType == pricesType && x.IsDefault == true)
                ?? billConfigs.FirstOrDefault(x => x.BillType == billType && x.PricesType == pricesType);
        }
        private double CalculateDebt(IEnumerable<UserBill> userBills, BillType billType, DateTime periodDebt)
        {
            return userBills?
                .Where(bill =>
                    bill.LastCost.HasValue &&
                    bill.BillType == billType)
                .Where(bill =>
                    (bill.Period.Value.Month == periodDebt.Month &&
                    bill.Period.Value.Year == periodDebt.Year) &&
                    bill.Status == UserBillStatus.Debt)
                .Sum(bill => bill.LastCost.Value) ?? 0;
        }
        private List<UserBill> GetListUserBillByApartment(string apartmentCode, List<long> billIds)
        {
            return _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => billIds.Contains(x.Id))
                     .ToList();
        }
        private CitizenTemp GetCitizenTempByApartment(string apartmentCode)
        {
            return _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
        }
        private BillConfig GetVehicleBillConfig(List<BillConfig> parkingBill, long? buildingId, long? urbanId, VehicleType vehicleType, bool isPrivate)
        {
            return GetBillConfig(parkingBill, buildingId, urbanId, vehicleType, isPrivate) ??
                   GetBillConfig(parkingBill, urbanId, buildingId, vehicleType, isPrivate) ??
                   GetBillConfig(parkingBill, null, null, vehicleType, null);
        }
        private BillConfig GetBillConfig(List<BillConfig> parkingBill, long? buildingId, long? urbanId, VehicleType vehicleType, bool? isPrivate)
        {
            return parkingBill.FirstOrDefault(x =>
                (buildingId.HasValue && x.BuildingId == buildingId.Value || urbanId.HasValue && x.UrbanId == urbanId.Value) &&
                x.VehicleType == vehicleType &&
                (!isPrivate.HasValue || x.IsPrivate == isPrivate.Value));
        }
        private (long?, long?) GetUrbanAndBuilding(string apartmentCode)
        {
            Apartment? apartment = _apartmentRepository.FirstOrDefault(x => x.ApartmentCode == apartmentCode);
            long? buildingId = apartment?.BuildingId ?? null;
            long? urbanId = apartment?.UrbanId ?? null;
            return (buildingId, urbanId);
        }
        private string GetVehicleNameById(long parkingId)
        {
            return _citizenParkingRepository.FirstOrDefault(parkingId)?.ParkingName ?? "";
        }
        private string GetVehicleName(CitizenVehiclePas vehicle)
        {
            int level = vehicle.level ?? 0;
            string vehicleName = string.Empty;
            switch (vehicle.vehicleType)
            {
                case VehicleType.Car:
                    vehicleName = vehicle.parkingId == null
                        ? $"Ô tô"
                        : GetVehicleNameById(vehicle.parkingId.Value);
                    break;
                case VehicleType.Motorbike:
                    vehicleName = $"Xe máy";
                    break;
                case VehicleType.Bicycle:
                    vehicleName = $"Xe đạp";
                    break;
                default:
                    break;
            };
            return vehicleName;
        }

        private string GetVehicleNameAndParking(CitizenVehiclePas vehicle)
        {
            int level = vehicle.level ?? 1;
            string vehicleName = string.Empty;
            switch (vehicle.vehicleType)
            {
                case VehicleType.Car:
                    vehicleName = vehicle.parkingId == null
                        ? $"Ô tô số {level}"
                        : "Ô tô - " + GetVehicleNameById(vehicle.parkingId.Value);
                    break;
                case VehicleType.Motorbike:
                    vehicleName = vehicle.parkingId == null ? $"Xe máy số {level}" : "Xe máy - " + GetVehicleNameById(vehicle.parkingId.Value);
                    break;
                case VehicleType.Bicycle:
                    vehicleName = $"Xe đạp số {level}";
                    break;
                default:
                    break;
            };
            return vehicleName;
        }

        private string GetVehicleTypeName(CitizenVehiclePas vehicle)
        {
            switch (vehicle.vehicleType)
            {
                case VehicleType.Car:
                    return "Ô tô";
                case VehicleType.Motorbike:
                    return "Xe máy";
                case VehicleType.Bicycle:
                    return "Xe đạp";
                default:
                    return "Xe khác";
            };
        }

        private string GetListVehicle(int? tenantId)
        {
            switch (tenantId)
            {
                case 62:  // hudlands
                    return " <tr>\r\n          " +
                        "<td style=\"width: 32px; padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{INDEX}</td>\r\n          " +
                        "<td style=\"padding: 6px; text-align: left; white-space: nowrap; border-width: 1px; border-style: solid;\">{VEHICLE_NAME}</td>\r\n          " +
                        "<td style=\"padding: 6px; text-align: left; white-space: nowrap; border-width: 1px; border-style: solid;\">{VEHICLE_CODE}</td>\r\n          " +
                        "<td style=\"padding: 6px; text-align: left; white-space: nowrap; border-width: 1px; border-style: solid;\">Phí xe T{PERIOD_MONTH}/{PERIOD_YEAR}</td>\r\n          " +
                        "<td style=\"padding: 6px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{NUMBER_MONTH_PARKING}</td>\r\n          " +
                        "<td style=\"padding: 6px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{UNIT_PARKING_ELEMENT}</td>\r\n          " +
                        "<td style=\"padding: 6px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_PARKING_ELEMENT}</td>\r\n        " +
                        "</tr>";
                default:
                    return "";
            }
        }
        private string GetTaxCode(CitizenTemp citizenTemp)
        {
            return citizenTemp?.TaxCode ?? "";
        }
        private string GetOtherBillName(UserBill otherBill, List<BillConfig> billConfigs)
        {
            var billConfig = billConfigs.FirstOrDefault(b => b.Id == otherBill.BillConfigId);
            return billConfig != null ? billConfig.Title : "Hóa đơn khác";
        }
        private string MapBillTypeToOtherName(BillType billType)
        {
            switch (billType)
            {
                case BillType.Electric:
                    return "Điện";
                case BillType.Lighting:
                    return "Ánh sáng";
                case BillType.Monthly:
                    return "Hàng tháng";
                case BillType.Other:
                    return "Khác";
                default:
                    return "";
            }
        }
        private decimal GetIndexHead(UserBill userBill) => userBill?.IndexHeadPeriod ?? 0;
        private decimal GetIndexEnd(UserBill userBill) => userBill?.IndexEndPeriod ?? 0;
        private decimal GetTotalIndex(UserBill userBill) => userBill?.TotalIndex ?? 0;
        private string GetPaymentType(UserBillPaymentMethod method)
        {
            if (method == UserBillPaymentMethod.Direct)
            {
                return "TM";
            }
            else
            {
                return "CK";
            }
        }

        private string GetQuarter(int month)
        {
            if (month < 4)
            {
                return "Q1";
            }
            else if (month < 7)
            {
                return "Q2";
            }
            else if (month < 10)
            {
                return "Q3";
            }
            else
            {
                return "Q4";
            }
        }

        private string GetListWaterBill(int? tenantId)
        {
            switch (tenantId)
            {
                case 62:  // hudlands
                    return "<tr>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">{WATER_CONSUMPTION_RANGE}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{WATER_CONSUMPTION_QUANTITY}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{UNIT_PRICE_WATER}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_WATER_UNPAID}</td>\r\n          </tr>";
                case 94:  // vinasinco 
                    return "<tr> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_HEAD_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_END_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {TOTAL_INDEX_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {WATER_UNIT_RANGE} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {UNIT_PRICE_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {COST_WATER_UNPAID} </td> </tr>";
                default:
                    return "";
            }
        }
        private string GetListElectricBill(int? tenantId)
        {
            switch (tenantId)
            {
                case 62:  // hudlands
                    return "<tr>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">{ELECTRIC_CONSUMPTION_RANGE}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{ELECTRIC_CONSUMPTION_QUANTITY}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{UNIT_PRICE_ELECTRIC}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_ELECTRIC_UNPAID}</td>\r\n          </tr>";
                case 94:  // vinasinco 
                    return "<tr> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_HEAD_ELECTRIC} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_END_ELECTRIC} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {TOTAL_INDEX_ELECTRIC} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {ELECTRIC_UNIT_RANGE} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {UNIT_PRICE_ELECTRIC} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {COST_ELECTRIC_UNPAID} </td> </tr>";
                default:
                    return "";
            }
        }
        private string GetListOtherBill(int? tenantId)
        {
            switch (tenantId)
            {
                case 62:  // hudlands
                    return "<tr>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">{ELECTRIC_CONSUMPTION_RANGE}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{ELECTRIC_CONSUMPTION_QUANTITY}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{UNIT_PRICE_ELECTRIC}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_ELECTRIC_UNPAID}</td>\r\n          </tr>";
                case 94:  // vinasinco 
                    return "<tr> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: center; font-weight: 600; \"> {INDEX} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; font-weight: 600; \"> {OTHER_BILL_NAME} </td> <td colspan=\"5\" style=\"border-width: 1px; border-style: solid\"> <table style=\" width: 100%; table-layout: auto; border-collapse: collapse; border-style: hidden; \" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\"> <tr> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \"> Số tháng </td> <td style=\" width: 100px; border-width: 1px; border-style: solid; padding: 6px; text-align: right; font-size: 14px; \"> Đơn giá/căn hộ </td> <td style=\" width: 164px; border-width: 1px; border-style: solid; padding: 6px; text-align: right; \"> Thành tiền </td> </tr> <tr> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \"> {NUMBER_MONTH_OTHER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \"> {UNIT_PRICE_OTHER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \"> {COST_OTHER_BILL} </td> </tr> </table> </td> </tr>";
                default:
                    return "";
            }
        }

        private string GetBuildingName(long? buildingId)
        {
            if (buildingId == null) return "";
            var building = _appOrganizationUnitRepository.FirstOrDefault(x => x.Id == buildingId);
            if (building != null) return building.DisplayName;
            return "";
        }

        private string GetUrbanName(long? urbanId)
        {
            if (urbanId == null) return "";
            var urban = _appOrganizationUnitRepository.FirstOrDefault(x => x.Id == urbanId);
            if (urban != null) return urban.DisplayName;
            return "";
        }
        #endregion
    }
}
