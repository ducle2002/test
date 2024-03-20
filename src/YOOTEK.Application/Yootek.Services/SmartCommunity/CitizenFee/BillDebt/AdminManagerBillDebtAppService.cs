using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Yootek.Services.Dto;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Yootek.Application;
using Abp.Linq.Extensions;


namespace Yootek.Services
{
    public interface IAdminManagerBillDebtAppService : IApplicationService
    {
        Task<object> GetAllBillDebtAsync(GetAllBillDebtInputDto input);
        Task<DataResult> CreateOrUpdateBillDebtAsync(BillDebtDto input);
        Task<DataResult> DeleteBillDebt(long id);
        Task<DataResult> DeleteMulti([FromBody] List<long> ids);
        Task<DataResult> DeleteMultiNC([FromBody] List<long> ids);
        Task<object> VerifyListPaymentBillDebt(List<VerifyPaymentBillDebtInput> input);
        Task<object> VerifyPaymentBillDebt(VerifyPaymentBillDebtInput input);
        Task<object> ImportExcelBillDebt([FromForm] ImportExcelBillDebtInput input);
    }

    //  [AbpAuthorize(PermissionNames.Pages_SmartCommunity_Fees)]
    [AbpAuthorize]
    public class AdminManagerBillDebtAppService : YootekAppServiceBase, IAdminManagerBillDebtAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepo;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepo;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<BillDebt, long> _billDebtRepo;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepo;

        public AdminManagerBillDebtAppService(
            IRepository<UserBill, long> userBillRepo,
            IRepository<UserBillPayment, long> userBillPaymentRepo,
            IRepository<User, long> userRepos,
            IRepository<CitizenTemp, long> citizenTempRepo,
            IAppNotifier appNotifier,
            IRepository<BillDebt, long> billDebtRepo,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepo
            )
        {
            _userBillRepo = userBillRepo;
            _userBillPaymentRepo = userBillPaymentRepo;
            _userRepos = userRepos;
            _appNotifier = appNotifier;
            _billDebtRepo = billDebtRepo;
            _citizenTempRepo = citizenTempRepo;
            _appOrganizationUnitRepo = appOrganizationUnitRepo;

        }

        private async Task<IQueryable<UserBillDebtDto>> QueryBillDebtMonthly(GetAllBillDebtInputDto input)
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = from bd in _billDebtRepo.GetAll()
                        join cz in _citizenTempRepo.GetAll() on bd.CitizenTempId equals cz.Id into tb_cz
                        from cz in tb_cz.DefaultIfEmpty()
                        join pm in _userBillPaymentRepo.GetAll() on bd.BillPaymentId equals pm.Id into tb_pm
                        from pm in tb_pm.DefaultIfEmpty()
                        select new UserBillDebtDto()
                        {
                            Id = bd.Id,
                            Title = bd.Title,
                            UserId = bd.UserId,
                            CitizenTempId = bd.CitizenTempId,
                            ApartmentCode = bd.ApartmentCode,
                            DebtTotal = bd.DebtTotal,
                            State = bd.State,
                            BillPaymentId = bd.BillPaymentId,
                            TenantId = bd.TenantId,
                            Period = bd.Period,
                            UserBillIds = bd.UserBillIds,
                            OrganizationUnitId = bd.OrganizationUnitId,
                            CreationTime = bd.CreationTime,
                            CitizenName = cz.FullName,
                            CitizenPhone = cz.PhoneNumber,
                            PaidAmount = bd.PaidAmount,
                            BillPayment = pm,
                            BuildingId = bd.BuildingId,
                            UrbanId = bd.UrbanId,
                        };
            query = query
                .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                .WhereIf(input.Period.HasValue, x => x.Period.Month == input.Period.Value.Month && x.Period.Year == input.Period.Value.Year)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => (x.CitizenName != null && x.CitizenName.ToLower().Contains(input.Keyword.ToLower())) || x.ApartmentCode.ToLower().Contains(input.Keyword.ToLower()))
                .ApplySearchFilter(input.Keyword, x => x.ApartmentCode)
                .OrderByDescending(x => x.CreationTime).AsQueryable();
            return query;
        }

        protected Task<List<UserBill>> SplitBills(string input)
        {
            var billIds = input.Split(",").Select(x => Convert.ToInt64(x)).ToArray();
            return _userBillRepo.GetAll().Where(x => billIds.Contains(x.Id)).ToListAsync();
        }

        public async Task<object> GetAllBillDebtAsync(GetAllBillDebtInputDto input)
        {
            try
            {
                var query = await QueryBillDebtMonthly(input);

                if (input.OrganizationUnitId.HasValue)
                {
                    var organizationIds = await _appOrganizationUnitRepo.GetAll().Where(x => x.ParentId == input.OrganizationUnitId).Select(x => x.Id).ToListAsync();
                    query = query.Where(x => x.OrganizationUnitId == input.OrganizationUnitId || (organizationIds.Count() > 0 && organizationIds.Contains(x.OrganizationUnitId.Value)));

                }

                var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

                foreach (var bill in result)
                {
                    if (!bill.UserBillIds.IsNullOrWhiteSpace())
                    {
                        bill.BillList = await SplitBills(bill.UserBillIds);
                        if (string.IsNullOrEmpty(bill.Title)) bill.Title = $"Công nợ tháng {bill.Period.Month}/{bill.Period.Year}";
                    }
                }

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> CreateOrUpdateBillDebtAsync(BillDebtDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _billDebtRepo.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _billDebtRepo.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_billdebt");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<BillDebt>();
                    await _billDebtRepo.InsertAsync(insertInput);
                    mb.statisticMetris(t1, 0, "is_billdebt");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> DeleteBillDebt(long id)
        {
            try
            {
                await _billDebtRepo.DeleteAsync(id);
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

        public async Task<DataResult> DeleteMulti([FromBody] List<long> ids)
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

        public async Task<DataResult> DeleteMultiNC([FromBody] List<long> ids)
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

        public async Task<object> VerifyListPaymentBillDebt(List<VerifyPaymentBillDebtInput> input)
        {
            try
            {
                if (input == null || input.Count() == 0) throw new Exception("Input payment null");

                foreach (var pm in input)
                {
                    await VerifyPaymentBillDebt(pm);
                }

                var data = DataResult.ResultSuccess("Admin payment success");
                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> VerifyPaymentBillDebt(VerifyPaymentBillDebtInput input)
        {
            try
            {
                var billDebt = await _billDebtRepo.FirstOrDefaultAsync(input.BillDebtId);
                if (billDebt == null) throw new Exception("Bill debt not found !");

                var checkDebt = false;


                //Kiểm tra số tiền thanh toán. Chỉ cho phép thanh toán số tiền bằng hoặc nhỏ hơn tổng hóa đơn
                if (input.Amount > 0 && (int)input.Amount < (int)billDebt.DebtTotal) checkDebt = true;

                // Tạo thanh toán thành công
                var payment = new UserBillPayment()
                {
                    Amount = checkDebt ? input.Amount : billDebt.DebtTotal,
                    ApartmentCode = billDebt.ApartmentCode,
                    UserBillIds = billDebt.UserBillIds,
                    BillDebtIds = billDebt.Id + "",
                    Method = UserBillPaymentMethod.AdminVerify,
                    Status = UserBillPaymentStatus.Success,
                    TypePayment = TypePayment.DebtBill,
                    Period = billDebt.Period,
                    Title = "Thanh toán hóa đơn tháng " + billDebt.Period.ToString("MM/yyyyy"),
                    TenantId = AbpSession.TenantId,
                    Description = input.Description,
                    BuildingId = billDebt.BuildingId,
                    UrbanId = billDebt.UrbanId

                };

                await _userBillPaymentRepo.InsertAndGetIdAsync(payment);
                billDebt.PaidAmount = billDebt.PaidAmount + input.Amount;
                billDebt.BillPaymentId = payment.Id;
                // Kiểm tra và tạo công nợ
                if (checkDebt)
                {
                    billDebt.DebtTotal = billDebt.DebtTotal - input.Amount;


                }
                else
                {
                    billDebt.DebtTotal = 0;
                    billDebt.State = DebtState.PAIED;

                    try
                    {
                        var billIds = billDebt.UserBillIds.Split(",").Select(x => Convert.ToInt64(x)).ToList();
                        var userbills = await _userBillRepo.GetAllListAsync(x => billIds.Contains(x.Id));
                        foreach (var userbill in userbills) userbill.Status = UserBillStatus.Paid;

                    }
                    catch { }
                }
                await _billDebtRepo.UpdateAsync(billDebt);
                await CurrentUnitOfWork.SaveChangesAsync();

                var data = DataResult.ResultSuccess("Admin payment success");
                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> ImportExcelBillDebt([FromForm] ImportExcelBillDebtInput input)
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

            try
            {
                var package = new ExcelPackage(stream);

                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);
                var listUserBills = new List<UserBillDebtDto>();

                if (AbpSession.TenantId == 43)
                {
                    listUserBills = ExtractExcelBillDebt(package, AbpSession.TenantId);
                }

                await CreateListBillDebtAsync(listUserBills);
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

        private List<UserBillDebtDto> ExtractExcelBillDebt(ExcelPackage package, int? tenantId)
        {
            var buildings = _appOrganizationUnitRepo.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
            var urbans = _appOrganizationUnitRepo.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);
            var worksheet = package.Workbook.Worksheets.First();

            var period = DateTime.Now;

            var rowCount = worksheet.Dimension.End.Row;
            var colCount = worksheet.Dimension.End.Column;


            const int BUILDING_INDEX = 2;
            const int APARTMENT_CODE_INDEX = 4;
            const int CUSTOMER_NAME_INDEX = 5;
            const int M_AREAS = 6;
            const int M_OLD_DEBT = 8;
            const int P_OLD_DEBT = 9;

            const int M_BILL = 10;
            const int P_BILL = 11;
            const int TOTAL_DEBT = 12;


            var countApartmentNull = 0;

            var listDebt = new List<UserBillDebtDto>();


            var currentUrban = new AppOrganizationUnit();
            var currentBuilding = new AppOrganizationUnit();

            for (var row = 3; row <= rowCount; row++)
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

                var properties = new BillProperites()
                {
                    customerName = customer
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
                userBill.Title = $"Phí dịch vụ tháng {period.Month}/{period.Year}";
                userBill.Properties = JsonConvert.SerializeObject(properties);
                userBill.BillType = BillType.Manager;
                userBill.TenantId = AbpSession.TenantId;
                userBill.ApartmentCode = apartmentCode;
                userBill.LastCost = double.Parse(worksheet.Cells[row, M_BILL].Value.ToString());
                userBill.Status = UserBillStatus.Debt;
                userBill.Period = period;
                userBill.DueDate = period;
                userBill.TotalIndex = worksheet.Cells[row, M_AREAS].Text.ToString() != ""
                    ? decimal.Parse(worksheet.Cells[row, M_AREAS].Value.ToString().Trim())
                    : 0;



                var buildingCode = worksheet.Cells[row, BUILDING_INDEX].Text.ToString() != ""
                    ? worksheet.Cells[row, BUILDING_INDEX].Value.ToString().Trim()
                    : null;
                var urbanCode = "CT8";
                if (buildingCode != null)
                {
                    var building = buildings.FirstOrDefault(x =>
                        x.ProjectCode == buildingCode);
                    if (building != null) userBill.BuildingId = building.ParentId;
                }

                if (urbanCode != null)
                {
                    var urban = urbans.FirstOrDefault(x =>
                        x.ProjectCode == urbanCode);
                    if (urban != null) userBill.UrbanId = urban.ParentId;
                }

                var billP = new UserBill();
                billP.ApartmentCode = userBill.ApartmentCode;
                billP.Period = userBill.Period;
                billP.BuildingId = userBill.BuildingId;
                billP.UrbanId = userBill.UrbanId;
                billP.Status = userBill.Status;
                billP.Properties = userBill.Properties;
                billP.TenantId = userBill.TenantId;
                billP.CitizenTempId = userBill.CitizenTempId;
                billP.DueDate = userBill.DueDate;

                billP.Title = $"Phí xe tháng {period.Month}/{period.Year}";
                billP.BillType = BillType.Parking;
                billP.LastCost = double.Parse(worksheet.Cells[row, P_BILL].Value.ToString());

                var billOldP = new UserBill();
                billOldP.ApartmentCode = userBill.ApartmentCode;
                billOldP.Period = userBill.Period;
                billOldP.BuildingId = userBill.BuildingId;
                billOldP.UrbanId = userBill.UrbanId;
                billOldP.Status = userBill.Status;
                billOldP.Properties = userBill.Properties;
                billOldP.TenantId = userBill.TenantId;
                billOldP.CitizenTempId = userBill.CitizenTempId;

                billOldP.Title = $"Phí xe cũ";
                billOldP.BillType = BillType.Parking;
                billOldP.Period = period.AddMonths(-1);
                billOldP.DueDate = period.AddMonths(-1);
                billOldP.LastCost = double.Parse(worksheet.Cells[row, P_OLD_DEBT].Value.ToString());

                var billOldM = new UserBill();
                billOldM.ApartmentCode = userBill.ApartmentCode;
                billOldM.Period = userBill.Period;
                billOldM.BuildingId = userBill.BuildingId;
                billOldM.UrbanId = userBill.UrbanId;
                billOldM.Status = userBill.Status;
                billOldM.Properties = userBill.Properties;
                billOldM.TenantId = userBill.TenantId;
                billOldM.CitizenTempId = userBill.CitizenTempId;
                billOldM.TotalIndex = userBill.TotalIndex;
                billOldM.Title = "Phí dịch vụ cũ";
                billOldM.BillType = BillType.Manager;
                billOldM.Period = period.AddMonths(-1);
                billOldM.DueDate = period.AddMonths(-1);
                billOldM.LastCost = double.Parse(worksheet.Cells[row, M_OLD_DEBT].Value.ToString());

                var billdebt = new UserBillDebtDto();
                billdebt.Title = "Tổng công nợ cũ";
                billdebt.PaidAmount = 0;
                billdebt.DebtTotal = double.Parse(worksheet.Cells[row, TOTAL_DEBT].Value.ToString());
                billdebt.ApartmentCode = userBill.ApartmentCode;
                billdebt.State = DebtState.DEBT;
                billdebt.Period = period;
                billdebt.TenantId = tenantId;
                billdebt.UrbanId = userBill.UrbanId;
                billdebt.BuildingId = userBill.BuildingId;
                billdebt.BillList = new List<UserBill>() { billOldM, billOldP, userBill, billP };
                listDebt.Add(billdebt);
            }

            return listDebt;
        }

        private async Task CreateListBillDebtAsync(List<UserBillDebtDto> billDebts)
        {
            try
            {
                if (billDebts == null || billDebts.Count() == 0) return;

                foreach (var debt in billDebts)
                {
                    if (debt == null) continue;
                    if (debt.BillList != null && debt.BillList.Count() > 0)
                    {
                        var listids = new List<long>();
                        foreach (UserBill userBill in debt.BillList)
                        {
                            userBill.Id = 0;
                            var id = await _userBillRepo.InsertAndGetIdAsync(userBill);
                            listids.Add(id);
                            userBill.Code = "HD" + userBill.Id +
                                            (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month) + "" +
                                            DateTime.Now.Year;
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }

                        debt.UserBillIds = string.Join(",", listids);
                    }

                    var billDebt = debt.MapTo<BillDebt>();
                    await _billDebtRepo.InsertAndGetIdAsync(billDebt);
                }

            }
            catch (Exception e)
            {
            }
        }

        private IQueryable<DebtUserBillDto> QueryBillDebtByUserBill(GetAllBillDebtInputDto input)
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = from ub in _userBillRepo.GetAll()
                        join cz in _citizenTempRepo.GetAll() on ub.CitizenTempId equals cz.Id into tb_cz
                        from cz in tb_cz.DefaultIfEmpty()
                        where ub.Status == UserBillStatus.Debt
                        select new DebtUserBillDto()
                        {
                            Id = ub.Id,
                            Title = ub.Title,
                            BuildingId = ub.BuildingId,
                            UrbanId = ub.UrbanId,
                            CitizenTempId = ub.CitizenTempId,
                            ApartmentCode = ub.ApartmentCode,
                            DebtTotal = ub.DebtTotal ?? (decimal)ub.LastCost,
                            TenantId = ub.TenantId,
                            Period = ub.Period,
                            CreationTime = ub.CreationTime,
                            CitizenName = cz.FullName,
                            CitizenPhone = cz.PhoneNumber,
                            Properites = ub.Properties,
                            BillType = ub.BillType,
                            LastCost = ub.LastCost
                        };

            query = query
                .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                .WhereIf(input.Period.HasValue, x => x.Period.Value.Month == input.Period.Value.Month && x.Period.Value.Year == input.Period.Value.Year)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => (x.CitizenName != null && x.CitizenName.ToLower().Contains(input.Keyword.ToLower())) || x.ApartmentCode.ToLower().Contains(input.Keyword.ToLower()))
                .OrderByDescending(x => x.CreationTime).AsQueryable();
            return query;
        }

        public async Task<object> GetAllBillDebtByUserBillAsync(GetAllBillDebtInputDto input)
        {
            try
            {
                var query = QueryBillDebtByUserBill(input);

                var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                foreach (var bill in result)
                {
                    bill.CitizenName = bill.CitizenName ?? GetCitizenNameFromApartmentCode(bill.ApartmentCode);
                }

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
     
        public string GetCitizenNameFromApartmentCode(string apartmentCode)
        {
            var citizens = _citizenTempRepo.GetAll().Select(x => new
            {
                Id = x.Id,
                FullName = x.FullName,
                ApartmentCode = x.ApartmentCode,
                IsStayed = x.IsStayed,
                RelationShip = x.RelationShip
            }).Where(x => x.ApartmentCode == apartmentCode).ToList();

            var citizenDefault = citizens.FirstOrDefault(x => x.IsStayed && x.RelationShip == RELATIONSHIP.Contractor)
                                ?? citizens.FirstOrDefault(x => x.RelationShip == RELATIONSHIP.Contractor)
                                ?? citizens.FirstOrDefault();

            return citizenDefault?.FullName;
        }
        protected async Task<IEnumerable<KeyValuePair<string, List<UserBill>>>> QueryUserBillDebtByMonthly(GetBillDebtByMonthlyInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                int month = input.Period.HasValue ? input.Period.Value.Month : 0;
                int year = input.Period.HasValue ? input.Period.Value.Year : 0;

                var listq = _userBillRepo.GetAll()
                    .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                    .Where(x => x.Status == UserBillStatus.Debt)
                    .WhereIf(input.Period.HasValue, x => x.Period.Value.Month == month && x.Period.Value.Year == year)
                    .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode.Contains(input.ApartmentCode))
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

        public async Task<object> GetBillDebtByMonth(GetBillDebtByMonthlyInput input)
        {
            try
            {
                var query = await QueryUserBillDebtByMonthly(input);
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

    }
}