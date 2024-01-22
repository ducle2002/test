using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Configuration;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Yootek.Services.Dto;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Yootek.Services
{
    public interface IAdminManagerBillAppService : IApplicationService
    {
        Task<object> GetAllBillConfigs(GetAllBillConfigInputDto input);
        Task<DataResult> CreateOrUpdateBillConfig(CreateOrUpdateBillConfigInputDto input);
        Task<object> CreateUserBill(CreateUserBillByAdminInput input);
    }

    //  [AbpAuthorize(PermissionNames.Pages_SmartCommunity_Fees)]
    [AbpAuthorize]
    public class AdminManagerBillAppService : YootekAppServiceBase, IAdminManagerBillAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<BillDebt, long> _billDebtRepository;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepository;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepo;
        private readonly IRepository<BillConfig, long> _billConfigRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<MeterMonthly, long> _meterMonthlyRepository;
        private readonly IRepository<Meter, long> _meterRepository;
        private readonly IRepository<MeterType, long> _meterTypeRepository;

        private readonly BillUtilAppService _billUtilAppService;
        private readonly IRepository<UserBillVehicleInfo, long> _billVehicleInfoRepository;
        private readonly IBillExcelExporter _billExcelExporter;
        private readonly IBillEmailUtil _billEmailUtil;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<CitizenParking, long> _citizenParkingRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IConfigurationRoot _appConfiguration;

        public AdminManagerBillAppService(
            IRepository<UserBill, long> userBillRepo,
            IRepository<BillConfig, long> billConfigRepo,
            IRepository<Apartment, long> apartmentRepository,
            IRepository<BillDebt, long> billDebtRepository,
            IRepository<UserBillPayment, long> userBillPaymentRepository,
            IRepository<User, long> userRepos,
            IRepository<MeterMonthly, long> meterMonthlyRepository,
            IRepository<Meter, long> meterRepository,
            IRepository<MeterType, long> meterTypeRepository,
            IRepository<CitizenTemp, long> citizenTempRepo,
            BillUtilAppService billUtilAppService,
            IRepository<UserBillVehicleInfo, long> billVehicleInfoRepository,
            IAppNotifier appNotifier,
            IBillExcelExporter billExcelExporter,
            IBillEmailUtil billEmailUtil,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IBackgroundJobManager backgroundJobManager,
            IAppConfigurationAccessor configurationAccessor,
            IRepository<CitizenParking, long> citizenParkingRepository)
        {
            _userBillRepo = userBillRepo;
            _citizenTempRepo = citizenTempRepo;
            _billDebtRepository = billDebtRepository;
            _apartmentRepository = apartmentRepository;
            _userBillPaymentRepository = userBillPaymentRepository;
            _userRepos = userRepos;
            _billConfigRepo = billConfigRepo;
            _billUtilAppService = billUtilAppService;
            _appNotifier = appNotifier;
            _billExcelExporter = billExcelExporter;
            _organizationUnitRepository = organizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _backgroundJobManager = backgroundJobManager;
            _billEmailUtil = billEmailUtil;
            _appConfiguration = configurationAccessor.Configuration;
            _citizenParkingRepository = citizenParkingRepository;
            _meterMonthlyRepository = meterMonthlyRepository;
            _meterRepository = meterRepository;
            _meterTypeRepository = meterTypeRepository;
            _billVehicleInfoRepository = billVehicleInfoRepository;
        }

        public async Task<object> GetApartmentInfoAsync(string apartmentCode)
        {
            try
            {
                var data = await _apartmentRepository.FirstOrDefaultAsync(x => x.ApartmentCode == apartmentCode);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> GetMonthPrepaymentByApartment(GetMonthPrepaymentDto input)
        {
            try
            {
                var data = await _userBillRepo.GetAll()
                    .Where(x => x.ApartmentCode == input.ApartmentCode && x.UrbanId == input.UrbanId
                    && x.BuildingId == input.BuildingId && x.BillType == input.BillType
                    && x.Status == UserBillStatus.Paid && x.IsPrepayment == true && x.IsPaymentPending != true)
                   .OrderByDescending(x => x.Period).Select(x => x.Period).FirstOrDefaultAsync();

                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllOtherBillConfigs()
        {
            try
            {
                var result = await _billConfigRepo.GetAll().Where(x => x.BillType == BillType.Other)
                    .Select(x => new OtherBillConfigDto()
                    {
                        Key = x.Title,
                        Id = x.Id,
                        PricesType = x.PricesType,
                        Properties = x.Properties
                    })
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

        public async Task<object> GetAllBillConfigs(GetAllBillConfigInputDto input)
        {
            try
            {
                var result = await _billConfigRepo.GetAll()
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                    .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType)
                    .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId)
                    .WhereIf(input.Level.HasValue, x => x.Level == input.Level)
                    .ApplySearchFilter(input.Keyword, x => x.Title)
                    .ApplySort(input.OrderBy, input.SortBy).ApplySort(OrderByBill.TITLE)
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

        public async Task<object> GetAllBillConfigGroup(GetAllBillConfigInputDto input)
        {
            try
            {
                var query = await _billConfigRepo.GetAll()
                    .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Title.Contains(input.Keyword))
                    .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType)
                    .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId)
                    .WhereIf(input.Level.HasValue, x => x.Level == input.Level)
                    .ToListAsync();

                var privates = query.Where(x => x.IsPrivate == true && x.PrivateKey.HasValue);
                var others = query.Where(x => x.IsPrivate != true);
                var result = new List<BillConfig>();
                if (privates != null && privates.Count() > 0)
                {
                    var group = privates.GroupBy(x => x.PrivateKey).Select(x => new
                    {
                        Key = x.Key,
                        Value = x.ToList()
                    }).ToDictionary(x => x.Key, y => y.Value);

                    if (group != null && group.Count() > 0)
                    {
                        foreach (var item in group)
                        {
                            var config = item.Value[0].MapTo<BillConfigDto>();
                            config.ListPrivates = item.Value;
                            result.Add(config);
                        }

                    }

                }
                result = result.Concat(others.MapTo<List<BillConfigDto>>()).ToList();

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

        public async Task<object> GetBillConfigByIdAsync(long id)
        {
            try
            {
                var data = await _billConfigRepo.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetBillDebtByApartmentAsync(string apartmentCode)
        {
            try
            {
                var data = await _userBillRepo.GetAllListAsync(x => x.ApartmentCode == apartmentCode && x.Status == UserBillStatus.Debt);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> CreateOrUpdateBillConfig(CreateOrUpdateBillConfigInputDto input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                var billConfig = input.MapTo<BillConfig>();

                // Check parent
                if (input.ParentId > 0)
                {
                    var parentBillConfig = await _billConfigRepo.FirstOrDefaultAsync(x => x.Id == input.ParentId);
                    if (parentBillConfig == null)
                    {
                        return DataResult.ResultError("Parent bill config not found", "Error");
                    }

                    billConfig.BillType = parentBillConfig.BillType;
                    billConfig.Level = parentBillConfig.Level + 1;
                }
                else // is root
                {
                    billConfig.ParentId = null;
                    billConfig.Level = 0;

                    // Check bill type in enum BillType
                    if (!Enum.IsDefined(typeof(BillType), input.BillType.Value))
                    {
                        return DataResult.ResultError("Bill type not found", "Error");
                    }
                }

                // Assign null to PricesType, Properties
                if (billConfig.PricesType == 0)
                {
                    billConfig.PricesType = null;
                }

                if (String.IsNullOrEmpty(billConfig.Properties))
                {
                    billConfig.Properties = null;
                }


                if (billConfig.Id > 0)
                {
                    var result = await _billConfigRepo.UpdateAsync(billConfig);
                    return DataResult.ResultSuccess(result, "Update success");
                }
                else
                {
                    if (!input.Code.IsNullOrEmpty())
                    {
                        var existingCodeBill = await _billConfigRepo.FirstOrDefaultAsync(x => x.Code == input.Code);
                        if (existingCodeBill != null)
                        {
                            return DataResult.ResultError("Mã phí dịch vụ đã tồn tại.", "Mã đã tồn tại.");
                        }
                    }

                    var result = await _billConfigRepo.InsertAsync(billConfig);
                    return DataResult.ResultSuccess(result, "Create success");
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> DeleteBillConfig(long id)
        {
            try
            {
                await _billConfigRepo.DeleteAsync(id);
                var data = DataResult.ResultSuccess(null, "Delete success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllUserBills(AdminGetAllUserBillsInputDto input)
        {
            try
            {
                // var ouUI = await _userOrganizationUnitRepository.GetAll()
                //     .Where(x => x.UserId == AbpSession.UserId)
                //     .Select(x => x.OrganizationUnitId)
                //     .ToListAsync();
                var query = await QueryUserBills(input);
                //var filteredQuery = query.Where(x => ouUI.Contains(x.OrganizationUnitId.Value));
                var pagedResult = await query.PageBy(input).ToListAsync();
                var result = pagedResult;


                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetUserBillById(long id)
        {
            try
            {
                var data = await _userBillRepo.GetAsync(id);
                return DataResult.ResultSuccess(data, "success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> GetUserBillTemplate(GetUserBillTemplateInput input)
        {
            try
            {
                var period = new DateTime(input.PeriodYear, input.PeriodMonth + 1, 1);
                var template = await _billEmailUtil.GetTemplateByApartment(input.ApartmentCode, period);
                var data = DataResult.ResultSuccess(template.ToString().Replace("OCTYPE html>", ""), "Get success");
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private async Task<IQueryable<UserBill>> QueryUserBills(AdminGetAllUserBillsInputDto input)
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = _userBillRepo.GetAll()
                .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                .WhereIf(input.UrbanId.HasValue,
                    x => (x.UrbanId.HasValue && x.UrbanId.Value == input.UrbanId.Value))
                .WhereIf(input.BuildingId.HasValue,
                    x => x.BuildingId.HasValue && x.BuildingId.Value == input.BuildingId.Value)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
                .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType)
                .WhereIf(!input.ApartmentCode.IsNullOrEmpty(),
                    x => x.ApartmentCode.ToLower().Contains(input.ApartmentCode.ToLower()))
                .WhereIf(!input.Keyword.IsNullOrEmpty(),
                    x => x.Title.Contains(input.Keyword) ||
                         x.ApartmentCode.Contains(input.Keyword) ||
                         x.Code.Contains(input.Keyword))
                .WhereIf(input.Period.HasValue,
                    x => x.Period.Value.Month == input.Period.Value.Month &&
                         x.Period.Value.Year == input.Period.Value.Year)
                .WhereIf(input.DueDateFrom.HasValue, x => x.DueDate >= input.DueDateFrom.Value)
                .WhereIf(input.DueDateTo.HasValue, x => x.DueDate <= input.DueDateTo.Value)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .OrderByDescending(x => x.ApartmentCode)
                .AsQueryable();

            return query;
        }

        public async Task<object> CreateOrUpdateUserBill(CreateOrUpdateUserBillInputDto input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                var userBill = input.MapTo<UserBill>();

                // Check billConfig
                // var billConfig = await _billConfigRepo.FirstOrDefaultAsync(x => x.Id == input.BillConfigId);
                // if (billConfig == null)
                // {
                //     return DataResult.ResultError("Bill config not found", "Error");
                // }

                // Check bill type in enum BillType
                if (!Enum.IsDefined(typeof(BillType), input.BillType))
                {
                    return DataResult.ResultError("Bill type not found", "Error");
                }

                var message = "Create success";
                if (userBill.Id > 0)
                {
                    message = "Update success";
                    userBill = await _userBillRepo.UpdateAsync(userBill);
                }
                else
                {
                    message = "Create success";
                    var id = await _userBillRepo.InsertAndGetIdAsync(userBill);
                    userBill.Code = "HD0" + id +
                                    (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month) + "" +
                                    DateTime.Now.Year;
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                // Nếu tính ở phía server
                // var costResult = await _billUtilAppService.CalculateUserBill(userBill, billConfig);
                // userBill.Cost = costResult.Cost;
                // userBill.LastCost = costResult.LastCost;
                // userBill.Surcharges = costResult.Surcharges;

                return DataResult.ResultSuccess(userBill, message);
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> CreateUserBill(CreateUserBillByAdminInput input)
        {
            try
            {
                UserBill userBill = input.MapTo<UserBill>();
                userBill.TenantId = AbpSession.TenantId;
                if (!Enum.IsDefined(typeof(BillType), input.BillType))
                {
                    return DataResult.ResultError("Bill type not found", "Error");
                }

                long id = await _userBillRepo.InsertAndGetIdAsync(userBill);
                userBill.Code = "HD0" + id + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month) +
                                "" + DateTime.Now.Year;
                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(true, "Create success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> DeleteUserBills([FromBody] List<long> ids)
        {
            try
            {
                await _userBillRepo.DeleteAsync(x => ids.Contains(x.Id));
                var data = DataResult.ResultSuccess(null, "Delete success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> DeleteBillMonthly(DeleteBillMonthlyInput input)
        {
            try
            {
                if (input.IsDeleteAll)
                {
                    await _userBillRepo.DeleteAsync(x => input.Ids.Contains(x.Id));
                }
                else
                {
                    await _userBillRepo.DeleteAsync(x => x.Status == UserBillStatus.Pending && input.Ids.Contains(x.Id));
                }

                var data = DataResult.ResultSuccess("Delete success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> DeleteOlderUserBills(List<long> ids)
        {
            try
            {
                await _userBillRepo.DeleteAsync(x => ids.Contains(x.Id));
                var data = DataResult.ResultSuccess(null, "Delete success");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [RemoteService(false)]
        [AbpAllowAnonymous]
        public async Task CheckUserBillMonthly()
        {
            try
            {
                DateTime now = DateTime.Now;
                var days = DateTime.DaysInMonth(now.Year, now.Month);
                var currentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
                var nextMonth = new DateTime(now.Year, now.Month, days, 0, 0, 0);

                var input = new AdminGetAllUserBillsInputDto()
                {
                    DueDateFrom = currentMonth,
                    DueDateTo = nextMonth,
                    Status = UserBillStatus.Pending
                };

                var query = await QueryUserBills(input);
                var result = await query.ToListAsync();

                foreach (var item in result)
                {
                    _billUtilAppService.SendUserBillToClient(result.ToArray());
                }
            }
            catch (Exception e)
            {
            }
        }

        public async Task<object> SendNotifyBillManual()
        {
            try
            {
                var input = new AdminGetAllUserBillsInputDto()
                {
                    Status = UserBillStatus.Pending
                };

                var query = await QueryUserBills(input);
                var result = await query.ToListAsync();

                await _billUtilAppService.SendUserBillToClient(result.ToArray());

                return DataResult.ResultSuccess(null, "Send success");
            }
            catch (Exception e)
            {
                Logger.Info(e.Message, e);
                return DataResult.ResultError(e.Message, "Error");
            }
        }

        public async Task<object> UploadBillsExcel([FromForm] ImportExcelBillInput input, CancellationToken cancellationToken)
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
            await file.CopyToAsync(stream, cancellationToken);
            if (input.Formulas == null || input.Formulas.Count() == 0)
            {
                input.Formulas = _billConfigRepo.GetAllList(x => x.BillType == input.Type && x.IsDefault == true);
                if (input.Formulas == null) input.Formulas = new List<BillConfig>();
            }

            try
            {
                var package = new ExcelPackage(stream);

                var listUserBill = new List<UserBill>();
                if (input.Type == BillType.Parking)
                {
                    if (input.ParkingType == ParkingBillType.ParkingLevel)
                    {
                        listUserBill = ExtractExcelUserBillParkingLevel(package, input.Formulas);
                    }
                    else
                    {
                        listUserBill = ExtractExcelUserBillParking(package, input.Formulas);
                    }
                }
                else
                {
                    try
                    {
                        listUserBill = ExtractExcelOnlyLastIndexUserBillEW(package, input.Type, input.Formulas);
                    }
                    catch
                    {
                        listUserBill = ExtractExcelUserBillEWM(package, input.Type, input.Formulas);
                    }
                }

                await CreateListUserBillAsync(listUserBill, input.ParkingType);

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
            const int CODE_CONFIG_INDEX = 12;
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

                var citizens = _citizenTempRepo.GetAll().Select(x => new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    ApartmentCode = x.ApartmentCode,
                    IsStayed = x.IsStayed,
                    RelationShip = x.RelationShip
                }).Where(x => x.ApartmentCode == apartmentCode).ToList();


                if (worksheet.Cells[row, CODE_CONFIG_INDEX].Text != "")
                {
                    try
                    {
                        var codeListConfig = worksheet.Cells[row, CODE_CONFIG_INDEX].Value.ToString().Trim().Split(',').Select(code => code.Trim())
                        .ToList();

                        formulas = _billConfigRepo.GetAll()
                            .Where(x => codeListConfig.Contains(x.Code))
                            .ToList();
                    }
                    catch { }
                }


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
            const int FIRST_INDEX = 6;
            const int LAST_INDEX = 7;
            const int BUILDING_INDEX = 8;
            const int URBAN_INDEX = 9;
            const int CODE_CONFIG_INDEX = 10;
            const int MONTH_NUMBER = 11;

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

                var citizens = _citizenTempRepo.GetAll().Select(x => new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    ApartmentCode = x.ApartmentCode,
                    IsStayed = x.IsStayed,
                    RelationShip = x.RelationShip
                }).Where(x => x.ApartmentCode == apartmentCode).ToList();

                var configCode = "";
                var checkConfigCode = false;
                if (worksheet.Cells[row, CODE_CONFIG_INDEX].Text != "")
                {
                    var configNewCode = worksheet.Cells[row, CODE_CONFIG_INDEX].Value.ToString();
                    if (configCode != configNewCode)
                    {
                        configCode = configNewCode;
                        checkConfigCode = true;
                    }
                    else
                    {
                        checkConfigCode = false;
                    }

                    try
                    {
                        var codeListConfig = configNewCode.Trim().Split(',').Select(code => code.Trim())
                        .ToList();

                        formulas = _billConfigRepo.GetAll()
                            .Where(x => codeListConfig.Contains(x.Code))
                            .ToList();
                    }
                    catch { }
                }
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
                userBill.Title = worksheet.Cells[row, TITLE_INDEX].Value != null ? worksheet.Cells[row, TITLE_INDEX].Value.ToString() : $"Hóa đơn tháng";
                userBill.BillType = billType;
                userBill.ApartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString();
                userBill.Status = UserBillStatus.Pending;
                userBill.Period = DateTime.Parse(worksheet.Cells[row, PERIOD_INDEX].Value.ToString());
                userBill.DueDate = DateTime.Parse(worksheet.Cells[row, DUE_DATE_INDEX].Value.ToString());
                userBill.IndexHeadPeriod = worksheet.Cells[row, FIRST_INDEX].Text.ToString() != "" ?
                    decimal.Parse(worksheet.Cells[row, FIRST_INDEX].Value.ToString())
                    : 0;
                userBill.MonthNumber = worksheet.Cells[row, MONTH_NUMBER].Text.ToString() != "" ?
                    int.Parse(worksheet.Cells[row, MONTH_NUMBER].Value.ToString())
                    : 1;

                //if (userBill.Period != null)
                //{
                //    var pre_period = userBill.Period.Value.AddMonths(-1);
                //    var pre_bill = _userBillRepo.FirstOrDefault(x =>
                //        x.ApartmentCode == apartmentCode && x.Period.Value.Year == pre_period.Year &&
                //        x.Period.Value.Month == pre_period.Month);
                //    if (pre_bill != null) userBill.IndexHeadPeriod = pre_bill.IndexEndPeriod;
                //    else userBill.IndexHeadPeriod = 0;
                //}
                if (userBill.Period != null)
                {
                    var pre_period = userBill.Period.Value.AddMonths(-1);
                    var pre_bill = _userBillRepo.FirstOrDefault(x =>
                            x.ApartmentCode == apartmentCode && x.Period.Value.Year == pre_period.Year &&
                            x.Period.Value.Month == pre_period.Month);

                    userBill.IndexHeadPeriod = userBill.IndexHeadPeriod > 0 ? userBill.IndexHeadPeriod : (pre_bill?.IndexEndPeriod ?? 0);
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

                if (buildingCode != checkBuildingCode || urbanCode != checkUrbanCode || checkConfigCode)
                {
                    checkBuildingCode = buildingCode;
                    checkUrbanCode = urbanCode;
                    var formulaBuildings =
                       CheckPriceConfigByBuilding(formulas, userBill.BuildingId, userBill.UrbanId);
                    // var formulaBuildings = CheckPriceConfigBill(formulas, userBill.BuildingId, userBill.UrbanId, codeListConfig);

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
                else if (billType.Equals(BillType.Manager))
                {
                    userBill.TotalIndex = decimal.Parse(worksheet.Cells[row, LAST_INDEX].Value.ToString());


                    if (priceM != null && priceM.Value > 0)
                    {
                        if (userBill.TotalIndex > 0)
                        {
                            userBill.LastCost = (double)(priceM.Value * userBill.TotalIndex);

                        }
                        else
                        {
                            var apartment = _apartmentRepository.FirstOrDefault(x => x.ApartmentCode == apartmentCode);
                            userBill.TotalIndex = apartment != null ? apartment.Area ?? 0 : 0;
                            userBill.LastCost = (double)(priceM.Value * userBill.TotalIndex);
                        }
                    }
                }
                else
                {
                    userBill.LastCost = double.Parse(worksheet.Cells[row, LAST_INDEX].Value.ToString());
                }

                if(userBill.MonthNumber > 1)
                {
                    userBill.LastCost = userBill.MonthNumber * userBill.LastCost;
                }

                listUserBill.Add(userBill);
            }

            return listUserBill;
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
            if (configM == null)
                configM = formulas.Where(x => x.PricesType == BillConfigPricesType.Rapport)
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

        private Tuple<List<BillPriceDto>, List<decimal>, BillPriceDto> CheckPriceConfigByApartment(
            List<BillConfig> formulas, long? buildingId, long? urbanId)
        {
            var levels = new List<BillPriceDto>();
            var percents = new List<decimal>();
            var priceM = new BillPriceDto();
            // Giá cố định

            var config = formulas.Where(x =>
                    x.PricesType == BillConfigPricesType.Normal && x.BuildingId == buildingId && x.IsPrivate == true)
                .FirstOrDefault();

            if (config == null)
                config = formulas.Where(x =>
                        x.PricesType == BillConfigPricesType.Normal && x.UrbanId == urbanId && x.IsPrivate == true)
                    .FirstOrDefault();

            if (config == null)
                config = formulas.Where(x => x.PricesType == BillConfigPricesType.Normal && x.IsPrivate != true)
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
            if (configM == null)
                configM = formulas.Where(x => x.PricesType == BillConfigPricesType.Rapport)
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

        private Tuple<List<BillPriceDto>, List<decimal>, BillPriceDto> CheckPriceConfigBill(
            List<BillConfig> formulas, long? buildingId, long? urbanId = null, List<string> codeList = null)
        {
            var levels = new List<BillPriceDto>();
            var percents = new List<decimal>();
            var priceM = new BillPriceDto();

            // Công thức hóa đơn điện nước
            var config = formulas
                .Where(x => x.PricesType == BillConfigPricesType.Level &&
                            ((x.BuildingId == buildingId && x.IsPrivate == true) ||
                             (x.UrbanId == urbanId && x.IsPrivate == true) ||
                             (x.IsPrivate != true)) &&
                            (codeList == null || codeList.Contains(x.Code)))
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
                .Where(x => x.PricesType == BillConfigPricesType.Percentage &&
                            ((x.BuildingId == buildingId && x.IsPrivate == true) ||
                             (x.UrbanId == urbanId && x.IsPrivate == true) ||
                             (x.IsPrivate != true)) &&
                            (codeList == null || codeList.Contains(x.Code)))
                .Select(x => x.Properties)
                .ToList();

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
            var configM = formulas
                .Where(x => x.PricesType == BillConfigPricesType.Rapport &&
                            ((x.BuildingId == buildingId && x.IsPrivate == true) ||
                             (x.UrbanId == urbanId && x.IsPrivate == true) ||
                             (x.IsPrivate != true)) &&
                            (codeList == null || codeList.Contains(x.Code)))
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

            //const int ECAR_NUMBER_INDEX = 10;
            //const int EMOTOR_NUMBER_INDEX = 11;
            //const int EBIKE_NUMBER_INDEX = 12;


            const int LAST_COST_INDEX = 10;
            const int BUILDING_INDEX = 11;
            const int URBAN_INDEX = 12;
            const int CODE_CONFIG_INDEX = 13;
            const int MONTH_NUMBER = 14;

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

                var citizens = _citizenTempRepo.GetAll().Select(x => new
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    ApartmentCode = x.ApartmentCode,
                    IsStayed = x.IsStayed,
                    RelationShip = x.RelationShip
                }).Where(x => x.ApartmentCode == apartmentCode).ToList();

                var codeListConfig = worksheet.Cells[row, CODE_CONFIG_INDEX].Text.ToString() != "" ? worksheet.Cells[row, CODE_CONFIG_INDEX].Value.ToString().Trim().Split(',')
                    .Select(code => code.Trim())
                    .ToList() : null;
                if (codeListConfig != null)
                {
                    formulas = _billConfigRepo.GetAll()
                        .Where(x => codeListConfig.Contains(x.Code))
                        .ToList();
                }

                BillConfig billConfig = null;

                if (formulas != null)
                {
                    billConfig = formulas.FirstOrDefault();
                }

                var properties = new BillProperites()
                {
                    customerName = customer,
                    formulas = formulas.Select(x => x.Id).ToArray(),
                    formulaDetails = formulas.ToArray(),
                    pricesType = 5,
                    vehicleFormulaDetail = billConfig
                    
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
                userBill.LastCost = worksheet.Cells[row, LAST_COST_INDEX].Text.ToString() != "" ? double.Parse(worksheet.Cells[row, LAST_COST_INDEX].Value.ToString()) : 0;
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
                //userBill.ECarNumber = worksheet.Cells[row, ECAR_NUMBER_INDEX].Text.ToString() != ""
                //   ? int.Parse(worksheet.Cells[row, ECAR_NUMBER_INDEX].Value.ToString())
                //   : 0;
                //userBill.EMotorNumber = worksheet.Cells[row, EMOTOR_NUMBER_INDEX].Text.ToString() != ""
                //    ? int.Parse(worksheet.Cells[row, EMOTOR_NUMBER_INDEX].Value.ToString())
                //    : 0;
                //userBill.EBikeNumber = worksheet.Cells[row, EBIKE_NUMBER_INDEX].Text.ToString() != ""
                //    ? int.Parse(worksheet.Cells[row, EBIKE_NUMBER_INDEX].Value.ToString())
                //    : 0;
                userBill.OtherVehicleNumber = worksheet.Cells[row, OTHER_NUMBER_INDEX].Text.ToString() != ""
                    ? int.Parse(worksheet.Cells[row, OTHER_NUMBER_INDEX].Value.ToString())
                    : 0;
                userBill.MonthNumber = worksheet.Cells[row, MONTH_NUMBER].Text.ToString() != ""
                   ? int.Parse(worksheet.Cells[row, MONTH_NUMBER].Value.ToString())
                   : 1;

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

                if (userBill.LastCost == 0 && billConfig != null)
                {
                    try
                    {
                        var billConfigProperties = JsonConvert.DeserializeObject<BillConfigPropertiesDto>(billConfig.Properties);
                        userBill.LastCost = (userBill.CarNumber ?? 0) * billConfigProperties.Prices[0].Value
                            + (userBill.MotorbikeNumber ?? 0) * billConfigProperties.Prices[1].Value
                             + (userBill.BicycleNumber ?? 0) * billConfigProperties.Prices[2].Value
                             //+ (userBill.ECarNumber ?? 0) * billConfigProperties.Prices[3].Value
                             //+ (userBill.EMotorNumber ?? 0) * billConfigProperties.Prices[4].Value
                             //+ (userBill.EBikeNumber ?? 0) * billConfigProperties.Prices[5].Value
                              + (userBill.OtherVehicleNumber ?? 0) * billConfigProperties.Prices[3].Value;
                    }
                    catch { }

                }
                if (userBill.MonthNumber > 1) userBill.LastCost = userBill.LastCost * userBill.MonthNumber;
                listUserBill.Add(userBill);
            }

            return listUserBill;
        }

        private List<UserBill> ExtractExcelUserBillParkingLevel(ExcelPackage package, List<BillConfig> formulas)
        {
            var userBills = new List<UserBill>();
            var vehicles = ExtractExcelVehicleParkingLevel(package);
            if (vehicles != null && vehicles.Count > 0)
            {
                var vehiclePeriod = vehicles.GroupBy(x => new { x.Period.Year, x.Period.Month, x.ApartmentCode }).Select(y => new
                {
                    Period = new DateTime(y.Key.Year, y.Key.Month, 1),
                    ApartmentCode = y.Key.ApartmentCode,
                    Items = y.ToList(),
                });

                foreach (var bill in vehiclePeriod)
                {
                    var userbill = new UserBill()
                    {
                        ApartmentCode = bill.ApartmentCode,
                        Period = bill.Period,
                        LastCost = 0,
                        UrbanId = bill.Items[0].UrbanId,
                        BuildingId = bill.Items[0].BuildingId,
                        DueDate = bill.Period.AddMonths(1),
                        BillType = BillType.Parking,
                        Status = bill.Period >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) ? UserBillStatus.Pending : UserBillStatus.Debt,
                        TenantId = AbpSession.TenantId,
                        Title = $"Hóa đơn xe tháng {bill.Period.Month}/{bill.Period.Year}",
                        MonthNumber = bill.Items[0].MonthNumber
                    };

                    var properties = new BillParkingProperties()
                    {
                        customerName = bill.Items[0].CustomerName,
                        pricesType = (int)BillConfigPricesType.ParkingLevel,
                        vehicles = new List<VehicleProperties>()
                    };

                    foreach (var vd in bill.Items)
                    {
                        var vehicle = new VehicleProperties()
                        {
                            apartmentCode = bill.ApartmentCode,
                            cardNumber = vd.CardNumber,
                            cost = (decimal)vd.LastCost,
                            level = vd.Level,
                            ownerName = vd.CustomerName,
                            parkingId = vd.ParkingId ?? 0,
                            vehicleCode = vd.VehicleCode,
                            vehicleName = vd.VehicleName,
                            vehicleType = (int)vd.VehicleType
                        };
                        properties.vehicles.Add(vehicle);
                        userbill.LastCost += vd.LastCost;

                    }

                    if (userbill.MonthNumber > 1) userbill.LastCost = userbill.LastCost * userbill.MonthNumber;
                    userbill.Properties = JsonConvert.SerializeObject(properties);

                    userBills.Add(userbill);
                }
            }

            return userBills;
        }

        private List<ExcelParkingLevelDto> ExtractExcelVehicleParkingLevel(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.First();
            var citizenParkings = _citizenParkingRepository.GetAllList();

            var rowCount = worksheet.Dimension.End.Row;
            var colCount = worksheet.Dimension.End.Column;

            const int URBAN_INDEX = 1;
            const int BUILDING_INDEX = 2;
            const int APARTMENT_CODE_INDEX = 3;
            const int CUSTOMER_NAME_INDEX = 4;
            const int CARD_NUMBER = 5;

            const int VEHICLE_TYPE = 6;
            const int VEHICLE_LEVEL = 7;
            const int VEHICLE_PRICE = 8;
            const int VEHICLE_CODE = 9;
            const int VEHICLE_NAME = 10;
            const int PARKING_CODE = 11;
            const int DESCRIPTION = 12;
            const int PERIOD = 13;
            const int MONTH_NUMBER = 14;

            var listVehicles = new List<ExcelParkingLevelDto>();

            var countApartmentNull = 0;

            for (var row = 2; row <= rowCount; row++)
            {
                var vehicle = new ExcelParkingLevelDto();

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

                vehicle.ApartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString();
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
                    if (building != null) vehicle.BuildingId = building.ParentId;
                }

                if (urbanCode != null)
                {
                    var urban = _organizationUnitRepository.FirstOrDefault(x =>
                        x.ProjectCode == urbanCode && x.Type == APP_ORGANIZATION_TYPE.URBAN);
                    if (urban != null) vehicle.UrbanId = urban.ParentId;
                }

                vehicle.LastCost = worksheet.Cells[row, VEHICLE_PRICE].Value != null ? double.Parse(worksheet.Cells[row, VEHICLE_PRICE].Value.ToString()) : 0;
                vehicle.Period = worksheet.Cells[row, PERIOD].Value != null ? DateTime.Parse(worksheet.Cells[row, PERIOD].Value.ToString()) : DateTime.Now;

                vehicle.CardNumber = worksheet.Cells[row, CARD_NUMBER].Text.ToString();
                vehicle.VehicleCode = worksheet.Cells[row, VEHICLE_CODE].Text.ToString();
                vehicle.VehicleName = worksheet.Cells[row, VEHICLE_NAME].Text.ToString();
                vehicle.Description = worksheet.Cells[row, DESCRIPTION].Text.ToString();
                vehicle.Level = worksheet.Cells[row, VEHICLE_LEVEL].Text.ToString() != ""
                    ? int.Parse(worksheet.Cells[row, VEHICLE_LEVEL].Value.ToString().Trim())
                    : 1;
                vehicle.MonthNumber = worksheet.Cells[row, MONTH_NUMBER].Text.ToString() != ""
                   ? int.Parse(worksheet.Cells[row, MONTH_NUMBER].Value.ToString().Trim())
                   : 1;
                if (worksheet.Cells[row, VEHICLE_TYPE].Value != null)
                    vehicle.VehicleType = GetVehicleTypeNumber(worksheet.Cells[row, VEHICLE_TYPE].Value.ToString().Trim());

                if (worksheet.Cells[row, PARKING_CODE].Value != null)
                {
                    var parkingIDstr = worksheet.Cells[row, PARKING_CODE].Value.ToString().Trim();
                    vehicle.ParkingId = (citizenParkings.FirstOrDefault(x => x.ParkingCode == parkingIDstr))?.Id;
                }
                listVehicles.Add(vehicle);
            }

            return listVehicles;
        }

        private async Task CreateListUserBillAsync(List<UserBill> userBills, ParkingBillType? parkingBillType)
        {
            try
            {
                if (userBills == null || userBills.Count() == 0) return;
                

                foreach (UserBill userBill in userBills)
                {
                    if (userBill.Id > 0) await _userBillRepo.UpdateAsync(userBill);
                    else
                    {
                        var id = await _userBillRepo.InsertAndGetIdAsync(userBill);
                        userBill.Code = "HD" + userBill.Id +
                                        (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month) + "" +
                                        DateTime.Now.Year;
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();

                    if (userBill.BillType == BillType.Parking && parkingBillType == ParkingBillType.ParkingLevel)
                    {
                        await CreateBillVehicleInfos(userBill);
                    }
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
            catch (Exception ex)
            {
                //var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> ExportBillsExcel(DownloadBillsExcelInputDto input)
        {
            try
            {
                var bills = await this._userBillRepo.GetAll()
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
                var bills = await this._userBillRepo.GetAll()
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


        [Obsolete]
        public async Task<object> CreateOrUpdateMonthlyUserBill(CreateOrUpdateMonthlyInvoice input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                //var userBill = input.MapTo<UserBill>();

                if (input.DeleteList.Count > 0)
                {
                    await _userBillRepo.DeleteAsync(x => input.DeleteList.Contains(x.Id));
                }

                int billDetailCount = 0;

                bool isCreate = false;
                bool isUpdate = false;

                foreach (CreateOrUpdateUserBillInputDto detail in input.BillDetail)
                {
                    var userBill1 = detail.MapTo<UserBill>();
                    userBill1.Title = input.Title;
                    userBill1.UrbanId = input.UrbanId ?? null;
                    userBill1.BuildingId = input.BuildingId ?? null;
                    userBill1.ApartmentCode = input.ApartmentCode;
                    userBill1.Period = input.Period;
                    userBill1.DueDate = input.DueDate;
                    userBill1.TenantId = AbpSession.TenantId;
                    userBill1.CitizenTempId = input.CitizenTempId;
                    userBill1.IndexEndPeriod = detail.IndexEndPeriod;
                    userBill1.IndexHeadPeriod = detail.IndexHeadPeriod;
                    userBill1.TotalIndex = detail.TotalIndex;
                    userBill1.BillConfigId = detail.BillConfigId ?? 0;
                    userBill1.MonthNumber = detail.MonthNumber ?? 1;
                    if (userBill1.BillType == BillType.Parking)
                    {
                        userBill1.CarNumber = detail.CarNumber;
                        userBill1.MotorbikeNumber = detail.MotorbikeNumber;
                        userBill1.BicycleNumber = detail.BicycleNumber;
                        userBill1.OtherVehicleNumber = detail.OtherVehicleNumber;
                        //userBill1.ECarNumber = detail.ECarNumber;
                        //userBill1.EMotorNumber = detail.EMotorNumber;
                        //userBill1.EBikeNumber = detail.EBikeNumber;
                    }

                    // userBill1.OrganizationUnitId = _smartHomeRepo.GetAll().Where(x => x.ApartmentCode == input.ApartmentCode).Select(x => x.OrganizationUnitId).FirstOrDefault();
                    if (userBill1.Id > 0)
                    {
                        isUpdate = true;
                        await _userBillRepo.UpdateAsync(userBill1);
                    }
                    else
                    {
                        isCreate = true;
                        if (userBill1.DueDate < DateTime.Now) userBill1.Status = UserBillStatus.Debt;
                        // var userBill =  await _userBillRepo.InsertAsync(userBill1);
                        var id = await _userBillRepo.InsertAndGetIdAsync(userBill1);
                        userBill1.Code = "HD" + userBill1.Id +
                                         (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month) +
                                         "" + DateTime.Now.Year;
                        if (userBill1.BillType == BillType.Parking)
                        {
                           await  CreateBillVehicleInfos(userBill1);
                        }
                    }
                }

                var message = "";
                if (isCreate && !isUpdate) message = "Create success!";
                else if (isUpdate) message = "Update success!";
                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(message);
            }
            catch (Exception ex)
            {
                //var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        protected async Task CreateBillVehicleInfos(UserBill bill)
        {
            try
            {
                 string listVehiclesString = JsonConvert.DeserializeObject<dynamic>(bill.Properties)?.vehicles?.ToString() ?? null;
                if (listVehiclesString != null)
                {
                    var vehicles = JsonConvert.DeserializeObject<List<CitizenVehiclePas>>(listVehiclesString);
                    foreach (var vehicle in vehicles)
                    {
                        var vh = new UserBillVehicleInfo()
                        {
                            CitizenVehicleId = vehicle.id,
                            Cost = (double)vehicle.cost,
                            ParkingId = vehicle.parkingId,
                            Period = bill.Period.Value,
                            TenantId = bill.TenantId,
                            UserBillId = bill.Id,
                            VehicleType = vehicle.vehicleType

                        };
                        await _billVehicleInfoRepository.InsertAsync(vh);
                    }
                }


            }catch { }
        }

        protected async Task<IEnumerable<KeyValuePair<string, List<UserBill>>>> QueryUserBillByMonth(GetAllBillsByMonthDto input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                int month = input.Period.HasValue ? input.Period.Value.Month : 0;
                int year = input.Period.HasValue ? input.Period.Value.Year : 0;

                var listq = _userBillRepo.GetAll()
                    .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                    .WhereIf(input.Period.HasValue, x => x.Period.Value.Month == month && x.Period.Value.Year == year)
                    .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode.Contains(input.ApartmentCode))
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                    .WhereIf(input.BillType.HasValue, x => x.BillType == input.BillType.Value)
                    .WhereIf(input.UrbanId.HasValue,
                        x => x.UrbanId.HasValue && x.UrbanId.Value == input.UrbanId.Value)
                    .WhereIf(input.BuildingId.HasValue,
                        x => x.BuildingId.HasValue && x.BuildingId.Value == input.BuildingId.Value)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x =>
                        (!string.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(input.Keyword.ToLower())) ||
                        (!string.IsNullOrEmpty(x.ApartmentCode) && x.ApartmentCode.Contains(input.Keyword)) ||
                        (!string.IsNullOrEmpty(x.Properties) && x.Properties.Contains(input.Keyword)))

                    .ApplySort(input.OrderBy, input.SortBy).ApplySort(OrderByBillByMonth.PERIOD, CommonENum.SortBy.DESC).AsQueryable()
                    .AsEnumerable()
                    .GroupBy(x => new
                    { x.Period.Value.Month, x.Period.Value.Year, x.ApartmentCode, x.BuildingId, x.UrbanId })
                    .Select(x => new
                    {
                        Key = string.Format("{0}|{1} , {2} , {3} , {4}",
                            x.Key.Year,
                            x.Key.Month < 10 ? "0" + x.Key.Month : x.Key.Month,
                            x.Key.ApartmentCode,
                            x.Key.BuildingId.HasValue ? x.Key.BuildingId.Value : "",
                            x.Key.UrbanId.HasValue ? x.Key.UrbanId.Value : ""),
                        BillDetail = x.ToList(),
                    })
                    .ToDictionary(x => x.Key, y => y.BillDetail);

                return listq;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<object> GetUserBillsByMonth(GetAllBillsByMonthDto input)
        {
            try
            {
                var query = await QueryUserBillByMonth(input);
                //.ApplySearchFilter(input.Keyword, x => x.DisplayName)

                //.Where(x => ouUI.Contains((long)(x.Value.FirstOrDefault()?.OrganizationUnitId)));

                var result = query
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToList();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        if (item.Value != null)
                        {
                            foreach (var bill in item.Value)
                            {
                                if (bill.IsPaymentPending == true) bill.Status = UserBillStatus.WaitForConfirm;
                            }
                        }
                    }
                }

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> ExportMonthlyInvoiceToExcel(GetAllBillsByMonthDto input)
        {
            try
            {
                var query = await QueryUserBillByMonth(input);
                var bills = query.ToList();

                var result = _billExcelExporter.ExportMonthlyToFile(bills);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> ExportAllDetailUserBillToExcel(GetAllBillsByMonthDto input)
        {
            try
            {
                var query = await QueryUserBillByMonth(input);
                var bills = query.ToList();

                var result = _billExcelExporter.ExportMonthlyToFile(bills);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        #region Common


        protected VehicleType GetVehicleTypeNumber(string type)
        {
            if (type.ToLower().Contains("electric car")
                || type.ToLower().Contains("ô tô điện")
                || type.ToLower().Contains("전기차".ToLower())) return VehicleType.ElectricCar;
            if (type.ToLower().Contains("electric motorcycle")
                || type.ToLower().Contains("electric motorbike")
                || type.ToLower().Contains("xe máy điện")
                || type.ToLower().Contains("전기 오토바이".ToLower())) return VehicleType.ElectricMotor;
            if (type.ToLower().Contains("Electric Bicycle")
                || type.ToLower().Contains("electric bike")
                || type.ToLower().Contains("xe đạp điện")
                || type.ToLower().Contains("전기 자전거".ToLower())) return VehicleType.ElectricBike;
            if (type.ToLower().Contains("car")
                || type.ToLower().Contains("ô tô")
                || type.ToLower().Contains("자동차".ToLower())) return VehicleType.Car;
            if (type.ToLower().Contains("motorbike")
                || type.ToLower().Contains("motorcycle")
                || type.ToLower().Contains("xe máy")
                || type.ToLower().Contains("오토바이".ToLower())) return VehicleType.Motorbike;
            if (type.ToLower().Contains("Bicycle")
                || type.ToLower().Contains("bike")
                || type.ToLower().Contains("xe đạp")
                || type.ToLower().Contains("자전거".ToLower())) return VehicleType.Bicycle;
            
            return VehicleType.Other;
        }


        private double CalculateDependOnLevel(BillPriceDto[] levels, List<decimal> percents, decimal amount)
        {
            string[] keys = { "start" };
            if (levels == null || levels.Length == 0)
            {
                return 0;
            }
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
                    var from = level.From;
                    if (i != 0) from = level.From - 1;
                    if (amount < level.To)
                    {
                        result += level.Value * (amount - from);
                        break;
                    }

                    if (i == levelIndex)
                    {
                        result += level.Value * (amount - from);
                        break;
                    }

                    result += level.Value * (level.To.Value - from);
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

        #endregion


        #region Bill by Apartment

        public async Task<object> GetUserBillByApartment(string apartmentCode, DateTime period)
        {
            try
            {
                var userBills = _userBillRepo.GetAll().Where(x =>
                    x.ApartmentCode == apartmentCode && x.Period.Value.Month == period.Month &&
                    x.Period.Value.Year == period.Year).ToList();

                var data = DataResult.ResultSuccess(userBills, "Get success");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllApartmentBills(GetAllBillsByMonthDto input)
        {
            try
            {
                var query = QueryUserBillByApartment(input);

                var result = query
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToList();

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        protected IEnumerable<KeyValuePair<string, List<ApartmentBillGetAllDto>>> QueryUserBillByApartment(
            GetAllBillsByMonthDto input)
        {
            try
            {
                var month = input.Period.HasValue ? input.Period.Value.Month : 0;
                var year = input.Period.HasValue ? input.Period.Value.Year : 0;

                var listq = _userBillRepo.GetAll().Select(x =>
                        new ApartmentBillGetAllDto()
                        {
                            Title = x.Title,
                            ApartmentCode = x.ApartmentCode,
                            Properties = x.Properties,
                            BuildingId = x.BuildingId,
                            Period = x.Period.Value,
                            DueDate = x.DueDate.Value,
                            Status = x.Status,
                            UrbanId = x.UrbanId,
                            BillType = x.BillType,
                            LastCost = x.LastCost ?? 0
                        })
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                    .WhereIf(input.Period.HasValue, x => x.Period.Month == month && x.Period.Year == year)
                    .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode.Contains(input.ApartmentCode))
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                    .ApplySearchFilter(input.Keyword, x => x.ApartmentCode)
                    .OrderByDescending(y => y.Period).AsQueryable().AsEnumerable()
                    .GroupBy(x => new { x.Period.Month, x.Period.Year, x.ApartmentCode, x.BuildingId, x.UrbanId })
                    .Select(x => new
                    {
                        Key = string.Format("{0}|{1} , {2} , {3} , {4}",
                            x.Key.Year,
                            x.Key.Month < 10 ? "0" + x.Key.Month : x.Key.Month,
                            x.Key.ApartmentCode,
                            x.Key.BuildingId.HasValue ? x.Key.BuildingId.Value : "",
                            x.Key.UrbanId.HasValue ? x.Key.UrbanId.Value : ""),
                        BillDetail = x.ToList(),
                    })
                    .ToDictionary(x => x.Key, y => y.BillDetail);


                return listq;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion



        #region Tạo hoá đơn của đồng hồ công tơ
        public async Task<object> CreateBillMeterMonthly(List<CreateBillMeterMonthlyInput> inputs)
        {

            var levels = new List<BillPriceDto>();
            var percents = new List<decimal>();
            var priceM = new BillPriceDto();
            try
            {
                var listUserBill = new List<UserBill>();
                foreach (var input in inputs)
                {
                    input.TenantId = AbpSession.TenantId;
                    var userBill = new UserBill();

                    var result = (from sm in _meterMonthlyRepository.GetAll()
                                  join meter in _meterRepository.GetAll() on sm.MeterId equals meter.Id into tb_mt
                                  from meter in tb_mt.DefaultIfEmpty()
                                  where sm.MeterId == input.MeterId
                                  select new MeterMonthlyDto
                                  {
                                      BuildingId = meter.BuildingId,
                                      UrbanId = meter.UrbanId,
                                      ApartmentCode = meter.ApartmentCode,
                                      BillType = _meterTypeRepository.GetAll().Where(m => m.Id == meter.MeterTypeId)
                                                             .Select(b => b.BillType).FirstOrDefault(),


                                  }).FirstOrDefault();


                    var buildingId = result?.BuildingId;
                    var urbanId = result?.UrbanId;
                    var apartmentCode = result?.ApartmentCode;
                    var billType = result?.BillType;

                    var apartment = await _apartmentRepository.FirstOrDefaultAsync(x => x.ApartmentCode == apartmentCode && x.UrbanId == urbanId && x.BuildingId == buildingId);
                    if (apartment != null)
                    {
                        var properties = new BillProperites();
                        userBill.Properties = JsonConvert.SerializeObject(properties);
                        userBill.IndexEndPeriod = input.Value;
                        userBill.TenantId = AbpSession.TenantId;
                        userBill.ApartmentCode = apartmentCode;
                        userBill.UrbanId = urbanId;
                        userBill.BuildingId = buildingId;
                        userBill.Title = $"Hoá đơn tháng {input.Period.Month}/{input.Period.Year}";
                        userBill.Status = UserBillStatus.Pending;
                        userBill.Period = input.Period;
                        userBill.BillType = (BillType)billType;
                        userBill.CreatorUserId = AbpSession.UserId;


                        if (input.Period != null)
                        {
                            int year = userBill.Period.Value.Year;
                            int month = userBill.Period.Value.Month;
                            int lastDayOfMonth = DateTime.DaysInMonth(year, month);

                            // Đặt DueDate thành ngày cuối cùng của tháng
                            userBill.DueDate = new DateTime(year, month, lastDayOfMonth);
                            var pre_period = input.Period.AddMonths(-1);
                            var pre_bill = _userBillRepo.FirstOrDefault(x =>
                                    x.ApartmentCode == apartmentCode && x.Period.Value.Year == pre_period.Year &&
                                    x.Period.Value.Month == pre_period.Month);

                            userBill.IndexHeadPeriod = userBill.IndexHeadPeriod > 0 ? userBill.IndexHeadPeriod : (pre_bill?.IndexEndPeriod ?? 0);
                        }
                        else userBill.DueDate = DateTime.Now;

                        var listBillConfig = apartment.BillConfig;    
                        if (listBillConfig == null)
                        {

                            var listBillConfigByBuilding = _billConfigRepo.GetAll().Where(x => x.IsDefault == true && x.BillType == billType).ToList();
                            if (listBillConfigByBuilding != null)
                            {
                                var formulaBuildings0 = CheckPriceConfigByBuilding(listBillConfigByBuilding, userBill.BuildingId, userBill.UrbanId);
                                var propertiesList = listBillConfigByBuilding
                                    .Select(config => config.Properties)
                                    .ToList();
                                levels = formulaBuildings0.Item1;
                                percents = formulaBuildings0.Item2;
                                priceM = formulaBuildings0.Item3;

                                userBill.IndexHeadPeriod = input.FirstValue ?? 0;
                                userBill.TotalIndex = userBill.IndexEndPeriod - userBill.IndexHeadPeriod;
                                //không phải theo level
                                if (levels.Count == 0)
                                {

                                    foreach (var m in listBillConfigByBuilding)
                                    {
                                        var propertiesm = JsonConvert.DeserializeObject<BillPropertiesDto>(m.Properties);
                                        var prices = propertiesm?.Prices;
                                        if (prices != null)
                                        {
                                            var listCitizens = _citizenTempRepo.GetAll().Select(x => new
                                            {
                                                Id = x.Id,
                                                FullName = x.FullName,
                                                ApartmentCode = x.ApartmentCode,
                                                IsStayed = x.IsStayed,
                                                RelationShip = x.RelationShip
                                            }).Where(x => x.ApartmentCode == apartmentCode && x.IsStayed == true).ToList();
                                            userBill.LastCost = CalculateDependOnNotLevel(percents, priceM, (decimal)userBill.TotalIndex, listCitizens.Count());

                                        }
                                    }



                                }
                                else if (billType.Equals(BillType.Electric) || billType.Equals(BillType.Water))
                                {

                                    userBill.LastCost =
                                        CalculateDependOnLevel(levels.ToArray(), percents, (decimal)userBill.TotalIndex);
                                }
                                else if (billType.Equals(BillType.Manager))
                                {
                                    if (priceM != null && priceM.Value > 0)
                                    {
                                        //nếu có giá trị thì nhân giá
                                        if (userBill.TotalIndex > 0)
                                        {
                                            userBill.LastCost = (double)(priceM.Value * userBill.TotalIndex);

                                        }
                                        //nếu không có giá ==> tính theo diện tích
                                        else
                                        {
                                            userBill.TotalIndex = apartment != null ? apartment.Area ?? 0 : 0;
                                            userBill.LastCost = (double)(priceM.Value * userBill.TotalIndex);
                                        }
                                    }
                                }
                                else
                                {
                                    userBill.LastCost = ((double?)(userBill.IndexEndPeriod - userBill.IndexHeadPeriod));
                                }
                            }
                        }
                        else
                        {
                            foreach (var i in listBillConfig)
                            {
                                var listBillId = _billConfigRepo.GetAll().Where(x => x.Id == i).ToList();

                                if (billType == BillType.Electric || billType == BillType.Water)
                                {
                                    var checkApartment = CheckPriceConfigByApartment(listBillId, userBill.BuildingId, userBill.UrbanId);
                                    levels = checkApartment.Item1;
                                    percents = checkApartment.Item2;
                                    priceM = checkApartment.Item3;
                                    if (levels.Count == 0)
                                    {
                                        userBill.IndexHeadPeriod = input.FirstValue ?? 0;
                                        userBill.TotalIndex = userBill.IndexEndPeriod - userBill.IndexHeadPeriod;
                                        foreach (var m in listBillId)
                                        {
                                            var propertiesm = JsonConvert.DeserializeObject<BillPropertiesDto>(m.Properties);
                                            var prices = propertiesm?.Prices;
                                            if (prices != null)
                                            {

                                                var totalLastCost = userBill.TotalIndex * prices[0].Value;
                                                userBill.LastCost = (userBill.LastCost ?? 0) + (double?)totalLastCost;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var formulaBuildings =
                                       CheckPriceConfigByBuilding(listBillId, userBill.BuildingId, userBill.UrbanId);

                                        levels = formulaBuildings.Item1;
                                        percents = formulaBuildings.Item2;
                                        priceM = formulaBuildings.Item3;


                                        userBill.TotalIndex = userBill.IndexEndPeriod - userBill.IndexHeadPeriod;
                                        userBill.LastCost =
                                            CalculateDependOnLevel(levels.ToArray(), percents, (decimal)userBill.TotalIndex);


                                    }


                                }

                                else if (billType == BillType.Manager)
                                {

                                    userBill.TotalIndex = userBill.IndexEndPeriod;
                                    if (priceM != null && priceM.Value > 0)
                                    {
                                        if (userBill.TotalIndex > 0)
                                        {
                                            userBill.LastCost = (double)(priceM.Value * userBill.TotalIndex);

                                        }
                                        else
                                        {
                                            userBill.TotalIndex = apartment.Area ?? 0;
                                            userBill.LastCost = (double)(priceM.Value * userBill.TotalIndex);
                                        }
                                    }



                                }
                                else
                                {
                                    var checkApartment = CheckPriceConfigByApartment(listBillId, userBill.BuildingId, userBill.UrbanId);
                                    levels = checkApartment.Item1;
                                    percents = checkApartment.Item2;
                                    priceM = checkApartment.Item3;
                                    if (levels.Count == 0)
                                    {
                                        userBill.IndexHeadPeriod = input.FirstValue ?? 0;
                                        userBill.TotalIndex = userBill.IndexEndPeriod - userBill.IndexHeadPeriod;
                                        foreach (var m in listBillId)
                                        {
                                            var propertiesm = JsonConvert.DeserializeObject<BillPropertiesDto>(m.Properties);
                                            var prices = propertiesm?.Prices;
                                            if (prices != null)
                                            {

                                                var totalLastCost = userBill.TotalIndex * prices[0].Value;
                                                userBill.LastCost = (userBill.LastCost ?? 0) + (double?)totalLastCost;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var formulaBuildings =
                                       CheckPriceConfigByBuilding(listBillId, userBill.BuildingId, userBill.UrbanId);

                                        levels = formulaBuildings.Item1;
                                        percents = formulaBuildings.Item2;
                                        priceM = formulaBuildings.Item3;


                                        userBill.TotalIndex = userBill.IndexEndPeriod - userBill.IndexHeadPeriod;
                                        userBill.LastCost =
                                            CalculateDependOnLevel(levels.ToArray(), percents, (decimal)userBill.TotalIndex);


                                    }

                                }
                                userBill.BillConfigId = i;
                            }
                        }
                        var citizens = _citizenTempRepo.GetAll().Select(x => new
                        {
                            Id = x.Id,
                            FullName = x.FullName,
                            ApartmentCode = x.ApartmentCode,
                            IsStayed = x.IsStayed,
                            RelationShip = x.RelationShip
                        }).Where(x => x.ApartmentCode == apartmentCode).ToList();
                        if (citizens.Any())
                        {
                            var citizenDefault =
                                citizens.FirstOrDefault(x => x.IsStayed && x.RelationShip == RELATIONSHIP.Contractor);
                            if (citizenDefault == null)
                                citizenDefault = citizens.FirstOrDefault(x => x.RelationShip == RELATIONSHIP.Contractor);
                            if (citizenDefault == null) citizenDefault = citizens.FirstOrDefault();

                            userBill.CitizenTempId = citizenDefault?.Id ?? 0;

                            properties.customerName = citizenDefault?.FullName ?? "";
                            //properties.formulas = billConfigs.Select(x => x.Id).ToArray();
                            //properties.formulaDetails = billConfigs.ToArray();


                        }
                        listUserBill.Add(userBill);

                        await CreateListUserBillAsync(listUserBill, null);
                    }



                }
                return DataResult.ResultSuccess(true, "Insert success!");
            }

            catch (Exception ex)
            {
                //var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }


        #endregion

        #region Không theo level
        private double CalculateDependOnNotLevel(List<decimal> percents, BillPriceDto priceM, decimal amount, int countCitizen)
        {
            if (percents.Count == 0 || priceM == null)
            {
                return 0;
            }

            decimal result = 0;

            if (percents != null && percents.Count() > 0)
            {
                decimal pctotal = 0;
                foreach (var pc in percents)
                {
                    pctotal = pctotal + result * (pc / 100);
                }

                result = pctotal + result;
            }
            if (priceM != null)
            {
                result = countCitizen * priceM.Value;
            }

            return (double)(int)result;
        }
        #endregion
    }
}