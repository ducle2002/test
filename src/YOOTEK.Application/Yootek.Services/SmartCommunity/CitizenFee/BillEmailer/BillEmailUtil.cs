using Abp.Application.Services;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Abp.Net.Mail;
using Abp.Organizations;
using Abp.UI;
using Yootek.Common.Enum;
using Yootek.Configuration;
using Yootek.Emailing;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.SmartCommunity.Phidichvu;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using NumToWords.VietNamese;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Math;
using Yootek.Services.SmartCommunity.BillingInvoice;

namespace Yootek.Services
{
    public class CitizenVehiclePas
    {
        public long id { get; set; }
        public string vehicleName { get; set; }
        public VehicleType vehicleType { get; set; }
        public string vehicleCode { get; set; }
        public long? parkingId { get; set; }
        public decimal? cost { get; set; }
        public int? level { get; set; }
        public DateTime? registrationDate { get; set; }
        public DateTime? expirationDate { get; set; }
    }


    public interface IBillEmailUtil : IApplicationService
    {
        Task<StringBuilder> GetTemplateByApartment(string apartmentCode, DateTime period);
        Task SendEmailToApartmentAsync(string apartmentCode, DateTime? tim, int? tenantId);
        Task<StringBuilder> CreateTemplateHTC(string apartmentCode, DateTime time, int? tenantId);
        Task<StringBuilder> CreateTemplateNOXH(string apartmentCode, DateTime time, int? tenantId);
        Task<StringBuilder> CreateTemplate(string apartmentCode, DateTime time, int? tenantId, CitizenTemp citizenTemp = null);
        Task<StringBuilder> CreateTemplateHTS(string apartmentCode, DateTime time, int? tenantId);
        Task<StringBuilder> CreateTemplateNC(string apartmentCode, DateTime period, int? tenantId);
        Task<StringBuilder> CreateTemplateVina22(string apartmentCode, DateTime period, int? tenantId);
        Task<StringBuilder> CreateTemplateTB(string apartmentCode, DateTime time, int? tenantId);
    }

    public class BillEmailUtil : YootekAppServiceBase, IBillEmailUtil
    {
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateProvider _emailTemplateProvider;
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<CitizenParking, long> _citizenParkingRepository;
        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepository;
        private readonly IRepository<BillConfig, long> _billConfigRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;
        private readonly IRepository<BillDebt, long> _billDebtRepos;
        private readonly IRepository<UserBillPayment, long> _billPaymentRepos;
        private readonly IRepository<BillEmailHistory, long> _billEmailHistoryRepos;
        private readonly IRepository<Apartment, long> _apartmentRepos;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IConfiguration _configuration;
        private readonly ITemplateBillAppService _templateBillAppService;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;

        public BillEmailUtil(
             IEmailSender emailSender,
             IEmailTemplateProvider emailTemplateProvider,
             IRepository<UserBill, long> userBillRepository,
             IRepository<BillConfig, long> billConfigRepository,
             IRepository<CitizenTemp, long> citizenTempRepository,
             IRepository<BillDebt, long> billDebtRepos,
             IRepository<CitizenVehicle, long> citizenVehicleRepository,
             IRepository<BillEmailHistory, long> billEmailHistoryRepos,
             IAppConfigurationAccessor configurationAccessor,
             IRepository<Apartment, long> apartmentRepos,
             IRepository<CitizenParking, long> citizenParkingRepository,
             ITemplateBillAppService templateBillAppService,
             IConfiguration configuration,
             IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
             IRepository<UserBillPayment, long> billPaymentRepos
            )
        {
            _emailSender = emailSender;
            _emailTemplateProvider = emailTemplateProvider;
            _userBillRepository = userBillRepository;
            _citizenVehicleRepository = citizenVehicleRepository;
            _billConfigRepository = billConfigRepository;
            _citizenTempRepository = citizenTempRepository;
            _billDebtRepos = billDebtRepos;
            _billEmailHistoryRepos = billEmailHistoryRepos;
            _appConfiguration = configurationAccessor.Configuration;
            _apartmentRepos = apartmentRepos;
            _citizenParkingRepository = citizenParkingRepository;
            _templateBillAppService = templateBillAppService;
            _configuration = configuration;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _billPaymentRepos = billPaymentRepos;
        }

        public async Task<StringBuilder> GetTemplateByApartment(string apartmentCode, DateTime period)
        {
            var template = new StringBuilder();
            if (AbpSession.TenantId == 64)
            {
                template = await CreateTemplateHTC(apartmentCode, period, AbpSession.TenantId);
            }
            else if (AbpSession.TenantId == 65)
            {
                template = await CreateTemplateNOXH(apartmentCode, period, AbpSession.TenantId);
            }
            else if (AbpSession.TenantId == 62)
            {
                template = await CreateTemplateHTS(apartmentCode, period, AbpSession.TenantId);
            }
            else if (AbpSession.TenantId == 43)
            {
                template =
                   await CreateTemplateNC(apartmentCode, period, AbpSession.TenantId);
            }
            else if (AbpSession.TenantId == 63)
            {
                template =
                   await CreateTemplateVina22(apartmentCode, period, AbpSession.TenantId);
            }
            else if (AbpSession.TenantId == 80)
            {
                template =
                    await CreateTemplateTB(apartmentCode, period, AbpSession.TenantId);
            }
            else if (AbpSession.TenantId == 94)
            {
                template =
                   await CreateTemplateVinasinco(apartmentCode, period, AbpSession.TenantId);
            }
            else if (AbpSession.TenantId == 97)
            {
                template = await CreateTemplateHiyori(apartmentCode, period,
                   AbpSession.TenantId);

            }
            else if (AbpSession.TenantId == 87 || AbpSession.TenantId == 82)
            {
                template = await CreateTemplateMhomes(apartmentCode, period,
                   AbpSession.TenantId);

            }
            else if (AbpSession.TenantId == 84)
            {
                template = await CreateTemplateTanPhong(apartmentCode, period,
                   AbpSession.TenantId);

            }
            else if (AbpSession.TenantId == 19)
            {
                template = await CreateTemplate(apartmentCode, period,
                   AbpSession.TenantId);

            }
            else if (AbpSession.TenantId == 49)
            {
                template = await CreateTemplateTrungDo(apartmentCode, period,
                   AbpSession.TenantId);

            }
            else if (AbpSession.TenantId == 114)
            {
                template = await CreateTemplateLT(apartmentCode, period,
                   AbpSession.TenantId);

            }
            else
            {
                template = await CreateTemplateGeneral(apartmentCode, period,
                   AbpSession.TenantId);

            }

            return template;
        }

        [RemoteService(false)]
        public async Task SendEmailToApartmentAsync(string apartmentCode, DateTime? tim, int? tenantId)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    if (tim == null) return;
                    var time = tim.Value;

                    //var template = _billEmailHistoryRepos.FirstOrDefault(x => x.ApartmentCode == apartmentCode && x.Period.Year == time.Year && x.Period.Month == time.Month);
                    //if (template != null) return;

                    var citizenTemp = _citizenTempRepository.FirstOrDefault(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor && x.IsStayed == true);
                    if (citizenTemp == null)
                    {
                        citizenTemp = _citizenTempRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor).OrderByDescending(x => x.OwnerGeneration).FirstOrDefault();

                    }

                    var currentPeriod = string.Format("{0:MM/yyyy}", time);
                    var emailTemplate = await GetTemplateByApartment(apartmentCode, time);
                    var history = new BillEmailHistory()
                    {
                        ApartmentCode = apartmentCode,
                        CitizenTempId = citizenTemp != null ? citizenTemp.Id : null,
                        Period = time,
                        EmailTemplate = emailTemplate.ToString(),
                        TenantId = tenantId

                    };
                    await _billEmailHistoryRepos.InsertAsync(history);

                    if (citizenTemp != null && !citizenTemp.Email.IsNullOrEmpty())
                    {
                        Logger.Fatal(_emailSender.ToJsonString());
                        await _emailSender.SendAsync(new MailMessage
                        {
                            To = { citizenTemp.Email },
                            Subject = $"Thông báo hóa đơn dịch vụ tháng {currentPeriod}",
                            Body = emailTemplate.ToString().Replace("OCTYPE html>", ""),
                            IsBodyHtml = true
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Fatal("Send email bill " + apartmentCode + ": " + e.Message);
                Logger.Fatal(JsonConvert.SerializeObject(e));
            }

        }

        
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplate(string apartmentCode, DateTime time, int? tenantId, CitizenTemp citizenTemp = null)
        {
            var nextMonth = time.AddMonths(1);
            var sender = tenantId == 19 ? "BQL " + "Sapphire Ha Long" : "Ban quản lý";

            var priceslist = _billConfigRepository.GetAllList();
            var bills = _userBillRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.Period.Value.Month == time.Month && x.Period.Value.Year == time.Year).ToList();
            var billDebt = new List<BillDebt>();
            citizenTemp = citizenTemp ?? _citizenTempRepository.FirstOrDefault(x =>
                      x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor &&
                      x.IsStayed == true);
            billDebt = citizenTemp != null ? _billDebtRepos.GetAllList(x => x.ApartmentCode == apartmentCode && x.CitizenTempId == citizenTemp.Id) : null;

            var electrictbill = bills.FirstOrDefault(x => x.BillType == BillType.Electric);
            var waterbill = bills.FirstOrDefault(x => x.BillType == BillType.Water);
            var debt = 0.0;
            if (billDebt != null && billDebt.Count > 0)
            {
                foreach (var bill in billDebt) debt += bill.DebtTotal.Value;
            }

            var DueDateElectric = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricEndPeriodDay, AbpSession.TenantId.Value);
            //  var DueMonthElectric = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.UserBillConfig.DueMonthElectric, AbpSession.TenantId.Value);
            var DueDateWater = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.WaterEndPeriodDay, AbpSession.TenantId.Value);
            // var DueMonthWater = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.UserBillConfig.DueMonthWater, AbpSession.TenantId.Value);

            DueDateWater = ValidateAndSetDueDate(DueDateWater, waterbill, nextMonth);
            DueDateElectric = ValidateAndSetDueDate(DueDateElectric, electrictbill, nextMonth);

            var emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));

            var dueDateEndE = new DateTime(time.Year, time.Month, DueDateElectric);
            var dueDateHeadE = dueDateEndE.AddDays(1).AddMonths(-1);

            var dueDateEndW = new DateTime(time.Year, time.Month, DueDateWater);
            var dueDateHeadW = dueDateEndW.AddDays(1).AddMonths(-1);

            var currentPeriod = string.Format("{0:MM/yyyy}", time);
            var currentPeriodDot = string.Format("{0:MM.yyyy}", time);
            var currentDay = string.Format("{0:dd/MM/yyyy}", time);
            var nextPeriod = string.Format("{0:MM.yyyy}", nextMonth);

            var priceE = priceslist.FirstOrDefault(x => x.BillType == BillType.Electric && x.PricesType == BillConfigPricesType.Level);
            var percentsE = priceslist.Where(x => x.BillType == BillType.Electric && x.PricesType == BillConfigPricesType.Percentage).ToList();
            var priceW = priceslist.FirstOrDefault(x => x.BillType == BillType.Water && x.PricesType == BillConfigPricesType.Level);
            var percentsW = priceslist.Where(x => x.BillType == BillType.Water && x.PricesType == BillConfigPricesType.Percentage).ToList();

            var managementBill = bills.FirstOrDefault(x => x.BillType == BillType.Manager);
            var priceM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport);
            var percentM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Percentage);

            var billConfigPropertiesE = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceE.Properties);
            var billConfigPropertiesW = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceW.Properties);
            var billConfigPropertiesM = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceM.Properties);

            emailTemplate.Replace("{PERIOD_HEAD}", currentPeriod);
            emailTemplate.Replace("{NOTI_DAY}", currentDay);
            emailTemplate.Replace("{CUSTOMER_NAME}", citizenTemp != null ? citizenTemp.FullName : "");
            emailTemplate.Replace("{APARTMENT_CODE}", apartmentCode);
            emailTemplate.Replace("{SENDER_NAME}", sender);
            emailTemplate.Replace("{E_HEAD_PERIOD}", string.Format("{0:dd.MM.yyyy}", dueDateHeadE));
            emailTemplate.Replace("{E_END_PERIOD}", string.Format("{0:dd.MM.yyyy}", dueDateEndE));
            emailTemplate.Replace("{DEBT_AMOUNT}", debt > 0 ? debt + "" : "");

            emailTemplate.Replace("{PERIOD_MONTH}", currentPeriodDot);
            emailTemplate.Replace("{E_HEAD_INDEX}", electrictbill != null ? (int)electrictbill.IndexHeadPeriod + "" : "0");
            emailTemplate.Replace("{E_END_INDEX}", electrictbill != null ? (int)electrictbill.IndexEndPeriod + "" : "0");
            emailTemplate.Replace("{E_TOTAL_INDEX}", electrictbill != null ? (int)electrictbill.TotalIndex + "" : "0");


            emailTemplate.Replace("{W_HEAD_PERIOD}", string.Format("{0:dd.MM.yyyy}", dueDateHeadW));
            emailTemplate.Replace("{W_END_PERIOD}", string.Format("{0:dd.MM.yyyy}", dueDateEndW));
            emailTemplate.Replace("{W_HEAD_INDEX}", waterbill != null ? (int)waterbill.IndexHeadPeriod + "" : "0");
            emailTemplate.Replace("{W_END_INDEX}", waterbill != null ? (int)waterbill.IndexEndPeriod + "" : "0");
            emailTemplate.Replace("{W_TOTAL_INDEX}", waterbill != null ? (int)waterbill.TotalIndex + "" : "0");
            //1
            var resultE = 0;
            var e_percent = 0.0;
            if (billConfigPropertiesE != null && billConfigPropertiesE.Prices != null)
            {
                var brk = false;
                var amount = electrictbill != null ? (int)electrictbill.TotalIndex.Value : 0;
                var index = 0;
                foreach (var level in billConfigPropertiesE.Prices)
                {
                    index++;
                    if (!brk)
                    {
                        if (amount < level.To)
                        {
                            var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                            emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                            resultE += (int)(level.Value * kla);
                            brk = true;
                            continue;
                        }

                        if (!level.To.HasValue)
                        {
                            var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                            emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                            resultE += (int)(level.Value * kla);
                            brk = true;
                            continue;
                        }

                        var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                        emailTemplate.Replace("{E_INDEX" + index + "}", kl + "");
                        emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                        resultE += (int)(level.Value * kl);
                    }
                    else
                    {
                        emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                    }
                }

                for (var i = 1; i < 7; i++)
                {
                    emailTemplate.Replace("{E_INDEX" + i + "}", "");
                    emailTemplate.Replace("{E_PRICE" + i + "}", "");
                    emailTemplate.Replace("{E_AMOUNT" + i + "}", "");
                }

                emailTemplate.Replace("{E_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultE));

                if (percentsE != null)
                {
                    foreach (var percent in percentsE)
                    {
                        var x = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(percent.Properties);
                        e_percent = e_percent + resultE * (x.Prices[0].Value / 100.0);
                    }
                    e_percent = Convert.ToInt32(e_percent);
                    emailTemplate.Replace("{E_PERCENT}", string.Format("{0:#,#.##}", e_percent));
                }
                emailTemplate.Replace("{E_INTO_MONEY}", string.Format("{0:#,#.##}", resultE + e_percent));
            }

            var resultW = 0;
            var W_percent = 0.0;
            if (billConfigPropertiesW != null && billConfigPropertiesW.Prices != null)
            {
                var brk = false;
                var amount = waterbill != null ? (int)waterbill.TotalIndex.Value : 0;
                var index = 0;
                foreach (var level in billConfigPropertiesW.Prices)
                {
                    index++;
                    if (!brk)
                    {
                        if (amount < level.To)
                        {
                            var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                            resultW += (int)(level.Value * kla);
                            brk = true;
                            continue;
                        }

                        if (!level.To.HasValue)
                        {
                            var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                            resultW += (int)(level.Value * kla);
                            brk = true;
                            continue;
                        }

                        var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                        emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                        emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                        resultW += (int)(level.Value * kl);
                    }
                    else
                    {
                        emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                    }
                }
                for (var i = 1; i < 7; i++)
                {
                    emailTemplate.Replace("{W_INDEX" + i + "}", "");
                    emailTemplate.Replace("{W_PRICE" + i + "}", "");
                    emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                }

                emailTemplate.Replace("{W_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultW));

                if (percentsW != null)
                {
                    foreach (var percent in percentsW)
                    {
                        var x = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(percent.Properties);
                        W_percent = W_percent + resultW * (x.Prices[0].Value / 100.0);
                    }
                    W_percent = Convert.ToInt32(W_percent);
                    emailTemplate.Replace("{W_PERCENT}", string.Format("{0:#,#.##}", W_percent));
                }
                emailTemplate.Replace("{W_INTO_MONEY}", string.Format("{0:#,#.##}", resultW + W_percent));
            }

            var amountM = 0;
            var m_percent = 0;
            if (managementBill != null)
            {
                emailTemplate.Replace("{ACREAGE_APARTMENT}", (int)managementBill.TotalIndex + "");
                emailTemplate.Replace("{PRICE_MANAGEMENT}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[0].Value));
                amountM = Convert.ToInt32((int)managementBill.TotalIndex * billConfigPropertiesM.Prices[0].Value);
                emailTemplate.Replace("{MANAGEMENT_AMOUNT}", string.Format("{0:#,#.##}", amountM));

                if (percentM != null)
                {
                    var x = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(percentM.Properties);
                    m_percent = Convert.ToInt32(amountM * (x.Prices[0].Value / 100.0));
                    emailTemplate.Replace("{PERCENT_MANAGEMENT}", string.Format("{0:#,#.##}", m_percent));
                    emailTemplate.Replace("{INTO_MONEY_MANAGEMENT}", string.Format("{0:#,#.##}", amountM + m_percent));
                    emailTemplate.Replace("{ORTHER_FEE_AMOUNT}", string.Format("{0:#,#.##}", amountM + m_percent));
                }
            }
            else
            {
                emailTemplate.Replace("{ACREAGE_APARTMENT}", "");
                emailTemplate.Replace("{PRICE_MANAGEMENT}", "");
                emailTemplate.Replace("{MANAGEMENT_AMOUNT}", "");
                emailTemplate.Replace("{PERCENT_MANAGEMENT}", "");
                emailTemplate.Replace("{INTO_MONEY_MANAGEMENT}", "");
                emailTemplate.Replace("{ORTHER_FEE_AMOUNT}", "");
            }

            var totalMoney = Convert.ToInt32(amountM + m_percent + e_percent + resultE + resultW + W_percent);
            emailTemplate.Replace("{TOTAL_MONEY}", string.Format("{0:#,#.##}", totalMoney));
            var textTotalMoney = VietNameseConverter.FormatCurrency((decimal)totalMoney);
            emailTemplate.Replace("{TOTAL_MONEY_TEXT}", textTotalMoney);
            emailTemplate.Replace("{PREVIOUS_BALANCE}", "");
            return emailTemplate;
        }

        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateHTC(string apartmentCode, DateTime time, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {

                var currentDay = string.Format("{0:dd/MM/yyyy}", DateTime.Now);

                var priceslist = _billConfigRepository.GetAllList();
                var bills = _userBillRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.Period.Value.Month == time.Month && x.Period.Value.Year == time.Year).ToList();
                var citizenTemp = _citizenTempRepository.FirstOrDefault(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor && x.IsStayed == true);
                if (citizenTemp == null)
                {
                    citizenTemp = _citizenTempRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor).OrderByDescending(x => x.OwnerGeneration).FirstOrDefault();

                }
                var billDebt = new List<BillDebt>();
                billDebt = citizenTemp != null ? _billDebtRepos.GetAllList(x => x.ApartmentCode == apartmentCode && x.CitizenTempId == citizenTemp.Id) : null;


                var debt = 0.0;
                if (billDebt != null && billDebt.Count > 0)
                {
                    foreach (var bill in billDebt) debt += bill.DebtTotal.Value;
                }



                var emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                var currentPeriod = string.Format("{0:MM/yyyy}", time);
                var today = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
                emailTemplate.Replace("{PERIOD_MONTH}", currentPeriod + "");
                emailTemplate.Replace("{DAY}", today + "");
                emailTemplate.Replace("{APARTMENT_CODE}", apartmentCode + "");

                var p_money = 0;
                var parkingBill = bills.FirstOrDefault(x => x.BillType == BillType.Parking);
                var trashBill = bills.FirstOrDefault(x => x.BillType == BillType.Residence);
                var citizenName = "";

                var priceP = priceslist.FirstOrDefault(x => x.BillType == BillType.Parking && x.PricesType == BillConfigPricesType.Parking && x.IsDefault == true);
                if (priceP == null)
                {
                    priceP = priceslist.FirstOrDefault(x => x.BillType == BillType.Parking && x.PricesType == BillConfigPricesType.Parking);

                }


                if (priceP != null)
                {
                    var billConfigPropertiesM = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceP.Properties);
                    if (billConfigPropertiesM != null && billConfigPropertiesM.Prices.Length == 4)
                    {
                        emailTemplate.Replace("{PRICE_CAR}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[0].Value));
                        emailTemplate.Replace("{PRICE_MOTOR}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[1].Value));
                        emailTemplate.Replace("{PRICE_BIKE}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[2].Value));
                        emailTemplate.Replace("{PRICE_OTHER}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[3].Value));
                    }
                    else
                    {
                        emailTemplate.Replace("{PRICE_CAR}", "");
                        emailTemplate.Replace("{PRICE_MOTOR}", "");
                        emailTemplate.Replace("{PRICE_BIKE}", "");
                        emailTemplate.Replace("{PRICE_OTHER}", "");
                    }

                }
                else
                {
                    emailTemplate.Replace("{PRICE_CAR}", "");
                    emailTemplate.Replace("{PRICE_MOTOR}", "");
                    emailTemplate.Replace("{PRICE_BIKE}", "");
                    emailTemplate.Replace("{PRICE_OTHER}", "");
                }

                if (parkingBill != null)
                {
                    p_money = (int)parkingBill.LastCost;
                    emailTemplate.Replace("{P_MONEY}", string.Format("{0:#,#.##}", (int)parkingBill.LastCost));
                    emailTemplate.Replace("{CAR_NUMBER}", parkingBill.CarNumber + "");
                    emailTemplate.Replace("{MOTOR_NUMBER}", parkingBill.MotorbikeNumber + "");
                    emailTemplate.Replace("{BIKE_NUMBER}", parkingBill.BicycleNumber + "");
                    emailTemplate.Replace("{OTHER_NUMBER}", parkingBill.OtherVehicleNumber + "");
                    try
                    {
                        var properties = JsonConvert.DeserializeObject<dynamic>(parkingBill.Properties);
                        citizenName = properties.customerName;
                    }
                    catch { }

                }
                else
                {
                    emailTemplate.Replace("{P_MONEY}", "");
                    emailTemplate.Replace("{CAR_NUMBER}", "");
                    emailTemplate.Replace("{MOTOR_NUMBER}", "");
                    emailTemplate.Replace("{BIKE_NUMBER}", "");
                    emailTemplate.Replace("{OTHER_NUMBER}", "");
                }

                var t_money = 0;
                if (trashBill != null)
                {
                    t_money = (int)trashBill.LastCost;
                    emailTemplate.Replace("{TRASH_MONEY}", string.Format("{0:#,#.##}", (int)trashBill.LastCost));
                }
                else
                {
                    emailTemplate.Replace("{TRASH_MONEY}", "");
                }

                // Management
                var m_money = 0;
                var managementBill = bills.FirstOrDefault(x => x.BillType == BillType.Manager);
                var priceM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport && x.IsDefault == true);
                if (priceM == null)
                {
                    priceM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport);
                }

                if (priceM != null)
                {
                    var billConfigPropertiesM = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceM.Properties);
                    emailTemplate.Replace("{M_PRICE}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[0].Value));
                }
                else
                {
                    emailTemplate.Replace("{M_PRICE}", "");
                }

                if (managementBill != null)
                {
                    m_money = (int)managementBill.LastCost;
                    emailTemplate.Replace("{ACREAGE_APARTMENT}", (decimal)managementBill.TotalIndex + "");
                    emailTemplate.Replace("{M_MONEY}", string.Format("{0:#,#.##}", (int)managementBill.LastCost));
                    if (string.IsNullOrEmpty(citizenName))
                    {
                        try
                        {
                            var properties = JsonConvert.DeserializeObject<dynamic>(managementBill.Properties);
                            citizenName = properties.customerName;
                        }
                        catch { }
                    }

                }
                else
                {
                    emailTemplate.Replace("{ACREAGE_APARTMENT}", "");
                    emailTemplate.Replace("{M_MONEY}", "");
                }

                var totalMoney = Convert.ToInt32(p_money + m_money + t_money);
                emailTemplate.Replace("{DAY}", currentDay);
                emailTemplate.Replace("{TOTAL_1}", totalMoney > 0 ? string.Format("{0:#,#.##}", totalMoney) : "0");

                emailTemplate.Replace("{DEBT}", debt > 0 ? string.Format("{0:#,#.##}", debt) : "0");
                emailTemplate.Replace("{CUSTOMER_NAME}", citizenName + "");

                var total2 = Convert.ToInt32(totalMoney + debt);
                emailTemplate.Replace("{TOTAL_2}", total2 > 0 ? string.Format("{0:#,#.##}", total2) : "0");

                return emailTemplate;

            }
        }

        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateNOXH(string apartmentCode, DateTime time, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {

                var currentDay = string.Format("{0:dd/MM/yyyy}", DateTime.Now);


                var priceslist = _billConfigRepository.GetAllList();
                var bills = _userBillRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.Period.Value.Month == time.Month && x.Period.Value.Year == time.Year).ToList();
                var citizenTemp = _citizenTempRepository.FirstOrDefault(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor && x.IsStayed == true);
                if (citizenTemp == null)
                {
                    citizenTemp = _citizenTempRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor).OrderByDescending(x => x.OwnerGeneration).FirstOrDefault();

                }
                var billDebt = new List<BillDebt>();
                billDebt = citizenTemp != null ? _billDebtRepos.GetAllList(x => x.ApartmentCode == apartmentCode && x.CitizenTempId == citizenTemp.Id) : null;


                var debt = 0.0;
                if (billDebt != null && billDebt.Count > 0)
                {
                    foreach (var bill in billDebt) debt += bill.DebtTotal.Value;
                }



                var emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                var currentPeriod = string.Format("{0:MM/yyyy}", time);
                var today = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
                emailTemplate.Replace("{PERIOD_MONTH}", currentPeriod + "");
                emailTemplate.Replace("{DAY}", today + "");
                emailTemplate.Replace("{APARTMENT_CODE}", apartmentCode + "");

                var p_money = 0;
                var parkingBill = bills.FirstOrDefault(x => x.BillType == BillType.Parking);
                var waterBill = bills.FirstOrDefault(x => x.BillType == BillType.Water);
                var citizenName = "";

                var priceP = priceslist.FirstOrDefault(x => x.BillType == BillType.Parking && x.PricesType == BillConfigPricesType.Parking && x.IsDefault == true);
                if (priceP == null)
                {
                    priceP = priceslist.FirstOrDefault(x => x.BillType == BillType.Parking && x.PricesType == BillConfigPricesType.Parking);

                }


                if (priceP != null)
                {
                    var billConfigPropertiesM = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceP.Properties);
                    if (billConfigPropertiesM != null && billConfigPropertiesM.Prices.Length == 4)
                    {
                        emailTemplate.Replace("{PRICE_CAR}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[0].Value));
                        emailTemplate.Replace("{PRICE_MOTOR}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[1].Value));
                        emailTemplate.Replace("{PRICE_BIKE}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[2].Value));
                        emailTemplate.Replace("{PRICE_OTHER}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[3].Value));
                    }
                    else
                    {
                        emailTemplate.Replace("{PRICE_CAR}", "");
                        emailTemplate.Replace("{PRICE_MOTOR}", "");
                        emailTemplate.Replace("{PRICE_BIKE}", "");
                        emailTemplate.Replace("{PRICE_OTHER}", "");
                    }

                }
                else
                {
                    emailTemplate.Replace("{PRICE_CAR}", "");
                    emailTemplate.Replace("{PRICE_MOTOR}", "");
                    emailTemplate.Replace("{PRICE_BIKE}", "");
                    emailTemplate.Replace("{PRICE_OTHER}", "");
                }

                if (parkingBill != null)
                {
                    p_money = (int)parkingBill.LastCost;
                    emailTemplate.Replace("{P_MONEY}", string.Format("{0:#,#.##}", (int)parkingBill.LastCost));
                    emailTemplate.Replace("{CAR_NUMBER}", parkingBill.CarNumber + "");
                    emailTemplate.Replace("{MOTOR_NUMBER}", parkingBill.MotorbikeNumber + "");
                    emailTemplate.Replace("{BIKE_NUMBER}", parkingBill.BicycleNumber + "");
                    emailTemplate.Replace("{OTHER_NUMBER}", parkingBill.OtherVehicleNumber + "");
                    try
                    {
                        var properties = JsonConvert.DeserializeObject<dynamic>(parkingBill.Properties);
                        citizenName = properties.customerName;
                    }
                    catch { }

                }
                else
                {
                    emailTemplate.Replace("{P_MONEY}", "");
                    emailTemplate.Replace("{CAR_NUMBER}", "");
                    emailTemplate.Replace("{MOTOR_NUMBER}", "");
                    emailTemplate.Replace("{BIKE_NUMBER}", "");
                    emailTemplate.Replace("{OTHER_NUMBER}", "");
                }
                /// water
                var w_money = 0;
                emailTemplate.Replace("{W_HEAD_INDEX}", waterBill != null ? (int)waterBill.IndexHeadPeriod + "" : "0");
                emailTemplate.Replace("{W_END_INDEX}", waterBill != null ? (int)waterBill.IndexEndPeriod + "" : "0");
                emailTemplate.Replace("{W_TOTAL_INDEX}", waterBill != null ? (int)waterBill.TotalIndex + "" : "0");

                var resultW = 0;
                var priceW = priceslist.FirstOrDefault(x => x.BillType == BillType.Water && x.PricesType == BillConfigPricesType.Level && x.IsPrivate == true);
                if (priceW == null) priceW = priceslist.FirstOrDefault(x => x.BillType == BillType.Water && x.PricesType == BillConfigPricesType.Level);


                var billConfigPropertiesW = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceW.Properties);

                if (billConfigPropertiesW != null && billConfigPropertiesW.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;
                    var index = 0;
                    foreach (var level in billConfigPropertiesW.Prices)
                    {
                        index++;
                        if (!brk)
                        {
                            if (amount < level.To)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultW += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultW += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                            resultW += (int)(level.Value * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                    }

                    emailTemplate.Replace("{W_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultW));
                }


                // Management
                var m_money = 0;
                var managementBill = bills.FirstOrDefault(x => x.BillType == BillType.Manager);
                var priceM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport && x.IsDefault == true);
                if (priceM == null)
                {
                    priceM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport);
                }

                if (priceM != null)
                {
                    var billConfigPropertiesM = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceM.Properties);
                    emailTemplate.Replace("{M_PRICE}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[0].Value));
                }
                else
                {
                    emailTemplate.Replace("{M_PRICE}", "");
                }

                if (managementBill != null)
                {
                    m_money = (int)managementBill.LastCost;
                    emailTemplate.Replace("{ACREAGE_APARTMENT}", (decimal)managementBill.TotalIndex + "");
                    emailTemplate.Replace("{M_MONEY}", string.Format("{0:#,#.##}", (int)managementBill.LastCost));
                    if (string.IsNullOrEmpty(citizenName))
                    {
                        try
                        {
                            var properties = JsonConvert.DeserializeObject<dynamic>(managementBill.Properties);
                            citizenName = properties.customerName;
                        }
                        catch { }
                    }

                }
                else
                {
                    emailTemplate.Replace("{ACREAGE_APARTMENT}", "");
                    emailTemplate.Replace("{M_MONEY}", "");
                }

                var totalMoney = Convert.ToInt32(p_money + m_money + resultW);
                emailTemplate.Replace("{DAY}", currentDay);
                emailTemplate.Replace("{TOTAL_1}", totalMoney > 0 ? string.Format("{0:#,#.##}", totalMoney) : "0");

                emailTemplate.Replace("{DEBT}", debt + "");
                emailTemplate.Replace("{CUSTOMER_NAME}", citizenName + "");

                var total2 = Convert.ToInt32(totalMoney + debt);
                emailTemplate.Replace("{TOTAL_2}", total2 > 0 ? string.Format("{0:#,#.##}", total2) : "0");

                return emailTemplate;

            }
        }

        // Nam Cường
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateNC(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                DateTime currentDate = DateTime.Now;
                string periodString = period.ToString("MM/yyyy");
                DateTime preMonthPeriod = period.AddMonths(-1);

                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year) ||
                            (x.Period.Value.Month == preMonthPeriod.Month &&
                            x.Period.Value.Year == preMonthPeriod.Year))
                     .ToList();
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);

                double managementDebt = CalculateDebt(userBills, BillType.Manager, preMonthPeriod);
                double parkingDebt = CalculateDebt(userBills, BillType.Parking, preMonthPeriod);

                double parkingMoney = GetBillCost(parkingBill);
                double managementMoney = GetBillCost(managementBill);
                double totalFeeAndDebt = parkingMoney + managementMoney + managementDebt + parkingDebt;
                decimal? acreageApartment = GetAcreageApartment(managementBill);
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                string customerName = GetCustomerName(userBills, citizenTemp);
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", $"{currentDate.Day:D2}")
                    .Replace("{MONTH_NOW}", $"{currentDate.Month:D2}")
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_PARKING}", "")
                    .Replace("{TOTAL_PRICE_MANAGEMENT_DEBT}", FormatCost(managementDebt))
                    .Replace("{TOTAL_PRICE_PARKING_DEBT}", FormatCost(parkingDebt))
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue((double?)acreageApartment))
                    .Replace("{TOTAL_PRICE_MANAGEMENT}", FormatCost(managementMoney))
                    .Replace("{TOTAL_PRICE_PARKING}", FormatCost(parkingMoney))
                    .Replace("{TOTAL_PRICE_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{TOTAL_PRICE_PAYMENT_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt))
                    .Replace("{MONTH_DUE}", $"{currentDate.Month:D2}")
                    .Replace("{YEAR_DUE}", $"{currentDate.Year}");
                return emailTemplate;
            }
        }
        // Vina 22
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateVina22(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                DateTime currentDate = DateTime.Now;
                string periodString = period.ToString("MM/yyyy");
                DateTime preMonthPeriod = period.AddMonths(-1);

                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);


                int numberMonthM = GetMonthNumber(managementBill);
                int numberMonthW = GetMonthNumber(waterBill);
                int numberMonthE = 0;
                int numberMonthP = GetMonthNumber(parkingBill);

                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                double costCardVehicleUnpaid = 0;

                List<UserBill> userBillDetbs = _userBillRepository.GetAll()
                 .Where(x => x.ApartmentCode == apartmentCode)
                 .Where(x => x.Status == UserBillStatus.Debt)
                 .ToList();
                double managementMoneyDebt = CalculateDebt(userBillDetbs, BillType.Manager, preMonthPeriod);
                double parkingMoneyDebt = CalculateDebt(userBillDetbs, BillType.Parking, preMonthPeriod);
                double waterMoneyDebt = CalculateDebt(userBillDetbs, BillType.Water, preMonthPeriod);
                double costCardVehicleMoneyDebt = 0;

                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;
                double costCardVehicle = costCardVehicleUnpaid + costCardVehicleMoneyDebt;

                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + costCardVehicleUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt + costCardVehicleMoneyDebt;
                double totalFeeAndDebt = costUnpaid + costDebt;

                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal totalIndexWater = indexEndWater - indexHeadWater;

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
                        StringBuilder vehicleRowTemplate = new("<tr>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{INDEX}</td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{APARTMENT_CODE}</td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{VEHICLE_TYPE}</td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{VEHICLE_CODE}</td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_PARKING_ELEMENT}</td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">\r\n                  <span style=\"color: #ef4444\">{P_MONTH_NUMBER}</span>\r\n              </td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_PARKING_ELEMENT}</td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_CARD_VEHICLE_ELEMENT}</td>\r\n          </tr>");
                        string vehicleName = GetVehicleName(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;

                        vehicleRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                            .Replace("{VEHICLE_TYPE}", $"{vehicleName}")
                            .Replace("{VEHICLE_CODE}", $"{vehicleCode}")
                            .Replace("{P_MONTH_NUMBER}", $"{numberMonthP}")
                            .Replace("{COST_PARKING_ELEMENT}", FormatCost((double?)vehicleCost))
                            .Replace("{COST_CARD_VEHICLE_ELEMENT}", $"{0}");
                        listRowVehicles += vehicleRowTemplate.ToString();
                    }
                }

                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }
                BillConfigPropertiesDto billWaterConfigProperties = new();
                if (priceWaterConfig?.Properties != null)
                {
                    billWaterConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceWaterConfig.Properties);
                }
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                string customerName = GetCustomerName(userBills, citizenTemp);
                //StringBuilder emailTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput() { TenantId = tenantId, Type = ETemplateBillType.BILL }));
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", $"{currentDate.Day:D2}")
                    .Replace("{MONTH_NOW}", $"{currentDate.Month:D2}")
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{MONTH_PERIOD}", $"{period.Month:D2}")
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", $"{28}")
                    .Replace("{PRE_MONTH_PERIOD}", $"{preMonthPeriod.Month:D2}")
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{M_MONTH_NUMBER}", $"{numberMonthM}")
                    .Replace("{E_MONTH_NUMBER}", $"{numberMonthE}")
                    .Replace("{P_MONTH_NUMBER}", $"{numberMonthP}")
                    .Replace("{W_MONTH_NUMBER}", $"{numberMonthW}")

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue((double?)acreageApartment))
                    .Replace("{INDEX_HEAD_WATER}", GetStringValue((double?)indexHeadWater))
                    .Replace("{INDEX_END_WATER}", GetStringValue((double?)indexEndWater))
                    .Replace("{TOTAL_INDEX_WATER}", GetStringValue((double?)totalIndexWater))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoney))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER}", FormatCost(waterMoney))

                    // parking
                    .Replace("{UNIT_PRICE_PARKING}", "")
                    .Replace("{COST_PARKING_UNPAID}", FormatCost(parkingMoney))
                    .Replace("{COST_PARKING_DEBT}", FormatCost(parkingMoneyDebt))
                    .Replace("{COST_PARKING}", FormatCost(parkingMoney))

                    // card vehicle parking
                    .Replace("{COST_CARD_VEHICLE_UNPAID}", FormatCost(costCardVehicleUnpaid))
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(costCardVehicleMoneyDebt))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(costCardVehicle))

                    // list vehicle
                    .Replace("{LIST_VEHICLE}", $"{listRowVehicles}")

                    // total money
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_PAYMENT_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt));
                return emailTemplate;
            }
        }

        // General
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateGeneral(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {

                DateTime currentDate = DateTime.Now;
                DateTime preMonthPeriod = period.AddMonths(-1);

                var paymentBill = _billPaymentRepos.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.Status == UserBillPaymentStatus.Success).OrderBy(x => x.CreationTime).Select(x => x.CreationTime).FirstOrDefault();

                var period_day_end = period.TotalDaysInMonth();
                string periodString = period.ToString("MM/yyyy");
                var head_period = new DateTime(period.Year, period.Month, 1);
                var end_period = new DateTime(period.Year, period.Month, period_day_end);
                var w_head_period = new DateTime(preMonthPeriod.Year, preMonthPeriod.Month, 1);
                var w_end_period = new DateTime(preMonthPeriod.Year, preMonthPeriod.Month, preMonthPeriod.TotalDaysInMonth());
                var paymentDay = paymentBill.ToString("dd/MM/yyyy");
                DateTime paymentDayDateTime = DateTime.ParseExact(paymentDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                // await CheckMailSettingsEmptyOrNull();
                // information of citizen who own apartment
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                string taxCode = GetTaxCode(citizenTemp);

                // userbill and billdebt for certain apartments and periods
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();

                // name and phone number of citizen who own apartment
                string customerName = GetCustomerName(userBills, citizenTemp);
                string customerPhoneNumber = citizenTemp?.PhoneNumber ?? "";
                long? buildingId = null;

                // userbill for each type
                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water);
                UserBill electricBill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);
                UserBill lightBill = userBills.FirstOrDefault(x => x.BillType == BillType.Lighting);
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();

                // day water, electric, management, ... 
                int dayWater = await GetDayWater(waterBill, currentDate);
                int dayElectric = await GetDayElectric(electricBill, currentDate);
                int dayManagement = 1;

                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                BillConfig priceElectricConfig = GetBillConfigPrice(billConfigs, BillType.Electric, BillConfigPricesType.Level);
                BillConfig priceLightConfig = GetBillConfigPrice(billConfigs, BillType.Lighting, BillConfigPricesType.Normal);

                // Check billConfig
                if (waterBill != null)
                {
                    try
                    {
                        var id = (long)JsonConvert.DeserializeObject<dynamic>(waterBill.Properties).formulas[0];
                        if (!(priceWaterConfig != null && priceWaterConfig.Id == id)) priceWaterConfig = billConfigs.FirstOrDefault(x => x.Id == id);
                    }
                    catch { }
                }

                if (managementBill != null)
                {
                    try
                    {
                        var id = (long)JsonConvert.DeserializeObject<dynamic>(managementBill.Properties).formulas[0];
                        if (!(priceManagementConfig != null && priceManagementConfig.Id == id)) priceManagementConfig = billConfigs.FirstOrDefault(x => x.Id == id);
                    }
                    catch { }
                }

                if (electricBill != null)
                {
                    try
                    {
                        var id = (long)JsonConvert.DeserializeObject<dynamic>(electricBill.Properties).formulas[0];
                        if (!(priceElectricConfig != null && priceElectricConfig.Id == id)) priceElectricConfig = billConfigs.FirstOrDefault(x => x.Id == id);
                    }
                    catch { }
                }

                // total unpaid bill amount
                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                double electricMoneyUnpaid = GetBillCost(electricBill);
                double lightMoneyUnpaid = GetBillCost(lightBill);
                double otherMoneyUnpaid = (double)otherBills.Sum(x => x.LastCost);

                // tax
                long waterMoneyBVMT = (long)(waterMoneyUnpaid * 0.1);
                long waterMoneyVAT = (long)(waterMoneyUnpaid * 0.05);
                //aden
                long eMoneyVAT = (long)(electricMoneyUnpaid * 0.08);
                long costlyElectricMoney = (long)(electricMoneyUnpaid * 0.05);

                double waterMoneyUnpaidAndTax = waterMoneyUnpaid + waterMoneyBVMT + waterMoneyVAT;
                double eMoneyUnpaidAndTax = electricMoneyUnpaid + eMoneyVAT + costlyElectricMoney; //aden

                List<UserBill> userBillDetbs = _userBillRepository.GetAll()
                  .Where(x => x.ApartmentCode == apartmentCode)
                  .Where(x => x.Status == UserBillStatus.Debt)
                  .ToList();

                // total debt bill amount for each type
                double managementMoneyDebt = CalculateDebt(userBillDetbs, BillType.Manager, preMonthPeriod);
                double parkingMoneyDebt = CalculateDebt(userBillDetbs, BillType.Parking, preMonthPeriod);
                double waterMoneyDebt = CalculateDebt(userBillDetbs, BillType.Water, preMonthPeriod);
                double electricMoneyDebt = CalculateDebt(userBillDetbs, BillType.Electric, preMonthPeriod);
                double lightMoneyDebt = CalculateDebt(userBillDetbs, BillType.Lighting, preMonthPeriod);
                double otherMoneyDebt = CalculateDebt(userBillDetbs, BillType.Other, preMonthPeriod);

                // pre_payment
                double managementMoneyPrePayment = CalculatePrePayment(userBills, BillType.Manager);
                double parkingMoneyPrePayment = CalculatePrePayment(userBills, BillType.Parking);
                double waterMoneyPrePayment = CalculatePrePayment(userBills, BillType.Water);
                double electricMoneyPrePayment = CalculatePrePayment(userBills, BillType.Electric);
                double lightMoneyPrePayment = CalculatePrePayment(userBills, BillType.Lighting);
                double otherMoneyPrePayment = CalculatePrePayment(userBills, BillType.Other);

                // total bill amount to be paid for each type
                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;
                double electricMoney = electricMoneyUnpaid + electricMoneyDebt;
                double lightMoney = lightMoneyUnpaid + lightMoneyDebt;
                double otherMoney = otherMoneyUnpaid + otherMoneyDebt;

                // total unpaid, debt, pre_payment bill amount 
                double costTax = waterMoneyBVMT + waterMoneyVAT;
                double costVaCos = electricMoneyUnpaid * 0.08 + electricMoneyUnpaid * 0.05;
                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + electricMoneyUnpaid + lightMoneyUnpaid + otherMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt + electricMoneyDebt + lightMoneyDebt + otherMoneyDebt;
                double costPrepayment = parkingMoneyPrePayment + waterMoneyPrePayment + managementMoneyPrePayment + electricMoneyPrePayment + lightMoneyPrePayment + otherMoneyPrePayment;
                double totalFeeAndDebt = costUnpaid + costDebt;
                double totalFeePayable = totalFeeAndDebt - costPrepayment;
                double totalFeePayableAndTax = totalFeePayable + costTax;  // Thanh Bình
                double totalFeeDebtAndTax = costUnpaid + costTax;
                double totalFinal = costUnpaid + costVaCos; //aden


                // paid
                double managementMoneyPaid = managementBill != null && managementBill.Status == UserBillStatus.Paid ? managementBill.LastCost.Value : 0;
                double parkingMoneyPaid = parkingBill != null && parkingBill.Status == UserBillStatus.Paid ? parkingBill.LastCost.Value : 0;

                // electric index, water index, rental area,...
                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal indexHeadElectric = GetIndexHead(electricBill);
                decimal indexEndElectric = GetIndexEnd(electricBill);
                decimal totalIndexWater = GetTotalIndex(waterBill);
                decimal totalIndexElectric = GetTotalIndex(electricBill);

                #region bill config for each type
                // parking 
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }

                // water
                BillConfigPropertiesDto billWaterConfigProperties = null;
                if (priceWaterConfig?.Properties != null)
                {
                    billWaterConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceWaterConfig.Properties);
                }

                // Electric
                BillConfigPropertiesDto billElectricConfigProperties = null;
                if (priceElectricConfig?.Properties != null)
                {
                    billElectricConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceElectricConfig.Properties);
                }

                // Management
                buildingId = managementBill != null ? managementBill.BuildingId : 0;
                BillConfigPropertiesDto billManagementConfigProperties = null;
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                #endregion
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                #region Sản lượng tiêu thụ nước

                //Hóa đơn nước
                var w_money = 0;
                var resultW = 0;
                if (waterBill != null)
                {
                    w_money = (int)waterBill.LastCost;
                    buildingId = buildingId > 0 ? buildingId : waterBill.BuildingId;
                }

                if (billWaterConfigProperties != null && billWaterConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;

                    var index = 0;
                    foreach (var level in billWaterConfigProperties.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                    }
                }


                #endregion

                #region Sản lượng tiêu thụ điện

                var resultE = 0;
                var e_percent = 0.0;
                if (billElectricConfigProperties != null && billElectricConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = electricBill != null ? (int)electricBill.TotalIndex.Value : 0;
                    var index = 0;
                    foreach (var level in billElectricConfigProperties.Prices)
                    {
                        index++;
                        if (!brk)
                        {
                            if (amount < level.To)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{E_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                            resultE += (int)(level.Value * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        }
                    }

                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{E_INDEX" + i + "}", "");
                        emailTemplate.Replace("{E_PRICE" + i + "}", "");
                        emailTemplate.Replace("{E_AMOUNT" + i + "}", "");
                    }

                    emailTemplate.Replace("{E_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultE));
                    emailTemplate.Replace("{E_INTO_MONEY}", string.Format("{0:#,#.##}", resultE + e_percent));
                }

                #endregion

                #region Phí quản lý xe
                // list vehicle parking
                double? p_money = 0;
                List<CitizenVehiclePas> listVehicles = new();
                if (parkingBill?.Properties != null)
                {
                    p_money = parkingBill.LastCost;
                    buildingId = buildingId > 0 ? buildingId : parkingBill.BuildingId;
                    string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                    if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                }
                string listRowVehicleCar = "";
                string listRowVehicleBike = "";
                string headerTemplatePaking = $"  <tr>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                    $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px;\">\r\n" +
                    $"           {{HEAD_NAME}}: {string.Format("{0:dd/MM/yyyy}", head_period)} - {string.Format("{0:dd/MM/yyyy}", end_period)}\r\n" +
                    $"          </td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n " +
                    $"         <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"        </tr>";
                if (listVehicles.Count > 0)
                {
                    foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                    {
                        string vehicleName = GetVehicleNameVinasinco(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;

                        StringBuilder vehicleRowTemplate = new(
                            $"        <tr>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                            $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">\r\n" +
                            $"            {vehicleCode}\r\n" +
                            $"          </td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">1</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                            $"        </tr>");

                        if (vehicle.vehicleType == VehicleType.Car)
                        {
                            if (listRowVehicleCar == "")
                            {
                                listRowVehicleCar += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi xe ô tô")
                                    .ToString();
                            }
                            listRowVehicleCar += vehicleRowTemplate.ToString();
                        }
                        else
                        {
                            if (listRowVehicleBike == "")
                            {
                                listRowVehicleBike += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi Xe máy/ Xe máy điện/ Xe đạp điện")
                                    .ToString();
                            }
                            listRowVehicleBike += vehicleRowTemplate.ToString();
                        }

                    }
                }
                #endregion


                #region Hóa đơn khác
                string listOtherBills = string.Empty;
                foreach (var (otherBillElement, index) in otherBills.Select((otherBillElement, index) => (otherBillElement, index)))
                {
                    long unitPrice = 0;
                    if (otherBillElement.BillConfigId != 0)
                    {
                        BillConfig otherBillConfig = billConfigs.FirstOrDefault(x => x.Id == otherBillElement.BillConfigId);
                        if (otherBillConfig?.Properties != null)
                        {
                            BillConfigPropertiesDto otherBillConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherBillConfig.Properties);
                            unitPrice = (long)(otherBillConfigProperties?.Prices[0]?.Value ?? 0);
                        }
                    }

                    // declare
                    string nameOtherBill = $"Phí khác số {index + 1}";
                    decimal totalIndex = GetTotalIndex(otherBillElement);
                    double costOtherBillUnpaid = GetBillCost(otherBillElement);
                    StringBuilder otherBillRowTemplate = new(GetListOtherBill(tenantId));

                    otherBillRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{NAME_OTHER_BILL}", $"{nameOtherBill}")
                            .Replace("{TOTAL_INDEX_OTHER_BILL}", GetStringValue(totalIndex))
                            .Replace("{UNIT_PRICE_OTHER_BILL}", FormatCost(unitPrice))
                            .Replace("{COST_OTHER_BILL_UNPAID}", FormatCost(costOtherBillUnpaid));
                    listOtherBills += otherBillRowTemplate.ToString();
                }
                #endregion

                // StringBuilder emailTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput() { TenantId = tenantId, Type = ETemplateBillType.BILL }));
                var textTotalMoney = VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt);
                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", FormatNumberToTwoDigits(currentDate.Day))
                    .Replace("{MONTH_NOW}", FormatNumberToTwoDigits(currentDate.Month))

                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{DAY_PERIOD}", FormatNumberToTwoDigits(period.Day))
                    .Replace("{MONTH_PERIOD}", FormatNumberToTwoDigits(period.Month))
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", FormatNumberToTwoDigits(dayWater))
                    .Replace("{DAY_ELECTRIC}", FormatNumberToTwoDigits(dayElectric))
                    .Replace("{DAY_MANAGEMENT}", FormatNumberToTwoDigits(dayManagement))
                    .Replace("{PRE_MONTH_PERIOD}", FormatNumberToTwoDigits(preMonthPeriod.Month))
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{CUSTOMER_PHONE_NUMBER}", $"{customerPhoneNumber}")
                    .Replace("{TAX_CODE}", $"{taxCode}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{MONTH_NAME}", $"{DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_ACRON}", $"{DateTime.Now.ToString("MMM", CultureInfo.InvariantCulture)}")
                    .Replace("{DAY_NAME_ACRON}", $"{DateTime.Now.ToString("dd", CultureInfo.InvariantCulture)}")
                    .Replace("{DAY_NAME_ACRON_PAYMENT}", $"{paymentDayDateTime.ToString("dd", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_ACRON_PAYMENT}", $"{paymentDayDateTime.ToString("MMM", CultureInfo.InvariantCulture)}")
                    .Replace("{YEAR_NAME_ACRON_PAYMENT}", $"{paymentDayDateTime.ToString("yyyy", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_PERIOD}", $"{period.ToString("MMMM", CultureInfo.InvariantCulture)}")
                    .Replace("{PAYMENT_DAY}", paymentDay + "")

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{HEAD_INDEX_W}", GetStringValue(indexHeadWater))
                    .Replace("{END_INDEX_W}", GetStringValue(indexEndWater))
                    .Replace("{TOTAL_INDEX_W}", GetStringValue(totalIndexWater))
                    .Replace("{HEAD_INDEX_E}", GetStringValue(indexHeadElectric))
                    .Replace("{END_INDEX_E}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_E}", GetStringValue(totalIndexElectric))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(billElectricConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoney))
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(managementMoneyPrePayment))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))

                    // electric
                    .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(electricMoneyUnpaid))
                    .Replace("{COST_ELECTRIC_PRE_PAYMENT}", FormatCost(electricMoneyPrePayment))
                    .Replace("{COST_ELECTRIC_DEBT}", FormatCost(electricMoneyDebt))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{W_BVMT}", FormatCost(waterMoneyBVMT))  // thanh bình
                    .Replace("{W_VAT}", FormatCost(waterMoneyVAT))  // thanh bình
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(waterMoneyPrePayment))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER_UNPAID_TAX}", FormatCost(waterMoneyUnpaidAndTax)) // thanh bình
                    .Replace("{COST_WATER}", FormatCost(waterMoney))

                    // parking
                    .Replace("{P_MONEY}", FormatCost(p_money))
                    // card parking
                    .Replace("{LIST_VEHICLE_CAR}", listRowVehicleCar)
                    .Replace("{LIST_VEHICLE_BIKE}", listRowVehicleBike)
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // other 
                    .Replace("{COST_OTHER_UNPAID}", FormatCost(otherMoneyUnpaid))
                    .Replace("{COST_OTHER_PRE_PAYMENT}", FormatCost(otherMoneyPrePayment))
                    .Replace("{COST_OTHER_DEBT}", FormatCost(otherMoneyDebt))
                    .Replace("{COST_OTHER}", FormatCost(otherMoney))

                    .Replace("{TOTAL_INDEX_O1}", "-")
                    .Replace("{PRICE_O1}", "-")
                    .Replace("{MONEY_O1}", "-")
                    .Replace("{BUILDING_CODE}", GetBuildingName(buildingId))

                    .Replace("{M_MONEY_PAID}", FormatCost(managementMoneyPaid))
                    .Replace("{P_MONEY_PAID}", FormatCost(parkingMoneyPaid))
                    // total money
                    .Replace("{TOTAL_1}", FormatCost(costUnpaid))
                    .Replace("{DEBT}", FormatCost(costDebt))
                    .Replace("{TOTAL_2}", FormatCost(totalFeePayable))
                    .Replace("{DAY_NOW_FULL}", string.Format("{0:dd/MM/yyyy}", currentDate))
                    .Replace("{HEAD_PERIOD}", string.Format("{0:dd/MM/yyyy}", head_period))
                    .Replace("{END_PERIOD}", string.Format("{0:dd/MM/yyyy}", end_period))
                    .Replace("{W_HEAD_PERIOD}", string.Format("{0:dd/MM/yyyy}", w_head_period))
                    .Replace("{W_END_PERIOD}", string.Format("{0:dd/MM/yyyy}", w_end_period))
                    .Replace("{COST_UNPAID_TAX}", FormatCost(totalFeeDebtAndTax))
                    .Replace("{W_MONEY}", FormatCost(w_money))
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PRE_PAYMENT}", FormatCost(costPrepayment))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_FINAL}", FormatCost(totalFeePayable))
                    .Replace("{TOTAL_MONEY_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeePayable));

                return emailTemplate;
            }
        }

        // Vinasinco
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateVinasinco(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                // format period datetime and current date
                DateTime currentDate = DateTime.Now;
                DateTime preMonthPeriod = period.AddMonths(-1);
                string periodString = period.ToString("MM/yyyy");

                // information of citizen who own apartment
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                string taxCode = GetTaxCode(citizenTemp);

                // userbill and billdebt for certain apartments and periods
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();

                // name and phone number of citizen who own apartment
                string customerName = GetCustomerName(userBills, citizenTemp);
                string customerPhoneNumber = citizenTemp?.PhoneNumber ?? "";

                // userbill for each type
                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water);
                UserBill electricBill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();

                // day water, electric, management, ... 
                int dayWater = await GetDayWater(waterBill, currentDate);
                int dayElectric = await GetDayElectric(electricBill, currentDate);
                int dayManagement = 1;

                // BillConfig (unit price) for each type 
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                BillConfig priceElectricConfig = GetBillConfigPrice(billConfigs, BillType.Electric, BillConfigPricesType.Level);

                // total unpaid bill amount
                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                double electricMoneyUnpaid = GetBillCost(electricBill);
                double otherMoneyUnpaid = (double)otherBills.Sum(x => x.LastCost);

                List<UserBill> userBillDetbs = _userBillRepository.GetAll()
               .Where(x => x.ApartmentCode == apartmentCode)
               .Where(x => x.Status == UserBillStatus.Debt)
               .ToList();

                // total debt bill amount for each type
                double managementMoneyDebt = CalculateDebt(userBillDetbs, BillType.Manager, preMonthPeriod);
                double parkingMoneyDebt = CalculateDebt(userBillDetbs, BillType.Parking, preMonthPeriod);
                double waterMoneyDebt = CalculateDebt(userBillDetbs, BillType.Water, preMonthPeriod);
                double electricMoneyDebt = CalculateDebt(userBillDetbs, BillType.Electric, preMonthPeriod);
                double otherMoneyDebt = CalculateDebt(userBillDetbs, BillType.Other, preMonthPeriod);

                // total bill amount to be paid for each type
                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;
                double electricMoney = electricMoneyUnpaid + electricMoneyDebt;
                double otherMoney = otherMoneyUnpaid + otherMoneyDebt;

                // total unpaid, debt, pre_payment bill amount 
                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + electricMoneyUnpaid + otherMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt + electricMoneyDebt + otherMoneyDebt;
                double costPrepayment = 0;
                double totalFeeAndDebt = costUnpaid + costDebt;
                double totalFeePayable = totalFeeAndDebt - costPrepayment;
                double costTax = totalFeePayable * 0.05;
                double totalFeeDebtAndTax = costUnpaid + costTax;
                double totalFeePayableAndTax = totalFeePayable + costTax;

                // electric index, water index, rental area,...
                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal indexHeadElectric = GetIndexHead(electricBill);
                decimal indexEndElectric = GetIndexEnd(electricBill);
                decimal totalIndexWater = GetTotalIndex(waterBill);
                decimal totalIndexElectric = GetTotalIndex(electricBill);

                #region bill config for each type
                // parking 
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }

                // water
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

                // Management
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                #endregion

                #region Sản lượng tiêu thụ nước
                string listWaterConsumptions = string.Empty;
                if (billWaterConfigProperties.Prices != null && billWaterConfigProperties.Prices.Any())
                {
                    List<PriceDto> listUnitPrices = billWaterConfigProperties.Prices.ToList();
                    foreach (var (unitPrice, index) in listUnitPrices.Select((unitPrice, index) => (unitPrice, index)))
                    {
                        // declare
                        int waterConsumptionQuantity;
                        long unitPriceWater;
                        long costWaterUnpaid;
                        string waterConsumptionContent = $"Nước sạch {FormatNumberToTwoDigits(dayWater)}/{FormatNumberToTwoDigits(period.Month)}/{period.Year} " +
                            $"- {FormatNumberToTwoDigits(dayWater)}/{FormatNumberToTwoDigits(currentDate.Month)}/{currentDate.Year}";
                        string utilityWater = "m3";
                        StringBuilder waterRowTemplate = new("<tr> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px\"> {WATER_CONSUMPTION_RANGE} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{UTILITY}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> {WATER_CONSUMPTION_QUANTITY}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{UNIT_PRICE_WATER} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{COST_WATER_UNPAID} </td> </tr>");

                        // caculate price for each unit and content
                        if (index == 0 && unitPrice.To < totalIndexWater)
                        {
                            waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        else if (unitPrice.To.HasValue && unitPrice.To < totalIndexWater)
                        {
                            waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From + 1);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        else
                        {
                            waterConsumptionQuantity = (int)(totalIndexWater - unitPrice.From) <= 0 ? 0 : (int)(totalIndexWater - unitPrice.From);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        if (index != 0)
                        {
                            waterConsumptionContent = string.Empty;
                            utilityWater = string.Empty;
                        }

                        // complete
                        waterRowTemplate
                            .Replace("{INDEX}", $"{index + 1}")
                            .Replace("{WATER_CONSUMPTION_RANGE}", waterConsumptionContent)
                            .Replace("{UTILITY}", utilityWater)
                            .Replace("{WATER_CONSUMPTION_QUANTITY}", $"{waterConsumptionQuantity}")
                            .Replace("{UNIT_PRICE_WATER}", FormatCost(unitPriceWater))
                            .Replace("{COST_WATER_UNPAID}", FormatCost(costWaterUnpaid));

                        listWaterConsumptions += waterRowTemplate.ToString();
                    }
                }
                #endregion

                #region Sản lượng tiêu thụ điện
                string listElectricConsumptions = string.Empty;
                if (billElectricConfigProperties.Prices != null && billElectricConfigProperties.Prices.Any())
                {
                    List<PriceDto> listUnitPrices = billElectricConfigProperties.Prices.ToList();
                    foreach (var (unitPrice, index) in listUnitPrices.Select((unitPrice, index) => (unitPrice, index)))
                    {
                        // declare
                        int electricConsumptionQuantity;
                        long unitPriceElectric;
                        long costElectricUnpaid;
                        string electricConsumptionContent = $"Điện tiêu thụ {FormatNumberToTwoDigits(dayElectric)}/{FormatNumberToTwoDigits(period.Month)}/{period.Year} " +
                            $"- {FormatNumberToTwoDigits(dayElectric)}/{FormatNumberToTwoDigits(currentDate.Month)}/{currentDate.Year}";
                        string utilityElectric = "kWh";
                        StringBuilder electricRowTemplate = new("<tr> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px\"> {ELECTRIC_CONSUMPTION_RANGE} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{UTILITY}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> {ELECTRIC_CONSUMPTION_QUANTITY}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{UNIT_PRICE_ELECTRIC} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{COST_ELECTRIC_UNPAID} </td> </tr>");

                        // calculate price for each unit and content
                        if (index == 0 && unitPrice.To < totalIndexElectric)
                        {
                            electricConsumptionQuantity = (int)(unitPrice.To - unitPrice.From);
                            unitPriceElectric = (long)unitPrice.Value;
                            costElectricUnpaid = electricConsumptionQuantity * unitPriceElectric;
                        }
                        else if (unitPrice.To.HasValue && unitPrice.To < totalIndexElectric)
                        {
                            electricConsumptionQuantity = (int)(unitPrice.To - unitPrice.From + 1);
                            unitPriceElectric = (long)unitPrice.Value;
                            costElectricUnpaid = electricConsumptionQuantity * unitPriceElectric;
                        }
                        else
                        {
                            electricConsumptionQuantity = (int)(totalIndexElectric - unitPrice.From) <= 0 ? 0 : (int)(totalIndexElectric - unitPrice.From);
                            unitPriceElectric = (long)unitPrice.Value;
                            costElectricUnpaid = electricConsumptionQuantity * unitPriceElectric;
                        }
                        if (index != 0)
                        {
                            electricConsumptionContent = string.Empty;
                            utilityElectric = string.Empty;
                        }

                        // complete
                        electricRowTemplate
                            .Replace("{INDEX}", $"{index + 1}")
                            .Replace("{ELECTRIC_CONSUMPTION_RANGE}", electricConsumptionContent)
                            .Replace("{UTILITY}", utilityElectric)
                            .Replace("{ELECTRIC_CONSUMPTION_QUANTITY}", $"{electricConsumptionQuantity}")
                            .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(unitPriceElectric))
                            .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(costElectricUnpaid));

                        listElectricConsumptions += electricRowTemplate.ToString();
                    }
                }
                #endregion

                #region Phí quản lý xe
                // list vehicle parking
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
                        StringBuilder vehicleRowTemplate = new("<tr> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px\"> {VEHICLE_TYPE} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> 1</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{COST_PARKING_ELEMENT} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{COST_PARKING_ELEMENT} </td> </tr>");
                        string vehicleName = GetVehicleNameVinasinco(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;

                        vehicleRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{VEHICLE_TYPE}", $"{vehicleName}")
                            .Replace("{COST_PARKING_ELEMENT}", FormatCost((double?)vehicleCost));
                        listRowVehicles += vehicleRowTemplate.ToString();
                    }
                }
                #endregion

                #region Hóa đơn khác
                string listOtherBills = string.Empty;
                foreach (var (otherBillElement, index) in otherBills.Select((otherBillElement, index) => (otherBillElement, index)))
                {
                    long unitPrice = 0;
                    if (otherBillElement.BillConfigId != 0)
                    {
                        BillConfig otherBillConfig = billConfigs.FirstOrDefault(x => x.Id == otherBillElement.BillConfigId);
                        if (otherBillConfig?.Properties != null)
                        {
                            BillConfigPropertiesDto otherBillConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherBillConfig.Properties);
                            unitPrice = (long)(otherBillConfigProperties?.Prices[0]?.Value ?? 0);
                        }
                    }

                    // declare
                    string nameOtherBill = $"Phí khác số {index + 1}";
                    decimal totalIndex = GetTotalIndex(otherBillElement);
                    double costOtherBillUnpaid = GetBillCost(otherBillElement);
                    StringBuilder otherBillRowTemplate = new("<tr> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px;\">{NAME_OTHER_BILL}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> {TOTAL_INDEX_OTHER_BILL}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> {UNIT_PRICE_OTHER_BILL}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> {COST_OTHER_BILL_UNPAID}</td> </tr>");
                    unitPrice = unitPrice == 0 ? (long)costOtherBillUnpaid : unitPrice;
                    otherBillRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{NAME_OTHER_BILL}", $"{nameOtherBill}")
                            .Replace("{TOTAL_INDEX_OTHER_BILL}", GetStringValue((long)totalIndex))
                            .Replace("{UNIT_PRICE_OTHER_BILL}", FormatCost(unitPrice))
                            .Replace("{COST_OTHER_BILL_UNPAID}", FormatCost(costOtherBillUnpaid));
                    listOtherBills += otherBillRowTemplate.ToString();
                }
                #endregion

                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", FormatNumberToTwoDigits(currentDate.Day))
                    .Replace("{MONTH_NOW}", FormatNumberToTwoDigits(currentDate.Month))
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{DAY_PERIOD}", FormatNumberToTwoDigits(period.Day))
                    .Replace("{MONTH_PERIOD}", FormatNumberToTwoDigits(period.Month))
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", FormatNumberToTwoDigits(dayWater))
                    .Replace("{DAY_ELECTRIC}", FormatNumberToTwoDigits(dayElectric))
                    .Replace("{DAY_MANAGEMENT}", FormatNumberToTwoDigits(dayManagement))
                    .Replace("{PRE_MONTH_PERIOD}", FormatNumberToTwoDigits(preMonthPeriod.Month))
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{CUSTOMER_PHONE_NUMBER}", $"{customerPhoneNumber}")
                    .Replace("{TAX_CODE}", $"{taxCode}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{CITIZEN_ADDRESS}", $"{citizenTemp?.Address ?? ""}")

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{INDEX_HEAD_WATER}", GetStringValue(indexHeadWater))
                    .Replace("{INDEX_END_WATER}", GetStringValue(indexEndWater))
                    .Replace("{TOTAL_INDEX_WATER}", GetStringValue(totalIndexWater))
                    .Replace("{INDEX_HEAD_ELECTRIC}", GetStringValue(indexHeadElectric))
                    .Replace("{INDEX_END_ELECTRIC}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_ELECTRIC}", GetStringValue(totalIndexElectric))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(billElectricConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoney))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))

                    // electric
                    .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(electricMoneyUnpaid))
                    .Replace("{COST_ELECTRIC_DEBT}", FormatCost(electricMoneyDebt))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER}", FormatCost(waterMoney))

                    // parking
                    .Replace("{UNIT_PRICE_PARKING}", "")
                    .Replace("{COST_PARKING_UNPAID}", FormatCost(parkingMoney))
                    .Replace("{COST_PARKING_DEBT}", FormatCost(parkingMoneyDebt))
                    .Replace("{COST_PARKING}", FormatCost(parkingMoney))

                    // card parking
                    .Replace("{COST_CARD_VEHICLE_UNPAID}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // other 
                    .Replace("{COST_OTHER_UNPAID}", FormatCost(otherMoneyUnpaid))
                    .Replace("{COST_OTHER_DEBT}", FormatCost(otherMoneyDebt))
                    .Replace("{COST_OTHER}", FormatCost(otherMoney))

                    // list vehicle, water, electric, management, ...
                    .Replace("{LIST_VEHICLE}", $"{listRowVehicles}")
                    .Replace("{LIST_WATER_CONSUMPTION}", $"{listWaterConsumptions}")
                    .Replace("{LIST_ELECTRIC_CONSUMPTION}", $"{listElectricConsumptions}")
                    .Replace("{LIST_OTHER_BILL}", $"{listOtherBills}")

                    // total money
                    .Replace("{COST_TAX}", FormatCost(costTax))
                    .Replace("{COST_UNPAID_TAX}", FormatCost(totalFeeDebtAndTax))
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PRE_PAYMENT}", FormatCost(costPrepayment))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_FINAL}", FormatCost(totalFeePayable))
                    .Replace("{COST_FINAL_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeePayable))

                    .Replace("{COST_FINAL_TAX}", FormatCost(totalFeePayableAndTax))
                    .Replace("{COST_FINAL_TAX_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeePayableAndTax));
                return emailTemplate;
            }
        }

        // Hiyori
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateHiyori(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                // format period datetime and current date
                DateTime currentDate = DateTime.Now;
                DateTime preMonthPeriod = period.AddMonths(-1);
                string periodString = period.ToString("MM/yyyy");

                // information of citizen who own apartment
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                string taxCode = GetTaxCode(citizenTemp);

                // userbill and billdebt for certain apartments and periods
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                List<UserBill> userBillDetbs = _userBillRepository.GetAll()
                    .Where(x => x.ApartmentCode == apartmentCode)
                    .Where(x => x.Status == UserBillStatus.Debt)
                    .ToList();
                //List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                //        x.ApartmentCode == apartmentCode &&
                //        x.Period.Year == period.Year &&
                //        x.Period.Month == period.Month &&
                //        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();

                // name and phone number of citizen who own apartment
                string customerName = GetCustomerName(userBills, citizenTemp);
                string customerPhoneNumber = citizenTemp?.PhoneNumber ?? "";

                // userbill for each type
                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water);
                UserBill electricBill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);
                UserBill lightBill = userBills.FirstOrDefault(x => x.BillType == BillType.Lighting);
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();

                // day water, electric, management, ... 
                int dayWater = await GetDayWater(waterBill, currentDate);
                int dayElectric = await GetDayElectric(electricBill, currentDate);
                int dayManagement = 1;

                // BillConfig (unit price) for each type 
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                BillConfig priceElectricConfig = GetBillConfigPrice(billConfigs, BillType.Electric, BillConfigPricesType.Level);
                BillConfig priceLightConfig = GetBillConfigPrice(billConfigs, BillType.Lighting, BillConfigPricesType.Normal);

                // total unpaid bill amount
                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                double electricMoneyUnpaid = GetBillCost(electricBill);
                double lightMoneyUnpaid = GetBillCost(lightBill);
                double otherMoneyUnpaid = (double)otherBills.Sum(x => x.LastCost);

                // tax
                long waterMoneyBVMT = (long)(waterMoneyUnpaid * 0.1);
                long waterMoneyVAT = (long)(waterMoneyUnpaid * 0.05);
                double waterMoneyUnpaidAndTax = waterMoneyUnpaid + waterMoneyBVMT + waterMoneyVAT;

                // total debt bill amount for each type
                double managementMoneyDebt = CalculateDebt(userBillDetbs, BillType.Manager, preMonthPeriod);
                double parkingMoneyDebt = CalculateDebt(userBillDetbs, BillType.Parking, preMonthPeriod);
                double waterMoneyDebt = CalculateDebt(userBillDetbs, BillType.Water, preMonthPeriod);
                double electricMoneyDebt = CalculateDebt(userBillDetbs, BillType.Electric, preMonthPeriod);
                double lightMoneyDebt = CalculateDebt(userBillDetbs, BillType.Lighting, preMonthPeriod);
                double otherMoneyDebt = CalculateDebt(userBillDetbs, BillType.Other, preMonthPeriod);

                // pre_payment
                double managementMoneyPrePayment = CalculatePrePayment(userBills, BillType.Manager);
                double parkingMoneyPrePayment = CalculatePrePayment(userBills, BillType.Parking);
                double waterMoneyPrePayment = CalculatePrePayment(userBills, BillType.Water);
                double electricMoneyPrePayment = CalculatePrePayment(userBills, BillType.Electric);
                double lightMoneyPrePayment = CalculatePrePayment(userBills, BillType.Lighting);
                double otherMoneyPrePayment = CalculatePrePayment(userBills, BillType.Other);

                // total bill amount to be paid for each type
                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;
                double electricMoney = electricMoneyUnpaid + electricMoneyDebt;
                double lightMoney = lightMoneyUnpaid + lightMoneyDebt;
                double otherMoney = otherMoneyUnpaid + otherMoneyDebt;

                // total unpaid, debt, pre_payment bill amount 
                double costTax = waterMoneyBVMT + waterMoneyVAT;
                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + electricMoneyUnpaid + lightMoneyUnpaid + otherMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt + electricMoneyDebt + lightMoneyDebt + otherMoneyDebt;
                double costPrepayment = parkingMoneyPrePayment + waterMoneyPrePayment + managementMoneyPrePayment + electricMoneyPrePayment + lightMoneyPrePayment + otherMoneyPrePayment;
                double totalFeeAndDebt = costUnpaid + costDebt;
                double totalFeePayable = totalFeeAndDebt - costPrepayment;
                double totalFeePayableAndTax = totalFeePayable + costTax;  // Thanh Bình
                double totalFeeDebtAndTax = costUnpaid + costTax;

                // electric index, water index, rental area,...
                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal indexHeadElectric = GetIndexHead(electricBill);
                decimal indexEndElectric = GetIndexEnd(electricBill);
                decimal totalIndexWater = GetTotalIndex(waterBill);
                decimal totalIndexElectric = GetTotalIndex(electricBill);

                #region bill config for each type
                // parking 
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }

                // water
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

                // Management
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                #endregion
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                #region Sản lượng tiêu thụ nước

                //Hóa đơn nước
                var w_money = 0;
                var resultW = 0;
                if (waterBill != null) w_money = (int)waterBill.LastCost;

                if (billWaterConfigProperties != null && billWaterConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;

                    var index = 0;
                    foreach (var level in billWaterConfigProperties.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                    }
                }


                #endregion

                #region Sản lượng tiêu thụ điện

                var resultE = 0;
                var e_percent = 0.0;
                if (billElectricConfigProperties != null && billElectricConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = electricBill != null ? (int)electricBill.TotalIndex.Value : 0;
                    var index = 0;
                    foreach (var level in billElectricConfigProperties.Prices)
                    {
                        index++;
                        if (!brk)
                        {
                            if (amount < level.To)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{E_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                            resultE += (int)(level.Value * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        }
                    }

                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{E_INDEX" + i + "}", "");
                        emailTemplate.Replace("{E_PRICE" + i + "}", "");
                        emailTemplate.Replace("{E_AMOUNT" + i + "}", "");
                    }

                    emailTemplate.Replace("{E_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultE));
                    emailTemplate.Replace("{E_INTO_MONEY}", string.Format("{0:#,#.##}", resultE + e_percent));
                }

                #endregion

                #region Phí quản lý xe
                // list vehicle parking
                int p_money = 0;
                int priceCar = 0;
                int priceMotor = 0;
                int priceBike = 0;
                int priceOther = 0;

                if (billParkingConfigProperties != null && billParkingConfigProperties.Prices.Length == 4)
                {
                    priceCar = (int)billParkingConfigProperties.Prices[0].Value;
                    priceMotor = (int)billParkingConfigProperties.Prices[1].Value;
                    priceBike = (int)billParkingConfigProperties.Prices[2].Value;
                    priceOther = (int)billParkingConfigProperties.Prices[3].Value;

                }

                emailTemplate.Replace("{PRICE_CAR}", string.Format("{0:#,#.##}", priceCar));
                emailTemplate.Replace("{PRICE_MOTOR}", string.Format("{0:#,#.##}", priceMotor));
                emailTemplate.Replace("{PRICE_BIKE}", string.Format("{0:#,#.##}", priceBike));
                emailTemplate.Replace("{PRICE_OTHER}", string.Format("{0:#,#.##}", priceOther));

                int numCar = 0;
                int numMotor = 0;
                int numBike = 0;
                int numOther = 0;

                if (parkingBill != null)
                {
                    p_money = (int)parkingBill.LastCost;
                    numCar = parkingBill.CarNumber ?? 0;
                    numMotor = parkingBill.MotorbikeNumber ?? 0;
                    numBike = parkingBill.BicycleNumber ?? 0;
                    numOther = parkingBill.OtherVehicleNumber ?? 0;
                }

                emailTemplate.Replace("{P_MONEY}", string.Format("{0:#,#.##}", p_money));
                emailTemplate.Replace("{CAR_NUMBER}", numCar + "");
                emailTemplate.Replace("{MOTOR_NUMBER}", numMotor + "");
                emailTemplate.Replace("{BIKE_NUMBER}", numBike + "");
                emailTemplate.Replace("{OTHER_NUMBER}", numOther + "");

                emailTemplate.Replace("{MONEY_CAR}", FormatCost(priceCar * numCar));
                emailTemplate.Replace("{MONEY_MOTOR}", FormatCost(priceMotor * numMotor));
                emailTemplate.Replace("{MONEY_BIKE}", FormatCost(priceBike * numBike));
                emailTemplate.Replace("{MONEY_OTHER}", FormatCost(priceOther * numOther));

                #endregion

                #region Hóa đơn khác
                string listOtherBills = string.Empty;
                foreach (var (otherBillElement, index) in otherBills.Select((otherBillElement, index) => (otherBillElement, index)))
                {
                    long unitPrice = 0;
                    if (otherBillElement.BillConfigId != 0)
                    {
                        BillConfig otherBillConfig = billConfigs.FirstOrDefault(x => x.Id == otherBillElement.BillConfigId);
                        if (otherBillConfig?.Properties != null)
                        {
                            BillConfigPropertiesDto otherBillConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherBillConfig.Properties);
                            unitPrice = (long)(otherBillConfigProperties?.Prices[0]?.Value ?? 0);
                        }
                    }

                    // declare
                    string nameOtherBill = $"Phí khác số {index + 1}";
                    decimal totalIndex = GetTotalIndex(otherBillElement);
                    double costOtherBillUnpaid = GetBillCost(otherBillElement);
                    StringBuilder otherBillRowTemplate = new(GetListOtherBill(tenantId));

                    otherBillRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{NAME_OTHER_BILL}", $"{nameOtherBill}")
                            .Replace("{TOTAL_INDEX_OTHER_BILL}", GetStringValue(totalIndex))
                            .Replace("{UNIT_PRICE_OTHER_BILL}", FormatCost(unitPrice))
                            .Replace("{COST_OTHER_BILL_UNPAID}", FormatCost(costOtherBillUnpaid));
                    listOtherBills += otherBillRowTemplate.ToString();
                }
                #endregion

                // StringBuilder emailTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput() { TenantId = tenantId, Type = ETemplateBillType.BILL }));

                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", FormatNumberToTwoDigits(currentDate.Day))
                    .Replace("{MONTH_NOW}", FormatNumberToTwoDigits(currentDate.Month))
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{DAY_PERIOD}", FormatNumberToTwoDigits(period.Day))
                    .Replace("{MONTH_PERIOD}", FormatNumberToTwoDigits(period.Month))
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", FormatNumberToTwoDigits(dayWater))
                    .Replace("{DAY_ELECTRIC}", FormatNumberToTwoDigits(dayElectric))
                    .Replace("{DAY_MANAGEMENT}", FormatNumberToTwoDigits(dayManagement))
                    .Replace("{PRE_MONTH_PERIOD}", FormatNumberToTwoDigits(preMonthPeriod.Month))
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{CUSTOMER_PHONE_NUMBER}", $"{customerPhoneNumber}")
                    .Replace("{TAX_CODE}", $"{taxCode}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{MONTH_NAME}", $"{DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}")

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{HEAD_INDEX_W}", GetStringValue(indexHeadWater))
                    .Replace("{END_INDEX_W}", GetStringValue(indexEndWater))
                    .Replace("{TOTAL_INDEX_W}", GetStringValue(totalIndexWater))
                    .Replace("{HEAD_INDEX_E}", GetStringValue(indexHeadElectric))
                    .Replace("{END_INDEX_E}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_E}", GetStringValue(totalIndexElectric))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(billElectricConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoney))
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(managementMoneyPrePayment))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))

                    // electric
                    .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(electricMoneyUnpaid))
                    .Replace("{COST_ELECTRIC_PRE_PAYMENT}", FormatCost(electricMoneyPrePayment))
                    .Replace("{COST_ELECTRIC_DEBT}", FormatCost(electricMoneyDebt))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{COST_WATER_BVMT}", FormatCost(waterMoneyBVMT))  // thanh bình
                    .Replace("{COST_WATER_VAT}", FormatCost(waterMoneyVAT))  // thanh bình
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(waterMoneyPrePayment))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER_UNPAID_TAX}", FormatCost(waterMoneyUnpaidAndTax)) // thanh bình
                    .Replace("{COST_WATER}", FormatCost(waterMoney))

                    // parking

                    // card parking
                    .Replace("{COST_CARD_VEHICLE_UNPAID}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // other 
                    .Replace("{COST_OTHER_UNPAID}", FormatCost(otherMoneyUnpaid))
                    .Replace("{COST_OTHER_PRE_PAYMENT}", FormatCost(otherMoneyPrePayment))
                    .Replace("{COST_OTHER_DEBT}", FormatCost(otherMoneyDebt))
                    .Replace("{COST_OTHER}", FormatCost(otherMoney))

                    .Replace("{TOTAL_INDEX_O1}", "-")
                    .Replace("{PRICE_O1}", "-")
                    .Replace("{MONEY_O1}", "-")
                    // total money
                    .Replace("{TOTAL_1}", FormatCost(costUnpaid))
                    .Replace("{DEBT}", FormatCost(costDebt))
                    .Replace("{TOTAL_2}", FormatCost(totalFeeAndDebt))

                    .Replace("{COST_UNPAID_TAX}", FormatCost(totalFeeDebtAndTax))
                    .Replace("{W_MONEY}", FormatCost(w_money))
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PRE_PAYMENT}", FormatCost(costPrepayment))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_FINAL}", FormatCost(totalFeePayable))
                    .Replace("{COST_FINAL_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeePayable));
                return emailTemplate;
            }
        }

        [RemoteService(false)]
        //Tan phong
        public async Task<StringBuilder> CreateTemplateTanPhong(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                // format period datetime and current date
                DateTime currentDate = DateTime.Now;
                DateTime preMonthPeriod = period.AddMonths(-1);
                string periodString = period.ToString("MM/yyyy");

                // information of citizen who own apartment
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                string taxCode = GetTaxCode(citizenTemp);

                // userbill and billdebt for certain apartments and periods
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();

                // name and phone number of citizen who own apartment
                string customerName = GetCustomerName(userBills, citizenTemp);
                string customerPhoneNumber = citizenTemp?.PhoneNumber ?? "";
                long? buildingId = null;
                // userbill for each type
                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water);
                UserBill electricBill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);
                UserBill lightBill = userBills.FirstOrDefault(x => x.BillType == BillType.Lighting);
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();

                // day water, electric, management, ... 
                int dayWater = await GetDayWater(waterBill, currentDate);
                int dayElectric = await GetDayElectric(electricBill, currentDate);
                int dayManagement = 1;

                // BillConfig (unit price) for each type 
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                BillConfig priceElectricConfig = GetBillConfigPrice(billConfigs, BillType.Electric, BillConfigPricesType.Level);
                BillConfig priceLightConfig = GetBillConfigPrice(billConfigs, BillType.Lighting, BillConfigPricesType.Normal);

                // total unpaid bill amount
                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                double electricMoneyUnpaid = GetBillCost(electricBill);
                double lightMoneyUnpaid = GetBillCost(lightBill);
                double otherMoneyUnpaid = (double)otherBills.Sum(x => x.LastCost);

                // tax
                long waterMoneyBVMT = (long)(waterMoneyUnpaid * 0.1);
                long waterMoneyVAT = (long)(waterMoneyUnpaid * 0.05);
                double waterMoneyUnpaidAndTax = waterMoneyUnpaid + waterMoneyBVMT + waterMoneyVAT;

                List<UserBill> userBillDetbs = _userBillRepository.GetAll()
                  .Where(x => x.ApartmentCode == apartmentCode)
                  .Where(x => x.Status == UserBillStatus.Debt)
                  .ToList();

                // total debt bill amount for each type
                double managementMoneyDebt = CalculateDebt(userBillDetbs, BillType.Manager, preMonthPeriod);
                double parkingMoneyDebt = CalculateDebt(userBillDetbs, BillType.Parking, preMonthPeriod);
                double waterMoneyDebt = CalculateDebt(userBillDetbs, BillType.Water, preMonthPeriod);
                double electricMoneyDebt = CalculateDebt(userBillDetbs, BillType.Electric, preMonthPeriod);
                double lightMoneyDebt = CalculateDebt(userBillDetbs, BillType.Lighting, preMonthPeriod);
                double otherMoneyDebt = CalculateDebt(userBillDetbs, BillType.Other, preMonthPeriod);

                // pre_payment
                double managementMoneyPrePayment = CalculatePrePayment(userBills, BillType.Manager);
                double parkingMoneyPrePayment = CalculatePrePayment(userBills, BillType.Parking);
                double waterMoneyPrePayment = CalculatePrePayment(userBills, BillType.Water);
                double electricMoneyPrePayment = CalculatePrePayment(userBills, BillType.Electric);
                double lightMoneyPrePayment = CalculatePrePayment(userBills, BillType.Lighting);
                double otherMoneyPrePayment = CalculatePrePayment(userBills, BillType.Other);

                // total bill amount to be paid for each type
                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;
                double electricMoney = electricMoneyUnpaid + electricMoneyDebt;
                double lightMoney = lightMoneyUnpaid + lightMoneyDebt;
                double otherMoney = otherMoneyUnpaid + otherMoneyDebt;

                // total unpaid, debt, pre_payment bill amount 
                double costTax = waterMoneyBVMT + waterMoneyVAT;
                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + electricMoneyUnpaid + lightMoneyUnpaid + otherMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt + electricMoneyDebt + lightMoneyDebt + otherMoneyDebt;
                double costPrepayment = parkingMoneyPrePayment + waterMoneyPrePayment + managementMoneyPrePayment + electricMoneyPrePayment + lightMoneyPrePayment + otherMoneyPrePayment;
                double totalFeeAndDebt = costUnpaid + costDebt;
                double totalFeePayable = totalFeeAndDebt - costPrepayment;
                double totalFeePayableAndTax = totalFeePayable + costTax;  // Thanh Bình
                double totalFeeDebtAndTax = costUnpaid + costTax;

                // electric index, water index, rental area,...
                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal indexHeadElectric = GetIndexHead(electricBill);
                decimal indexEndElectric = GetIndexEnd(electricBill);
                decimal totalIndexWater = GetTotalIndex(waterBill);
                decimal totalIndexElectric = GetTotalIndex(electricBill);

                #region bill config for each type
                // parking 
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }

                // water
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

                // Management
                buildingId = managementBill != null ? managementBill.BuildingId : 0;
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                #endregion
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                #region Sản lượng tiêu thụ nước

                //Hóa đơn nước
                var w_money = 0;
                var resultW = 0;
                if (waterBill != null)
                {
                    w_money = (int)waterBill.LastCost;
                    buildingId = buildingId > 0 ? buildingId : waterBill.BuildingId;
                }

                if (billWaterConfigProperties != null && billWaterConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;

                    var index = 0;
                    foreach (var level in billWaterConfigProperties.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                    }
                }


                #endregion
                #region Phí quản lý xe
                // list vehicle parking
                int p_money = 0;
                int priceCar = 0;
                int priceMotor = 0;
                int priceBike = 0;
                int priceOther = 0;

                if (billParkingConfigProperties != null && billParkingConfigProperties.Prices.Length == 4)
                {
                    priceCar = (int)billParkingConfigProperties.Prices[0].Value;
                    priceMotor = (int)billParkingConfigProperties.Prices[1].Value;
                    priceBike = (int)billParkingConfigProperties.Prices[2].Value;
                    priceOther = (int)billParkingConfigProperties.Prices[3].Value;

                }

                emailTemplate.Replace("{PRICE_CAR}", string.Format("{0:#,#.##}", priceCar));
                emailTemplate.Replace("{PRICE_MOTOR}", string.Format("{0:#,#.##}", priceMotor));
                emailTemplate.Replace("{PRICE_BIKE}", string.Format("{0:#,#.##}", priceBike));
                emailTemplate.Replace("{PRICE_OTHER}", string.Format("{0:#,#.##}", priceOther));

                int numCar = 0;
                int numMotor = 0;
                int numBike = 0;
                int numOther = 0;

                if (parkingBill != null)
                {
                    p_money = (int)parkingBill.LastCost;
                    numCar = parkingBill.CarNumber ?? 0;
                    numMotor = parkingBill.MotorbikeNumber ?? 0;
                    numBike = parkingBill.BicycleNumber ?? 0;
                    numOther = parkingBill.OtherVehicleNumber ?? 0;
                }

                emailTemplate.Replace("{P_MONEY}", string.Format("{0:#,#.##}", p_money));
                emailTemplate.Replace("{CAR_NUMBER}", numCar + "");
                emailTemplate.Replace("{MOTOR_NUMBER}", numMotor + "");
                emailTemplate.Replace("{BIKE_NUMBER}", numBike + "");
                emailTemplate.Replace("{OTHER_NUMBER}", numOther + "");

                emailTemplate.Replace("{MONEY_CAR}", FormatCost(priceCar * numCar));
                emailTemplate.Replace("{MONEY_MOTOR}", FormatCost(priceMotor * numMotor));
                emailTemplate.Replace("{MONEY_BIKE}", FormatCost(priceBike * numBike));
                emailTemplate.Replace("{MONEY_OTHER}", FormatCost(priceOther * numOther));

                #endregion

                #region Hóa đơn khác
                string listOtherBills = string.Empty;
                foreach (var (otherBillElement, index) in otherBills.Select((otherBillElement, index) => (otherBillElement, index)))
                {
                    long unitPrice = 0;
                    if (otherBillElement.BillConfigId != 0)
                    {
                        BillConfig otherBillConfig = billConfigs.FirstOrDefault(x => x.Id == otherBillElement.BillConfigId);
                        if (otherBillConfig?.Properties != null)
                        {
                            BillConfigPropertiesDto otherBillConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherBillConfig.Properties);
                            unitPrice = (long)(otherBillConfigProperties?.Prices[0]?.Value ?? 0);
                        }
                    }

                    // declare
                    string nameOtherBill = $"Phí khác số {index + 1}";
                    decimal totalIndex = GetTotalIndex(otherBillElement);
                    double costOtherBillUnpaid = GetBillCost(otherBillElement);
                    StringBuilder otherBillRowTemplate = new(GetListOtherBill(tenantId));

                    otherBillRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{NAME_OTHER_BILL}", $"{nameOtherBill}")
                            .Replace("{TOTAL_INDEX_OTHER_BILL}", GetStringValue(totalIndex))
                            .Replace("{UNIT_PRICE_OTHER_BILL}", FormatCost(unitPrice))
                            .Replace("{COST_OTHER_BILL_UNPAID}", FormatCost(costOtherBillUnpaid));
                    listOtherBills += otherBillRowTemplate.ToString();
                }
                #endregion

                // StringBuilder emailTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput() { TenantId = tenantId, Type = ETemplateBillType.BILL }));

                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", FormatNumberToTwoDigits(currentDate.Day))
                    .Replace("{MONTH_NOW}", FormatNumberToTwoDigits(currentDate.Month))
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{DAY_PERIOD}", FormatNumberToTwoDigits(period.Day))
                    .Replace("{MONTH_PERIOD}", FormatNumberToTwoDigits(period.Month))
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", FormatNumberToTwoDigits(dayWater))
                    .Replace("{DAY_ELECTRIC}", FormatNumberToTwoDigits(dayElectric))
                    .Replace("{DAY_MANAGEMENT}", FormatNumberToTwoDigits(dayManagement))
                    .Replace("{PRE_MONTH_PERIOD}", FormatNumberToTwoDigits(preMonthPeriod.Month))
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                     .Replace("{EW_PERIOD_MONTH}", FormatNumberToTwoDigits(preMonthPeriod.Month))
                    .Replace("{EW_PERIOD_YEAR}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{CUSTOMER_PHONE_NUMBER}", $"{customerPhoneNumber}")
                    .Replace("{TAX_CODE}", $"{taxCode}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{MONTH_NAME}", $"{DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}")
                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{HEAD_INDEX_W}", GetStringValue(indexHeadWater))
                    .Replace("{END_INDEX_W}", GetStringValue(indexEndWater))
                    .Replace("{TOTAL_INDEX_W}", GetStringValue(totalIndexWater))
                    .Replace("{HEAD_INDEX_E}", GetStringValue(indexHeadElectric))
                    .Replace("{END_INDEX_E}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_E}", GetStringValue(totalIndexElectric))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(billElectricConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoney))
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(managementMoneyPrePayment))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))

                    // electric
                    .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(electricMoneyUnpaid))
                    .Replace("{COST_ELECTRIC_PRE_PAYMENT}", FormatCost(electricMoneyPrePayment))
                    .Replace("{COST_ELECTRIC_DEBT}", FormatCost(electricMoneyDebt))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{COST_WATER_BVMT}", FormatCost(waterMoneyBVMT))  // thanh bình
                    .Replace("{COST_WATER_VAT}", FormatCost(waterMoneyVAT))  // thanh bình
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(waterMoneyPrePayment))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER_UNPAID_TAX}", FormatCost(waterMoneyUnpaidAndTax)) // thanh bình
                    .Replace("{COST_WATER}", FormatCost(waterMoney))
                    .Replace("{W_BVMT}", FormatCost(waterMoneyBVMT))  // thanh bình
                    .Replace("{W_VAT}", FormatCost(waterMoneyVAT))  // thanh bình
                    .Replace("{W_AMOUNT_TOTAL}", FormatCost(resultW))

                    // card parking
                    .Replace("{COST_CARD_VEHICLE_UNPAID}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // other 
                    .Replace("{COST_OTHER_UNPAID}", FormatCost(otherMoneyUnpaid))
                    .Replace("{COST_OTHER_PRE_PAYMENT}", FormatCost(otherMoneyPrePayment))
                    .Replace("{COST_OTHER_DEBT}", FormatCost(otherMoneyDebt))
                    .Replace("{COST_OTHER}", FormatCost(otherMoney))
                    .Replace("{BUILDING_CODE}", GetBuildingName(buildingId))
                    .Replace("{TOTAL_INDEX_O1}", "-")
                    .Replace("{PRICE_O1}", "-")
                    .Replace("{MONEY_O1}", "-")
                    // total money
                    .Replace("{TOTAL_1}", FormatCost(costUnpaid))
                    .Replace("{DEBT}", FormatCost(costDebt))
                    .Replace("{TOTAL_2}", FormatCost(totalFeeAndDebt))

                    .Replace("{COST_UNPAID_TAX}", FormatCost(totalFeeDebtAndTax))
                    .Replace("{W_MONEY}", FormatCost(w_money))
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PRE_PAYMENT}", FormatCost(costPrepayment))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_FINAL}", FormatCost(totalFeePayable))
                    .Replace("{COST_FINAL_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeePayable));
                return emailTemplate;
            }
        }

        [RemoteService(false)]
        //Mhomes
        public async Task<StringBuilder> CreateTemplateMhomes(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {

                DateTime currentDate = DateTime.Now;
                DateTime preMonthPeriod = period.AddMonths(-1);

                var period_day_end = period.TotalDaysInMonth();
                string periodString = period.ToString("MM/yyyy");
                var head_period = new DateTime(period.Year, period.Month, 1);
                var end_period = new DateTime(period.Year, period.Month, period_day_end);
                var w_head_period = new DateTime(preMonthPeriod.Year, preMonthPeriod.Month, 1);
                var w_end_period = new DateTime(preMonthPeriod.Year, preMonthPeriod.Month, preMonthPeriod.TotalDaysInMonth());

                // information of citizen who own apartment
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                string taxCode = GetTaxCode(citizenTemp);

                // userbill and billdebt for certain apartments and periods
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();

                // name and phone number of citizen who own apartment
                string customerName = GetCustomerName(userBills, citizenTemp);
                string customerPhoneNumber = citizenTemp?.PhoneNumber ?? "";
                long? buildingId = null;

                // userbill for each type
                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water);
                UserBill electricBill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);
                UserBill lightBill = userBills.FirstOrDefault(x => x.BillType == BillType.Lighting);
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();

                // day water, electric, management, ... 
                int dayWater = await GetDayWater(waterBill, currentDate);
                int dayElectric = await GetDayElectric(electricBill, currentDate);
                int dayManagement = 1;

                // BillConfig (unit price) for each type 
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                BillConfig priceElectricConfig = GetBillConfigPrice(billConfigs, BillType.Electric, BillConfigPricesType.Level);
                BillConfig priceLightConfig = GetBillConfigPrice(billConfigs, BillType.Lighting, BillConfigPricesType.Normal);

                // total unpaid bill amount
                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                double electricMoneyUnpaid = GetBillCost(electricBill);
                double lightMoneyUnpaid = GetBillCost(lightBill);
                double otherMoneyUnpaid = (double)otherBills.Sum(x => x.LastCost);

                // tax
                long waterMoneyBVMT = (long)(waterMoneyUnpaid * 0.1);
                long waterMoneyVAT = (long)(waterMoneyUnpaid * 0.05);
                double waterMoneyUnpaidAndTax = waterMoneyUnpaid + waterMoneyBVMT + waterMoneyVAT;

                List<UserBill> userBillDetbs = _userBillRepository.GetAll()
                  .Where(x => x.ApartmentCode == apartmentCode)
                  .Where(x => x.Status == UserBillStatus.Debt)
                  .ToList();

                // total debt bill amount for each type
                double managementMoneyDebt = CalculateDebt(userBillDetbs, BillType.Manager, preMonthPeriod);
                double parkingMoneyDebt = CalculateDebt(userBillDetbs, BillType.Parking, preMonthPeriod);
                double waterMoneyDebt = CalculateDebt(userBillDetbs, BillType.Water, preMonthPeriod);
                double electricMoneyDebt = CalculateDebt(userBillDetbs, BillType.Electric, preMonthPeriod);
                double lightMoneyDebt = CalculateDebt(userBillDetbs, BillType.Lighting, preMonthPeriod);
                double otherMoneyDebt = CalculateDebt(userBillDetbs, BillType.Other, preMonthPeriod);

                // pre_payment
                double managementMoneyPrePayment = CalculatePrePayment(userBills, BillType.Manager);
                double parkingMoneyPrePayment = CalculatePrePayment(userBills, BillType.Parking);
                double waterMoneyPrePayment = CalculatePrePayment(userBills, BillType.Water);
                double electricMoneyPrePayment = CalculatePrePayment(userBills, BillType.Electric);
                double lightMoneyPrePayment = CalculatePrePayment(userBills, BillType.Lighting);
                double otherMoneyPrePayment = CalculatePrePayment(userBills, BillType.Other);

                // total bill amount to be paid for each type
                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;
                double electricMoney = electricMoneyUnpaid + electricMoneyDebt;
                double lightMoney = lightMoneyUnpaid + lightMoneyDebt;
                double otherMoney = otherMoneyUnpaid + otherMoneyDebt;

                // total unpaid, debt, pre_payment bill amount 
                double costTax = waterMoneyBVMT + waterMoneyVAT;
                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + electricMoneyUnpaid + lightMoneyUnpaid + otherMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt + electricMoneyDebt + lightMoneyDebt + otherMoneyDebt;
                double costPrepayment = parkingMoneyPrePayment + waterMoneyPrePayment + managementMoneyPrePayment + electricMoneyPrePayment + lightMoneyPrePayment + otherMoneyPrePayment;
                double totalFeeAndDebt = costUnpaid + costDebt;
                double totalFeePayable = totalFeeAndDebt - costPrepayment;
                double totalFeePayableAndTax = totalFeePayable + costTax;  // Thanh Bình
                double totalFeeDebtAndTax = costUnpaid + costTax;

                // paid
                double managementMoneyPaid = managementBill != null && managementBill.Status == UserBillStatus.Paid ? managementBill.LastCost.Value : 0;
                double parkingMoneyPaid = parkingBill != null && parkingBill.Status == UserBillStatus.Paid ? parkingBill.LastCost.Value : 0;

                // electric index, water index, rental area,...
                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal indexHeadElectric = GetIndexHead(electricBill);
                decimal indexEndElectric = GetIndexEnd(electricBill);
                decimal totalIndexWater = GetTotalIndex(waterBill);
                decimal totalIndexElectric = GetTotalIndex(electricBill);

                #region bill config for each type
                // parking 
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }

                // water
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

                // Management
                buildingId = managementBill != null ? managementBill.BuildingId : 0;
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                #endregion
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                #region Sản lượng tiêu thụ nước

                //Hóa đơn nước
                var w_money = 0;
                var resultW = 0;
                if (waterBill != null)
                {
                    w_money = (int)waterBill.LastCost;
                    buildingId = buildingId > 0 ? buildingId : waterBill.BuildingId;
                }

                if (billWaterConfigProperties != null && billWaterConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;

                    var index = 0;
                    foreach (var level in billWaterConfigProperties.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                    }
                }


                #endregion

                #region Sản lượng tiêu thụ điện

                var resultE = 0;
                var e_percent = 0.0;
                if (billElectricConfigProperties != null && billElectricConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = electricBill != null ? (int)electricBill.TotalIndex.Value : 0;
                    var index = 0;
                    foreach (var level in billElectricConfigProperties.Prices)
                    {
                        index++;
                        if (!brk)
                        {
                            if (amount < level.To)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{E_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                            resultE += (int)(level.Value * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        }
                    }

                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{E_INDEX" + i + "}", "");
                        emailTemplate.Replace("{E_PRICE" + i + "}", "");
                        emailTemplate.Replace("{E_AMOUNT" + i + "}", "");
                    }

                    emailTemplate.Replace("{E_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultE));
                    emailTemplate.Replace("{E_INTO_MONEY}", string.Format("{0:#,#.##}", resultE + e_percent));
                }

                #endregion

                #region Phí quản lý xe
                // list vehicle parking
                double? p_money = 0;
                List<CitizenVehiclePas> listVehicles = new();
                if (parkingBill?.Properties != null)
                {
                    p_money = parkingBill.LastCost;
                    buildingId = buildingId > 0 ? buildingId : parkingBill.BuildingId;
                    string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                    if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                }
                string listRowVehicleCar = "";
                string listRowVehicleBike = "";
                string headerTemplatePaking = $"  <tr>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                    $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px;\">\r\n" +
                    $"           {{HEAD_NAME}}: {string.Format("{0:dd/MM/yyyy}", head_period)} - {string.Format("{0:dd/MM/yyyy}", end_period)}\r\n" +
                    $"          </td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n " +
                    $"         <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"        </tr>";
                if (listVehicles.Count > 0)
                {
                    foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                    {
                        string vehicleName = GetVehicleNameVinasinco(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;

                        StringBuilder vehicleRowTemplate = new(
                            $"        <tr>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                            $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">\r\n" +
                            $"            {vehicleCode}\r\n" +
                            $"          </td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">1</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                            $"        </tr>");

                        if (vehicle.vehicleType == VehicleType.Car)
                        {
                            if (listRowVehicleCar == "")
                            {
                                listRowVehicleCar += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi xe ô tô")
                                    .ToString();
                            }
                            listRowVehicleCar += vehicleRowTemplate.ToString();
                        }
                        else
                        {
                            if (listRowVehicleBike == "")
                            {
                                listRowVehicleBike += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi Xe máy/ Xe máy điện/ Xe đạp điện")
                                    .ToString();
                            }
                            listRowVehicleBike += vehicleRowTemplate.ToString();
                        }

                    }
                }
                #endregion


                #region Hóa đơn khác
                string listOtherBills = string.Empty;
                foreach (var (otherBillElement, index) in otherBills.Select((otherBillElement, index) => (otherBillElement, index)))
                {
                    long unitPrice = 0;
                    if (otherBillElement.BillConfigId != 0)
                    {
                        BillConfig otherBillConfig = billConfigs.FirstOrDefault(x => x.Id == otherBillElement.BillConfigId);
                        if (otherBillConfig?.Properties != null)
                        {
                            BillConfigPropertiesDto otherBillConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherBillConfig.Properties);
                            unitPrice = (long)(otherBillConfigProperties?.Prices[0]?.Value ?? 0);
                        }
                    }

                    // declare
                    string nameOtherBill = $"Phí khác số {index + 1}";
                    decimal totalIndex = GetTotalIndex(otherBillElement);
                    double costOtherBillUnpaid = GetBillCost(otherBillElement);
                    StringBuilder otherBillRowTemplate = new(GetListOtherBill(tenantId));

                    otherBillRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{NAME_OTHER_BILL}", $"{nameOtherBill}")
                            .Replace("{TOTAL_INDEX_OTHER_BILL}", GetStringValue(totalIndex))
                            .Replace("{UNIT_PRICE_OTHER_BILL}", FormatCost(unitPrice))
                            .Replace("{COST_OTHER_BILL_UNPAID}", FormatCost(costOtherBillUnpaid));
                    listOtherBills += otherBillRowTemplate.ToString();
                }
                #endregion

                // StringBuilder emailTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput() { TenantId = tenantId, Type = ETemplateBillType.BILL }));

                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", FormatNumberToTwoDigits(currentDate.Day))
                    .Replace("{MONTH_NOW}", FormatNumberToTwoDigits(currentDate.Month))

                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{DAY_PERIOD}", FormatNumberToTwoDigits(period.Day))
                    .Replace("{MONTH_PERIOD}", FormatNumberToTwoDigits(period.Month))
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", FormatNumberToTwoDigits(dayWater))
                    .Replace("{DAY_ELECTRIC}", FormatNumberToTwoDigits(dayElectric))
                    .Replace("{DAY_MANAGEMENT}", FormatNumberToTwoDigits(dayManagement))
                    .Replace("{PRE_MONTH_PERIOD}", FormatNumberToTwoDigits(preMonthPeriod.Month))
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{CUSTOMER_PHONE_NUMBER}", $"{customerPhoneNumber}")
                    .Replace("{TAX_CODE}", $"{taxCode}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{MONTH_NAME}", $"{DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_ACRON}", $"{DateTime.Now.ToString("MMM", CultureInfo.InvariantCulture)}")
                    .Replace("{DAY_NAME_ACRON}", $"{DateTime.Now.ToString("dd", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_PERIOD}", $"{period.ToString("MMMM", CultureInfo.InvariantCulture)}")

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{HEAD_INDEX_W}", GetStringValue(indexHeadWater))
                    .Replace("{END_INDEX_W}", GetStringValue(indexEndWater))
                    .Replace("{TOTAL_INDEX_W}", GetStringValue(totalIndexWater))
                    .Replace("{HEAD_INDEX_E}", GetStringValue(indexHeadElectric))
                    .Replace("{END_INDEX_E}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_E}", GetStringValue(totalIndexElectric))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(billElectricConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoney))
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(managementMoneyPrePayment))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))

                    // electric
                    .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(electricMoneyUnpaid))
                    .Replace("{COST_ELECTRIC_PRE_PAYMENT}", FormatCost(electricMoneyPrePayment))
                    .Replace("{COST_ELECTRIC_DEBT}", FormatCost(electricMoneyDebt))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{W_BVMT}", FormatCost(waterMoneyBVMT))  // thanh bình
                    .Replace("{W_VAT}", FormatCost(waterMoneyVAT))  // thanh bình
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(waterMoneyPrePayment))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER_UNPAID_TAX}", FormatCost(waterMoneyUnpaidAndTax)) // thanh bình
                    .Replace("{COST_WATER}", FormatCost(waterMoney))

                    // parking
                    .Replace("{P_MONEY}", FormatCost(p_money))
                    // card parking
                    .Replace("{LIST_VEHICLE_CAR}", listRowVehicleCar)
                    .Replace("{LIST_VEHICLE_BIKE}", listRowVehicleBike)
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // other 
                    .Replace("{COST_OTHER_UNPAID}", FormatCost(otherMoneyUnpaid))
                    .Replace("{COST_OTHER_PRE_PAYMENT}", FormatCost(otherMoneyPrePayment))
                    .Replace("{COST_OTHER_DEBT}", FormatCost(otherMoneyDebt))
                    .Replace("{COST_OTHER}", FormatCost(otherMoney))

                    .Replace("{TOTAL_INDEX_O1}", "-")
                    .Replace("{PRICE_O1}", "-")
                    .Replace("{MONEY_O1}", "-")
                    .Replace("{BUILDING_CODE}", GetBuildingName(buildingId))

                    .Replace("{M_MONEY_PAID}", FormatCost(managementMoneyPaid))
                    .Replace("{P_MONEY_PAID}", FormatCost(parkingMoneyPaid))
                    // total money
                    .Replace("{TOTAL_1}", FormatCost(costUnpaid))
                    .Replace("{DEBT}", FormatCost(costDebt))
                    .Replace("{TOTAL_2}", FormatCost(totalFeeAndDebt))
                    .Replace("{DAY_NOW_FULL}", string.Format("{0:dd/MM/yyyy}", currentDate))
                    .Replace("{HEAD_PERIOD}", string.Format("{0:dd/MM/yyyy}", head_period))
                    .Replace("{END_PERIOD}", string.Format("{0:dd/MM/yyyy}", end_period))
                    .Replace("{W_HEAD_PERIOD}", string.Format("{0:dd/MM/yyyy}", w_head_period))
                    .Replace("{W_END_PERIOD}", string.Format("{0:dd/MM/yyyy}", w_end_period))
                    .Replace("{COST_UNPAID_TAX}", FormatCost(totalFeeDebtAndTax))
                    .Replace("{W_MONEY}", FormatCost(w_money))
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PRE_PAYMENT}", FormatCost(costPrepayment))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_FINAL}", FormatCost(totalFeePayable))
                    .Replace("{COST_FINAL_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeePayable));
                return emailTemplate;
            }
        }


        // Thanh Bình
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateTB(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                DateTime currentDate = DateTime.Now;
                string periodString = period.ToString("MM/yyyy");
                DateTime preMonthPeriod = period.AddMonths(-1);

                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);

                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                long waterMoneyBVMT = (long)(waterMoneyUnpaid * 0.1);
                long waterMoneyVAT = (long)(waterMoneyUnpaid * 0.05);

                double managementMoneyDebt = 0;
                double parkingMoneyDebt = 0;
                double waterMoneyDebt = 0;

                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt + waterMoneyBVMT + waterMoneyVAT;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;

                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt;
                double totalFeeAndDebt = costUnpaid + costDebt + waterMoneyBVMT + waterMoneyVAT;

                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal totalIndexWater = indexEndWater - indexHeadWater;

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
                        StringBuilder vehicleRowTemplate = new("<tr>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_TYPE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_CODE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{COST_PARKING_ELEMENT}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">1</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{COST_PARKING_ELEMENT}</td>\r\n          </tr>");
                        string vehicleName = GetVehicleName(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;

                        vehicleRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                            .Replace("{VEHICLE_TYPE}", $"{vehicleName}")
                            .Replace("{VEHICLE_CODE}", $"{vehicleCode}")
                            .Replace("{COST_PARKING_ELEMENT}", FormatCost((double?)vehicleCost));
                        listRowVehicles += vehicleRowTemplate.ToString();
                    }
                }

                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }
                BillConfigPropertiesDto billWaterConfigProperties = new();
                if (priceWaterConfig?.Properties != null)
                {
                    billWaterConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceWaterConfig.Properties);
                }
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                string customerName = GetCustomerName(userBills, citizenTemp);
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", $"{currentDate.Day:D2}")
                    .Replace("{MONTH_NOW}", $"{currentDate.Month:D2}")
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{MONTH_PERIOD}", $"{period.Month:D2}")
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", $"{28}")
                    .Replace("{PRE_MONTH_PERIOD}", $"{preMonthPeriod.Month:D2}")
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue((double?)acreageApartment))
                    .Replace("{INDEX_HEAD_WATER}", GetStringValue((double?)indexHeadWater))
                    .Replace("{INDEX_END_WATER}", GetStringValue((double?)indexEndWater))
                    .Replace("{TOTAL_INDEX_WATER}", GetStringValue((double?)totalIndexWater))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoneyUnpaid))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))
                    .Replace("{M_MONEY_PAID}", FormatCost(managementMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{COST_WATER_BVMT}", FormatCost(waterMoneyBVMT))
                    .Replace("{COST_WATER_VAT}", FormatCost(waterMoneyVAT))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER}", FormatCost(waterMoney))

                    // parking
                    .Replace("{UNIT_PRICE_PARKING}", "")
                    .Replace("{COST_PARKING_UNPAID}", FormatCost(parkingMoney))
                    .Replace("{COST_PARKING_DEBT}", FormatCost(parkingMoneyDebt))
                    .Replace("{COST_PARKING}", FormatCost(parkingMoney))

                    // card parking
                    .Replace("{COST_CARD_VEHICLE_UNPAID}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // list vehicle
                    .Replace("{LIST_VEHICLE}", $"{listRowVehicles}")
                    // total money
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_PAYMENT_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt));
                return emailTemplate;
            }
        }


        //hts
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateHTS(string apartmentCode, DateTime time, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var carTableTemplate =
                    "<tr>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n          <td style=\"white-space: nowrap; border-width: 1px; border-style: solid;\">\r\n            <span style=\"display: inline-block; width: 50%; padding: 6px 6px 6px 8px\">- {CAR_PNAME}</span><span style=\"display: inline-block; width: 50%; border-width: 0px 0px 0px 1px; border-style: solid; padding: 6px 6px 6px 8px\">BKS: {CAR_CODE}</span>\r\n          </td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">1</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{PRICE_PCAR}</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{PRICE_PCAR}</td>\r\n        </tr>";

                var bikeTableTemplate =
                    "<tr>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n          <td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">- {BIKE_NAME}</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">1</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{BIKE_PRICE}</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{BIKE_PRICE}</td>\r\n        </tr>";

                var otherTableTemplate =
                    "<tr>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n          <td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">- {OTHER_NAME}</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{DIENTICH}</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{OTHER_PRICE}</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{OTHER_MONEY}</td>\r\n        </tr>";
                var currentDay = string.Format("{0:dd/MM/yyyy}", DateTime.Now);

                long buildingId = 0;
                long urbandId = 0;
                var apartment = _apartmentRepos.FirstOrDefault(x => x.ApartmentCode == apartmentCode);
                if (apartment != null)
                {
                    buildingId = apartment.BuildingId.HasValue ? apartment.BuildingId.Value : 0;
                    urbandId = apartment.UrbanId.HasValue ? apartment.UrbanId.Value : 0;
                }

                var priceslist = _billConfigRepository.GetAllList();

                var bills = _userBillRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.Period.Value.Month == time.Month && x.Period.Value.Year == time.Year).ToList();
                var citizenTemp = _citizenTempRepository.FirstOrDefault(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor && x.IsStayed == true);
                var taxCode = "";
                if (citizenTemp == null)
                {
                    citizenTemp = _citizenTempRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor).OrderByDescending(x => x.OwnerGeneration).FirstOrDefault();

                }
                var billDebt = new List<BillDebt>();
                billDebt = citizenTemp != null ? _billDebtRepos.GetAllList(x => x.ApartmentCode == apartmentCode && x.CitizenTempId == citizenTemp.Id) : null;
                taxCode = citizenTemp != null ? citizenTemp.TaxCode + "" : "";

                var debt = 0.0;
                if (billDebt != null && billDebt.Count > 0)
                {
                    foreach (var bill in billDebt) debt += bill.DebtTotal.Value;
                }

                var paymentBill = _billPaymentRepos.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.Status == UserBillPaymentStatus.Success).OrderBy(x => x.CreationTime).Select(x => x.CreationTime).FirstOrDefault();

                var emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                var currentPeriod = string.Format("{0:MM/yyyy}", time);
                var period_month = string.Format("{0:MM}", time);
                var period_day_end = time.TotalDaysInMonth();
                var head_period = new DateTime(time.Year, time.Month, 1);
                var end_period = new DateTime(time.Year, time.Month, period_day_end);
                var period_year = string.Format("{0:yyyy}", time);
                var today = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
                var paymentDay = paymentBill.ToString("dd/MM/yyyy");
                emailTemplate.Replace("{PERIOD_MONTH}", period_month + "");
                emailTemplate.Replace("{PERIOD_DAY}", period_day_end + "");
                emailTemplate.Replace("{PERIOD_YEAR}", period_year + "");
                emailTemplate.Replace("{DAY}", today + "");
                emailTemplate.Replace("{APARTMENT_CODE}", apartmentCode + "");
                emailTemplate.Replace("{MST}", taxCode);
                emailTemplate.Replace("{HEAD_PERIOD}", string.Format("{0:dd/MM/yyyy}", head_period));
                emailTemplate.Replace("{END_PERIOD}", string.Format("{0:dd/MM/yyyy}", end_period));
                emailTemplate.Replace("{MONTH_PERIOD}", string.Format("{0:MM/yyyy}", time));
                emailTemplate.Replace("{PAYMENT_DAY}", paymentDay + "");

                var p_money = 0;


                //Hóa đơn nước
                var w_money = 0;
                var waterbill = bills.FirstOrDefault(x => x.BillType == BillType.Water);

                var priceW = priceslist.FirstOrDefault(x => x.BillType == BillType.Water && x.PricesType == BillConfigPricesType.Level && x.BuildingId == buildingId && x.IsPrivate == true);
                if (priceW == null) priceW = priceslist.FirstOrDefault(x => x.BillType == BillType.Water && x.PricesType == BillConfigPricesType.Level && x.UrbanId == urbandId && x.IsPrivate == true);
                if (priceW == null) priceW = priceslist.FirstOrDefault(x => x.BillType == BillType.Water && x.PricesType == BillConfigPricesType.Level && x.IsPrivate != true);


                var billConfigPropertiesW = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceW.Properties);
                var resultW = 0;
                if (waterbill != null) w_money = (int)waterbill.LastCost;

                if (billConfigPropertiesW != null && billConfigPropertiesW.Prices != null)
                {
                    var brk = false;
                    var amount = waterbill != null ? (int)waterbill.TotalIndex.Value : 0;
                    if (waterbill != null)
                    {
                        emailTemplate.Replace("{HEAD_INDEX}", waterbill.IndexHeadPeriod > 0 ? waterbill.IndexHeadPeriod + "" : "0");
                        emailTemplate.Replace("{END_INDEX}", waterbill.IndexEndPeriod > 0 ? waterbill.IndexEndPeriod + "" : "0");
                    }
                    else
                    {
                        emailTemplate.Replace("{HEAD_INDEX}", "0");
                        emailTemplate.Replace("{END_INDEX}", "0");
                    }
                    var index = 0;
                    foreach (var level in billConfigPropertiesW.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                    }
                }

                //Hóa đơn gửi xe
                var parkingBill = bills.FirstOrDefault(x => x.BillType == BillType.Parking);
                var citizenName = "";
                if (parkingBill == null)
                {
                    emailTemplate.Replace("{CAR_TEMP}", "");
                    emailTemplate.Replace("{BIKE_TEMP}", "");

                }
                else
                {
                    var priceP = priceslist.Where(x => x.BillType == BillType.Parking && x.PricesType == BillConfigPricesType.ParkingLevel).ToList();
                    //if (priceP == null || priceP.Count() == 0)
                    //{

                    //    emailTemplate.Replace("{CAR_TEMP}", "");
                    //    emailTemplate.Replace("{BIKE_TEMP}", "");
                    //}

                    var carPrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Car && x.BuildingId == buildingId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Car && x.UrbanId == urbandId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Car);
                    var motorPrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Motorbike && x.BuildingId == buildingId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Motorbike && x.UrbanId == urbandId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Motorbike);
                    var bikePrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Bicycle && x.BuildingId == buildingId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Bicycle && x.UrbanId == urbandId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Bicycle);
                    var otherPrice = priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Other && x.BuildingId == buildingId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Other && x.UrbanId == urbandId && x.IsPrivate == true) ??
                        priceP.FirstOrDefault(x => x.VehicleType == VehicleType.Other);

                    var carProperties = new BillConfigPropertiesDto();
                    var bikeProperties = new BillConfigPropertiesDto();
                    var motorProperties = new BillConfigPropertiesDto();
                    var otherProperties = new BillConfigPropertiesDto();
                    //var maxCar = 1;
                    //var maxMotor = 1;
                    //var maxBike = 1;
                    //var maxOther = 1;
                    //try
                    //{
                    //    carProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(carPrice.Properties);
                    //    maxCar = carProperties.Prices.Max(x => x.From).Value;
                    //}
                    //catch
                    //{
                    //    carProperties = null;
                    //}

                    //try
                    //{
                    //    bikeProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(bikePrice.Properties);
                    //    maxBike = bikeProperties.Prices.Max(x => x.From).Value;
                    //}
                    //catch
                    //{
                    //    bikeProperties = null;
                    //}

                    //try
                    //{
                    //    motorProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(motorPrice.Properties);
                    //    maxMotor = motorProperties.Prices.Max(x => x.From).Value;
                    //}
                    //catch
                    //{
                    //    motorProperties = null;
                    //}

                    //try
                    //{
                    //    otherProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherPrice.Properties);
                    //    maxOther = otherProperties.Prices.Max(x => x.From).Value;
                    //}
                    //catch
                    //{
                    //    otherProperties = null;
                    //}

                    p_money = (int)parkingBill.LastCost;
                    try
                    {
                        var properties = JsonConvert.DeserializeObject<dynamic>(parkingBill.Properties);
                        citizenName = properties.customerName;
                        string a = properties.vehicles + "";
                        var vehicles = JsonConvert.DeserializeObject<CitizenVehiclePas[]>(a);
                        //var iCar = 1;
                        //var iMotor = 1;
                        //var iBike = 1;
                        //var iOther = 1;
                        //var cCar = 0;
                        //var cMotor = 0;
                        //var cBike = 0;
                        //var cOther = 0;
                        var tempCar = "";
                        var tempMotor = "";
                        var tempBike = "";
                        foreach (var vehicle in vehicles)
                        {

                            switch (vehicle.vehicleType)
                            {
                                case VehicleType.Car:
                                    try
                                    {
                                        var carPkgPros = carProperties;
                                        var tempC = new StringBuilder(carTableTemplate);
                                        var name = $"Phí gửi ô tô số {vehicle.level ?? 1}";
                                        if (vehicle.parkingId > 0)
                                        {
                                            var pkgPrice = priceP.Where(x => x.ParkingId == vehicle.parkingId).FirstOrDefault();
                                            if (pkgPrice != null) name = pkgPrice.Title;

                                        }

                                        tempC.Replace("{PRICE_PCAR}", string.Format("{0:#,#.##}", vehicle.cost ?? 0));
                                        tempC.Replace("{CAR_PNAME}", name);
                                        tempC.Replace("{CAR_CODE}", vehicle.vehicleCode + "");
                                        tempCar = tempCar + "\r\n" + tempC.ToString();
                                    }
                                    catch { }
                                    break;
                                case VehicleType.Motorbike:
                                    try
                                    {
                                        var tempMo = new StringBuilder(carTableTemplate);
                                        var name = $"Phí gửi xe m";
                                        tempMo.Replace("{CAR_PNAME}", name);
                                        tempMo.Replace("{CAR_CODE}", vehicle.vehicleCode + "");
                                        tempMo.Replace("{PRICE_PCAR}", string.Format("{0:#,#.##}", vehicle.cost ?? 0));
                                        tempMotor = tempMotor + "\r\n" + tempMo.ToString();
                                    }
                                    catch { }
                                    break;
                                case VehicleType.Bicycle:
                                    try
                                    {
                                        var tempB = new StringBuilder(bikeTableTemplate);
                                        var name = $"Phí gửi xe đạp";
                                        tempB.Replace("{BIKE_NAME}", name);
                                        tempB.Replace("{BIKE_PRICE}", string.Format("{0:#,#.##}", vehicle.cost));
                                        tempBike = tempBike + "\r\n" + tempB.ToString();
                                    }
                                    catch { }
                                    break;
                                case VehicleType.Other:
                                    break;
                                default:
                                    break;
                            }


                        }

                        var allTemp = tempCar + "\r\n" + tempMotor;
                        emailTemplate.Replace("{CAR_TEMP}", allTemp);
                        emailTemplate.Replace("{BIKE_TEMP}", tempBike);
                    }
                    catch
                    {
                        emailTemplate.Replace("{CAR_TEMP}", "");
                        emailTemplate.Replace("{BIKE_TEMP}", "");
                    }


                }

                //hóa đơn khác
                var t_money = 0;
                var otherBills = bills.Where(x => x.BillType == BillType.Other).ToList();
                StringBuilder other_temp = new StringBuilder();
                if (otherBills != null)
                {
                    int i = 0;
                    foreach (var ob in otherBills)
                    {
                        i++;
                        t_money += (int)ob.LastCost;
                        var defaultName = $"Phí khác số {i}";
                        if (ob.BillConfigId > 0)
                        {
                            var config = priceslist.FirstOrDefault(x => x.Id == ob.BillConfigId);

                            var price = 0;
                            if (config != null)
                            {
                                defaultName = config.Title;
                                try
                                {
                                    var a = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(config.Properties);
                                    price = (int)a.Prices[0].Value;

                                }
                                catch { }
                            }
                            other_temp.AppendLine("<tr>")
                                .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>")
                                .AppendLine($"<td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">- {defaultName} tháng {ob.Period.Value.Month}/{ob.Period.Value.Year}</td>")
                                .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{ob.TotalIndex}</td>")
                                .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{string.Format("{0:#,#.##}", price)}</td>")
                                .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{string.Format("{0:#,#.##}", (int)ob.LastCost)}</td>")
                                .AppendLine($"</tr>");
                        }
                        else
                        {
                            other_temp.AppendLine("<tr>")
                              .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>")
                              .AppendLine($"<td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">- {defaultName} tháng {ob.Period.Value.Month}/{ob.Period.Value.Year}</td>")
                              .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{ob.TotalIndex}</td>")
                              .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>")
                              .AppendLine($"<td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{string.Format("{0:#,#.##}", (int)ob.LastCost)}</td>")
                              .AppendLine($"</tr>");
                        }
                    }
                }

                emailTemplate.Replace("{OTHER_TEMP}", other_temp.ToString());
                // Management
                var m_money = 0;
                var managementBill = bills.FirstOrDefault(x => x.BillType == BillType.Manager);

                var priceM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport && x.BuildingId == buildingId);
                if (priceM == null)
                {
                    priceM = priceslist.FirstOrDefault(x => x.BillType == BillType.Manager && x.PricesType == BillConfigPricesType.Rapport);
                }

                if (priceM != null)
                {
                    var billConfigPropertiesM = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceM.Properties);
                    emailTemplate.Replace("{M_PRICE}", string.Format("{0:#,#.##}", (int)billConfigPropertiesM.Prices[0].Value));
                }
                else
                {
                    emailTemplate.Replace("{M_PRICE}", "");
                }

                if (managementBill != null)
                {
                    m_money = (int)managementBill.LastCost;
                    emailTemplate.Replace("{DIENTICH}", managementBill.TotalIndex > 0 ? managementBill.TotalIndex.Value.ToString("#.##") : "");
                    emailTemplate.Replace("{M_MONEY}", string.Format("{0:#,#.##}", (int)managementBill.LastCost));
                    if (string.IsNullOrEmpty(citizenName))
                    {
                        try
                        {
                            var properties = JsonConvert.DeserializeObject<dynamic>(managementBill.Properties);
                            citizenName = properties.customerName;
                        }
                        catch { }
                    }

                }
                else
                {
                    emailTemplate.Replace("{DIENTICH}", "");
                    emailTemplate.Replace("{M_MONEY}", "");
                }

                var prePaied = 0;
                var mBill = bills.FirstOrDefault(x => x.BillType == BillType.Manager && x.Status == UserBillStatus.Paid);
                if (mBill != null) prePaied = prePaied + (int)mBill.LastCost;

                var pkBill = bills.FirstOrDefault(x => x.BillType == BillType.Parking && x.Status == UserBillStatus.Paid);
                if (pkBill != null) prePaied = prePaied + (int)pkBill.LastCost;

                var totalMoney = Convert.ToInt32(w_money + p_money + m_money + t_money + debt);
                emailTemplate.Replace("{DAY}", currentDay);
                emailTemplate.Replace("{TOTAL_1}", string.Format("{0:#,#.##}", totalMoney));

                emailTemplate.Replace("{DEBT_MONEY}", debt + "");
                emailTemplate.Replace("{CUSTOMER_NAME}", citizenName + "");
                emailTemplate.Replace("{PRE_PAYMENT}", string.Format("{0:#,#.##}", prePaied));
                var total2 = Convert.ToInt32(totalMoney - prePaied);
                emailTemplate.Replace("{TOTAL_2}", total2 > 0 ? string.Format("{0:#,#.##}", total2) : "0");

                return emailTemplate;

            }
        }

        // Aden
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateAden(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {

                DateTime currentDate = DateTime.Now;
                DateTime preMonthPeriod = period.AddMonths(-1);

                var paymentBill = _billPaymentRepos.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.Status == UserBillPaymentStatus.Success).OrderBy(x => x.CreationTime).Select(x => x.CreationTime).FirstOrDefault();

                var period_day_end = period.TotalDaysInMonth();
                string periodString = period.ToString("MM/yyyy");
                var head_period = new DateTime(period.Year, period.Month, 1);
                var end_period = new DateTime(period.Year, period.Month, period_day_end);
                var w_head_period = new DateTime(preMonthPeriod.Year, preMonthPeriod.Month, 1);
                var w_end_period = new DateTime(preMonthPeriod.Year, preMonthPeriod.Month, preMonthPeriod.TotalDaysInMonth());
                var paymentDay = paymentBill.ToString("dd/MM/yyyy");
                DateTime paymentDayDateTime = DateTime.ParseExact(paymentDay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                // await CheckMailSettingsEmptyOrNull();
                // information of citizen who own apartment
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                string taxCode = GetTaxCode(citizenTemp);

                // userbill and billdebt for certain apartments and periods
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();

                // name and phone number of citizen who own apartment
                string customerName = GetCustomerName(userBills, citizenTemp);
                string customerPhoneNumber = citizenTemp?.PhoneNumber ?? "";
                long? buildingId = null;

                // userbill for each type
                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking);
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water);
                UserBill electricBill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);
                UserBill lightBill = userBills.FirstOrDefault(x => x.BillType == BillType.Lighting);
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager);
                List<UserBill> otherBills = userBills.Where(x => x.BillType == BillType.Other).ToList();

                // day water, electric, management, ... 
                int dayWater = await GetDayWater(waterBill, currentDate);
                int dayElectric = await GetDayElectric(electricBill, currentDate);
                int dayManagement = 1;

                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                BillConfig priceElectricConfig = GetBillConfigPrice(billConfigs, BillType.Electric, BillConfigPricesType.Level);
                BillConfig priceLightConfig = GetBillConfigPrice(billConfigs, BillType.Lighting, BillConfigPricesType.Normal);

                // Check billConfig
                if (waterBill != null)
                {
                    try
                    {
                        var id = (long)JsonConvert.DeserializeObject<dynamic>(waterBill.Properties).formulas[0];
                        if (!(priceWaterConfig != null && priceWaterConfig.Id == id)) priceWaterConfig = billConfigs.FirstOrDefault(x => x.Id == id);
                    }
                    catch { }
                }

                if (managementBill != null)
                {
                    try
                    {
                        var id = (long)JsonConvert.DeserializeObject<dynamic>(managementBill.Properties).formulas[0];
                        if (!(priceManagementConfig != null && priceManagementConfig.Id == id)) priceManagementConfig = billConfigs.FirstOrDefault(x => x.Id == id);
                    }
                    catch { }
                }

                if (electricBill != null)
                {
                    try
                    {
                        var id = (long)JsonConvert.DeserializeObject<dynamic>(electricBill.Properties).formulas[0];
                        if (!(priceElectricConfig != null && priceElectricConfig.Id == id)) priceElectricConfig = billConfigs.FirstOrDefault(x => x.Id == id);
                    }
                    catch { }
                }

                // total unpaid bill amount
                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                double electricMoneyUnpaid = GetBillCost(electricBill);
                double lightMoneyUnpaid = GetBillCost(lightBill);
                double otherMoneyUnpaid = (double)otherBills.Sum(x => x.LastCost);

                // tax
                long waterMoneyBVMT = (long)(waterMoneyUnpaid * 0.1);
                long waterMoneyVAT = (long)(waterMoneyUnpaid * 0.05);
                long eMoneyVAT = (long)(electricMoneyUnpaid * 0.08);
                long costlyElectricMoney = (long)(electricMoneyUnpaid * 0.05);
                double waterMoneyUnpaidAndTax = waterMoneyUnpaid + waterMoneyBVMT + waterMoneyVAT;
                double eMoneyUnpaidAndTax = electricMoneyUnpaid + eMoneyVAT + costlyElectricMoney;

                List<UserBill> userBillDetbs = _userBillRepository.GetAll()
                  .Where(x => x.ApartmentCode == apartmentCode)
                  .Where(x => x.Status == UserBillStatus.Debt)
                  .ToList();

                // total debt bill amount for each type
                double managementMoneyDebt = CalculateDebt(userBillDetbs, BillType.Manager, preMonthPeriod);
                double parkingMoneyDebt = CalculateDebt(userBillDetbs, BillType.Parking, preMonthPeriod);
                double waterMoneyDebt = CalculateDebt(userBillDetbs, BillType.Water, preMonthPeriod);
                double electricMoneyDebt = CalculateDebt(userBillDetbs, BillType.Electric, preMonthPeriod);
                double lightMoneyDebt = CalculateDebt(userBillDetbs, BillType.Lighting, preMonthPeriod);
                double otherMoneyDebt = CalculateDebt(userBillDetbs, BillType.Other, preMonthPeriod);

                // pre_payment
                double managementMoneyPrePayment = CalculatePrePayment(userBills, BillType.Manager);
                double parkingMoneyPrePayment = CalculatePrePayment(userBills, BillType.Parking);
                double waterMoneyPrePayment = CalculatePrePayment(userBills, BillType.Water);
                double electricMoneyPrePayment = CalculatePrePayment(userBills, BillType.Electric);
                double lightMoneyPrePayment = CalculatePrePayment(userBills, BillType.Lighting);
                double otherMoneyPrePayment = CalculatePrePayment(userBills, BillType.Other);

                // total bill amount to be paid for each type
                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;
                double electricMoney = electricMoneyUnpaid + electricMoneyDebt;
                double lightMoney = lightMoneyUnpaid + lightMoneyDebt;
                double otherMoney = otherMoneyUnpaid + otherMoneyDebt;

                // total unpaid, debt, pre_payment bill amount 
                double costTax = waterMoneyBVMT + waterMoneyVAT;
                double costVaCos = electricMoneyUnpaid * 0.08 + electricMoneyUnpaid * 0.05;
                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + electricMoneyUnpaid + lightMoneyUnpaid + otherMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt + electricMoneyDebt + lightMoneyDebt + otherMoneyDebt;
                double costPrepayment = parkingMoneyPrePayment + waterMoneyPrePayment + managementMoneyPrePayment + electricMoneyPrePayment + lightMoneyPrePayment + otherMoneyPrePayment;
                double totalFeeAndDebt = costUnpaid + costDebt;
                double totalFeePayable = totalFeeAndDebt - costPrepayment;
                double totalFeePayableAndTax = totalFeePayable + costTax;  // Thanh Bình
                double totalFeeDebtAndTax = costUnpaid + costTax;
                double totalFinal = costUnpaid + costVaCos; //aden


                // paid
                double managementMoneyPaid = managementBill != null && managementBill.Status == UserBillStatus.Paid ? managementBill.LastCost.Value : 0;
                double parkingMoneyPaid = parkingBill != null && parkingBill.Status == UserBillStatus.Paid ? parkingBill.LastCost.Value : 0;


                // electric index, water index, rental area,...
                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal indexHeadElectric = GetIndexHead(electricBill);
                decimal indexEndElectric = GetIndexEnd(electricBill);
                decimal totalIndexWater = GetTotalIndex(waterBill);
                decimal totalIndexElectric = GetTotalIndex(electricBill);

                #region bill config for each type
                // parking 
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }

                // water
                BillConfigPropertiesDto billWaterConfigProperties = null;
                if (priceWaterConfig?.Properties != null)
                {
                    billWaterConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceWaterConfig.Properties);
                }

                // Electric
                BillConfigPropertiesDto billElectricConfigProperties = null;
                if (priceElectricConfig?.Properties != null)
                {
                    billElectricConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceElectricConfig.Properties);
                }

                // Management
                buildingId = managementBill != null ? managementBill.BuildingId : 0;
                BillConfigPropertiesDto billManagementConfigProperties = null;
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                #endregion
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                #region Sản lượng tiêu thụ nước

                //Hóa đơn nước
                var w_money = 0;
                var resultW = 0;
                if (waterBill != null)
                {
                    w_money = (int)waterBill.LastCost;
                    buildingId = buildingId > 0 ? buildingId : waterBill.BuildingId;
                }

                if (billWaterConfigProperties != null && billWaterConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;

                    var index = 0;
                    foreach (var level in billWaterConfigProperties.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                    }
                }


                #endregion

                #region Sản lượng tiêu thụ điện

                var resultE = 0;
                var e_percent = 0.0;
                var e_vat = 0.08;

                if (billElectricConfigProperties != null && billElectricConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = electricBill != null ? (int)electricBill.TotalIndex.Value : 0;
                    var index = 0;
                    foreach (var level in billElectricConfigProperties.Prices)
                    {
                        index++;
                        if (!brk)
                        {
                            if (amount < level.To)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{E_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                            resultE += (int)(level.Value * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        }
                    }

                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{E_INDEX" + i + "}", "");
                        emailTemplate.Replace("{E_PRICE" + i + "}", "");
                        emailTemplate.Replace("{E_AMOUNT" + i + "}", "");
                    }

                    emailTemplate.Replace("{E_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultE));
                    emailTemplate.Replace("{E_VAT}", string.Format("{0:#,#.##}", resultE * e_vat));
                    emailTemplate.Replace("{E_INTO_MONEY}", string.Format("{0:#,#.##}", resultE + e_percent));
                }

                #endregion

                #region Phí quản lý xe
                // list vehicle parking
                double? p_money = 0;
                List<CitizenVehiclePas> listVehicles = new();
                if (parkingBill?.Properties != null)
                {
                    p_money = parkingBill.LastCost;
                    buildingId = buildingId > 0 ? buildingId : parkingBill.BuildingId;
                    string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                    if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                }
                string listRowVehicleCar = "";
                string listRowVehicleBike = "";
                string headerTemplatePaking = $"  <tr>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                    $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px;\">\r\n" +
                    $"           {{HEAD_NAME}}: {string.Format("{0:dd/MM/yyyy}", head_period)} - {string.Format("{0:dd/MM/yyyy}", end_period)}\r\n" +
                    $"          </td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n " +
                    $"         <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                    $"        </tr>";
                if (listVehicles.Count > 0)
                {
                    foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                    {
                        string vehicleName = GetVehicleNameVinasinco(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;

                        StringBuilder vehicleRowTemplate = new(
                            $"        <tr>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                            $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">\r\n" +
                            $"            {vehicleCode}\r\n" +
                            $"          </td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">1</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                            $"        </tr>");

                        if (vehicle.vehicleType == VehicleType.Car)
                        {
                            if (listRowVehicleCar == "")
                            {
                                listRowVehicleCar += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi xe ô tô")
                                    .ToString();
                            }
                            listRowVehicleCar += vehicleRowTemplate.ToString();
                        }
                        else
                        {
                            if (listRowVehicleBike == "")
                            {
                                listRowVehicleBike += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi Xe máy/ Xe máy điện/ Xe đạp điện")
                                    .ToString();
                            }
                            listRowVehicleBike += vehicleRowTemplate.ToString();
                        }

                    }
                }
                #endregion


                #region Hóa đơn khác
                string listOtherBills = string.Empty;
                foreach (var (otherBillElement, index) in otherBills.Select((otherBillElement, index) => (otherBillElement, index)))
                {
                    long unitPrice = 0;
                    if (otherBillElement.BillConfigId != 0)
                    {
                        BillConfig otherBillConfig = billConfigs.FirstOrDefault(x => x.Id == otherBillElement.BillConfigId);
                        if (otherBillConfig?.Properties != null)
                        {
                            BillConfigPropertiesDto otherBillConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(otherBillConfig.Properties);
                            unitPrice = (long)(otherBillConfigProperties?.Prices[0]?.Value ?? 0);
                        }
                    }

                    // declare
                    string nameOtherBill = $"Phí khác số {index + 1}";
                    decimal totalIndex = GetTotalIndex(otherBillElement);
                    double costOtherBillUnpaid = GetBillCost(otherBillElement);
                    StringBuilder otherBillRowTemplate = new(GetListOtherBill(tenantId));

                    otherBillRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            .Replace("{NAME_OTHER_BILL}", $"{nameOtherBill}")
                            .Replace("{TOTAL_INDEX_OTHER_BILL}", GetStringValue(totalIndex))
                            .Replace("{UNIT_PRICE_OTHER_BILL}", FormatCost(unitPrice))
                            .Replace("{COST_OTHER_BILL_UNPAID}", FormatCost(costOtherBillUnpaid));
                    listOtherBills += otherBillRowTemplate.ToString();
                }
                #endregion

                // StringBuilder emailTemplate = new(_templateBillAppService.GetContentOfTemplateBill(new GetTemplateOfTenantInput() { TenantId = tenantId, Type = ETemplateBillType.BILL }));
                var textTotalMoney = VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt);
                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", FormatNumberToTwoDigits(currentDate.Day))
                    .Replace("{MONTH_NOW}", FormatNumberToTwoDigits(currentDate.Month))

                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{DAY_PERIOD}", FormatNumberToTwoDigits(period.Day))
                    .Replace("{MONTH_PERIOD}", FormatNumberToTwoDigits(period.Month))
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", FormatNumberToTwoDigits(dayWater))
                    .Replace("{DAY_ELECTRIC}", FormatNumberToTwoDigits(dayElectric))
                    .Replace("{DAY_MANAGEMENT}", FormatNumberToTwoDigits(dayManagement))
                    .Replace("{PRE_MONTH_PERIOD}", FormatNumberToTwoDigits(preMonthPeriod.Month))
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{CUSTOMER_PHONE_NUMBER}", $"{customerPhoneNumber}")
                    .Replace("{TAX_CODE}", $"{taxCode}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{MONTH_NAME}", $"{DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_ACRON}", $"{DateTime.Now.ToString("MMM", CultureInfo.InvariantCulture)}")
                    .Replace("{DAY_NAME_ACRON}", $"{DateTime.Now.ToString("dd", CultureInfo.InvariantCulture)}")
                    .Replace("{DAY_NAME_ACRON_PAYMENT}", $"{paymentDayDateTime.ToString("dd", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_ACRON_PAYMENT}", $"{paymentDayDateTime.ToString("MMM", CultureInfo.InvariantCulture)}")
                    .Replace("{YEAR_NAME_ACRON_PAYMENT}", $"{paymentDayDateTime.ToString("yyyy", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_PERIOD}", $"{period.ToString("MMMM", CultureInfo.InvariantCulture)}")
                    .Replace("{PAYMENT_DAY}", paymentDay + "")

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue(acreageApartment))
                    .Replace("{HEAD_INDEX_W}", GetStringValue(indexHeadWater))
                    .Replace("{END_INDEX_W}", GetStringValue(indexEndWater))
                    .Replace("{TOTAL_INDEX_W}", GetStringValue(totalIndexWater))
                    .Replace("{HEAD_INDEX_E}", GetStringValue(indexHeadElectric))
                    .Replace("{END_INDEX_E}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_E}", GetStringValue(totalIndexElectric))

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_ELECTRIC}", FormatCost(billElectricConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoney))
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(managementMoneyPrePayment))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))

                    // electric
                    .Replace("{COST_ELECTRIC_UNPAID}", FormatCost(electricMoneyUnpaid))
                    .Replace("{COST_ELECTRIC_PRE_PAYMENT}", FormatCost(electricMoneyPrePayment))
                    .Replace("{COST_ELECTRIC_DEBT}", FormatCost(electricMoneyDebt))
                    .Replace("{COST_ELECTRIC}", FormatCost(electricMoney))
                    .Replace("{E_VAT}", FormatCost(eMoneyVAT))
                    .Replace("{E_COSTLY}", FormatCost(costlyElectricMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{W_BVMT}", FormatCost(waterMoneyBVMT))  // thanh bình
                    .Replace("{W_VAT}", FormatCost(waterMoneyVAT))  // thanh bình
                    .Replace("{COST_WATER_PRE_PAYMENT}", FormatCost(waterMoneyPrePayment))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER_UNPAID_TAX}", FormatCost(waterMoneyUnpaidAndTax)) // thanh bình
                    .Replace("{COST_WATER}", FormatCost(waterMoney))

                    // parking
                    .Replace("{P_MONEY}", FormatCost(p_money))
                    // card parking
                    .Replace("{LIST_VEHICLE_CAR}", listRowVehicleCar)
                    .Replace("{LIST_VEHICLE_BIKE}", listRowVehicleBike)
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // other 
                    .Replace("{COST_OTHER_UNPAID}", FormatCost(otherMoneyUnpaid))
                    .Replace("{COST_OTHER_PRE_PAYMENT}", FormatCost(otherMoneyPrePayment))
                    .Replace("{COST_OTHER_DEBT}", FormatCost(otherMoneyDebt))
                    .Replace("{COST_OTHER}", FormatCost(otherMoney))

                    .Replace("{TOTAL_INDEX_O1}", "-")
                    .Replace("{PRICE_O1}", "-")
                    .Replace("{MONEY_O1}", "-")
                    .Replace("{BUILDING_CODE}", GetBuildingName(buildingId))

                    .Replace("{M_MONEY_PAID}", FormatCost(managementMoneyPaid))
                    .Replace("{P_MONEY_PAID}", FormatCost(parkingMoneyPaid))
                    // total money
                    .Replace("{TOTAL_1}", FormatCost(costUnpaid))
                    .Replace("{DEBT}", FormatCost(costDebt))
                    .Replace("{TOTAL_2}", FormatCost(totalFeePayable))
                    .Replace("{DAY_NOW_FULL}", string.Format("{0:dd/MM/yyyy}", currentDate))
                    .Replace("{HEAD_PERIOD}", string.Format("{0:dd/MM/yyyy}", head_period))
                    .Replace("{END_PERIOD}", string.Format("{0:dd/MM/yyyy}", end_period))
                    .Replace("{W_HEAD_PERIOD}", string.Format("{0:dd/MM/yyyy}", w_head_period))
                    .Replace("{W_END_PERIOD}", string.Format("{0:dd/MM/yyyy}", w_end_period))
                    .Replace("{COST_UNPAID_TAX}", FormatCost(totalFeeDebtAndTax))
                    .Replace("{W_MONEY}", FormatCost(w_money))
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PRE_PAYMENT}", FormatCost(costPrepayment))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_FINAL}", FormatCost(totalFeePayable))
                    .Replace("{AMOUNT_FINAL}", FormatCost(totalFinal))
                    .Replace("{TOTAL_MONEY_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeePayable))
                    .Replace("{TOTAL_PRICE_PAYMENT_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt));

                return emailTemplate;
            }
        }

        //La Thanh
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateLT(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                DateTime currentDate = DateTime.Now;
                string periodString = period.ToString("MM/yyyy");
                DateTime preMonthPeriod = period.AddMonths(-1);

                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                var dueDate = userBills[0]?.DueDate ?? currentDate;
                var creationTime = userBills[0]?.CreationTime ?? currentDate;
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                var head_period = new DateTime(period.Year, period.Month, 1);
                var period_day_end = period.TotalDaysInMonth();
                var end_period = new DateTime(period.Year, period.Month, period_day_end);





                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);
                long waterMoneyBVMT = (long)(waterMoneyUnpaid * 0.1);
                long waterMoneyVAT = (long)(waterMoneyUnpaid * 0.05);
                long waterMoneyCOVID = (long)(waterMoneyUnpaid * 0.15);

                double managementMoneyDebt = 0;
                double parkingMoneyDebt = 0;
                double waterMoneyDebt = 0;

                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt + waterMoneyBVMT + waterMoneyVAT;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;

                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid;
                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt;
                double totalFeeAndDebt = costUnpaid + costDebt;

                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal totalIndexWater = indexEndWater - indexHeadWater;



                //List<CitizenVehiclePas> listVehicles = new();
                ////if (parkingBill?.Properties != null)
                ////{
                ////    string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                ////    if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                ////}
                //#region Phí quản lý xe
                //// list vehicle parking
                //double? p_money = 0;
                //if (parkingBill?.Properties != null)
                //{
                //    p_money = parkingBill.LastCost;
                //    string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(parkingBill?.Properties)?.vehicles?.ToString() ?? null;
                //    if (listVehiclesString != null) listVehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                //}
                //string listRowVehicleCar = "";
                //string listRowVehicleBike = "";
                //string headerTemplatePaking = $"  <tr>\r\n" +
                //    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                //    $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px;\">\r\n" +
                //    $"           {{HEAD_NAME}}: {string.Format("{0:dd/MM/yyyy}", head_period)} - {string.Format("{0:dd/MM/yyyy}", end_period)}\r\n" +
                //    $"          </td>\r\n" +
                //    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                //    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n " +
                //    $"         <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                //    $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                //    $"        </tr>";
                //if (listVehicles.Count > 0)
                //{
                //    foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                //    {
                //        string vehicleName = GetVehicleNameVinasinco(vehicle);
                //        string vehicleCode = vehicle?.vehicleCode ?? "";
                //        decimal vehicleCost = vehicle?.cost ?? 0;

                //        StringBuilder vehicleRowTemplate = new(
                //            $"        <tr>\r\n" +
                //            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\"></td>\r\n" +
                //            $"          <td colspan=\"2\" style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">\r\n" +
                //            $"            {vehicleCode}\r\n" +
                //            $"          </td>\r\n" +
                //            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">1</td>\r\n" +
                //            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                //            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{FormatCost(vehicleCost)}</td>\r\n" +
                //            $"          <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n" +
                //            $"        </tr>");

                //        if (vehicle.vehicleType == VehicleType.Car)
                //        {
                //            if (listRowVehicleCar == "")
                //            {
                //                listRowVehicleCar += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi xe ô tô")
                //                    .ToString();
                //            }
                //            listRowVehicleCar += vehicleRowTemplate.ToString();
                //        }
                //        else
                //        {
                //            if (listRowVehicleBike == "")
                //            {
                //                listRowVehicleBike += new StringBuilder(headerTemplatePaking).Replace("{HEAD_NAME}", "Phí gửi Xe máy/ Xe máy điện/ Xe đạp điện")
                //                    .ToString();
                //            }
                //            listRowVehicleBike += vehicleRowTemplate.ToString();
                //        }

                //    }
                //}
                //#endregion

                string listRowVehicles = "";
                List<CitizenVehiclePas> listVehicles = new();
                if (listVehicles.Count > 0)
                {
                    foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                    {
                        StringBuilder vehicleRowTemplate = new("<tr>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_TYPE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_CODE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{COST_VEHICLE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{COST_PARKING_ELEMENT}</td>\r\n          </tr>");
                        string vehicleName = GetVehicleName(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;
                        // var vehicleCount = vehicle?.level ?? 0;
                        vehicleRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            //.Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                            .Replace("{VEHICLE_TYPE}", $"{vehicleName}")
                            .Replace("{VEHICLE_CODE}", $"{vehicleCode}")
                            // .Replace("{VEHICLE_COUNT}", $"{vehicleCount}")
                            .Replace("{COST_VEHICLE}", $"{vehicleCost}")
                            .Replace("{COST_PARKING_ELEMENT}", FormatCost((double?)vehicleCost));
                        listRowVehicles += vehicleRowTemplate.ToString();
                    }
                }
                else
                {

                }

                // list vehicle parking
                int p_money = 0;
                int priceCar = 0;
                int priceMotor = 0;
                int priceBike = 0;
                int priceOther = 0;
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }
                BillConfigPropertiesDto billWaterConfigProperties = new();
                if (priceWaterConfig?.Properties != null)
                {
                    billWaterConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceWaterConfig.Properties);
                }
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                string customerName = GetCustomerName(userBills, citizenTemp);
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                if (billParkingConfigProperties.Prices != null && billParkingConfigProperties.Prices.Length == 4)
                {

                    priceCar = (int)billParkingConfigProperties.Prices[0].Value;
                    priceMotor = (int)billParkingConfigProperties.Prices[1].Value;
                    priceBike = (int)billParkingConfigProperties.Prices[2].Value;
                    priceOther = (int)billParkingConfigProperties.Prices[3].Value;


                }

                emailTemplate.Replace("{PRICE_CAR}", string.Format("{0:#,#.##}", priceCar));
                emailTemplate.Replace("{PRICE_MOTOR}", string.Format("{0:#,#.##}", priceMotor));
                emailTemplate.Replace("{PRICE_BIKE}", string.Format("{0:#,#.##}", priceBike));
                emailTemplate.Replace("{PRICE_OTHER}", string.Format("{0:#,#.##}", priceOther));

                int numCar = 0;
                int numMotor = 0;
                int numBike = 0;
                int numOther = 0;

                if (parkingBill != null)
                {
                    p_money = (int)parkingBill.LastCost;
                    numCar = parkingBill.CarNumber ?? 0;
                    numMotor = parkingBill.MotorbikeNumber ?? 0;
                    numBike = parkingBill.BicycleNumber ?? 0;
                    numOther = parkingBill.OtherVehicleNumber ?? 0;
                }

                emailTemplate.Replace("{P_MONEY}", string.Format("{0:#,#.##}", p_money));
                emailTemplate.Replace("{CAR_NUMBER}", numCar + "");
                emailTemplate.Replace("{MOTOR_NUMBER}", numMotor + "");
                emailTemplate.Replace("{BIKE_NUMBER}", numBike + "");
                emailTemplate.Replace("{OTHER_NUMBER}", numOther + "");

                emailTemplate.Replace("{MONEY_CAR}", FormatCost(priceCar * numCar));
                emailTemplate.Replace("{MONEY_MOTOR}", FormatCost(priceMotor * numMotor));
                emailTemplate.Replace("{MONEY_BIKE}", FormatCost(priceBike * numBike));
                emailTemplate.Replace("{MONEY_OTHER}", FormatCost(priceOther * numOther));


                //var listVehicle = new CitizenVehicleDto();
                //listVehicle = (CitizenVehicleDto)await GetNameVehicleByApartment(apartmentCode);
                //if (listVehicle != null && listVehicle.VehicleType == VehicleType.Car)
                //{
                //    emailTemplate.Replace("{LIST_CAR}", listVehicle.VehicleCode);
                //}
                //if (listVehicle != null && listVehicle.VehicleType == VehicleType.Motorbike)
                //{
                //    emailTemplate.Replace("{LIST_MOTORBIKE}", listVehicle.VehicleCode);
                //}

                string listWaterConsumptions = string.Empty;
                int dayWater = await GetDayWater(waterBill, currentDate);

                if (billWaterConfigProperties.Prices != null && billWaterConfigProperties.Prices.Any())
                {
                    List<PriceDto> listUnitPrices = billWaterConfigProperties.Prices.ToList();
                    foreach (var (unitPrice, index) in listUnitPrices.Select((unitPrice, index) => (unitPrice, index)))
                    {
                        // declare
                        int waterConsumptionQuantity;
                        long unitPriceWater;
                        long costWaterUnpaid;

                        string utilityWater = "m3";
                        StringBuilder waterRowTemplate = new("<tr> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px\"> {U_WATER} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{WATER_CONSUMPTION_QUANTITY}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> {UNIT_PRICE_WATER}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{COST_WATER_UNPAID} </td> </tr>");
                        string unitWaterName = $"Từ {unitPrice?.From} tới {unitPrice?.To} (m3)" ?? "";

                        // caculate price for each unit and content
                        if (index == 0 && unitPrice.To < totalIndexWater)
                        {
                            waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        else if (unitPrice.To.HasValue && unitPrice.To < totalIndexWater)
                        {
                            waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From + 1);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        else
                        {
                            waterConsumptionQuantity = (int)(totalIndexWater - unitPrice.From) <= 0 ? 0 : (int)(totalIndexWater - unitPrice.From);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        if (index != 0)
                        {
                            utilityWater = string.Empty;
                        }

                        // complete
                        waterRowTemplate
                            .Replace("{INDEX}", $"{index + 1}")
                            .Replace("{UTILITY}", utilityWater)
                            .Replace("{U_WATER}", unitWaterName)
                            .Replace("{WATER_CONSUMPTION_QUANTITY}", $"{waterConsumptionQuantity}")
                            .Replace("{UNIT_PRICE_WATER}", FormatCost(unitPriceWater))
                            .Replace("{COST_WATER_UNPAID}", FormatCost(costWaterUnpaid));

                        listWaterConsumptions += waterRowTemplate.ToString();
                    }
                }
                var w_money = 0;
                var resultW = 0;
                if (waterBill != null) w_money = (int)waterBill.LastCost;

                if (billWaterConfigProperties != null && billWaterConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;

                    var index = 0;
                    foreach (var level in billWaterConfigProperties.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;
                        string unitWaterName = $"Từ {level?.From} tới {level?.To} (m3)" ?? "";
                        emailTemplate.Replace("{U_WATER" + index + "}", unitWaterName + "");

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                        emailTemplate.Replace("{INDEX" + i + "}", "");
                    }
                    emailTemplate.Replace("{W_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultW));

                }
                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", $"{currentDate.Day:D2}")
                    .Replace("{MONTH_NOW}", $"{currentDate.Month:D2}")
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{MONTH_PERIOD}", $"{period.Month:D2}")
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", $"{28}")
                    .Replace("{PRE_MONTH_PERIOD}", $"{preMonthPeriod.Month:D2}")
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{DUE_DATE}", FormatDateTime(dueDate, "dd/MM/yyyy"))
                    .Replace("{CREATIONTIME}", FormatDateTime(creationTime, "dd/MM/yyyy"))

                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue((double?)acreageApartment))
                    .Replace("{INDEX_HEAD_WATER}", GetStringValue((double?)indexHeadWater))
                    .Replace("{INDEX_END_WATER}", GetStringValue((double?)indexEndWater))
                    .Replace("{TOTAL_INDEX_WATER}", GetStringValue((double?)totalIndexWater))
                    .Replace("{LIST_WATER}", listWaterConsumptions)

                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoneyUnpaid))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))
                    .Replace("{M_MONEY_PAID}", FormatCost(managementMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{COST_WATER_BVMT}", FormatCost(waterMoneyBVMT))
                    .Replace("{COST_WATER_VAT}", FormatCost(waterMoneyVAT))
                    .Replace("{COST_WATER_COVID}", FormatCost(waterMoneyCOVID))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER}", FormatCost(waterMoney))
                    .Replace("{W_MONEY}", FormatCost(w_money))


                    // parking
                    .Replace("{UNIT_PRICE_PARKING}", "")
                    .Replace("{COST_PARKING_UNPAID}", FormatCost(parkingMoney))
                    .Replace("{COST_PARKING_DEBT}", FormatCost(parkingMoneyDebt))
                    .Replace("{COST_PARKING}", FormatCost(parkingMoney))

                    // card parking
                    .Replace("{COST_CARD_VEHICLE_UNPAID}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // list vehicle
                    .Replace("{LIST_VEHICLE}", $"{listRowVehicles}")
                    //.Replace("{LIST_WATER}", $"{listRowWaterConfig}")
                    // total money
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_PAYMENT_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt));
                return emailTemplate;
            }
        }
       

        //Trung do
        [RemoteService(false)]
        public async Task<StringBuilder> CreateTemplateTrungDo(string apartmentCode, DateTime period, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                DateTime currentDate = DateTime.Now;
                string periodString = period.ToString("MM/yyyy");
                DateTime preMonthPeriod = period.AddMonths(-1);

                List<BillConfig> billConfigs = _billConfigRepository.GetAllList();
                List<UserBill> userBills = _userBillRepository.GetAll()
                     .Where(x => x.ApartmentCode == apartmentCode)
                     .Where(x => (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year))
                     .ToList();
                var dueDate = userBills[0]?.DueDate ?? currentDate;
                var creationTime = userBills[0]?.CreationTime ?? currentDate;
                CitizenTemp citizenTemp =
                    _citizenTempRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.RelationShip == RELATIONSHIP.Contractor &&
                        x.IsStayed == true) ??
                    _citizenTempRepository.GetAll()
                        .Where(x => x.ApartmentCode == apartmentCode && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();
                List<BillDebt>? billDebt = _billDebtRepos.GetAllList(x =>
                        x.ApartmentCode == apartmentCode &&
                        x.Period.Year == period.Year &&
                        x.Period.Month == period.Month &&
                        (citizenTemp == null || x.CitizenTempId == citizenTemp.Id)).ToList();
                UserBill parkingBill = userBills.FirstOrDefault(x => x.BillType == BillType.Parking && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill waterBill = userBills.FirstOrDefault(x => x.BillType == BillType.Water && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                UserBill electricBill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);
                UserBill managementBill = userBills.FirstOrDefault(x => x.BillType == BillType.Manager && (x.Period.Value.Month == period.Month &&
                            x.Period.Value.Year == period.Year));
                var priceslist = _billConfigRepository.GetAllList();
                var priceE = priceslist.FirstOrDefault(x => x.BillType == BillType.Electric && x.PricesType == BillConfigPricesType.Level);
                var percentsE = priceslist.Where(x => x.BillType == BillType.Electric && x.PricesType == BillConfigPricesType.Percentage).ToList();
                BillConfig priceParkingConfig = GetBillConfigPrice(billConfigs, BillType.Parking, BillConfigPricesType.Parking);
                BillConfig priceManagementConfig = GetBillConfigPrice(billConfigs, BillType.Manager, BillConfigPricesType.Rapport);
                BillConfig priceWaterConfig = GetBillConfigPrice(billConfigs, BillType.Water, BillConfigPricesType.Level);
                var head_period = new DateTime(period.Year, period.Month, 1);
                var period_day_end = period.TotalDaysInMonth();
                var end_period = new DateTime(period.Year, period.Month, period_day_end);





                double parkingMoneyUnpaid = GetBillCost(parkingBill);
                double waterMoneyUnpaid = GetBillCost(waterBill);
                double managementMoneyUnpaid = GetBillCost(managementBill);

                double electricMoneyUnpaid = GetBillCost(electricBill);


                double managementMoneyDebt = 0;
                double parkingMoneyDebt = 0;
                double waterMoneyDebt = 0;
                var e_vat = 0.08;
                var e_ht = 0.09;
                double managementMoney = managementMoneyUnpaid + managementMoneyDebt;
                double waterMoney = waterMoneyUnpaid + waterMoneyDebt;
                double parkingMoney = parkingMoneyUnpaid + parkingMoneyDebt;

                double costUnpaid = parkingMoneyUnpaid + waterMoneyUnpaid + managementMoneyUnpaid + electricMoneyUnpaid;

                double costDebt = managementMoneyDebt + parkingMoneyDebt + waterMoneyDebt;
                double totalFeeAndDebt = costUnpaid + costDebt;

                decimal? acreageApartment = GetAcreageApartment(managementBill);
                decimal indexHeadWater = GetIndexHead(waterBill);
                decimal indexEndWater = GetIndexEnd(waterBill);
                decimal totalIndexWater = indexEndWater - indexHeadWater;
                decimal indexHeadElectric = GetIndexHead(electricBill);
                decimal indexEndElectric = GetIndexEnd(electricBill);
                decimal totalIndexElectric = GetTotalIndex(electricBill);



                string listRowVehicles = "";
                List<CitizenVehiclePas> listVehicles = new();
                if (listVehicles.Count > 0)
                {
                    foreach (var (vehicle, index) in listVehicles.Select((vehicle, index) => (vehicle, index)))
                    {
                        StringBuilder vehicleRowTemplate = new("<tr>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_TYPE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_CODE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\"></td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{COST_VEHICLE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{COST_PARKING_ELEMENT}</td>\r\n          </tr>");
                        string vehicleName = GetVehicleName(vehicle);
                        string vehicleCode = vehicle?.vehicleCode ?? "";
                        decimal vehicleCost = vehicle?.cost ?? 0;
                        // var vehicleCount = vehicle?.level ?? 0;
                        vehicleRowTemplate
                            .Replace("{INDEX}", GetStringValue(index + 1))
                            //.Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                            .Replace("{VEHICLE_TYPE}", $"{vehicleName}")
                            .Replace("{VEHICLE_CODE}", $"{vehicleCode}")
                            // .Replace("{VEHICLE_COUNT}", $"{vehicleCount}")
                            .Replace("{COST_VEHICLE}", $"{vehicleCost}")
                            .Replace("{COST_PARKING_ELEMENT}", FormatCost((double?)vehicleCost));
                        listRowVehicles += vehicleRowTemplate.ToString();
                    }
                }
                else
                {

                }

                // list vehicle parking
                int p_money = 0;
                int priceCar = 0;
                int priceMotor = 0;
                int priceBike = 0;
                int priceOther = 0;
                int priceECar = 0;
                int priceEMotor = 0;
                int priceEBike = 0;
                BillConfigPropertiesDto billParkingConfigProperties = new();
                if (priceParkingConfig?.Properties != null)
                {
                    billParkingConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceParkingConfig.Properties);
                }
                BillConfigPropertiesDto billWaterConfigProperties = new();
                if (priceWaterConfig?.Properties != null)
                {
                    billWaterConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceWaterConfig.Properties);
                }
                BillConfigPropertiesDto billManagementConfigProperties = new();
                if (priceManagementConfig?.Properties != null)
                {
                    billManagementConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceManagementConfig.Properties);
                }
                string customerName = GetCustomerName(userBills, citizenTemp);
                StringBuilder emailTemplate = new StringBuilder(_emailTemplateProvider.GetUserBillTemplate(tenantId));
                if (billParkingConfigProperties.Prices != null && billParkingConfigProperties.Prices.Length == 4)
                {

                    priceCar = (int)billParkingConfigProperties.Prices[0].Value;
                    priceMotor = (int)billParkingConfigProperties.Prices[1].Value;
                    priceBike = (int)billParkingConfigProperties.Prices[2].Value;
                    priceECar = (int)billParkingConfigProperties.Prices[3].Value;
                    priceEMotor = (int)billParkingConfigProperties.Prices[4].Value;
                    priceEBike = (int)billParkingConfigProperties.Prices[5].Value;
                    priceOther = (int)billParkingConfigProperties.Prices[6].Value;


                }

                emailTemplate.Replace("{PRICE_CAR}", string.Format("{0:#,#.##}", priceCar));
                emailTemplate.Replace("{PRICE_MOTOR}", string.Format("{0:#,#.##}", priceMotor));
                emailTemplate.Replace("{PRICE_BIKE}", string.Format("{0:#,#.##}", priceBike));
                emailTemplate.Replace("{PRICE_ECAR}", string.Format("{0:#,#.##}", priceECar));
                emailTemplate.Replace("{PRICE_EMOTOR}", string.Format("{0:#,#.##}", priceEMotor));
                emailTemplate.Replace("{PRICE_EBIKE}", string.Format("{0:#,#.##}", priceEBike));
                emailTemplate.Replace("{PRICE_OTHER}", string.Format("{0:#,#.##}", priceOther));

                int numCar = 0;
                int numMotor = 0;
                int numBike = 0;
                int numOther = 0;
                //int numECar = 0;
                //int numEMotor = 0;
                //int numEBike = 0;

                if (parkingBill != null)
                {
                    p_money = (int)parkingBill.LastCost;
                    numCar = parkingBill.CarNumber ?? 0;
                    numMotor = parkingBill.MotorbikeNumber ?? 0;
                    numBike = parkingBill.BicycleNumber ?? 0;
                    numOther = parkingBill.OtherVehicleNumber ?? 0;
                    //numECar = parkingBill.ECarNumber ?? 0;
                    //numOther = parkingBill.EMotorNumber ?? 0;
                    //numOther = parkingBill.EBikeNumber ?? 0;
                }

                emailTemplate.Replace("{P_MONEY}", string.Format("{0:#,#.##}", p_money));
                emailTemplate.Replace("{CAR_NUMBER}", numCar + "");
                emailTemplate.Replace("{MOTOR_NUMBER}", numMotor + "");
                emailTemplate.Replace("{BIKE_NUMBER}", numBike + "");
                emailTemplate.Replace("{OTHER_NUMBER}", numOther + "");

                emailTemplate.Replace("{MONEY_CAR}", FormatCost(priceCar * numCar));
                emailTemplate.Replace("{MONEY_ECAR}", FormatCost(priceCar * numCar));
                emailTemplate.Replace("{MONEY_MOTOR}", FormatCost(priceMotor * numMotor));
                emailTemplate.Replace("{MONEY_EMOTOR}", FormatCost(priceMotor * numMotor));
                emailTemplate.Replace("{MONEY_ALLMOTOR}", FormatCost(priceMotor * numMotor));
                emailTemplate.Replace("{MONEY_BIKE}", FormatCost(priceBike * numBike));
                emailTemplate.Replace("{MONEY_EBIKE}", FormatCost(priceBike * numBike));
                emailTemplate.Replace("{MONEY_ALLBIKE}", FormatCost(priceBike * numBike));
                emailTemplate.Replace("{MONEY_OTHER}", FormatCost(priceOther * numOther));

                string listWaterConsumptions = string.Empty;
                int dayWater = await GetDayWater(waterBill, currentDate);

                if (billWaterConfigProperties.Prices != null && billWaterConfigProperties.Prices.Any())
                {
                    List<PriceDto> listUnitPrices = billWaterConfigProperties.Prices.ToList();
                    foreach (var (unitPrice, index) in listUnitPrices.Select((unitPrice, index) => (unitPrice, index)))
                    {
                        // declare
                        int waterConsumptionQuantity;
                        long unitPriceWater;
                        long costWaterUnpaid;

                        string utilityWater = "m3";
                        StringBuilder waterRowTemplate = new("<tr> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px\"> {U_WATER} </td> <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{WATER_CONSUMPTION_QUANTITY}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\"> {UNIT_PRICE_WATER}</td> <td style=\"border-width: 1px; border-style: solid; padding: 6px 10px; text-align: right;\">{COST_WATER_UNPAID} </td> </tr>");
                        string unitWaterName = $"Từ {unitPrice?.From} tới {unitPrice?.To} (m3)" ?? "";

                        // caculate price for each unit and content
                        if (index == 0 && unitPrice.To < totalIndexWater)
                        {
                            waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        else if (unitPrice.To.HasValue && unitPrice.To < totalIndexWater)
                        {
                            waterConsumptionQuantity = (int)(unitPrice.To - unitPrice.From + 1);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        else
                        {
                            waterConsumptionQuantity = (int)(totalIndexWater - unitPrice.From) <= 0 ? 0 : (int)(totalIndexWater - unitPrice.From);
                            unitPriceWater = (long)unitPrice.Value;
                            costWaterUnpaid = waterConsumptionQuantity * unitPriceWater;
                        }
                        if (index != 0)
                        {
                            utilityWater = string.Empty;
                        }

                        // complete
                        waterRowTemplate
                            .Replace("{INDEX}", $"{index + 1}")
                            .Replace("{UTILITY}", utilityWater)
                            .Replace("{U_WATER}", unitWaterName)
                            .Replace("{WATER_CONSUMPTION_QUANTITY}", $"{waterConsumptionQuantity}")
                            .Replace("{UNIT_PRICE_WATER}", FormatCost(unitPriceWater))
                            .Replace("{COST_WATER_UNPAID}", FormatCost(costWaterUnpaid));

                        listWaterConsumptions += waterRowTemplate.ToString();
                    }
                }
                var w_money = 0;
                var resultW = 0;
                if (waterBill != null) w_money = (int)waterBill.LastCost;

                if (billWaterConfigProperties != null && billWaterConfigProperties.Prices != null)
                {
                    var brk = false;
                    var amount = waterBill != null ? (int)waterBill.TotalIndex.Value : 0;

                    var index = 0;
                    foreach (var level in billWaterConfigProperties.Prices)
                    {
                        index++;
                        //var levelVal = level.Value + (0.05 * level.Value) + (0.1 * level.Value);
                        var levelVal = level.Value;
                        string unitWaterName = $"Từ {level?.From} tới {level?.To} (m3)" ?? "";
                        emailTemplate.Replace("{U_WATER" + index + "}", unitWaterName + "");

                        if (!brk)
                        {

                            if (amount < level.To)
                            {
                                var kla = amount > 0 ? (index == 1 ? amount - level.From : amount - level.From + 1) : 0;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{W_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                                emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kla)));
                                resultW += (int)(levelVal * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{W_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                            emailTemplate.Replace("{W_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(levelVal * kl)));
                            resultW += (int)(levelVal * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{W_PRICE" + index + "}", string.Format("{0:#,#.##}", levelVal));
                        }
                    }
                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{W_INDEX" + i + "}", "");
                        emailTemplate.Replace("{W_PRICE" + i + "}", "");
                        emailTemplate.Replace("{W_AMOUNT" + i + "}", "");
                        emailTemplate.Replace("{INDEX" + i + "}", "");
                    }
                    emailTemplate.Replace("{W_TOTAL_AMOUNT}", string.Format("{0:#,#.##}", resultW));

                }
                var resultE = 0;
                var billConfigPropertiesE = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(priceE.Properties);
                var electrictbill = userBills.FirstOrDefault(x => x.BillType == BillType.Electric);

                if (billConfigPropertiesE != null && billConfigPropertiesE.Prices != null)
                {
                    var brk = false;
                    var amount = electrictbill != null ? (int)electrictbill.TotalIndex.Value : 0;
                    var index = 0;
                    foreach (var level in billConfigPropertiesE.Prices)
                    {
                        index++;
                        if (!brk)
                        {
                            if (amount < level.To)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            if (!level.To.HasValue)
                            {
                                var kla = index == 1 ? amount - level.From : amount - level.From + 1;
                                emailTemplate.Replace("{E_INDEX" + index + "}", kla + "");
                                emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                                emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kla)));
                                resultE += (int)(level.Value * kla);
                                brk = true;
                                continue;
                            }

                            var kl = index == 1 ? level.To - level.From : level.To - level.From + 1;
                            emailTemplate.Replace("{E_INDEX" + index + "}", kl + "");
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                            emailTemplate.Replace("{E_AMOUNT" + index + "}", string.Format("{0:#,#.##}", (int)(level.Value * kl)));
                            resultE += (int)(level.Value * kl);
                        }
                        else
                        {
                            emailTemplate.Replace("{E_PRICE" + index + "}", string.Format("{0:#,#.##}", level.Value));
                        }
                    }

                    for (var i = 1; i < 7; i++)
                    {
                        emailTemplate.Replace("{E_INDEX" + i + "}", "");
                        emailTemplate.Replace("{E_PRICE" + i + "}", "");
                        emailTemplate.Replace("{E_AMOUNT" + i + "}", "");
                    }

                    emailTemplate.Replace("{E_TOTAL_AMOUNT}",
                                            string.Format("{0:#,#.##}", electricBill.LastCost.HasValue ? string.Format("{0:#,#}", Math.Round(electricBill.LastCost.Value)) : null));


                    // emailTemplate.Replace("{E_INTO_MONEY}", string.Format("{0:#,#.##}", resultE + e_percent));
                    emailTemplate.Replace("{E_INTO_MONEY}", string.Format("{0:#,#.##}", resultE));
                }
                
                double parkingMoneyPaid = parkingBill != null && parkingBill.Status == UserBillStatus.Paid ? parkingBill.LastCost.Value : 0;


                // edit EmailTemplate
                emailTemplate
                    .Replace("{DAY_NOW}", $"{currentDate.Day:D2}")
                    .Replace("{MONTH_NOW}", $"{currentDate.Month:D2}")
                    .Replace("{YEAR_NOW}", $"{currentDate.Year}")
                    .Replace("{PERIOD}", $"{periodString}")
                    .Replace("{MONTH_PERIOD}", $"{period.Month:D2}")
                    .Replace("{YEAR_PERIOD}", $"{period.Year}")
                    .Replace("{DAY_WATER}", $"{28}")
                    .Replace("{PRE_MONTH_PERIOD}", $"{preMonthPeriod.Month:D2}")
                    .Replace("{PRE_YEAR_PERIOD}", $"{preMonthPeriod.Year}")
                    .Replace("{CUSTOMER_NAME}", $"{customerName}")
                    .Replace("{APARTMENT_CODE}", $"{apartmentCode}")
                    .Replace("{DUE_DATE}", FormatDateTime(dueDate, "dd/MM/yyyy"))
                    .Replace("{CREATIONTIME}", FormatDateTime(creationTime, "dd/MM/yyyy"))
                    .Replace("{MONTH_NAME_PERIOD}", $"{period.ToString("MMMM", CultureInfo.InvariantCulture)}")
                    .Replace("{MONTH_NAME_ACRON}", $"{DateTime.Now.ToString("MMM", CultureInfo.InvariantCulture)}")
                    // amount 
                    .Replace("{ACREAGE_APARTMENT}", GetStringValue((double?)acreageApartment))
                    .Replace("{HEAD_INDEX_W}", GetStringValue((double?)indexHeadWater))
                    .Replace("{END_INDEX_W}", GetStringValue((double?)indexEndWater))
                    .Replace("{HEAD_INDEX_E}", GetStringValue(indexHeadElectric))
                    .Replace("{END_INDEX_E}", GetStringValue(indexEndElectric))
                    .Replace("{TOTAL_INDEX_W}", GetStringValue((double?)totalIndexWater))
                    .Replace("{TOTAL_INDEX_E}", GetStringValue(totalIndexElectric))
                    .Replace("{LIST_WATER}", listWaterConsumptions)
                    .Replace("{E_VAT}", string.Format("{0:#,#.##}", resultE * e_vat))
                    .Replace("{E_HT}", string.Format("{0:#,#.##}", resultE * e_ht))
                    .Replace("{P_MONEY_PAID}", FormatCost(parkingMoneyPaid))


                    // unit price
                    .Replace("{UNIT_PRICE_MANAGEMENT}", FormatCost(billManagementConfigProperties?.Prices[0].Value))
                    .Replace("{UNIT_PRICE_WATER}", FormatCost(billWaterConfigProperties?.Prices[0].Value))

                    // management
                    .Replace("{COST_MANAGEMENT_UNPAID}", FormatCost(managementMoneyUnpaid))
                    .Replace("{COST_MANAGEMENT_DEBT}", FormatCost(managementMoneyDebt))
                    .Replace("{COST_MANAGEMENT}", FormatCost(managementMoney))
                    .Replace("{M_MONEY_PAID}", FormatCost(managementMoney))

                    // water
                    .Replace("{COST_WATER_UNPAID}", FormatCost(waterMoneyUnpaid))
                    .Replace("{COST_WATER_DEBT}", FormatCost(waterMoneyDebt))
                    .Replace("{COST_WATER}", FormatCost(waterMoney))
                    .Replace("{W_MONEY}", FormatCost(w_money))


                    // parking
                    .Replace("{UNIT_PRICE_PARKING}", "")
                    .Replace("{COST_PARKING_UNPAID}", FormatCost(parkingMoney))
                    .Replace("{COST_PARKING_DEBT}", FormatCost(parkingMoneyDebt))
                    .Replace("{COST_PARKING}", FormatCost(parkingMoney))

                    // card parking
                    .Replace("{COST_CARD_VEHICLE_UNPAID}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE_DEBT}", FormatCost(0))
                    .Replace("{COST_CARD_VEHICLE}", FormatCost(0))

                    // list vehicle
                    .Replace("{LIST_VEHICLE}", $"{listRowVehicles}")
                    //.Replace("{LIST_WATER}", $"{listRowWaterConfig}")
                    // total money
                    .Replace("{COST_UNPAID}", FormatCost(costUnpaid))
                    .Replace("{COST_DEBT}", FormatCost(costDebt))
                    .Replace("{COST_PAYMENT}", FormatCost(totalFeeAndDebt))
                    .Replace("{COST_PAYMENT_TEXT}", VietNameseConverter.FormatCurrency((decimal)totalFeeAndDebt))
                .Replace("{TOTAL_2}", FormatCost(totalFeeAndDebt));
                return emailTemplate;
            }
        }



        #region Helper methods
        private double GetBillCost(UserBill bill) => bill?.LastCost ?? 0;
        private int GetMonthNumber(UserBill bill) => bill != null ? bill.MonthNumber ?? 1 : 0;
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
        private BillConfig GetBillConfigPrice(IEnumerable<BillConfig> billConfigs, BillType billType, BillConfigPricesType pricesType)
        {
            return billConfigs.FirstOrDefault(x => x.BillType == billType && x.PricesType == pricesType && x.IsDefault == true)
                ?? billConfigs.FirstOrDefault(x => x.BillType == billType && x.PricesType == pricesType);
        }
        private int ValidateAndSetDueDate(int currentDueDate, UserBill bill, DateTime nextMonth)
        {
            if (currentDueDate < 1) return bill?.DueDate?.Day ?? 1;
            if (currentDueDate > DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month))
            {
                return bill?.DueDate?.Day ?? DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            }
            return currentDueDate;
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
                .Sum(bill => (bill.DebtTotal > 0 ? (double)bill.DebtTotal.Value : bill.LastCost.Value)) ?? 0;
        }
        private double CalculatePrePayment(IEnumerable<UserBill> userBills, BillType billType)
        {
            return userBills?
                .Where(bill =>
                    bill.LastCost.HasValue &&
                    bill.BillType == billType &&
                    bill.Status == UserBillStatus.Paid)
                .Sum(bill => bill.LastCost.Value) ?? 0;
        }
        private decimal GetIndexHead(UserBill userBill) => userBill?.IndexHeadPeriod ?? 0;
        private decimal GetIndexEnd(UserBill userBill) => userBill?.IndexEndPeriod ?? 0;
        private decimal GetTotalIndex(UserBill userBill) => userBill?.TotalIndex ?? 0;
        private string GetVehicleNameById(long parkingId)
        {
            return _citizenVehicleRepository.FirstOrDefault(parkingId)?.VehicleName ?? "";
        }
        private string GetVehicleName(CitizenVehiclePas vehicle)
        {
            string vehicleName = string.Empty;
            switch (vehicle.vehicleType)
            {
                case VehicleType.ElectricCar:
                    vehicleName = $"Ô tô điện";
                    break;
                case VehicleType.ElectricMotor:
                    vehicleName = $"Xe máy điện";
                    break;
                case VehicleType.ElectricBike:
                    vehicleName = $"Xe đạp điện";
                    break;
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
        private string GetVehicleNameVinasinco(CitizenVehiclePas vehicle)
        {
            string vehicleName = string.Empty;
            switch (vehicle.vehicleType)
            {
                case VehicleType.Car:
                    vehicleName = $"Ô tô {vehicle.vehicleName}, biển số {vehicle.vehicleCode}";
                    break;
                case VehicleType.Motorbike:
                    vehicleName = $"Xe máy {vehicle.vehicleName}, biển số {vehicle.vehicleCode}";
                    break;
                case VehicleType.Bicycle:
                    vehicleName = $"Xe đạp";
                    break;
                default:
                    break;
            };
            return vehicleName;
        }
        private string GetListWaterConsumption(int? tenantId)
        {
            switch (tenantId)
            {
                case 62:  // hudlands
                    return "<tr>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">{WATER_CONSUMPTION_RANGE}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{WATER_CONSUMPTION_QUANTITY}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{UNIT_PRICE_WATER}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_WATER_UNPAID}</td>\r\n          </tr>";
                case 65:  // nhà ở xã hội (65)
                    return "<tr>\r\n <td\r\n style=\"\r\n padding: 4px;\r\n text-align: left;\r\n font-weight: 600;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n Sinh hoạt {INDEX}\r\n </td>\r\n <td\r\n style=\"\r\n width: 68px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n ></td>\r\n <td\r\n style=\"\r\n width: 68px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n ></td>\r\n <td\r\n style=\"\r\n width: 68px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n {WATER_CONSUMPTION_QUANTITY}\r\n </td>\r\n <td\r\n style=\"\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n {UNIT_PRICE_WATER}\r\n </td>\r\n <td\r\n style=\"\r\n width: 164px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n {COST_WATER_UNPAID}\r\n </td>\r\n </tr>";
                case 64:  // htc
                    return "<tr>\r\n <td\r\n style=\"\r\n padding: 4px;\r\n text-align: left;\r\n font-weight: 600;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n Sinh hoạt {INDEX}\r\n </td>\r\n <td\r\n style=\"\r\n width: 68px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n ></td>\r\n <td\r\n style=\"\r\n width: 68px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n ></td>\r\n <td\r\n style=\"\r\n width: 68px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n {WATER_CONSUMPTION_QUANTITY}\r\n </td>\r\n <td\r\n style=\"\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n {UNIT_PRICE_WATER}\r\n </td>\r\n <td\r\n style=\"\r\n width: 164px;\r\n padding: 4px;\r\n text-align: center;\r\n white-space: nowrap;\r\n border-width: 1px;\r\n border-style: solid;\r\n \"\r\n >\r\n {COST_WATER_UNPAID}\r\n </td>\r\n </tr>";
                case 81:  // ct1-a10
                    return "<tr style=\"font-style: italic\"> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: center; font-style: italic; \" > {INDEX} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: left; \" > Tiền nước HS{INDEX}: </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {WATER_CONSUMPTION_QUANTITY} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" ></td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {UNIT_PRICE_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" ></td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {COST_WATER_UNPAID} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" ></td> </tr>";
                case 94:  // vinasinco 
                    return "<tr> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_HEAD_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_END_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {TOTAL_INDEX_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {WATER_UNIT_RANGE} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {UNIT_PRICE_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {COST_WATER_UNPAID} </td> </tr>";
                case 114: //lathanh
                    return "<tr> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_HEAD_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {INDEX_END_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {TOTAL_INDEX_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {WATER_UNIT_RANGE} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {UNIT_PRICE_WATER} </td> <td style=\" border-width: 1px; border-style: solid; padding: 6px; text-align: right; \" > {COST_WATER_UNPAID} </td> </tr>";
                default:
                    return "";
            }
        }
        private string GetListVehicle(int? tenantId)
        {
            switch (tenantId)
            {
                case 80:  // Thanh Bình 11NO
                    return "<tr>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{INDEX}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_TYPE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">{VEHICLE_CODE}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{COST_PARKING_ELEMENT}</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: center;\">1</td>\r\n              <td style=\"border-width: 1px; border-style: solid; padding: 6px; text-align: right;\">{COST_PARKING_ELEMENT}</td>\r\n          </tr>";
                case 63:  // Vina 2.2
                    return "<tr>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{INDEX}</td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{APARTMENT_CODE}</td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{VEHICLE_TYPE}</td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{VEHICLE_CODE}</td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_PARKING_ELEMENT}</td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 10px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">\r\n                  <span style=\"color: #ef4444\">1,0</span>\r\n              </td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_PARKING_ELEMENT}</td>\r\n              <td style=\"padding: 10px; text-align: right; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_CARD_VEHICLE_ELEMENT}</td>\r\n          </tr>";
                case 62:  // hudlands
                    return "<tr>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n          <td style=\"white-space: nowrap; border-width: 1px; border-style: solid;\">\r\n            <span style=\"display: inline-block; width: 50%; padding: 6px 6px 6px 8px\">- {VEHICLE_TYPE}</span><span style=\"display: inline-block; width: 50%; border-width: 0px 0px 0px 1px; border-style: solid; padding: 6px 6px 6px 8px\">BKS: {VEHICLE_CODE}</span>\r\n          </td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">1</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_PARKING_ELEMENT}</td>\r\n          <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_PARKING_ELEMENT}</td>\r\n        </tr>";
                default:
                    return "";
            }
        }
        private string GetListOtherBill(int? tenantId)
        {
            switch (tenantId)
            {
                case 62:  // hudlands
                    return "<tr>\r\n      <td\r\n        style=\"\r\n          padding: 6px;\r\n          text-align: center;\r\n          white-space: nowrap;\r\n          border-width: 1px;\r\n          border-style: solid;\r\n        \"\r\n      ></td>\r\n      <td\r\n        style=\"\r\n          padding: 6px 6px 6px 8px;\r\n          white-space: nowrap;\r\n          border-width: 1px;\r\n          border-style: solid;\r\n        \"\r\n      >\r\n        - {NAME_OTHER_BILL} tháng {MONTH_PERIOD}/{YEAR_PERIOD}\r\n      </td>\r\n      <td\r\n        style=\"\r\n          padding: 6px;\r\n          text-align: center;\r\n          white-space: nowrap;\r\n          border-width: 1px;\r\n          border-style: solid;\r\n        \"\r\n      >\r\n        {TOTAL_INDEX_OTHER_BILL}\r\n      </td>\r\n      <td\r\n        style=\"\r\n          padding: 6px;\r\n          text-align: center;\r\n          white-space: nowrap;\r\n          border-width: 1px;\r\n          border-style: solid;\r\n        \"\r\n      >\r\n        {UNIT_PRICE_OTHER_BILL}\r\n      </td>\r\n      <td\r\n        style=\"\r\n          padding: 6px;\r\n          text-align: center;\r\n          white-space: nowrap;\r\n          border-width: 1px;\r\n          border-style: solid;\r\n        \"\r\n      >\r\n        {COST_OTHER_BILL_UNPAID}\r\n      </td>\r\n    </tr>";
                default:
                    return "";
            }
        }
        private string GetListElectricConsumption(int? tenantId)
        {
            switch (tenantId)
            {
                case 47:  // dev
                    return "<tr>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\"></td>\r\n              <td style=\"padding: 6px 6px 6px 8px; white-space: nowrap; border-width: 1px; border-style: solid;\">{ELECTRIC_CONSUMPTION_RANGE}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{ELECTRIC_CONSUMPTION_QUANTITY}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{UNIT_PRICE_ELECTRIC}</td>\r\n              <td style=\"padding: 6px; text-align: center; white-space: nowrap; border-width: 1px; border-style: solid;\">{COST_ELECTRIC_UNPAID}</td>\r\n          </tr>";
                default:
                    return "";
            }
        }
        private string GetTaxCode(CitizenTemp? citizenTemp)
        {
            return citizenTemp?.TaxCode ?? "";
        }
        private async Task<int> GetDayWater(UserBill userBill, DateTime dateTime)
        {
            int dueDateWater = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.WaterEndPeriodDay, AbpSession.TenantId.Value);
            return ValidateAndSetDueDate(dueDateWater, userBill, dateTime);
        }
        private async Task<int> GetDayElectric(UserBill userBill, DateTime dateTime)
        {
            int dueDateElectric = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricEndPeriodDay, AbpSession.TenantId.Value);
            return ValidateAndSetDueDate(dueDateElectric, userBill, dateTime);
        }

        private string GetBuildingName(long? buildingId)
        {
            if (buildingId == null && buildingId == 0) return "";
            var building = _appOrganizationUnitRepository.FirstOrDefault(x => x.Id == buildingId);
            if (building != null) return building.DisplayName;
            return "";
        }
        private async Task<object> GetNameVehicleByApartment(string? apartmentCode)
        {
            //if (apartmentCode == null) return "";
            var listNameVehicle = _citizenVehicleRepository.GetAll().Where(x => x.ApartmentCode == apartmentCode && x.State == CitizenVehicleState.ACCEPTED).GroupBy(x => x.VehicleType);
            return listNameVehicle;

        }
        #endregion
    }
}
