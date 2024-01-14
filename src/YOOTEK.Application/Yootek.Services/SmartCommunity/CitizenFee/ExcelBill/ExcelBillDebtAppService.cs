using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yootek.Services.SmartCommunity.ExcelBill
{
    public interface IExcelBillDebtAppService : IApplicationService
    {
        Task<DataResult> ImportExcelBillDebt([FromForm] ImportExcelBillDebtInput input, CancellationToken cancellationToken);
    }
    public class ExcelBillDebtAppService : YootekAppServiceBase, IExcelBillDebtAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepository;
        private readonly IRepository<BillDebt, long> _billDebtRepository;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly IConfiguration _configuration;

        public ExcelBillDebtAppService(
            IRepository<UserBill, long> userBillRepository,
            IRepository<UserBillPayment, long> userBillPaymentRepository,
            IRepository<CitizenTemp, long> citizenTempRepository,
            IRepository<BillDebt, long> billDebtRepository,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IConfiguration configuration
            )
        {
            _userBillRepository = userBillRepository;
            _userBillPaymentRepository = userBillPaymentRepository;
            _billDebtRepository = billDebtRepository;
            _citizenTempRepository = citizenTempRepository;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _configuration = configuration;
        }
        public async Task<DataResult> ImportExcelBillDebt([FromForm] ImportExcelBillDebtInput input, CancellationToken cancellationToken)
        {
            try
            {
                IFormFile file = input.Form;
                string fileName = file.FileName;
                string fileExt = Path.GetExtension(fileName);
                if (!IsFileExtensionSupported(fileExt))
                {
                    return DataResult.ResultError("File not support", "Error");
                }
                string filePath = Path.GetRandomFileName() + fileExt;
                FileStream stream = File.Create(filePath);
                await file.CopyToAsync(stream, cancellationToken);
                ExcelPackage package = new(stream);

                Dictionary<int, Func<ExcelPackage, List<ExcelBillDebtDto>>> tenantExtractionMethods = new()
                {
                    { _configuration.GetValue<int>("CustomTenant:VCITowerTenantId"), ExtractExcelBillDebtVCI },
                    { _configuration.GetValue<int>("CustomTenant:NamCuongTenantId"), ExtractExcelBillDebtNC },
                };
                Func<ExcelPackage, List<ExcelBillDebtDto>> defaultExtractionMethod = ExtractExcelBillDebtGeneral;

                if (tenantExtractionMethods.TryGetValue((int)AbpSession.TenantId, out var extractionMethod))
                {
                    await ImportExcelToTenant(extractionMethod, package, filePath, stream);
                }
                else
                {
                    await ImportExcelToTenant(defaultExtractionMethod, package, filePath, stream);
                }

                return DataResult.ResultSuccess("Upload success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private async Task ImportExcelToTenant(Func<ExcelPackage, List<ExcelBillDebtDto>> extractExcelFunc,
            ExcelPackage package, string filePath, FileStream stream)
        {
            using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
            {
                try
                {
                    List<ExcelBillDebtDto> excelBillDebtDtos = extractExcelFunc(package);
                   
                    await stream.DisposeAsync();
                    stream.Close();
                    File.Delete(filePath);
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
        }
        private List<ExcelBillDebtDto> ExtractExcelBillDebtGeneral(ExcelPackage package)
        {
            List<ExcelBillDebtDto> excelBillDebtDtos = new();
            List<AppOrganizationUnit> buildings = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
            List<AppOrganizationUnit> urbans = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);
            ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
            int rowCount = worksheet.Dimension.End.Row;
            int colCount = worksheet.Dimension.End.Column;

            const int INDEX = 1;
            const int URBAN_INDEX = 2;
            const int BUILDING_INDEX = 3;
            const int APARTMENT_CODE_INDEX = 4;
            const int CUSTOMER_NAME_INDEX = 5;
            const int MONTH_PERIOD = 6;
            const int COST_MANAGER = 7;
            const int COST_MOTOBIKE = 8;
            const int COST_BIKE = 9;
            const int COST_CAR = 10;
            const int COST_OTHER_VEHICLE = 11;
            const int MOTOBIKE_NUMBER = 12;
            const int BIKE_NUMBER = 13;
            const int CAR_NUMBER = 14;
            const int OTHER_VEHICLE_NUMBER = 15;
            const int ACREAGE_APARTMENT = 16;
            const int COST_WATER = 17;
            const int COST_ELECTRIC = 18;
            const int INDEX_HEAD_WATER = 19;
            const int INDEX_END_WATER = 20;
            const int INDEX_HEAD_ELECTRIC = 21;
            const int INDEX_END_ELECTRIC = 22;
            const int DEBT_TOTAL = 23;
            const int AMOUNT_PAYMENT = 24;
            const int PAYMENT_METHOD = 25;
            const int LIST_BILL_CONFIG = 26;

            int countApartmentNull = 0;
            List<BillType> userBillTypes = new() { BillType.Electric, BillType.Water, BillType.Parking, BillType.Manager };
            for (int row = 2; row <= rowCount; row++)
            {
                DateTime period = GetCellValueNotDefault<DateTime>(worksheet, row, MONTH_PERIOD);
                string apartmentCode = GetCellValue<string>(worksheet, row, APARTMENT_CODE_INDEX);
                string customerName = GetCellValue<string>(worksheet, row, CUSTOMER_NAME_INDEX);
                string buildingCode = GetCellValue<string>(worksheet, row, BUILDING_INDEX);
                string urbanCode = GetCellValue<string>(worksheet, row, URBAN_INDEX);
                double debtTotal = GetCellValue<double>(worksheet, row, DEBT_TOTAL);
                double amountPayment = GetCellValue<double>(worksheet, row, AMOUNT_PAYMENT);
                int paymentMethod = GetCellValue<int>(worksheet, row, PAYMENT_METHOD);
                if (string.IsNullOrEmpty(apartmentCode) || string.IsNullOrWhiteSpace(apartmentCode))
                {
                    countApartmentNull++;
                    if (countApartmentNull >= 10) break;
                    continue;
                }
                ExcelBillDebtDto excelBillDebt = new();
                UserBill userBill = new();
                string titleUserBill = string.Empty;

                CitizenTemp citizens = _citizenTempRepository
                    .GetAll()
                    .Where(x => x.ApartmentCode == apartmentCode)
                    .OrderBy(x => x.IsStayed ? 0 : 1) // Sort by IsStayed, with stayed citizens first
                    .ThenBy(x => x.RelationShip == RELATIONSHIP.Contractor ? 0 : 1) // Then by Relationship, with Contractors first
                    .FirstOrDefault();
                if (citizens != null)
                {
                    customerName = string.IsNullOrWhiteSpace(customerName) ? citizens.FullName : customerName;
                    userBill.CitizenTempId = citizens.Id;
                }
                else
                {
                    customerName = string.IsNullOrWhiteSpace(customerName) ? string.Empty : customerName;
                }
                userBill.BuildingId = FindParentId(buildings, buildingCode);
                userBill.UrbanId = FindParentId(urbans, urbanCode);
                userBill.Properties = JsonConvert.SerializeObject(new BillProperty() { customerName = customerName });
                UserBillStatus status = debtTotal > 0 ? UserBillStatus.Debt : UserBillStatus.Paid;

                List<UserBill> listUserBillTemps = new();
                // insert UserBill
                for (int i = 0; i < userBillTypes.Count; i++)
                {
                    double lastCost = 0;
                    decimal indexHeadPeriod = 0;
                    decimal indexEndPeriod = 0;
                    int numberCar = 0;
                    int numberMotobike = 0;
                    int numberBike = 0;
                    int otherVehicle = 0;
                    decimal totalIndex = 0;
                    switch (userBillTypes[i])
                    {
                        case BillType.Electric:
                            lastCost = GetCellValue<double>(worksheet, row, COST_ELECTRIC);
                            indexHeadPeriod = GetCellValue<decimal>(worksheet, row, INDEX_HEAD_ELECTRIC);
                            indexEndPeriod = GetCellValue<decimal>(worksheet, row, INDEX_END_ELECTRIC);
                            totalIndex = indexEndPeriod - indexHeadPeriod;
                            titleUserBill = $"Hóa đơn tiền điện tháng {period:MM/yyyy}";
                            break;
                        case BillType.Water:
                            lastCost = GetCellValue<double>(worksheet, row, COST_WATER);
                            indexHeadPeriod = GetCellValue<decimal>(worksheet, row, INDEX_HEAD_WATER);
                            indexEndPeriod = GetCellValue<decimal>(worksheet, row, INDEX_END_WATER);
                            totalIndex = indexEndPeriod - indexHeadPeriod;
                            titleUserBill = $"Hóa đơn tiền nước tháng {period:MM/yyyy}";
                            break;
                        case BillType.Parking:
                            numberCar = GetCellValue<int>(worksheet, row, CAR_NUMBER);
                            numberMotobike = GetCellValue<int>(worksheet, row, MOTOBIKE_NUMBER);
                            numberBike = GetCellValue<int>(worksheet, row, BIKE_NUMBER);
                            otherVehicle = GetCellValue<int>(worksheet, row, OTHER_VEHICLE_NUMBER);
                            double costCar = GetCellValue<double>(worksheet, row, COST_CAR);
                            double costMotobike = GetCellValue<double>(worksheet, row, COST_MOTOBIKE);
                            double costBike = GetCellValue<double>(worksheet, row, COST_BIKE);
                            double costOtherVehicle = GetCellValue<double>(worksheet, row, COST_OTHER_VEHICLE);
                            lastCost = costCar + costMotobike + costBike + costOtherVehicle;
                            titleUserBill = $"Phí gửi xe tháng {period:MM/yyyy}";
                            break;
                        case BillType.Manager:
                            totalIndex = GetCellValue<decimal>(worksheet, row, ACREAGE_APARTMENT);
                            lastCost = GetCellValue<double>(worksheet, row, COST_MANAGER);
                            titleUserBill = $"Phí quản lý tháng {period:MM/yyyy}";
                            break;
                        default:
                            break;
                    }
                    if (lastCost <= 0) continue;
                    UserBill userBillTemp = new()
                    {
                        Title = titleUserBill,
                        Properties = userBill.Properties,
                        BillType = userBillTypes[i],
                        TenantId = AbpSession.TenantId,
                        ApartmentCode = apartmentCode,
                        LastCost = lastCost,
                        Status = status,
                        IndexEndPeriod = indexEndPeriod,
                        Period = period,
                        DueDate = period,
                        UrbanId = userBill.UrbanId,
                        BuildingId = userBill.BuildingId,
                        CitizenTempId = userBill.CitizenTempId,
                        Amount = 0,
                        BicycleNumber = numberBike,
                        CarNumber = numberCar,
                        MotorbikeNumber = numberMotobike,
                        IndexHeadPeriod = indexHeadPeriod,
                        OtherVehicleNumber = otherVehicle,
                        TotalIndex = totalIndex,
                        BillConfigId = 0,
                    };
                    listUserBillTemps.Add(userBillTemp);
                }
                excelBillDebt.UserBills = listUserBillTemps;
                // insert UserBillPayment
                if (amountPayment > 0)
                {
                    UserBillPayment userBillPayment = new()
                    {
                        Title = $"Thanh toán hóa đơn tháng {period:MM/yyyy}",
                        Method = UserBillPaymentMethod.Direct,
                        Properties = userBill.Properties,
                        TenantId = AbpSession.TenantId,
                        Status = UserBillPaymentStatus.Success,
                        Amount = amountPayment,
                        Period = period,
                        TypePayment = status == UserBillStatus.Debt ? TypePayment.DebtBill : TypePayment.Bill,
                        ApartmentCode = apartmentCode,
                        UrbanId = userBill.UrbanId,
                        BuildingId = userBill.BuildingId,
                    };
                    excelBillDebt.UserBillPayments = new() { userBillPayment };
                }

                // insert BillDebt
                if (status == UserBillStatus.Debt)
                {
                    BillDebt billDebt = new()
                    {
                        Title = $"Công nợ tháng {period:MM/yyyy}",
                        TenantId = AbpSession.TenantId,
                        Period = period,
                        ApartmentCode = apartmentCode,
                        State = DebtState.DEBT,
                        PaidAmount = amountPayment,
                        DebtTotal = debtTotal,
                        BuildingId = userBill.BuildingId,
                        UrbanId = userBill.UrbanId,
                        CitizenTempId = userBill.CitizenTempId,
                        UserId = userBill.CitizenTempId,
                    };
                    excelBillDebt.BillDebts = new() { billDebt };
                }
                excelBillDebtDtos.Add(excelBillDebt);
            }
            return excelBillDebtDtos;
        }
        private List<ExcelBillDebtDto> ExtractExcelBillDebtVCI(ExcelPackage package)
        {
            List<ExcelBillDebtDto> excelBillDebtDtos = new();
            List<AppOrganizationUnit> buildings = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
            List<AppOrganizationUnit> urbans = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);
            ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
            int rowCount = worksheet.Dimension.End.Row;
            int colCount = worksheet.Dimension.End.Column;

            const int INDEX = 1;
            const int URBAN_INDEX = 2;
            const int BUILDING_INDEX = 3;
            const int APARTMENT_CODE_INDEX = 4;
            const int CUSTOMER_NAME_INDEX = 5;
            const int MONTH_PERIOD = 6;
            const int COST_MANAGER = 7;
            const int COST_MOTOBIKE = 8;
            const int COST_BIKE = 9;
            const int COST_CAR = 10;
            const int INDEX_END_WATER = 11;
            const int INDEX_END_ELECTRIC = 12;
            const int COST_WATER = 13;
            const int COST_ELECTRIC = 14;
            const int DEBT_TOTAL = 15;
            const int AMOUNT_PAYMENT = 16;

            int countApartmentNull = 0;
            List<BillType> userBillTypes = new() { BillType.Electric, BillType.Water, BillType.Parking, BillType.Manager };
            for (int row = 2; row <= rowCount; row++)
            {
                DateTime period = GetCellValueNotDefault<DateTime>(worksheet, row, MONTH_PERIOD);
                string apartmentCode = GetCellValue<string>(worksheet, row, APARTMENT_CODE_INDEX);
                string customerName = GetCellValue<string>(worksheet, row, CUSTOMER_NAME_INDEX);
                string buildingCode = GetCellValue<string>(worksheet, row, BUILDING_INDEX);
                string urbanCode = GetCellValue<string>(worksheet, row, URBAN_INDEX);
                double debtTotal = GetCellValue<double>(worksheet, row, DEBT_TOTAL);
                double amountPayment = GetCellValue<double>(worksheet, row, AMOUNT_PAYMENT);

                if (string.IsNullOrEmpty(apartmentCode) || string.IsNullOrWhiteSpace(apartmentCode))
                {
                    countApartmentNull++;
                    if (countApartmentNull >= 10) break;
                    continue;
                }
                ExcelBillDebtDto excelBillDebt = new();
                UserBill userBill = new();
                string titleUserBill = string.Empty;

                CitizenTemp citizens = _citizenTempRepository
                    .GetAll()
                    .Where(x => x.ApartmentCode == apartmentCode)
                    .OrderBy(x => x.IsStayed ? 0 : 1) // Sort by IsStayed, with stayed citizens first
                    .ThenBy(x => x.RelationShip == RELATIONSHIP.Contractor ? 0 : 1) // Then by Relationship, with Contractors first
                    .FirstOrDefault();
                if (citizens != null)
                {
                    customerName = string.IsNullOrWhiteSpace(customerName) ? citizens.FullName : customerName;
                    userBill.CitizenTempId = citizens.Id;
                }
                else
                {
                    customerName = string.IsNullOrWhiteSpace(customerName) ? string.Empty : customerName;
                }
                userBill.BuildingId = FindParentId(buildings, buildingCode);
                userBill.UrbanId = FindParentId(urbans, urbanCode);
                userBill.Properties = JsonConvert.SerializeObject(new BillProperty() { customerName = customerName });
                UserBillStatus status = debtTotal > 0 ? UserBillStatus.Debt : UserBillStatus.Paid;

                List<UserBill> listUserBillTemps = new();
                // insert UserBill
                for (int i = 0; i < userBillTypes.Count; i++)
                {
                    double lastCost = 0;
                    decimal indexEndPeriod = 0;
                    switch (userBillTypes[i])
                    {
                        case BillType.Electric:
                            lastCost = GetCellValue<double>(worksheet, row, COST_ELECTRIC);
                            indexEndPeriod = GetCellValue<decimal>(worksheet, row, INDEX_END_ELECTRIC);
                            titleUserBill = $"Hóa đơn tiền điện tháng {period:MM/yyyy}";
                            break;
                        case BillType.Water:
                            lastCost = GetCellValue<double>(worksheet, row, COST_WATER);
                            indexEndPeriod = GetCellValue<decimal>(worksheet, row, INDEX_END_WATER);
                            titleUserBill = $"Hóa đơn tiền nước tháng {period:MM/yyyy}";
                            break;
                        case BillType.Parking:
                            double costCar = GetCellValue<double>(worksheet, row, COST_CAR);
                            double costMotobike = GetCellValue<double>(worksheet, row, COST_MOTOBIKE);
                            double costBike = GetCellValue<double>(worksheet, row, COST_BIKE);
                            lastCost = costCar + costMotobike + costBike;
                            titleUserBill = $"Phí gửi xe tháng {period:MM/yyyy}";
                            break;
                        case BillType.Manager:
                            lastCost = GetCellValue<double>(worksheet, row, COST_MANAGER);
                            titleUserBill = $"Phí quản lý tháng {period:MM/yyyy}";
                            break;
                        default:
                            break;
                    }
                    if (lastCost <= 0) continue;
                    UserBill userBillTemp = new()
                    {
                        Title = titleUserBill,
                        Properties = userBill.Properties,
                        BillType = userBillTypes[i],
                        TenantId = AbpSession.TenantId,
                        ApartmentCode = apartmentCode,
                        LastCost = lastCost,
                        Status = status,
                        IndexEndPeriod = indexEndPeriod,
                        Period = period,
                        DueDate = period,
                        UrbanId = userBill.UrbanId,
                        BuildingId = userBill.BuildingId,
                        CitizenTempId = userBill.CitizenTempId,
                    };
                    listUserBillTemps.Add(userBillTemp);
                }
                excelBillDebt.UserBills = listUserBillTemps;
                // insert UserBillPayment
                if (amountPayment > 0)
                {
                    UserBillPayment userBillPayment = new()
                    {
                        Title = $"Thanh toán hóa đơn tháng {period:MM/yyyy}",
                        Method = UserBillPaymentMethod.Direct,
                        Properties = userBill.Properties,
                        TenantId = AbpSession.TenantId,
                        Status = UserBillPaymentStatus.Success,
                        Amount = amountPayment,
                        Period = period,
                        TypePayment = status == UserBillStatus.Debt ? TypePayment.DebtBill : TypePayment.Bill,
                        ApartmentCode = apartmentCode,
                        UrbanId = userBill.UrbanId,
                        BuildingId = userBill.BuildingId,
                    };
                    excelBillDebt.UserBillPayments = new() { userBillPayment };
                }

                // insert BillDebt
                if (status == UserBillStatus.Debt)
                {
                    BillDebt billDebt = new()
                    {
                        Title = $"Công nợ tháng {period:MM/yyyy}",
                        TenantId = AbpSession.TenantId,
                        Period = period,
                        ApartmentCode = apartmentCode,
                        State = DebtState.DEBT,
                        PaidAmount = amountPayment,
                        DebtTotal = debtTotal,
                        BuildingId = userBill.BuildingId,
                        UrbanId = userBill.UrbanId,
                        CitizenTempId = userBill.CitizenTempId,
                        UserId = userBill.CitizenTempId,
                    };
                    excelBillDebt.BillDebts = new() { billDebt };
                }
                excelBillDebtDtos.Add(excelBillDebt);
            }
            return excelBillDebtDtos;
        }
        // Nam Cường
        private List<ExcelBillDebtDto> ExtractExcelBillDebtNC(ExcelPackage package)
        {
            List<ExcelBillDebtDto> excelBillDebtDtos = new();
            DateTime period = DateTime.Now.AddMonths(-1);
            DateTime prePeriod = period.AddMonths(-1);
            List<AppOrganizationUnit> buildings = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
            List<AppOrganizationUnit> urbans = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);
            ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
            int rowCount = worksheet.Dimension.End.Row;
            int colCount = worksheet.Dimension.End.Column;

            const int INDEX = 1;
            const int BUILDING_INDEX = 2;
            const int APARTMENT_NUMBER_INDEX = 3;
            const int APARTMENT_CODE_INDEX = 4;
            const int CUSTOMER_NAME_INDEX = 5;
            const int ACREAGE_APARTMENT_INDEX = 6;
            const int PHONE_NUMBER_CUSTOMER_INDEX = 7;
            const int COST_MANAGER_DEBT_INDEX = 8;
            const int COST_VEHICLE_DEBT_INDEX = 9;
            const int COST_MANAGER_INDEX = 10;
            const int COST_VEHICLE_INDEX = 11;

            int countApartmentNull = 0;
            List<BillType> userBillTypes = new() { BillType.Manager, BillType.Parking, BillType.Manager, BillType.Parking };
            List<UserBillStatus> userBillStatus = new() { UserBillStatus.Debt, UserBillStatus.Debt, UserBillStatus.Pending, UserBillStatus.Pending };
            List<string> titleUserBills = new()
            {
                $"Phí dịch vụ cũ trước tháng {period:MM/yyyy}",
                $"Phí xe cũ trước tháng {period:MM/yyyy}",
                $"Phí dịch vụ tháng {period:MM/yyyy}",
                $"Phí dịch vụ cũ tháng {period:MM/yyyy}",
            };
            List<int> indexLastCost = new()
            {
                COST_MANAGER_DEBT_INDEX,
                COST_VEHICLE_DEBT_INDEX,
                COST_MANAGER_INDEX,
                COST_VEHICLE_INDEX
            };

            for (int row = 2; row <= rowCount; row++)
            {
                string apartmentCode = GetCellValue<string>(worksheet, row, APARTMENT_CODE_INDEX);
                string customerName = GetCellValue<string>(worksheet, row, CUSTOMER_NAME_INDEX);
                string buildingCode = GetCellValue<string>(worksheet, row, BUILDING_INDEX);
                double totalIndex = GetCellValue<double>(worksheet, row, ACREAGE_APARTMENT_INDEX);

                if (string.IsNullOrEmpty(apartmentCode) || string.IsNullOrWhiteSpace(apartmentCode))
                {
                    countApartmentNull++;
                    if (countApartmentNull >= 10) break;
                    continue;
                }
                ExcelBillDebtDto excelBillDebt = new();
                UserBill userBill = new();

                CitizenTemp citizens = _citizenTempRepository
                    .GetAll()
                    .Where(x => x.ApartmentCode == apartmentCode)
                    .OrderBy(x => x.IsStayed ? 0 : 1) // Sort by IsStayed, with stayed citizens first
                    .ThenBy(x => x.RelationShip == RELATIONSHIP.Contractor ? 0 : 1) // Then by Relationship, with Contractors first
                    .FirstOrDefault();
                if (citizens != null)
                {
                    customerName = string.IsNullOrWhiteSpace(customerName) ? citizens.FullName : customerName;
                    userBill.CitizenTempId = citizens.Id;
                }
                else
                {
                    customerName = string.IsNullOrWhiteSpace(customerName) ? string.Empty : customerName;
                }
                userBill.BuildingId = FindParentId(buildings, buildingCode);
                userBill.UrbanId = FindUrbanId(buildings, userBill.BuildingId);
                userBill.Properties = JsonConvert.SerializeObject(new BillProperty() { customerName = customerName });

                List<UserBill> listUserBillTemps = new();

                // insert UserBill
                for (int i = 0; i < userBillTypes.Count; i++)
                {
                    double lastCost = GetCellValue<double>(worksheet, row, indexLastCost[i]);
                    if (lastCost <= 0) continue;
                    BillType billType = userBillTypes[i];
                    UserBill userBillTemp = new()
                    {
                        Title = titleUserBills[i],
                        Properties = userBill.Properties,
                        BillType = userBillTypes[i],
                        TenantId = AbpSession.TenantId,
                        ApartmentCode = apartmentCode,
                        LastCost = lastCost,
                        Status = userBillStatus[i],
                        Period = userBillStatus[i] == UserBillStatus.Debt ? prePeriod : period,
                        DueDate = userBillStatus[i] == UserBillStatus.Debt ? prePeriod : period,
                        TotalIndex = billType == BillType.Manager ? (decimal)totalIndex : 0,
                        UrbanId = userBill.UrbanId,
                        BuildingId = userBill.BuildingId,
                        CitizenTempId = userBill.CitizenTempId,
                    };
                    listUserBillTemps.Add(userBillTemp);
                }
                excelBillDebt.UserBills = listUserBillTemps;

                // insert BillDebt
                if (listUserBillTemps.Any(x => x.Status == UserBillStatus.Debt))
                {
                    BillDebt billDebt = new()
                    {
                        Title = $"Công nợ tháng {period:MM/yyyy}",
                        TenantId = AbpSession.TenantId,
                        Period = period,
                        ApartmentCode = apartmentCode,
                        PaidAmount = 0,
                        State = DebtState.DEBT,
                        DebtTotal = listUserBillTemps.Where(x => x.Status == UserBillStatus.Debt).Sum(x => x.LastCost),
                        BuildingId = userBill.BuildingId,
                        UrbanId = userBill.UrbanId,
                        CitizenTempId = userBill.CitizenTempId,
                        UserId = userBill.CitizenTempId,
                    };
                    excelBillDebt.BillDebts = new() { billDebt };
                }
                excelBillDebtDtos.Add(excelBillDebt);
            }
            return excelBillDebtDtos;
        }
     
        #region method helper
        private static long? FindParentId(IEnumerable<AppOrganizationUnit> list, string codeToFind)
        {
            if (codeToFind != null)
            {
                var item = list.FirstOrDefault(x => x.ProjectCode == codeToFind);
                return item?.ParentId;
            }
            return null;
        }
        private static long? FindUrbanId(IEnumerable<AppOrganizationUnit> list, long? buildingId)
        {
            if (buildingId != null)
            {
                var item = list.FirstOrDefault(x => x.Id == buildingId);
                return item?.ParentId;
            }
            return null;
        }
        #endregion
    }
}
