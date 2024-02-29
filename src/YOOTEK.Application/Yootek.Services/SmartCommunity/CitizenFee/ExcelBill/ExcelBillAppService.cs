using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Core.Dto;
using Yootek.EntityDb;
using Yootek.MultiTenancy;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services.SmartCommunity.ExcelBill
{
    public interface IExcelBillAppService : IApplicationService
    {
        Task<object> UploadBillsExcel([FromForm] ImportExcelBillInput input);
        Task<object> ExportBillsExcel(DownloadBillsExcelInputDto input);
        Task<object> ExportAllBillsExcel(DownloadAllBillsExcelInputDto input);
        // DataResult ExportStatisticBillExcel(ExportStatisticBillInput input);
        Task<DataResult> ExportStatisticBillExcel(BillQueryInput input);
    }
    public class ExcelBillAppService : YootekAppServiceBase, IExcelBillAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepository;
        private readonly IRepository<BillDebt, long> _billDebtRepository;
        private readonly IBillExcelExporter _billExcelExporter;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<BillConfig, long> _billConfigRepository;
        private readonly IRepository<Apartment, long> _smartHomeRepository;
        private readonly IStatisticBillAppService _statisticBillAppService;

        public ExcelBillAppService(
            IRepository<UserBill, long> userBillRepository,
            IRepository<BillConfig, long> billConfigRepository,
            IRepository<UserBillPayment, long> userBillPaymentRepository,
            IRepository<CitizenTemp, long> citizenTempRepository,
            IRepository<BillDebt, long> billDebtRepository,
            IBillExcelExporter billExcelExporter,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<Apartment, long> smartHomeRepository,
            IStatisticBillAppService statisticBillAppService,
            IRepository<Tenant, int> tenantRepository
            )
        {
            _userBillRepository = userBillRepository;
            _billConfigRepository = billConfigRepository;
            _userBillPaymentRepository = userBillPaymentRepository;
            _smartHomeRepository = smartHomeRepository;
            _billExcelExporter = billExcelExporter;
            _billDebtRepository = billDebtRepository;
            _citizenTempRepository = citizenTempRepository;
            _organizationUnitRepository = organizationUnitRepository;
            _statisticBillAppService = statisticBillAppService;
            _tenantRepository = tenantRepository;
        }

        public async Task<object> UploadBillsExcel([FromForm] ImportExcelBillInput input)
        {
            var file = input.Form;

            var fileName = file.FileName;
            var fileExt = Path.GetExtension(fileName);
            if (fileExt != ".xlsx" && fileExt != ".xls")
            {
                return DataResult.ResultError("File not support", "Error");
            }

            var filePath = Path.GetRandomFileName() + fileExt;
            var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            if (input.Formulas == null || input.Formulas.Count() == 0)
            {
                input.Formulas = _billConfigRepository.GetAllList(x => x.BillType == input.Type && x.IsDefault == true);
                if (input.Formulas == null) input.Formulas = new List<BillConfig>();
            }

            try
            {
                var package = new ExcelPackage(stream);

                var listUserBill = new List<UserBill>();
                if (input.Type == BillType.Parking)
                {
                    listUserBill = ExtractExcelUserBillParking(package, input.Formulas);
                }
                else
                {
                    try
                    {
                        //var id = _appConfiguration["CustomTenant:HudlandTenantId"];
                        //var id2 = _appConfiguration["CustomTenant:TXComplexTenantId"];
                        //if (AbpSession.TenantId == 62 || AbpSession.TenantId == 59)
                        //{
                        //    // var billConfigs = _billConfigRepo.GetAllList(x => x.BillType == input.Type);

                        //}
                        //else
                        //{
                        //    listUserBill = ExtractExcelUserBillEWM(package, input.Type, input.Formulas);
                        //}
                        listUserBill = ExtractExcelOnlyLastIndexUserBillEW(package, input.Type, input.Formulas);
                    }
                    catch
                    {
                        listUserBill = ExtractExcelUserBillEWM(package, input.Type, input.Formulas);
                    }
                }

                await CreateListUserBillAsync(listUserBill);

                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);

                return DataResult.ResultSuccess(null, "Upload success");
            }
            catch (Exception ex)
            {
                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private List<UserBill> ExtractExcelUserBillEWM(ExcelPackage package, BillType billType,
            List<BillConfig> formulas)
        {
            var worksheet = package.Workbook.Worksheets.First();

            var rowCount = worksheet.Dimension.End.Row;
            var colCount = worksheet.Dimension.End.Column;

            const int TITLE_INDEX = 1;
            const int APARTMENT_CODE_INDEX = 2;
            const int CUSTOMER_NAME_INDEX = 3;
            const int PERIOD_INDEX = 4;
            const int DUE_DATE_INDEX = 5;
            const int LAST_COST_INDEX = 6;
            const int TOTAL_INDEX = 7;
            const int HEAD_INDEX = 8;
            const int LAST_INDEX = 9;
            const int BUILDING_INDEX = 10;
            const int URBAN_INDEX = 11;
            /// Hudland

            var listUserBill = new List<UserBill>();
            var countApartmentNull = 0;
            for (var row = 2; row <= rowCount; row++)
            {
                var userBill = new UserBill();


                var apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString().Trim();
                if (string.IsNullOrEmpty(apartmentCode) || string.IsNullOrWhiteSpace(apartmentCode))
                {
                    countApartmentNull++;
                    if (countApartmentNull == 10) break;
                    continue;
                }

                var customer = Convert.ToString(worksheet.Cells[row, CUSTOMER_NAME_INDEX].Value);

                var citizens = _citizenTempRepository.GetAll().Select(x => new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    ApartmentCode = x.ApartmentCode,
                    IsStayed = x.IsStayed,
                    RelationShip = x.RelationShip
                }).Where(x => x.ApartmentCode == apartmentCode).ToList();

                var properties = new BillProperites()
                {
                    customerName = customer,
                    formulas = formulas.Select(x => x.Id).ToArray(),
                    formulaDetails = formulas.ToArray()
                };

                if (citizens.Any())
                {
                    var citizenDefault =
                        citizens.FirstOrDefault(x => x.IsStayed && x.RelationShip == RELATIONSHIP.Contractor);
                    if (citizenDefault == null)
                        citizenDefault = citizens.FirstOrDefault(x => x.RelationShip == RELATIONSHIP.Contractor);
                    if (citizenDefault == null) citizenDefault = citizens.FirstOrDefault();

                    if (customer != null)
                    {
                        var check = citizens.Where(x => x.FullName.Trim().ToLower() == customer.Trim().ToLower())
                            .FirstOrDefault();
                        if (check == null) check = citizenDefault;
                        properties.customerName = check.FullName;
                        userBill.CitizenTempId = check.Id;
                    }
                    else
                    {
                        userBill.CitizenTempId = citizenDefault.Id;
                        properties.customerName = citizenDefault.FullName;
                    }
                }

                userBill.Properties = JsonConvert.SerializeObject(properties);


                userBill.TenantId = AbpSession.TenantId;
                userBill.Title = worksheet.Cells[row, TITLE_INDEX].Value.ToString();
                userBill.BillType = billType;
                userBill.ApartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString();
                userBill.LastCost = double.Parse(worksheet.Cells[row, LAST_COST_INDEX].Value.ToString());
                userBill.Status = UserBillStatus.Pending;
                userBill.Period = DateTime.Parse(worksheet.Cells[row, PERIOD_INDEX].Value.ToString());
                userBill.DueDate = DateTime.Parse(worksheet.Cells[row, DUE_DATE_INDEX].Value.ToString());

                string buildingCode = null;
                string urbanCode = null;

                if (billType.Equals(BillType.Electric) || billType.Equals(BillType.Water))
                {
                    userBill.IndexHeadPeriod = decimal.Parse(worksheet.Cells[row, HEAD_INDEX].Value.ToString());
                    userBill.IndexEndPeriod = decimal.Parse(worksheet.Cells[row, LAST_INDEX].Value.ToString());
                    userBill.TotalIndex = decimal.Parse(worksheet.Cells[row, TOTAL_INDEX].Value.ToString());

                    buildingCode = worksheet.Cells[row, BUILDING_INDEX].Text.ToString() != ""
                        ? worksheet.Cells[row, BUILDING_INDEX].Value.ToString().Trim()
                        : null;
                    urbanCode = worksheet.Cells[row, URBAN_INDEX].Text.ToString() != ""
                        ? worksheet.Cells[row, URBAN_INDEX].Value.ToString().Trim()
                        : null;
                }

                if (billType.Equals(BillType.Manager))
                {
                    userBill.TotalIndex = decimal.Parse(worksheet.Cells[row, TOTAL_INDEX].Value.ToString());
                    buildingCode = worksheet.Cells[row, HEAD_INDEX].Text.ToString() != ""
                        ? worksheet.Cells[row, HEAD_INDEX].Value.ToString().Trim()
                        : null;
                    urbanCode = worksheet.Cells[row, LAST_INDEX].Text.ToString() != ""
                        ? worksheet.Cells[row, LAST_INDEX].Value.ToString().Trim()
                        : null;
                }

                if (buildingCode != null)
                {
                    var building = _organizationUnitRepository.FirstOrDefault(x =>
                        x.ProjectCode == buildingCode && x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                    if (building != null) userBill.BuildingId = building.ParentId;
                }

                if (urbanCode != null)
                {
                    var urban = _organizationUnitRepository.FirstOrDefault(x =>
                        x.ProjectCode == urbanCode && x.Type == APP_ORGANIZATION_TYPE.URBAN);
                    if (urban != null) userBill.UrbanId = urban.ParentId;
                }

                listUserBill.Add(userBill);
            }

            return listUserBill;
        }

        private List<UserBill> ExtractExcelOnlyLastIndexUserBillEW(ExcelPackage package, BillType billType,
            List<BillConfig> formulas)
        {
            var levels = new List<BillPriceDto>();
            var percents = new List<decimal>();
            var priceM = new BillPriceDto();
            var worksheet = package.Workbook.Worksheets.First();

            var rowCount = worksheet.Dimension.End.Row;
            var colCount = worksheet.Dimension.End.Column;

            const int TITLE_INDEX = 1;
            const int APARTMENT_CODE_INDEX = 2;
            const int CUSTOMER_NAME_INDEX = 3;
            const int PERIOD_INDEX = 4;
            const int DUE_DATE_INDEX = 5;
            const int LAST_INDEX = 6;
            const int BUILDING_INDEX = 7;
            const int URBAN_INDEX = 8;


            var listUserBill = new List<UserBill>();
            var countApartmentNull = 0;

            var checkBuildingCode = "";
            var checkUrbanCode = "";

            for (var row = 2; row <= rowCount; row++)
            {
                var userBill = new UserBill();

                var apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString().Trim();
                if (string.IsNullOrEmpty(apartmentCode) || string.IsNullOrWhiteSpace(apartmentCode))
                {
                    countApartmentNull++;
                    if (countApartmentNull == 10) break;
                    continue;
                }

                var customer = Convert.ToString(worksheet.Cells[row, CUSTOMER_NAME_INDEX].Value);

                var citizens = _citizenTempRepository.GetAll().Select(x => new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    ApartmentCode = x.ApartmentCode,
                    IsStayed = x.IsStayed,
                    RelationShip = x.RelationShip
                }).Where(x => x.ApartmentCode == apartmentCode).ToList();

                var properties = new BillProperites()
                {
                    customerName = customer,
                    formulas = formulas.Select(x => x.Id).ToArray(),
                    formulaDetails = formulas.ToArray()
                };

                if (citizens.Any())
                {
                    var citizenDefault =
                        citizens.FirstOrDefault(x => x.IsStayed && x.RelationShip == RELATIONSHIP.Contractor);
                    if (citizenDefault == null)
                        citizenDefault = citizens.FirstOrDefault(x => x.RelationShip == RELATIONSHIP.Contractor);
                    if (citizenDefault == null) citizenDefault = citizens.FirstOrDefault();

                    if (customer != null)
                    {
                        var check = citizens.Where(x => x.FullName.Trim().ToLower() == customer.Trim().ToLower())
                            .FirstOrDefault();
                        if (check == null) check = citizenDefault;
                        properties.customerName = check.FullName;
                        userBill.CitizenTempId = check.Id;
                    }
                    else
                    {
                        userBill.CitizenTempId = citizenDefault.Id;
                        properties.customerName = citizenDefault.FullName;
                    }
                }

                userBill.Properties = JsonConvert.SerializeObject(properties);


                userBill.TenantId = AbpSession.TenantId;
                userBill.Title = worksheet.Cells[row, TITLE_INDEX].Value.ToString();
                userBill.BillType = billType;
                userBill.ApartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString();
                userBill.Status = UserBillStatus.Pending;
                userBill.Period = DateTime.Parse(worksheet.Cells[row, PERIOD_INDEX].Value.ToString());
                userBill.DueDate = DateTime.Parse(worksheet.Cells[row, DUE_DATE_INDEX].Value.ToString());

                if (userBill.Period != null)
                {
                    var pre_period = userBill.Period.Value.AddMonths(-1);
                    var pre_bill = _userBillRepository.FirstOrDefault(x =>
                        x.ApartmentCode == apartmentCode && x.Period.Value.Year == pre_period.Year &&
                        x.Period.Value.Month == pre_period.Month);
                    if (pre_bill != null) userBill.IndexHeadPeriod = pre_bill.IndexEndPeriod;
                    else userBill.IndexHeadPeriod = 0;
                }

                var buildingCode = worksheet.Cells[row, BUILDING_INDEX].Text.ToString() != ""
                    ? worksheet.Cells[row, BUILDING_INDEX].Value.ToString().Trim()
                    : null;
                var urbanCode = worksheet.Cells[row, URBAN_INDEX].Text.ToString() != ""
                    ? worksheet.Cells[row, URBAN_INDEX].Value.ToString().Trim()
                    : null;
                if (buildingCode != null)
                {
                    var building = _organizationUnitRepository.FirstOrDefault(x =>
                        x.ProjectCode == buildingCode && x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                    if (building != null)
                    {
                        userBill.BuildingId = building.ParentId;
                    }
                }

                if (urbanCode != null)
                {
                    var urban = _organizationUnitRepository.FirstOrDefault(x =>
                        x.ProjectCode == urbanCode && x.Type == APP_ORGANIZATION_TYPE.URBAN);
                    if (urban != null)
                    {
                        userBill.UrbanId = urban.ParentId;
                    }
                }

                if (buildingCode != checkBuildingCode || urbanCode != checkUrbanCode)
                {
                    checkBuildingCode = buildingCode;
                    checkUrbanCode = urbanCode;
                    var formulaBuildings =
                        CheckPriceConfigByBuilding(formulas, userBill.BuildingId, userBill.UrbanId);
                    levels = formulaBuildings.Item1;
                    percents = formulaBuildings.Item2;
                    priceM = formulaBuildings.Item3;
                }


                if (billType.Equals(BillType.Electric) || billType.Equals(BillType.Water))
                {
                    userBill.IndexEndPeriod = decimal.Parse(worksheet.Cells[row, LAST_INDEX].Value.ToString());

                    userBill.TotalIndex = userBill.IndexEndPeriod - userBill.IndexHeadPeriod;
                    userBill.LastCost =
                        CalculateDependOnLevel(levels.ToArray(), percents, (decimal)userBill.TotalIndex);
                }

                if (billType.Equals(BillType.Manager))
                {
                    userBill.TotalIndex = decimal.Parse(worksheet.Cells[row, LAST_INDEX].Value.ToString());

                    var apartment = _smartHomeRepository.FirstOrDefault(x => x.ApartmentCode == apartmentCode);
                    if (priceM != null && priceM.Value > 0)
                    {
                        if (apartment != null && apartment.Area > 0)
                        {
                            userBill.LastCost = (double)(priceM.Value * apartment.Area.Value);
                        }
                        else
                        {
                            userBill.LastCost = (double)(priceM.Value * userBill.TotalIndex);
                        }
                    }
                }

                listUserBill.Add(userBill);
            }

            return listUserBill;
        }
        private List<UserBill> ExtractExcelUserBillParking(ExcelPackage package, List<BillConfig> formulas)
        {
            var billType = BillType.Parking;
            var worksheet = package.Workbook.Worksheets.First();

            var rowCount = worksheet.Dimension.End.Row;
            var colCount = worksheet.Dimension.End.Column;

            const int TITLE_INDEX = 1;
            const int APARTMENT_CODE_INDEX = 2;
            const int CUSTOMER_NAME_INDEX = 3;
            const int PERIOD_INDEX = 4;
            const int DUE_DATE_INDEX = 5;

            const int CAR_NUMBER_INDEX = 6;
            const int MOTORBIKE_NUMBER_INDEX = 7;
            const int BICYCLE_NUMBER_INDEX = 8;
            const int OTHER_NUMBER_INDEX = 9;
            const int LAST_COST_INDEX = 10;
            const int BUILDING_INDEX = 11;
            const int URBAN_INDEX = 12;

            var listUserBill = new List<UserBill>();

            var countApartmentNull = 0;

            for (var row = 2; row <= rowCount; row++)
            {
                var userBill = new UserBill();


                var apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Text.ToString() != ""
                    ? worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString().Trim()
                    : null;
                if (string.IsNullOrEmpty(apartmentCode) || string.IsNullOrWhiteSpace(apartmentCode))
                {
                    countApartmentNull++;
                    if (countApartmentNull == 10) break;
                    continue;
                }

                var customer = Convert.ToString(worksheet.Cells[row, CUSTOMER_NAME_INDEX].Value);

                var citizens = _citizenTempRepository.GetAll().Select(x => new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    ApartmentCode = x.ApartmentCode,
                    IsStayed = x.IsStayed,
                    RelationShip = x.RelationShip
                }).Where(x => x.ApartmentCode == apartmentCode).ToList();

                var properties = new BillProperites()
                {
                    customerName = customer,
                    formulas = formulas.Select(x => x.Id).ToArray(),
                    formulaDetails = formulas.ToArray()
                };

                if (citizens.Any())
                {
                    var citizenDefault =
                        citizens.FirstOrDefault(x => x.IsStayed && x.RelationShip == RELATIONSHIP.Contractor);
                    if (citizenDefault == null)
                        citizenDefault = citizens.FirstOrDefault(x => x.RelationShip == RELATIONSHIP.Contractor);
                    if (citizenDefault == null) citizenDefault = citizens.FirstOrDefault();

                    if (customer != null)
                    {
                        var check = citizens.Where(x => x.FullName.Trim().ToLower() == customer.Trim().ToLower())
                            .FirstOrDefault();
                        if (check == null) check = citizenDefault;
                        properties.customerName = check.FullName;
                        userBill.CitizenTempId = check.Id;
                    }
                    else
                    {
                        userBill.CitizenTempId = citizenDefault.Id;
                        properties.customerName = citizenDefault.FullName;
                    }
                }

                userBill.Properties = JsonConvert.SerializeObject(properties);

                userBill.TenantId = AbpSession.TenantId;
                userBill.Title = worksheet.Cells[row, TITLE_INDEX].Value.ToString();
                userBill.BillType = billType;
                userBill.ApartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString();
                userBill.LastCost = double.Parse(worksheet.Cells[row, LAST_COST_INDEX].Value.ToString());
                userBill.Status = UserBillStatus.Pending;
                userBill.Period = DateTime.Parse(worksheet.Cells[row, PERIOD_INDEX].Value.ToString());
                userBill.DueDate = DateTime.Parse(worksheet.Cells[row, DUE_DATE_INDEX].Value.ToString());

                userBill.CarNumber = worksheet.Cells[row, CAR_NUMBER_INDEX].Text.ToString() != ""
                    ? int.Parse(worksheet.Cells[row, CAR_NUMBER_INDEX].Value.ToString())
                    : 0;
                userBill.MotorbikeNumber = worksheet.Cells[row, MOTORBIKE_NUMBER_INDEX].Text.ToString() != ""
                    ? int.Parse(worksheet.Cells[row, MOTORBIKE_NUMBER_INDEX].Value.ToString())
                    : 0;
                userBill.BicycleNumber = worksheet.Cells[row, BICYCLE_NUMBER_INDEX].Text.ToString() != ""
                    ? int.Parse(worksheet.Cells[row, BICYCLE_NUMBER_INDEX].Value.ToString())
                    : 0;
                userBill.OtherVehicleNumber = worksheet.Cells[row, OTHER_NUMBER_INDEX].Text.ToString() != ""
                    ? int.Parse(worksheet.Cells[row, OTHER_NUMBER_INDEX].Value.ToString())
                    : 0;

                var buildingCode = worksheet.Cells[row, BUILDING_INDEX].Text.ToString() != ""
                    ? worksheet.Cells[row, BUILDING_INDEX].Value.ToString().Trim()
                    : null;
                var urbanCode = worksheet.Cells[row, URBAN_INDEX].Text.ToString() != ""
                    ? worksheet.Cells[row, URBAN_INDEX].Value.ToString().Trim()
                    : null;
                if (buildingCode != null)
                {
                    var building = _organizationUnitRepository.FirstOrDefault(x =>
                        x.ProjectCode == buildingCode && x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                    if (building != null) userBill.BuildingId = building.ParentId;
                }

                if (urbanCode != null)
                {
                    var urban = _organizationUnitRepository.FirstOrDefault(x =>
                        x.ProjectCode == urbanCode && x.Type == APP_ORGANIZATION_TYPE.URBAN);
                    if (urban != null) userBill.UrbanId = urban.ParentId;
                }

                listUserBill.Add(userBill);
            }

            return listUserBill;
        }
        private async Task CreateListUserBillAsync(List<UserBill> userBills)
        {
            try
            {
                if (userBills == null || userBills.Count() == 0) return;

                foreach (UserBill userBill in userBills)
                {
                    var id = await _userBillRepository.InsertAndGetIdAsync(userBill);
                    userBill.Code = "HD" + userBill.Id +
                                    (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month) + "" +
                                    DateTime.Now.Year;
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                //if (userBills.Count() < 1000)
                //{


                //}
                //else
                //{
                //    await _backgroundJobManager
                //          .EnqueueAsync<UserBillBackGroundJobs, List<UserBill>>(userBills);
                //}
            }
            catch
            {
            }
        }
        public async Task<object> ExportBillsExcel(DownloadBillsExcelInputDto input)
        {
            try
            {
                var bills = await this._userBillRepository.GetAll()
                    .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id))
                    .ToListAsync();

                var result = _billExcelExporter.ExportToFile(bills);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> ExportAllBillsExcel(DownloadAllBillsExcelInputDto input)
        {
            try
            {
                var bills = await this._userBillRepository.GetAll()
                    .Where(x => x.BillType == input.BillType)
                    .OrderByDescending(x => x.Period)
                    .ToListAsync();

                var result = _billExcelExporter.ExportToFile(bills);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        private double CalculateDependOnLevel(BillPriceDto[] levels, List<decimal> percents, decimal amount)
        {
            string[] keys = { "start" };
            Array.Sort(keys, levels);

            //var levelIndex = 0;
            //for (var i = 0; i < levels.Count(); i++)
            //{
            //    if (amount >= levels[i].From)
            //    {
            //        levelIndex = i;
            //    }
            //}

            //var result = 0.0;
            //for (var i = 0; i <= levelIndex; i++)
            //{
            //    var level = levels[i];

            //    if (amount < level.To)
            //    {
            //        result += (double)(level.Value * (amount - level.From));
            //        break;
            //    }

            //    if (!level.To.HasValue)
            //    {
            //        result += (double)(level.Value * (amount - level.From));
            //        break;
            //    }

            //    result += (double)(level.Value * (level.To - level.From));
            //}
            decimal result = 0;
            if (levels != null && levels.Count() > 0)
            {
                var levelIndex = 0;
                for (var i = 0; i < levels.Count(); i++)
                {
                    if (amount >= levels[i].From)
                    {
                        levelIndex = i;
                    }
                }

                for (var i = 0; i <= levelIndex; i++)
                {
                    var level = levels[i];
                    if (i != 0) level.From = level.From - 1;
                    if (amount < level.To)
                    {
                        result += level.Value * (amount - level.From);
                        break;
                    }

                    if (i == levelIndex)
                    {
                        result += level.Value * (amount - level.From);
                        break;
                    }

                    result += level.Value * (level.To.Value - level.From);
                }
            }

            if (percents != null && percents.Count() > 0)
            {
                decimal pctotal = 0;
                foreach (var pc in percents)
                {
                    pctotal = pctotal + result * (pc / 100);
                }

                result = pctotal + result;
            }

            return (double)(int)result;
        }
        private Tuple<List<BillPriceDto>, List<decimal>, BillPriceDto> CheckPriceConfigByBuilding(
            List<BillConfig> formulas, long? buildingId, long? urbanId = null)
        {
            var levels = new List<BillPriceDto>();
            var percents = new List<decimal>();
            var priceM = new BillPriceDto();
            // Công thức hóa đơn điện nước

            var config = formulas.Where(x =>
                    x.PricesType == BillConfigPricesType.Level && x.BuildingId == buildingId && x.IsPrivate == true)
                .FirstOrDefault();

            if (config == null)
                config = formulas.Where(x =>
                        x.PricesType == BillConfigPricesType.Level && x.UrbanId == urbanId && x.IsPrivate == true)
                    .FirstOrDefault();

            if (config == null)
                config = formulas.Where(x => x.PricesType == BillConfigPricesType.Level && x.IsPrivate != true)
                    .FirstOrDefault();
            if (config != null)
            {
                try
                {
                    var cfg = JsonConvert.DeserializeObject<BillPropertiesDto>(config.Properties);
                    levels = cfg.Prices.ToList();
                }
                catch
                {
                }
            }


            // Giá phần trăm (VAT, BVMT...)
            var configPercents = formulas
                .Where(x => x.PricesType == BillConfigPricesType.Percentage && x.BuildingId == buildingId &&
                            x.IsPrivate == true).Select(x => x.Properties).ToList();

            if (configPercents == null || configPercents.Count() == 0)
                configPercents = formulas
                    .Where(x => x.PricesType == BillConfigPricesType.Percentage && x.UrbanId == urbanId &&
                                x.IsPrivate == true).Select(x => x.Properties).ToList();

            if (configPercents == null || configPercents.Count() == 0)
                configPercents = formulas
                    .Where(x => x.PricesType == BillConfigPricesType.Percentage && x.IsPrivate != true)
                    .Select(x => x.Properties).ToList();
            if (configPercents != null && configPercents.Count() > 0)
            {
                try
                {
                    foreach (var pro in configPercents)
                    {
                        var cfg = JsonConvert.DeserializeObject<BillPropertiesDto>(pro);
                        percents.Add(cfg.Prices[0].Value);
                    }
                }
                catch
                {
                }
            }


            // Công thức phí quản lý theo m2 căn hộ

            var configM = formulas.Where(x => x.PricesType == BillConfigPricesType.Rapport && x.BuildingId == buildingId)
                .FirstOrDefault();

            if (configM == null)
                configM = formulas.Where(x =>
                        x.PricesType == BillConfigPricesType.Rapport && x.IsPrivate == true && x.UrbanId == urbanId)
                    .FirstOrDefault();

            if (configM == null)
                configM = formulas.Where(x => x.PricesType == BillConfigPricesType.Rapport && x.IsPrivate != true)
                    .FirstOrDefault();

            if (configM != null)
            {
                try
                {
                    var cfg = JsonConvert.DeserializeObject<BillPropertiesDto>(configM.Properties);
                    priceM = cfg.Prices[0];
                }
                catch
                {
                }
            }

            return new Tuple<List<BillPriceDto>, List<decimal>, BillPriceDto>(levels, percents, priceM);
        }

        /*public DataResult ExportStatisticBillExcel(ExportStatisticBillInput input)
        {
            try
            {
                List<DataStatisticUserBill> result = new();

                if (input.FormIdScope == FormIdStatisticScope.BUILDING)
                {
                    result.AddRange(QueryStatisticBillForScope(FormIdStatisticScope.BUILDING, input.BuildingId, input.UrbanId, input.Year));
                }
                else if (input.FormIdScope == FormIdStatisticScope.URBAN)
                {
                    result.AddRange(QueryStatisticBillForScope(FormIdStatisticScope.URBAN, input.BuildingId, input.UrbanId, input.Year));

                    List<AppOrganizationUnit> listBuildings = GetListBuildings((long)input.UrbanId);
                    foreach (AppOrganizationUnit building in listBuildings)
                    {
                        result.AddRange(QueryStatisticBillForScope(FormIdStatisticScope.BUILDING, building.Id, input.UrbanId, input.Year));
                    }
                }
                else if (input.FormIdScope == FormIdStatisticScope.TENANT)
                {
                    result.AddRange(QueryStatisticBillForScope(FormIdStatisticScope.TENANT, null, null, input.Year));

                    List<AppOrganizationUnit> listUrbans = GetListUrbans((int)AbpSession.TenantId);
                    foreach (AppOrganizationUnit urban in listUrbans)
                    {
                        result.AddRange(QueryStatisticBillForScope(FormIdStatisticScope.URBAN, null, urban.Id, input.Year));
                    }
                }
                else
                {
                    return DataResult.ResultSuccess("Scope invalid");
                }

                FileDto file = _billExcelExporter.ExportStatisticBill(result);
                return DataResult.ResultSuccess(file, "Export statistic bill success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }*/

        public async Task<DataResult> ExportStatisticBillExcel(BillQueryInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    List<DataStatisticScope> result = new();
                    if (input.UrbanId == null && input.BuildingId == null) // tenant scope 
                    {
                        result.Add(new()
                        {
                            Data = GetDataStatisticScope(input.UrbanId, input.BuildingId),
                            ScopeName = GetTenantName(AbpSession.TenantId),
                        }

                        );
                        List<DataStatisticScope> dataUrbans = new();
                        List<AppOrganizationUnit> listUrbans = GetListUrbans((int)AbpSession.TenantId);
                        result.AddRange(listUrbans
                            .Where(urban => buIds.Contains(urban.Id))
                            .Select(urban => new DataStatisticScope
                            {
                                Data = GetDataStatisticScope(urban.Id, input.BuildingId),
                                ScopeName = urban.DisplayName,
                            }));
                        //foreach (AppOrganizationUnit urban in listUrbans)
                        //{
                        //    dataUrbans.Add(new DataStatisticScope()
                        //    {
                        //        Data = GetDataStatisticScope(urban.Id, input.BuildingId),
                        //        ScopeName = urban.DisplayName,
                        //    });
                        //}
                        //result.AddRange(dataUrbans);
                    }
                    else if (input.UrbanId != null && input.BuildingId == null)
                    {
                        result.Add(new DataStatisticScope()
                        {
                            Data = GetDataStatisticScope(input.UrbanId, input.BuildingId),
                            ScopeName = GetUrbanName(input.UrbanId),
                        });
                        List<DataStatisticScope> dataBuildings = new();
                        List<AppOrganizationUnit> listBuildings = GetListBuildings((long)input.UrbanId);
                        result.AddRange(listBuildings
                            .Where(building => buIds.Contains(building.Id))
                            .Select(building => new DataStatisticScope
                            {
                                Data = GetDataStatisticScope(input.UrbanId, building.Id),
                                ScopeName = building.DisplayName,
                            }));
                        //foreach (AppOrganizationUnit building in listBuildings)
                        //{
                        //    dataBuildings.Add(new DataStatisticScope()
                        //    {
                        //        Data = GetDataStatisticScope(input.UrbanId, building.Id),
                        //        ScopeName = building.DisplayName,
                        //    });
                        //}
                        //result.AddRange(dataBuildings);
                    }
                    else if (input.BuildingId != null)
                    {
                        result.Add(new DataStatisticScope()
                        {
                            Data = GetDataStatisticScope(input.UrbanId, input.BuildingId),
                            ScopeName = GetBuildingName(input.BuildingId),
                        });
                    }
                    else
                    {
                        return DataResult.ResultSuccess(null, "Scope is invalid");
                    }

                    FileDto file = _billExcelExporter.ExportStatisticBill(result);
                    return DataResult.ResultSuccess(file, "Export statistic bill success");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private Dictionary<string, DataStatisticBillTenantDto> GetDataStatisticScope(long? urbanId, long? buildingId)
        {
            DateTime now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            var result = new Dictionary<string, DataStatisticBillTenantDto>();

            if (currentMonth >= 6)
            {
                for (int i = currentMonth - 6 + 1; i <= currentMonth; i++)
                {
                    var input = new GetTotalStatisticUserBillInput()
                    {
                        BuildingId = buildingId,
                        UrbanId = urbanId,
                        Period = new DateTime(currentYear, i, 1)
                    }
                    ;
                    var dt = _statisticBillAppService.QueryBillMonthlyStatistics(input);
                    result.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), dt);
                }
            }
            else
            {

                for (var i = 7 + currentMonth; i <= 6; i++)
                {
                    var input = new GetTotalStatisticUserBillInput()
                    {
                        BuildingId = buildingId,
                        UrbanId = urbanId,
                        Period = new DateTime(currentYear - 1, i, 1)
                    };
                    var dt = _statisticBillAppService.QueryBillMonthlyStatistics(input);
                    result.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), dt);
                }

                for (var i = 1; i <= currentMonth; i++)
                {
                    var input = new GetTotalStatisticUserBillInput()
                    {
                        BuildingId = buildingId,
                        UrbanId = urbanId,
                        Period = new DateTime(currentYear, i, 1)
                    };
                    var dt = _statisticBillAppService.QueryBillMonthlyStatistics(input);
                    result.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), dt);
                }
            }

            return result;
        }

        #region method helpers
        /*List<DataStatisticUserBill> QueryStatisticBillForScope(FormIdStatisticScope scope, long? buildingId, long? urbanId, int year)
        {
            GetStatisticBillInput getStatisticBillInput = new()
            {
                BuildingId = buildingId,
                UrbanId = urbanId,
                DateFrom = new DateTime(year, 1, 1),
                DateTo = new DateTime(year, 12, 1),
                FormIdDateTime = FormIdStatisticDateTime.MONTH,
                FormIdScope = scope
            };
            return _statisticBillAppService.QueryStatisticBill(getStatisticBillInput);
        }*/
        private List<AppOrganizationUnit> GetListBuildings(long urbanId)
        {
            return (from org in _organizationUnitRepository.GetAll()
                    join orgParent in _organizationUnitRepository.GetAll()
                    on org.ParentId equals orgParent.Id
                    where orgParent.ParentId == urbanId && org.Type == APP_ORGANIZATION_TYPE.BUILDING
                    select new AppOrganizationUnit
                    {
                        Id = orgParent.Id,
                        DisplayName = orgParent.DisplayName,
                    }).ToList();
        }
        private List<AppOrganizationUnit> GetListUrbans(int tenantId)
        {
            return (from org in _organizationUnitRepository.GetAll()
                    where org.TenantId == tenantId && org.Type == APP_ORGANIZATION_TYPE.URBAN
                    select new AppOrganizationUnit
                    {
                        Id = (long)org.ParentId,
                        DisplayName = org.DisplayName,
                    }).ToList();
        }
        private string GetTenantName(int? tenantId)
        {
            return _tenantRepository.FirstOrDefault((int)tenantId)?.Name ?? "";
        }
        private string GetUrbanName(long? urbanId)
        {
            return _organizationUnitRepository.FirstOrDefault(urbanId ??= 0)?.DisplayName ?? "";
        }
        private string GetBuildingName(long? buildingId)
        {
            return _organizationUnitRepository.FirstOrDefault(buildingId ??= 0)?.DisplayName ?? "";
        }
        #endregion
    }
}
