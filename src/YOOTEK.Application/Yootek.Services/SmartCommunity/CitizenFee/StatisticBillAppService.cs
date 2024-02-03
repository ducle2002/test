using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.MultiTenancy;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Yootek.Authorization;
using Yootek.QueriesExtension;

namespace Yootek.Services
{
    public interface IStatisticBillAppService
    {
        DataResult GetStatisticBill(GetStatisticBillInput input);
        DataResult GetTotalStatisticUserBill(GetTotalStatisticUserBillInput input);
        DataResult GetStatisticBillUrban();
        DataResult GetStatisticBillBuilding(GetStatisticBillBuildingInput input);
        List<DataStatisticUserBill> QueryStatisticBill(GetStatisticBillInput input);
        DataStatisticBillTenantDto QueryBillMonthlyStatistics(GetTotalStatisticUserBillInput input);
        Task ReportUserBillPaymentMonthlyScheduler();
    }
    [AbpAuthorize]
    public class StatisticBillAppService : YootekAppServiceBase, IStatisticBillAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<BillDebt, long> _billDebtRepository;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IRepository<ApartmentType, long> _apartmentTypeRepository;
        private readonly IRepository<UserBillPaymentHistory, long> _billPaymentHistoryRepos;
        private readonly IRepository<UserBillVehicleInfo, long> _billVehicleInfoRepository;
        private readonly IRepository<BillStatistic, long> _billStatisticRepos;
        private readonly IBillExcelExporter _billExcelExporter;

        public StatisticBillAppService(
            IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepository,
            IRepository<UserBillPayment, long> userBillPaymentRepository,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<Tenant, int> tenantRepository,
            IRepository<Apartment, long> apartmentRepository,
            IRepository<ApartmentType, long> apartmentTypeRepository,
            IRepository<UserBillPaymentHistory, long> billPaymentHistoryRepos,
            IRepository<BillStatistic, long> billStatisticRepos,
            IRepository<UserBillVehicleInfo, long> billVehicleInfoRepository,
            IBillExcelExporter billExcelExporter
            )
        {
            _userBillRepository = userBillRepo;
            _billDebtRepository = billDebtRepository;
            _userBillPaymentRepository = userBillPaymentRepository;
            _organizationUnitRepository = organizationUnitRepository;
            _tenantRepository = tenantRepository;
            _apartmentRepository = apartmentRepository;
            _apartmentTypeRepository = apartmentTypeRepository;
            _billPaymentHistoryRepos = billPaymentHistoryRepos;
            _billStatisticRepos = billStatisticRepos;
            _billExcelExporter = billExcelExporter;
            _billVehicleInfoRepository = billVehicleInfoRepository;
        }
        public DataResult GetStatisticBill(GetStatisticBillInput input)
        {
            try
            {
                return DataResult.ResultSuccess(QueryStatisticBill(input), "Statistic bill success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [RemoteService(false)]
        public List<DataStatisticUserBill> QueryStatisticBill(GetStatisticBillInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    string tenantName = GetTenantName(AbpSession.TenantId);
                    string urbanName = GetUrbanName(input.UrbanId);
                    string buildingName = GetBuildingName(input.BuildingId);

                    List<DataStatisticUserBill> result = new();
                    switch (input.FormIdScope)
                    {
                        case FormIdStatisticScope.TENANT:
                            switch (input.FormIdDateTime)
                            {
                                case FormIdStatisticDateTime.YEAR:
                                    int yearStart = input.DateFrom.Value.Year;
                                    int yearEnd = input.DateTo.Value.Year;
                                    for (int year = yearStart; year <= yearEnd; year++)
                                    {
                                        ProcessFormIdStatisticYearTenant(result, tenantName, urbanName, buildingName, year);
                                    }
                                    break;
                                case FormIdStatisticDateTime.MONTH:
                                    DateTime dateStart = new(input.DateFrom.Value.Year, input.DateFrom.Value.Month, 1);
                                    DateTime dateTo = new(input.DateTo.Value.Year, input.DateTo.Value.Month, 1);
                                    for (DateTime date = dateStart; date <= dateTo; date = date.AddMonths(1))
                                    {
                                        ProcessFormIdStatisticMonthTenant(result, tenantName, urbanName, buildingName, date.Year, date.Month);
                                    };
                                    break;
                            };
                            break;
                        case FormIdStatisticScope.URBAN:
                            switch (input.FormIdDateTime)
                            {
                                case FormIdStatisticDateTime.YEAR:
                                    int yearStart = input.DateFrom.Value.Year;
                                    int yearEnd = input.DateTo.Value.Year;
                                    for (int year = yearStart; year <= yearEnd; year++)
                                    {
                                        ProcessFormIdStatisticYearUrban(result, tenantName, urbanName, buildingName, (long)input.UrbanId, year);
                                    }
                                    break;
                                case FormIdStatisticDateTime.MONTH:
                                    DateTime dateStart = new(input.DateFrom.Value.Year, input.DateFrom.Value.Month, 1);
                                    DateTime dateTo = new(input.DateTo.Value.Year, input.DateTo.Value.Month, 1);
                                    for (DateTime date = dateStart; date <= dateTo; date = date.AddMonths(1))
                                    {
                                        ProcessFormIdStatisticMonthUrban(result, tenantName, urbanName, buildingName, (long)input.UrbanId, date.Year, date.Month);
                                    };
                                    break;
                            };
                            break;
                        case FormIdStatisticScope.BUILDING:
                            switch (input.FormIdDateTime)
                            {
                                case FormIdStatisticDateTime.YEAR:
                                    int yearStart = input.DateFrom.Value.Year;
                                    int yearEnd = input.DateTo.Value.Year;
                                    for (int year = yearStart; year <= yearEnd; year++)
                                    {
                                        ProcessFormIdStatisticYearBuilding(result, tenantName, urbanName, buildingName, (long)input.BuildingId, year);
                                    }
                                    break;
                                case FormIdStatisticDateTime.MONTH:
                                    DateTime dateStart = new(input.DateFrom.Value.Year, input.DateFrom.Value.Month, 1);
                                    DateTime dateTo = new(input.DateTo.Value.Year, input.DateTo.Value.Month, 1);
                                    for (DateTime date = dateStart; date <= dateTo; date = date.AddMonths(1))
                                    {
                                        ProcessFormIdStatisticMonthBuilding(result, tenantName, urbanName, buildingName, (long)input.BuildingId, date.Year, date.Month);
                                    };
                                    break;
                            };
                            break;
                        default:
                            break;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        #region method helper QueryStatisticBill
        private void ProcessFormIdStatisticYearTenant(List<DataStatisticUserBill> result, string tenantName, string urbanName, string buildingName, int year)
        {
            IEnumerable<UserBillDto> queryUserBills = _userBillRepository.GetAll()
                .Where(x => x.Period.Value.Year == year)
                .Select(x => new UserBillDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Amount = x.Amount,
                    ApartmentCode = x.ApartmentCode,
                    BillType = x.BillType,
                    CitizenTempId = x.CitizenTempId,
                    IndexEndPeriod = x.IndexEndPeriod,
                    IndexHeadPeriod = x.IndexHeadPeriod,
                    LastCost = x.LastCost,
                    Period = x.Period,
                    Properties = x.Properties,
                    Status = x.Status,
                    Title = x.Title,
                    TotalIndex = x.TotalIndex,
                }).AsEnumerable();
            // Group UserBillDto items by ApartmentCode
            var groupUserBill = queryUserBills.GroupBy(x => new { x.ApartmentCode, x.Period.Value.Month }).AsEnumerable();

            // Calculate statistics
            result.Add(new DataStatisticUserBill
            {
                NameArea = tenantName,
                TenantName = tenantName,
                UrbanName = urbanName,
                BuildingName = buildingName,
                Year = year,
                Total = (double)_userBillRepository.GetAll().Where(x => x.Period.Value.Year == year).Sum(x => x.LastCost),
                TotalUnpaid = (double)_userBillRepository.GetAll().Where(x => x.Status == UserBillStatus.Pending && x.Period.Value.Year == year).Sum(x => x.LastCost),
                TotalPaid = (double)_userBillPaymentRepository.GetAll().Where(x => x.Period.Value.Year == year).Sum(x => x.Amount),
                TotalDebt = (double)_billDebtRepository.GetAll().Where(x => x.Period.Year == year).Sum(x => x.DebtTotal),
                Count = groupUserBill.Count(),
                CountDebt = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Debt)),
                CountUnpaid = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Pending) && group.All(x => x.Status != UserBillStatus.Debt)),
                CountPaid = groupUserBill.Count(group => group.All(x => x.Status != UserBillStatus.Debt && x.Status != UserBillStatus.Pending)),
            });
        }
        private void ProcessFormIdStatisticMonthTenant(List<DataStatisticUserBill> result, string tenantName, string urbanName, string buildingName, int year, int month)
        {
            // Filter and select UserBillDto items for the specified year and month
            IEnumerable<UserBillDto> queryUserBills = _userBillRepository.GetAll()
                .Where(x => x.Period.Value.Year == year && x.Period.Value.Month == month)
                .Select(x => new UserBillDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Amount = x.Amount,
                    ApartmentCode = x.ApartmentCode,
                    BillType = x.BillType,
                    CitizenTempId = x.CitizenTempId,
                    IndexEndPeriod = x.IndexEndPeriod,
                    IndexHeadPeriod = x.IndexHeadPeriod,
                    LastCost = x.LastCost,
                    Period = x.Period,
                    Properties = x.Properties,
                    Status = x.Status,
                    Title = x.Title,
                    TotalIndex = x.TotalIndex,
                }).AsEnumerable();

            // Group UserBillDto items by ApartmentCode
            var groupUserBill = queryUserBills.GroupBy(x => x.ApartmentCode).AsEnumerable();

            // Calculate statistics
            result.Add(new DataStatisticUserBill
            {
                NameArea = tenantName,
                Year = year,
                Month = month,
                TenantName = tenantName,
                UrbanName = urbanName,
                BuildingName = buildingName,
                Total = (double)_userBillRepository.GetAll().Where(x => x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.LastCost),
                TotalUnpaid = (double)_userBillRepository.GetAll().Where(x => x.Status == UserBillStatus.Pending && x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.LastCost),
                TotalPaid = (double)_userBillPaymentRepository.GetAll().Where(x => x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.Amount),
                TotalDebt = (double)_billDebtRepository.GetAll().Where(x => x.Period.Year == year && x.Period.Month == month).Sum(x => x.DebtTotal),
                Count = groupUserBill.Count(),
                CountDebt = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Debt)),
                CountUnpaid = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Pending) && group.All(x => x.Status != UserBillStatus.Debt)),
                CountPaid = groupUserBill.Count(group => group.All(x => x.Status != UserBillStatus.Debt && x.Status != UserBillStatus.Pending)),
            });
        }
        private void ProcessFormIdStatisticYearUrban(List<DataStatisticUserBill> result, string tenantName, string urbanName, string buildingName, long urbanId, int year)
        {
            IEnumerable<UserBillDto> queryUserBills = _userBillRepository.GetAll()
                .Where(x => x.UrbanId == urbanId && x.Period.Value.Year == year)
                .Select(x => new UserBillDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Amount = x.Amount,
                    ApartmentCode = x.ApartmentCode,
                    BillType = x.BillType,
                    CitizenTempId = x.CitizenTempId,
                    IndexEndPeriod = x.IndexEndPeriod,
                    IndexHeadPeriod = x.IndexHeadPeriod,
                    LastCost = x.LastCost,
                    Period = x.Period,
                    Properties = x.Properties,
                    Status = x.Status,
                    Title = x.Title,
                    TotalIndex = x.TotalIndex,
                }).AsEnumerable();
            // Group UserBillDto items by ApartmentCode
            var groupUserBill = queryUserBills.GroupBy(x => new { x.ApartmentCode, x.Period.Value.Month }).AsEnumerable();

            // Calculate statistics
            result.Add(new DataStatisticUserBill
            {
                UrbanId = urbanId,
                Year = year,
                NameArea = urbanName,
                TenantName = tenantName,
                UrbanName = urbanName,
                BuildingName = buildingName,
                Total = (double)_userBillRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Period.Value.Year == year).Sum(x => x.LastCost),
                TotalUnpaid = (double)_userBillRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Status == UserBillStatus.Pending && x.Period.Value.Year == year).Sum(x => x.LastCost),
                TotalPaid = (double)_userBillPaymentRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Period.Value.Year == year).Sum(x => x.Amount),
                TotalDebt = (double)_billDebtRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Period.Year == year).Sum(x => x.DebtTotal),
                Count = groupUserBill.Count(),
                CountDebt = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Debt)),
                CountUnpaid = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Pending) && group.All(x => x.Status != UserBillStatus.Debt)),
                CountPaid = groupUserBill.Count(group => group.All(x => x.Status != UserBillStatus.Debt && x.Status != UserBillStatus.Pending)),
            });
        }
        private void ProcessFormIdStatisticMonthUrban(List<DataStatisticUserBill> result, string tenantName, string urbanName, string buildingName, long urbanId, int year, int month)
        {
            // Filter and select UserBillDto items for the specified year and month
            IEnumerable<UserBillDto> queryUserBills = _userBillRepository.GetAll()
                .Where(x => x.UrbanId == urbanId && x.Period.Value.Year == year && x.Period.Value.Month == month)
                .Select(x => new UserBillDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Amount = x.Amount,
                    ApartmentCode = x.ApartmentCode,
                    BillType = x.BillType,
                    CitizenTempId = x.CitizenTempId,
                    IndexEndPeriod = x.IndexEndPeriod,
                    IndexHeadPeriod = x.IndexHeadPeriod,
                    LastCost = x.LastCost,
                    Period = x.Period,
                    Properties = x.Properties,
                    Status = x.Status,
                    Title = x.Title,
                    TotalIndex = x.TotalIndex,
                }).AsEnumerable();

            // Group UserBillDto items by ApartmentCode
            var groupUserBill = queryUserBills.GroupBy(x => x.ApartmentCode).AsEnumerable();

            // Calculate statistics
            result.Add(new DataStatisticUserBill
            {
                UrbanId = urbanId,
                Year = year,
                Month = month,
                NameArea = urbanName,
                TenantName = tenantName,
                UrbanName = urbanName,
                BuildingName = buildingName,
                Total = (double)_userBillRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.LastCost),
                TotalUnpaid = (double)_userBillRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Status == UserBillStatus.Pending && x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.LastCost),
                TotalPaid = (double)_userBillPaymentRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.Amount),
                TotalDebt = (double)_billDebtRepository.GetAll().Where(x => x.UrbanId == urbanId && x.Period.Year == year && x.Period.Month == month).Sum(x => x.DebtTotal),
                Count = groupUserBill.Count(),
                CountDebt = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Debt)),
                CountUnpaid = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Pending) && group.All(x => x.Status != UserBillStatus.Debt)),
                CountPaid = groupUserBill.Count(group => group.All(x => x.Status != UserBillStatus.Debt && x.Status != UserBillStatus.Pending)),
            });
        }
        private void ProcessFormIdStatisticYearBuilding(List<DataStatisticUserBill> result, string tenantName, string urbanName, string buildingName, long buildingId, int year)
        {
            IEnumerable<UserBillDto> queryUserBills = _userBillRepository.GetAll()
                .Where(x => x.BuildingId == buildingId && x.Period.Value.Year == year)
                .Select(x => new UserBillDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Amount = x.Amount,
                    ApartmentCode = x.ApartmentCode,
                    BillType = x.BillType,
                    CitizenTempId = x.CitizenTempId,
                    IndexEndPeriod = x.IndexEndPeriod,
                    IndexHeadPeriod = x.IndexHeadPeriod,
                    LastCost = x.LastCost,
                    Period = x.Period,
                    Properties = x.Properties,
                    Status = x.Status,
                    Title = x.Title,
                    TotalIndex = x.TotalIndex,
                }).AsEnumerable();
            // Group UserBillDto items by ApartmentCode
            var groupUserBill = queryUserBills.GroupBy(x => new { x.ApartmentCode, x.Period.Value.Month }).AsEnumerable();

            // Calculate statistics
            result.Add(new DataStatisticUserBill
            {
                BuildingId = buildingId,
                Year = year,
                NameArea = buildingName,
                TenantName = tenantName,
                UrbanName = urbanName,
                BuildingName = buildingName,
                Total = (double)_userBillRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Period.Value.Year == year).Sum(x => x.LastCost),
                TotalUnpaid = (double)_userBillRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Status == UserBillStatus.Pending && x.Period.Value.Year == year).Sum(x => x.LastCost),
                TotalPaid = (double)_userBillPaymentRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Period.Value.Year == year).Sum(x => x.Amount),
                TotalDebt = (double)_billDebtRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Period.Year == year).Sum(x => x.DebtTotal),
                Count = groupUserBill.Count(),
                CountDebt = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Debt)),
                CountUnpaid = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Pending) && group.All(x => x.Status != UserBillStatus.Debt)),
                CountPaid = groupUserBill.Count(group => group.All(x => x.Status != UserBillStatus.Debt && x.Status != UserBillStatus.Pending)),
            });
        }
        private void ProcessFormIdStatisticMonthBuilding(List<DataStatisticUserBill> result, string tenantName, string urbanName, string buildingName, long buildingId, int year, int month)
        {
            // Filter and select UserBillDto items for the specified year and month
            IEnumerable<UserBillDto> queryUserBills = _userBillRepository.GetAll()
                .Where(x => x.BuildingId == buildingId && x.Period.Value.Year == year && x.Period.Value.Month == month)
                .Select(x => new UserBillDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Amount = x.Amount,
                    ApartmentCode = x.ApartmentCode,
                    BillType = x.BillType,
                    CitizenTempId = x.CitizenTempId,
                    IndexEndPeriod = x.IndexEndPeriod,
                    IndexHeadPeriod = x.IndexHeadPeriod,
                    LastCost = x.LastCost,
                    Period = x.Period,
                    Properties = x.Properties,
                    Status = x.Status,
                    Title = x.Title,
                    TotalIndex = x.TotalIndex,
                }).AsEnumerable();

            // Group UserBillDto items by ApartmentCode
            var groupUserBill = queryUserBills.GroupBy(x => x.ApartmentCode).AsEnumerable();

            // Calculate statistics
            result.Add(new DataStatisticUserBill
            {
                BuildingId = buildingId,
                Year = year,
                Month = month,
                NameArea = buildingName,
                TenantName = tenantName,
                UrbanName = urbanName,
                BuildingName = buildingName,
                Total = (double)_userBillRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.LastCost),
                TotalUnpaid = (double)_userBillRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Status == UserBillStatus.Pending && x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.LastCost),
                TotalPaid = (double)_userBillPaymentRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Period.Value.Year == year && x.Period.Value.Month == month).Sum(x => x.Amount),
                TotalDebt = (double)_billDebtRepository.GetAll().Where(x => x.BuildingId == buildingId && x.Period.Year == year && x.Period.Month == month).Sum(x => x.DebtTotal),
                Count = groupUserBill.Count(),
                CountDebt = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Debt)),
                CountUnpaid = groupUserBill.Count(group => group.Any(x => x.Status == UserBillStatus.Pending) && group.All(x => x.Status != UserBillStatus.Debt)),
                CountPaid = groupUserBill.Count(group => group.All(x => x.Status != UserBillStatus.Debt && x.Status != UserBillStatus.Pending)),
            });
        }
        #endregion

        public DataResult GetTotalStatisticUserBill(GetTotalStatisticUserBillInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var result = QueryBillMonthlyStatistics(input);

                    return DataResult.ResultSuccess(result, "Statistic bill in multi tenant success !");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public DataResult GetStatisticBillUrban()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    int yearCurrent = DateTime.Now.AddMonths(-1).Year;
                    int monthCurrent = DateTime.Now.AddMonths(-1).Month;
                    List<DataStatisticBillUrbanDto> result = new();
                    List<StatisticBillUrbanDto> urbanDtos =
                        (from org in _organizationUnitRepository.GetAll()
                         join orgParent in _organizationUnitRepository.GetAll()
                         on org.ParentId equals orgParent.Id
                         where org.TenantId == AbpSession.TenantId
                         where org.Type == APP_ORGANIZATION_TYPE.URBAN
                         select new StatisticBillUrbanDto()
                         {
                             UrbanId = orgParent.Id,
                             DisplayName = orgParent.DisplayName,
                         }).ToList();
                    IQueryable<UserBill> queryUserBill = _userBillRepository.GetAll()
                        .Where(x => x.CreationTime.Year == yearCurrent & x.CreationTime.Month == monthCurrent)
                        .Where(x => urbanDtos.Select(x => x.UrbanId).Contains(x.UrbanId.Value));
                    IQueryable<UserBillPayment> queryBillPayment = _userBillPaymentRepository.GetAll()
                        .Where(x => x.CreationTime.Year == yearCurrent & x.CreationTime.Month == monthCurrent)
                        .Where(x => urbanDtos.Select(x => x.UrbanId).Contains(x.UrbanId.Value));
                    IQueryable<BillDebt> queryBillDebt = _billDebtRepository.GetAll()
                        .Where(x => x.CreationTime.Year == yearCurrent & x.CreationTime.Month == monthCurrent)
                        .Where(x => urbanDtos.Select(x => x.UrbanId).Contains(x.UrbanId.Value));
                    foreach (StatisticBillUrbanDto urban in urbanDtos)
                    {
                        result.Add(new DataStatisticBillUrbanDto()
                        {
                            UrbanId = urban.UrbanId,
                            DisplayName = urban.DisplayName,
                            TotalCost = (double)queryUserBill.Where(x => x.UrbanId == urban.UrbanId).Sum(x => x.LastCost),
                            TotalPaid = (double)queryBillPayment.Where(x => x.UrbanId == urban.UrbanId).Sum(x => x.Amount),
                            TotalDebt = (double)queryBillDebt.Where(x => x.UrbanId == urban.UrbanId).Sum(x => x.DebtTotal),
                            TotalCostElectric = (double)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Electric).Sum(x => x.LastCost),
                            TotalCostWater = (double)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Water).Sum(x => x.LastCost),
                            TotalCarFee = (double)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Parking).Sum(x => x.LastCost),
                            TotalEIndex = (long)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Electric).Sum(x => x.TotalIndex),
                            TotalWIndex = (long)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Water).Sum(x => x.TotalIndex),
                            TotalManager = (long)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Manager).Sum(x => x.LastCost),
                            CarNumber = (long)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Parking).Sum(x => x.CarNumber),
                            MotorbikeNumber = (long)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Parking).Sum(x => x.MotorbikeNumber),
                            BicycleNumber = (long)queryUserBill.Where(x => x.UrbanId == urban.UrbanId
                                && x.BillType == BillType.Parking).Sum(x => x.BicycleNumber),
                        });
                    }
                    return DataResult.ResultSuccess(result, "Statistic bill in tenant success !");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public DataResult GetStatisticBillBuilding(GetStatisticBillBuildingInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    int yearCurrent = DateTime.Now.AddMonths(-1).Year;
                    int monthCurrent = DateTime.Now.AddMonths(-1).Month;
                    List<DataStatisticBillBuildingDto> result = new();
                    List<StatisticBillBuildingDto> buildingDtos =
                        (from org in _organizationUnitRepository.GetAll()
                         join orgParent in _organizationUnitRepository.GetAll()
                         on org.ParentId equals orgParent.Id
                         where org.TenantId == AbpSession.TenantId
                         where orgParent.ParentId == input.UrbanId
                         where org.Type == APP_ORGANIZATION_TYPE.BUILDING
                         select new StatisticBillBuildingDto()
                         {
                             BuildingId = orgParent.Id,
                             DisplayName = orgParent.DisplayName,
                         }).ToList();
                    IQueryable<UserBill> queryUserBill = _userBillRepository.GetAll()
                        .Where(x => x.CreationTime.Year == yearCurrent & x.CreationTime.Month == monthCurrent)
                        .Where(x => buildingDtos.Select(x => x.BuildingId).Contains(x.BuildingId.Value));
                    IQueryable<UserBillPayment> queryBillPayment = _userBillPaymentRepository.GetAll()
                        .Where(x => x.CreationTime.Year == yearCurrent & x.CreationTime.Month == monthCurrent)
                        .Where(x => buildingDtos.Select(x => x.BuildingId).Contains(x.BuildingId.Value));
                    IQueryable<BillDebt> queryBillDebt = _billDebtRepository.GetAll()
                        .Where(x => x.CreationTime.Year == yearCurrent & x.CreationTime.Month == monthCurrent)
                        .Where(x => buildingDtos.Select(x => x.BuildingId).Contains(x.BuildingId.Value));
                    foreach (StatisticBillBuildingDto building in buildingDtos)
                    {
                        result.Add(new DataStatisticBillBuildingDto()
                        {
                            BuildingId = building.BuildingId,
                            DisplayName = building.DisplayName,
                            TotalCost = (double)queryUserBill.Where(x => x.BuildingId == building.BuildingId).Sum(x => x.LastCost),
                            TotalPaid = (double)queryBillPayment.Where(x => x.BuildingId == building.BuildingId).Sum(x => x.Amount),
                            TotalDebt = (double)queryBillDebt.Where(x => x.BuildingId == building.BuildingId).Sum(x => x.DebtTotal),
                            TotalCostElectric = (double)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Electric).Sum(x => x.LastCost),
                            TotalCostWater = (double)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Water).Sum(x => x.LastCost),
                            TotalEIndex = (long)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Electric).Sum(x => x.TotalIndex),
                            TotalWIndex = (long)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Water).Sum(x => x.TotalIndex),
                            TotalManager = (long)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Manager).Sum(x => x.LastCost),
                            TotalCarFee = (double)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Parking).Sum(x => x.LastCost),
                            CarNumber = (long)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Parking).Sum(x => x.CarNumber),
                            MotorbikeNumber = (long)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Parking).Sum(x => x.MotorbikeNumber),
                            BicycleNumber = (long)queryUserBill.Where(x => x.BuildingId == building.BuildingId
                                && x.BillType == BillType.Parking).Sum(x => x.BicycleNumber),
                        });
                    }
                    return DataResult.ResultSuccess(result, "Statistic bill in urban success !");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [RemoteService(false)]
        public DataStatisticBillTenantDto QueryBillMonthlyStatistics(GetTotalStatisticUserBillInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                DateTime fromDay = new DateTime(), toDay = new DateTime();
                if (input.FromDay.HasValue)
                {
                    fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

                }
                if (input.ToDay.HasValue)
                {
                    toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

                }

                int yearCurrent = DateTime.Now.Year;
                int monthCurrent = DateTime.Now.Month;
                if (input.Period.HasValue)
                {
                    yearCurrent = input.Period.Value.Year;
                    monthCurrent = input.Period.Value.Month;

                }

                IQueryable<UserBill> queryUserBill = _userBillRepository.GetAll()
                        .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                        .WhereIf(input.FromDay.HasValue, u => u.Period >= fromDay)
                        .WhereIf(input.ToDay.HasValue, u => u.Period <= toDay)
                        .WhereIf(!input.FromDay.HasValue && !input.ToDay.HasValue, x => x.Period.Value.Year == yearCurrent & x.Period.Value.Month == monthCurrent)
                        .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                        .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId);

                IQueryable<UserBillPayment> queryBillPayment = _userBillPaymentRepository.GetAll()
                    .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                    .WhereIf(input.FromDay.HasValue, u => u.CreationTime >= fromDay)
                    .WhereIf(input.ToDay.HasValue, u => u.CreationTime <= toDay)
                    .WhereIf(!input.FromDay.HasValue && !input.ToDay.HasValue, x => x.CreationTime.Year == yearCurrent & x.CreationTime.Month == monthCurrent)
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId);
                var result = new DataStatisticBillTenantDto()
                {
                    TotalCost = queryUserBill.Sum(x => x.LastCost) ?? 0,
                    TotalCostElectric = queryUserBill.Where(x => x.BillType == BillType.Electric).Sum(x => x.LastCost) ?? 0,
                    TotalCostWater = queryUserBill.Where(x => x.BillType == BillType.Water).Sum(x => x.LastCost) ?? 0,
                    TotalCostParking = queryUserBill.Where(x => x.BillType == BillType.Parking).Sum(x => x.LastCost) ?? 0,
                    TotalCostManager = queryUserBill.Where(x => x.BillType == BillType.Manager).Sum(x => x.LastCost) ?? 0,
                    TotalCostResidence = queryUserBill.Where(x => x.BillType == BillType.Residence).Sum(x => x.LastCost) ?? 0,
                    TotalCostOther = queryUserBill.Where(x => x.BillType == BillType.Other).Sum(x => x.LastCost) ?? 0,


                    TotalDebt = queryUserBill.Where(x => x.Status == UserBillStatus.Debt).Sum(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal) ?? 0,
                    TotalDebtElectric = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Electric).Sum(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal) ?? 0,
                    TotalDebtWater = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Water).Sum(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal) ?? 0,
                    TotalDebtParking = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Parking).Sum(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal) ?? 0,
                    TotalDebtManager = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Manager).Sum(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal) ?? 0,
                    TotalDebtResidence = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Residence).Sum(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal) ?? 0,
                    TotalDebtOther = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Other).Sum(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal) ?? 0,


                    TotalPaid = (queryUserBill.Where(x => x.Status == UserBillStatus.Paid).Sum(x => x.LastCost) ?? 0) + (queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.DebtTotal > 0).Sum(x => x.LastCost - (double)x.DebtTotal) ?? 0),
                    TotalPaidElectric = (queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Electric).Sum(x => x.LastCost) ?? 0) + (queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.DebtTotal > 0 && x.BillType == BillType.Electric).Sum(x => x.LastCost - (double)x.DebtTotal) ?? 0),
                    TotalPaidWater = (queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Water).Sum(x => x.LastCost) ?? 0) + (queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.DebtTotal > 0 && x.BillType == BillType.Water).Sum(x => x.LastCost - (double)x.DebtTotal) ?? 0),
                    TotalPaidParking = (queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Parking).Sum(x => x.LastCost) ?? 0) + (queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.DebtTotal > 0 && x.BillType == BillType.Parking).Sum(x => x.LastCost - (double)x.DebtTotal) ?? 0),
                    TotalPaidManager = (queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Manager).Sum(x => x.LastCost) ?? 0) + (queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.DebtTotal > 0 && x.BillType == BillType.Manager).Sum(x => x.LastCost - (double)x.DebtTotal) ?? 0),
                    TotalPaidResidence = (queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Residence).Sum(x => x.LastCost) ?? 0) + (queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.DebtTotal > 0 && x.BillType == BillType.Residence).Sum(x => x.LastCost - (double)x.DebtTotal) ?? 0),
                    TotalPaidOther = (queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Other).Sum(x => x.LastCost) ?? 0) + (queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.DebtTotal > 0 && x.BillType == BillType.Other).Sum(x => x.LastCost - (double)x.DebtTotal) ?? 0),


                    TotalPaymentIncome = queryBillPayment.Sum(x => x.Amount) ?? 0,
                    TotalPaymentWithBanking = queryBillPayment.Where(x => x.Method == UserBillPaymentMethod.Banking).Sum(x => x.Amount) ?? 0,
                    TotalPaymentWithDirect = queryBillPayment.Where(x => x.Method == UserBillPaymentMethod.Direct).Sum(x => x.Amount) ?? 0,
                    TotalPaymentWithMomo = queryBillPayment.Where(x => x.Method == UserBillPaymentMethod.Momo).Sum(x => x.Amount) ?? 0,
                    TotalPaymentWithVNPay = queryBillPayment.Where(x => x.Method == UserBillPaymentMethod.VNPay).Sum(x => x.Amount) ?? 0,
                    TotalPaymentWithZaloPay = queryBillPayment.Where(x => x.Method == UserBillPaymentMethod.ZaloPay).Sum(x => x.Amount) ?? 0,


                    TotalEIndex = (long)queryUserBill.Where(x => x.BillType == BillType.Electric).Sum(x => x.TotalIndex),
                    TotalWIndex = (long)queryUserBill.Where(x => x.BillType == BillType.Water).Sum(x => x.TotalIndex),

                    CarNumber = (long)queryUserBill.Where(x => x.BillType == BillType.Parking).Sum(x => x.CarNumber),
                    MotorbikeNumber = (long)queryUserBill.Where(x => x.BillType == BillType.Parking).Sum(x => x.MotorbikeNumber),
                    BicycleNumber = (long)queryUserBill.Where(x => x.BillType == BillType.Parking).Sum(x => x.BicycleNumber),
                };

                return result;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                return new DataStatisticBillTenantDto();
            }
        }
        #region method helpers GetBillStatistics
        private IQueryable<UserBill> QueryStatsBill(BillQueryInput input)
        {
            var query = _userBillRepository.GetAll()
                .WhereIf(input.Type.HasValue, x => x.BillType == input.Type)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                .Where(x => x.Period.HasValue && x.DueDate.HasValue).AsQueryable();
            return query;
        }
        private async Task<IQueryable<PaymentStatisticDto>> QueryPaymentStatisticAsync(BillQueryInput input)
        {
            var query = (from pm in _userBillPaymentRepository.GetAll()
                         select new PaymentStatisticDto()
                         {
                             ApartmentCode = pm.ApartmentCode,
                             Period = pm.Period.Value,
                             TotalPaid = (decimal)pm.Amount.Value,
                             TypePayment = pm.TypePayment,
                             BuildingId = pm.BuildingId,
                             UrbanId = pm.UrbanId
                         })
                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .AsQueryable();
            return query;
        }
        private async Task<IQueryable<UserBillStatisticDto>> QueryUserBillStatisticAsync(BillQueryInput input)
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = (from b in _userBillRepository.GetAll()
                         select new UserBillStatisticDto()
                         {
                             ApartmentCode = b.ApartmentCode,
                             BillType = b.BillType,
                             BuildingId = b.BuildingId,
                             LastCost = b.LastCost,
                             Period = b.Period.Value,
                             Status = b.Status,
                             UrbanId = b.UrbanId,
                             DebtTotal = b.DebtTotal
                         })
                .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .WhereIf(input.Type > 0, x => x.BillType == input.Type)
                .AsQueryable();

            return query;
        }
        private async Task<IQueryable<BillDebtStatisticDto>> QueryBillDebtStatisticAsync(BillQueryInput input)
        {
            var query = (from bd in _billDebtRepository.GetAll()
                         select new BillDebtStatisticDto()
                         {
                             ApartmentCode = bd.ApartmentCode,
                             BuildingId = bd.BuildingId,
                             Period = bd.Period,
                             TotalDebt = bd.DebtTotal,
                             TotalPaid = bd.PaidAmount,
                             UrbanId = bd.UrbanId,
                             State = bd.State
                         }).WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .AsQueryable();

            return query;
        }
        #endregion

        public async Task<object> GetApartmentBillStatistics(BillQueryInput input)
        {
            //var query = QueryStatsBill(input);
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                DateTime now = DateTime.Now;
                int currentMonth = now.Month;
                int currentYear = now.Year;

                Dictionary<string, BillStatisticsQueryOutput> dataResult =
                    new Dictionary<string, BillStatisticsQueryOutput>();

                switch (input.QueryCase)
                {
                    case QueryCaseBillStatistics.ByMonth:
                        if (currentMonth > input.NumberRange)
                        {
                            for (int i = currentMonth - input.NumberRange + 1; i <= currentMonth; i++)
                            {
                                var result = await QueryStatisticsByMonth(input, i, currentYear);
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), result);
                            }
                        }
                        else
                        {
                            for (var i = 13 - (input.NumberRange - currentMonth); i <= 12; i++)
                            {
                                var result = await QueryStatisticsByMonth(input, i, currentYear - 1);
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), result);
                            }

                            for (var i = 1; i <= currentMonth; i++)
                            {
                                var result = await QueryStatisticsByMonth(input, i, currentYear);
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), result);
                            }
                        }

                        break;
                    case QueryCaseBillStatistics.ByYear:
                        for (int i = 1; i <= input.NumberRange; i++)
                        {
                            var result = await QueryStatisticsByYear(input, currentYear - input.NumberRange + i);
                            dataResult.Add((currentYear - input.NumberRange + i) + "", result);
                        }

                        break;
                }

                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                mb.statisticMetris(t1, 0, "get_statistic_citizen");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        #region method helper GetApartmentBillStatistics
        private async Task<BillStatisticsQueryOutput> QueryStatisticsByYear(BillQueryInput input, int currentYear)
        {
            var result = new BillStatisticsQueryOutput();
            var paymentQuery = await QueryPaymentStatisticAsync(input);

            var debtQuery = await QueryBillDebtStatisticAsync(input);


            var query = await QueryUserBillStatisticAsync(input);

            result.CountDebt = await query.Where(x => x.Status == UserBillStatus.Debt)
                .Where(x => x.Period.Year == currentYear).CountAsync();
            result.CountPaid = await query.Where(x => x.Status == UserBillStatus.Paid)
                .Where(x => x.Period.Year == currentYear).CountAsync();
            result.CountPending = await query.Where(x => x.Status == UserBillStatus.Pending)
                .Where(x => x.Period.Year == currentYear).CountAsync();


            result.SumPaid = await query.Where(x => x.Status == UserBillStatus.Paid)
                .Where(x => x.Period.Year == currentYear).Select(x => x.LastCost).SumAsync();

            result.SumUnpaid = await query
                .Where(x => x.Status == UserBillStatus.WaitForConfirm
                            || x.Status == UserBillStatus.WaitForConfirm
                            || x.Status == UserBillStatus.Pending)
                .Where(x => x.Period.Year == currentYear)
                .Select(x => x.LastCost).SumAsync();
            if (input.Type.HasValue)
            {
                result.SumDebt = await query
                    .Where(x => x.Status == UserBillStatus.Debt)
                    .Where(x => x.Period.Year == currentYear)
                    .Select(x => x.LastCost).SumAsync();
            }
            else
            {
                result.SumPaid += await debtQuery.Where(x => x.State == DebtState.DEBT && x.Period.Year == currentYear)
                    .Select(x => x.TotalPaid).SumAsync();

                result.SumDebt = await debtQuery.Where(x => x.State == DebtState.DEBT && x.Period.Year == currentYear)
                    .Select(x => x.TotalDebt).SumAsync();
            }

            return result;
        }
        private async Task<BillStatisticsQueryOutput> QueryStatisticsByMonth(BillQueryInput input, int i,
            int currentYear)
        {
            var result = new BillStatisticsQueryOutput();
            var paymentQuery = await QueryPaymentStatisticAsync(input);

            var debtQuery = await QueryBillDebtStatisticAsync(input);


            var query = await QueryUserBillStatisticAsync(input);

            result.CountDebt = await query.Where(x => x.Status == UserBillStatus.Debt)
                .Where(x => x.Period.Month == i && x.Period.Year == currentYear).CountAsync();
            result.CountPaid = await query.Where(x => x.Status == UserBillStatus.Paid)
                .Where(x => x.Period.Month == i && x.Period.Year == currentYear).CountAsync();
            result.CountPending = await query.Where(x => x.Status == UserBillStatus.Pending)
                .Where(x => x.Period.Month == i && x.Period.Year == currentYear).CountAsync();


            result.SumPaid = await query
.Where(x => x.Status == UserBillStatus.Paid)
.Where(x => x.Period.Month == i && x.Period.Year == currentYear)
.Select(x => (double)x.LastCost)
.SumAsync()
+
await query
 .Where(x => x.Status == UserBillStatus.Debt)
 .Where(x => x.Period.Month == i && x.Period.Year == currentYear)
 .Select(x => x.DebtTotal == 0 || x.DebtTotal == null ? 0 : (double)x.LastCost - (double)x.DebtTotal)
 .SumAsync();
            result.SumUnpaid = await query
                .Where(x => x.Status == UserBillStatus.WaitForConfirm
                            || x.Status == UserBillStatus.WaitForConfirm
                            || x.Status == UserBillStatus.Pending)
                .Where(x => x.Period.Month == i && x.Period.Year == currentYear)
                .Select(x => x.LastCost).SumAsync();
            var debtSum = await query
           .Where(x => x.Status == UserBillStatus.Debt)
           .Where(x => x.Period.Month == i && x.Period.Year == currentYear)
            .Select(x => x.DebtTotal == 0 || x.DebtTotal == null ? (decimal)x.LastCost : x.DebtTotal).SumAsync();
            result.SumDebt = debtSum > 0 ? (double)debtSum : 0;

            return result;
        }
        #endregion

        #region method helpers
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

        #region Mhome excel

        public async Task<object> ExportAllDetailUserBillToExcel(GetAllBillsByMonthDto input)
        {
            try
            {

                DateTime fromDay = new DateTime(), toDay = new DateTime();
                if (input.ToDay.HasValue)
                {
                    toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);
                }
                else toDay = DateTime.Now;

                if (input.FromDay.HasValue)
                {
                    fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);
                }
                else
                {
                    fromDay = toDay.AddMonths(-5);
                }

                var listMonths = MonthsBetween(fromDay, toDay).Select(x => new DateTime(x.Year, x.Month, 1)).ToList();

                var headDayMonth = new DateTime(toDay.Year, toDay.Month, toDay.Day, 0, 0, 0);

                List<ApartmentDetailtDto> apartments = (from apt in _apartmentRepository.GetAll()
                                                        join aptt in _apartmentTypeRepository.GetAll() on apt.TypeId equals aptt.Id into tb_aptt
                                                        from aptt in tb_aptt.DefaultIfEmpty()
                                                        join ub in _organizationUnitRepository.GetAll() on apt.UrbanId equals ub.Id into tb_ub
                                                        from ub in tb_ub.DefaultIfEmpty()
                                                        join bd in _organizationUnitRepository.GetAll() on apt.BuildingId equals bd.Id into tb_bd
                                                        from bd in tb_bd.DefaultIfEmpty()
                                                        select new ApartmentDetailtDto()
                                                        {
                                                            ApartmentCode = apt.ApartmentCode,
                                                            BuildingId = apt.BuildingId,
                                                            UrbanId = apt.UrbanId,
                                                            ApartmentTypeName = aptt.Name,
                                                            BuildingName = bd.DisplayName,
                                                            UrbanName = ub.DisplayName,
                                                            Area = apt.Area ?? 0
                                                        })
                                                            .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                                            .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                                            .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode == input.ApartmentCode)
                                                            .OrderBy(x => x.UrbanId)
                                                            .ThenBy(x => x.BuildingId)
                                                            .ToList();



                var month = input.Period.HasValue ? input.Period.Value.Month : 0;
                var year = input.Period.HasValue ? input.Period.Value.Year : 0;
                var current = DateTime.Now;
                var query = _userBillRepository.GetAll().Select(x =>
                        new ApartmentBillGetAllDto()
                        {
                            ApartmentCode = x.ApartmentCode,
                            BuildingId = x.BuildingId,
                            Period = x.Period.Value,
                            Status = x.Status,
                            UrbanId = x.UrbanId,
                            BillType = x.BillType,
                            LastCost = x.LastCost ?? 0,
                            DebtTotal = x.DebtTotal ?? 0,
                            Id = x.Id,
                            IsPrepayment = x.IsPrepayment
                        })
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId).AsQueryable();

                var queryBill = await query
                    .WhereIf(input.Period.HasValue, x => x.Period.Month == month && x.Period.Year == year)
                    .WhereIf(input.FromDay.HasValue, u => u.Period.Month >= fromDay.Month && u.Period.Year >= fromDay.Year)
                    .WhereIf(input.ToDay.HasValue, u => u.Period.Month <= toDay.Month && u.Period.Year <= toDay.Year)
                    .Where(x => (x.Status == UserBillStatus.Debt && x.Period.Month < toDay.Month && x.Period.Year <= toDay.Year) || x.Status == UserBillStatus.Paid || x.Period.Month == toDay.Month && x.Period.Year == toDay.Year)
                    .AsQueryable()
                    .ToListAsync();

                var queryPaymentAll = _billPaymentHistoryRepos.GetAll().Select(x =>
                       new ApartmentPaymentHistoryDto()
                       {
                           ApartmentCode = x.ApartmentCode,
                           BuildingId = x.BuildingId,
                           Period = x.Period,
                           Status = x.Status,
                           UrbanId = x.UrbanId,
                           TenantId = x.TenantId,
                           CreationTime = x.CreationTime,
                           PayAmount = x.PayAmount,
                           BicycleNumber = x.BicycleNumber,
                           BikePrice = x.BikePrice,
                           CarPrice = x.CarPrice,
                           CarNumber = x.CarNumber,
                           BillType = x.BillType,
                           DebtTotal = x.DebtTotal,
                           IndexEndPeriod = x.IndexEndPeriod,
                           IndexHeadPeriod = x.IndexHeadPeriod,
                           LastCost = x.LastCost,
                           MotorbikeNumber = x.MotorbikeNumber,
                           OtherVehiclePrice = x.OtherVehiclePrice,
                           MotorPrice = x.MotorPrice,
                           TotalIndex = x.TotalIndex
                       })
                   .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                   .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                    .AsQueryable();

                var paymentAlls = await queryPaymentAll.ToListAsync();
                var queryPayment = await queryPaymentAll
                   .WhereIf(input.FromDay.HasValue, u => u.CreationTime.Month >= fromDay.Month && u.CreationTime.Year >= fromDay.Year)
                   .WhereIf(input.ToDay.HasValue, u => u.CreationTime.Month <= toDay.Month && u.CreationTime.Year <= toDay.Year)
                   .AsQueryable()
                   .ToListAsync();

                var dataExcels = new List<ApartmentPaymentBillDetailDto>();
                foreach (var apartment in apartments)
                {
                    var detail = new ApartmentPaymentBillDetailDto()
                    {
                        ApartmentCode = apartment.ApartmentCode,
                        Area = apartment.Area,
                        ApartmentTypeName = apartment.ApartmentTypeName,
                        BuildingName = apartment.BuildingName,
                        UrbanName = apartment.UrbanName,
                        ManagementCellPaidMonths = new List<CellBillPaidMonthlyDto>(),
                        WaterCellPaidMonths = new List<CellBillPaidMonthlyDto>(),
                        ElectricCellPaidMonths = new List<CellBillPaidMonthlyDto>(),
                        CarCellPaidMonths = new List<CellBillPaidMonthlyDto>(),
                        MotorCellPaidMonths = new List<CellBillPaidMonthlyDto>(),
                        BikeCellPaidMonths = new List<CellBillPaidMonthlyDto>(),
                        ParkingCellPaidMonths = new List<CellBillPaidMonthlyDto>()
                    };


                    var billAlls = query.Where(x => x.ApartmentCode == apartment.ApartmentCode && x.BuildingId == apartment.BuildingId && x.UrbanId == apartment.UrbanId);
                    var bills = queryBill.Where(x => x.ApartmentCode == apartment.ApartmentCode && x.BuildingId == apartment.BuildingId && x.UrbanId == apartment.UrbanId);
                    var payments = queryPayment.Where(x => x.ApartmentCode == apartment.ApartmentCode && x.BuildingId == apartment.BuildingId && x.UrbanId == apartment.UrbanId);
                    var paymentAll = paymentAlls.Where(x => x.ApartmentCode == apartment.ApartmentCode && x.BuildingId == apartment.BuildingId && x.UrbanId == apartment.UrbanId).ToList();

                    detail.ManagementDebtAll = billAlls.Where(x => (x.Status == UserBillStatus.Debt && x.Period.Month < toDay.Month && x.Period.Year <= toDay.Year)).Sum(x => x.DebtTotal);
                    detail.ManagementCostAll = billAlls.Where(x => x.Period.Month == current.Month && x.Period.Year == current.Year).Sum(x => x.LastCost);
                    detail.ManagementTotalAll = (double)detail.ManagementDebtAll + detail.ManagementCostAll;
                    var currentManagementPaidAll = paymentAll.Where(x => x.CreationTime.Month == current.Month && x.CreationTime.Year == current.Year).Sum(x => x.PayAmount ?? 0);
                    detail.ManagementPaidAll = paymentAll.Where(x => x.CreationTime.Month == current.Month && x.CreationTime.Year == current.Year).Sum(x => x.PayAmount ?? 0);
                    detail.ManagementPrepayAll = billAlls.Where(x => x.Period.Month >= toDay.Month && x.Period.Year >= toDay.Year && x.Status == UserBillStatus.Paid && x.IsPrepayment == true).Sum(x => x.LastCost);
                    detail.ManagementBalanceAll = detail.ManagementPrepayAll - currentManagementPaidAll;

                    detail.ManagementDebt = bills.Where(x => x.BillType == BillType.Manager && (x.Status == UserBillStatus.Debt && x.Period.Month < toDay.Month && x.Period.Year <= toDay.Year)).Sum(x => x.DebtTotal);
                    detail.ManagementCost = bills.Where(x => x.BillType == BillType.Manager && x.Period.Month == toDay.Month && x.Period.Year == toDay.Year).Sum(x => x.LastCost);
                    detail.ManagementTotal = (double)detail.ManagementDebt + detail.ManagementCost;
                    detail.ManagementPaid = payments.Where(x => x.BillType == BillType.Manager && x.CreationTime.Month == toDay.Month && x.CreationTime.Year == toDay.Year).Sum(x => x.PayAmount ?? 0);
                    detail.ManagementPrepay = bills.Where(x => x.BillType == BillType.Manager && x.Period.Month >= toDay.Month && x.Period.Year >= toDay.Year && x.Status == UserBillStatus.Paid && x.IsPrepayment == true).Sum(x => x.LastCost);
                    detail.ManagementBalance = detail.ManagementPrepay - detail.ManagementPaid;


                    detail.ElectricDebt = bills.Where(x => x.BillType == BillType.Electric && (x.Status == UserBillStatus.Debt && x.Period.Month < toDay.Month && x.Period.Year <= toDay.Year)).Sum(x => x.DebtTotal);
                    detail.ElectricCost = bills.Where(x => x.BillType == BillType.Electric && x.Period.Month == toDay.Month && x.Period.Year == toDay.Year).Sum(x => x.LastCost);
                    detail.ElectricTotal = (double)detail.ElectricDebt + detail.ElectricCost;
                    detail.ElectricPaid = payments.Where(x => x.BillType == BillType.Electric && x.CreationTime.Month == toDay.Month && x.CreationTime.Year == toDay.Year).Sum(x => x.PayAmount ?? 0);
                    detail.ElectricPrepay = bills.Where(x => x.BillType == BillType.Electric && x.Period.Month >= toDay.Month && x.Period.Year >= toDay.Year && x.Status == UserBillStatus.Paid && x.IsPrepayment == true).Sum(x => x.LastCost);
                    detail.ElectricBalance = detail.ElectricPrepay - detail.ElectricPaid;

                    detail.WaterDebt = bills.Where(x => x.BillType == BillType.Water && (x.Status == UserBillStatus.Debt && x.Period.Month < toDay.Month && x.Period.Year <= toDay.Year)).Sum(x => x.DebtTotal);
                    detail.WaterCost = bills.Where(x => x.BillType == BillType.Water && x.Period.Month == toDay.Month && x.Period.Year == toDay.Year).Sum(x => x.LastCost);
                    detail.WaterTotal = (double)detail.WaterDebt + detail.WaterCost;
                    detail.WaterPaid = payments.Where(x => x.BillType == BillType.Water && x.CreationTime.Month == toDay.Month && x.CreationTime.Year == toDay.Year).Sum(x => x.PayAmount ?? 0);
                    detail.WaterPrepay = bills.Where(x => x.BillType == BillType.Water && x.Period.Month >= toDay.Month && x.Period.Year >= toDay.Year && x.Status == UserBillStatus.Paid && x.IsPrepayment == true).Sum(x => x.LastCost);
                    detail.WaterBalance = detail.WaterPrepay - detail.WaterPaid;


                    var parkingDebt = bills.Where(x => x.BillType == BillType.Parking && (x.Status == UserBillStatus.Debt && x.Period.Month < toDay.Month && x.Period.Year <= toDay.Year)).ToList();
                    var parkingBills = bills.Where(x => x.BillType == BillType.Parking && x.Period.Month == toDay.Month && x.Period.Year == toDay.Year).ToList();

                    var parkingPayments = payments.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == toDay.Month && x.CreationTime.Year == toDay.Year).ToList();
                    var parkingPrepaids = bills.Where(x => x.BillType == BillType.Parking && x.Period.Month >= toDay.Month && x.Period.Year >= toDay.Year && x.Status == UserBillStatus.Paid && x.IsPrepayment == true).ToList();



                    detail.ParkingDebt = parkingDebt.Sum(x => x.DebtTotal);
                    detail.ParkingCost = parkingBills.Sum(x => x.LastCost);
                    detail.ParkingTotal = (double)detail.ParkingDebt + detail.ParkingCost;
                    detail.ParkingPaid = parkingPayments.Sum(x => x.PayAmount ?? 0);
                    detail.ParkingPrepay = parkingPrepaids.Sum(x => x.LastCost);
                    detail.ParkingBalance = detail.ParkingPrepay - detail.ParkingPaid;

                    var vehicleDebts = _billVehicleInfoRepository.GetAllList(x => parkingDebt.Select(x => x.Id).ToList().Contains(x.UserBillId));
                    var vehicleMonths = _billVehicleInfoRepository.GetAllList(x => parkingBills.Select(x => x.Id).ToList().Contains(x.UserBillId));

                    var vehiclePrepaids = _billVehicleInfoRepository.GetAllList(x => parkingPrepaids.Select(x => x.Id).ToList().Contains(x.UserBillId));

                    detail.CarDebt = vehicleDebts.Where(x => x.VehicleType == VehicleType.Car).Sum(x => x.Cost);
                    detail.CarCost = vehicleMonths.Where(x => x.VehicleType == VehicleType.Car).Sum(x => x.Cost);
                    detail.CarTotal = detail.CarDebt + detail.CarCost;
                    detail.CarPaid = payments.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == toDay.Month && x.CreationTime.Year == toDay.Year).Sum(x => x.CarPrice ?? 0);
                    detail.CarPrepay = vehiclePrepaids.Where(x => x.VehicleType == VehicleType.Car).Sum(x => x.Cost);
                    detail.CarBalance = detail.CarPrepay - detail.CarPaid;

                    detail.MotorDebt = vehicleDebts.Where(x => x.VehicleType == VehicleType.Motorbike || x.VehicleType == VehicleType.Other).Sum(x => x.Cost);
                    detail.MotorCost = vehicleMonths.Where(x => x.VehicleType == VehicleType.Motorbike || x.VehicleType == VehicleType.Other).Sum(x => x.Cost);
                    detail.MotorTotal = detail.MotorDebt + detail.MotorCost;
                    detail.MotorPaid = payments.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == toDay.Month && x.CreationTime.Year == toDay.Year).Sum(x => x.MotorPrice ?? 0);
                    detail.MotorPrepay = vehiclePrepaids.Where(x => x.VehicleType == VehicleType.Motorbike || x.VehicleType == VehicleType.Other).Sum(x => x.Cost);
                    detail.MotorBalance = detail.MotorPrepay - detail.MotorPaid;

                    detail.BikeDebt = vehicleDebts.Where(x => x.VehicleType == VehicleType.Bicycle).Sum(x => x.Cost);
                    detail.BikeCost = vehicleMonths.Where(x => x.VehicleType == VehicleType.Bicycle).Sum(x => x.Cost);
                    detail.BikeTotal = detail.BikeDebt + detail.BikeCost;
                    detail.BikePaid = payments.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == toDay.Month && x.CreationTime.Year == toDay.Year).Sum(x => x.BikePrice ?? 0);
                    detail.BikePrepay = vehiclePrepaids.Where(x => x.VehicleType == VehicleType.Bicycle).Sum(x => x.Cost);
                    detail.BikeBalance = detail.BikePrepay - detail.BikePaid;

                    var i = 0;
                    foreach (var m in listMonths)
                    {
                        i++;
                        var cellPaidM = new CellBillPaidMonthlyDto()
                        {
                            Period = m,
                            TotalPaid = 0
                        };
                        var cellPaidE = new CellBillPaidMonthlyDto()
                        {
                            Period = m,
                            TotalPaid = 0
                        };
                        var cellPaidW = new CellBillPaidMonthlyDto()
                        {
                            Period = m,
                            TotalPaid = 0
                        };
                        var cellPaidCar = new CellBillPaidMonthlyDto()
                        {
                            Period = m,
                            TotalPaid = 0
                        };
                        var cellPaidMotor = new CellBillPaidMonthlyDto()
                        {
                            Period = m,
                            TotalPaid = 0
                        };
                        var cellPaidBike = new CellBillPaidMonthlyDto()
                        {
                            Period = m,
                            TotalPaid = 0
                        };

                        var cellPaidParking = new CellBillPaidMonthlyDto()
                        {
                            Period = m,
                            TotalPaid = 0
                        };

                        if (i == listMonths.Count)
                        {
                            cellPaidM.TotalPaid = detail.ManagementPaid;
                            cellPaidE.TotalPaid = detail.ElectricPaid;
                            cellPaidW.TotalPaid = detail.WaterPaid;
                            cellPaidCar.TotalPaid = detail.CarPaid;
                            cellPaidMotor.TotalPaid = detail.MotorPaid;
                            cellPaidBike.TotalPaid = detail.BikePaid;
                            cellPaidParking.TotalPaid = detail.ParkingPaid;
                        }
                        else
                        {
                            //var billStatistic = _billStatisticRepos.FirstOrDefault(x => x.ApartmentCode == apartment.ApartmentCode && x.Period.Month == m.Month && x.Period.Year == m.Year && x.BuildingId == apartment.BuildingId && x.UrbanId == apartment.UrbanId);
                            //if (billStatistic != null)
                            //{
                            //    cellPaidM.TotalPaid = billStatistic.TotalManagementPaid;
                            //    cellPaidE.TotalPaid = billStatistic.TotalElectrictPaid;
                            //    cellPaidW.TotalPaid = billStatistic.TotalWaterPaid;
                            //    cellPaidCar.TotalPaid = billStatistic.TotalCarPaid;
                            //    cellPaidMotor.TotalPaid = billStatistic.TotalMotorPaid;
                            //    cellPaidBike.TotalPaid = billStatistic.TotalBikePaid;
                            //    cellPaidParking.TotalPaid = billStatistic.TotalParkingPaid;
                            //}

                            cellPaidM.TotalPaid = paymentAll.Where(x => x.BillType == BillType.Manager && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.PayAmount ?? 0);
                            cellPaidE.TotalPaid = paymentAll.Where(x => x.BillType == BillType.Electric && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.PayAmount ?? 0);
                            cellPaidW.TotalPaid = paymentAll.Where(x => x.BillType == BillType.Water && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.PayAmount ?? 0);
                            cellPaidCar.TotalPaid = paymentAll.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.CarPrice ?? 0);
                            cellPaidMotor.TotalPaid = paymentAll.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.MotorPrice ?? 0);
                            var a = paymentAll.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.OtherVehiclePrice ?? 0);
                            cellPaidMotor.TotalPaid = cellPaidMotor.TotalPaid + a;
                            cellPaidBike.TotalPaid = paymentAll.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.BikePrice ?? 0);
                            cellPaidParking.TotalPaid = paymentAll.Where(x => x.BillType == BillType.Parking && x.CreationTime.Month == m.Month && x.CreationTime.Year == m.Year).Sum(x => x.PayAmount ?? 0);
                        }

                        detail.ManagementCellPaidMonths.Add(cellPaidM);
                        detail.ElectricCellPaidMonths.Add(cellPaidE);
                        detail.WaterCellPaidMonths.Add(cellPaidW);
                        detail.ParkingCellPaidMonths.Add(cellPaidParking);
                        detail.CarCellPaidMonths.Add(cellPaidCar);
                        detail.MotorCellPaidMonths.Add(cellPaidMotor);
                        detail.BikeCellPaidMonths.Add(cellPaidBike);
                    }

                    dataExcels.Add(detail);
                }

                var result = _billExcelExporter.ExportAllDetailUserBills(dataExcels, listMonths);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [RemoteService(true)]
        [AbpAllowAnonymous]
        public async Task ReportUserBillPaymentMonthlyScheduler()
        {
            var currentDate = DateTime.Now;
            if (currentDate.Day != 1) return;
            var period = currentDate.AddMonths(-1);
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenants = await _tenantRepository.GetAllListAsync();
                foreach (var tenantItem in tenants)
                {
                    using (CurrentUnitOfWork.SetTenantId(tenantItem.Id))
                    {
                        try
                        {
                            List<ApartmentDetailtDto> apartments = (from apt in _apartmentRepository.GetAll()
                                                                    join aptt in _apartmentTypeRepository.GetAll() on apt.TypeId equals aptt.Id into tb_aptt
                                                                    from aptt in tb_aptt.DefaultIfEmpty()
                                                                    join ub in _organizationUnitRepository.GetAll() on apt.UrbanId equals ub.Id into tb_ub
                                                                    from ub in tb_ub.DefaultIfEmpty()
                                                                    join bd in _organizationUnitRepository.GetAll() on apt.BuildingId equals bd.Id into tb_bd
                                                                    from bd in tb_bd.DefaultIfEmpty()
                                                                    select new ApartmentDetailtDto()
                                                                    {
                                                                        ApartmentCode = apt.ApartmentCode,
                                                                        BuildingId = apt.BuildingId,
                                                                        UrbanId = apt.UrbanId,
                                                                        ApartmentTypeName = aptt.Name,
                                                                        BuildingName = bd.DisplayName,
                                                                        UrbanName = ub.DisplayName,
                                                                        Area = apt.Area ?? 0
                                                                    })
                                                            .OrderBy(x => x.UrbanId)
                                                            .ThenBy(x => x.BuildingId)
                                                            .ToList();

                            var queryPayment = _billPaymentHistoryRepos.GetAll().Select(x => new ApartmentPaymentHistoryDto()
                            {
                                ApartmentCode = x.ApartmentCode,
                                BuildingId = x.BuildingId,
                                Period = x.Period,
                                Status = x.Status,
                                UrbanId = x.UrbanId,
                                TenantId = x.TenantId,
                                CreationTime = x.CreationTime,
                                PayAmount = x.PayAmount,
                                BicycleNumber = x.BicycleNumber,
                                BikePrice = x.BikePrice,
                                CarPrice = x.CarPrice,
                                CarNumber = x.CarNumber,
                                BillType = x.BillType,
                                DebtTotal = x.DebtTotal,
                                IndexEndPeriod = x.IndexEndPeriod,
                                IndexHeadPeriod = x.IndexHeadPeriod,
                                LastCost = x.LastCost,
                                MotorbikeNumber = x.MotorbikeNumber,
                                MotorPrice = x.MotorPrice,
                                TotalIndex = x.TotalIndex,
                                UserBillId = x.UserBillId
                            }).AsQueryable();

                            var listPayments = await queryPayment.Where(u => u.CreationTime.Month == period.Month && u.CreationTime.Year == period.Year)
                              .ToListAsync();

                            //var queryVehiclePayment = _billVehicleInfoRepository.GetAll().Select(x => new UserBillVehicleInfo()
                            //{
                            //    Id = x.Id,
                            //    Detail = x.Detail,
                            //    NumberVehicle = x.NumberVehicle,
                            //    PriceType = x.PriceType,
                            //    TotalCost = x.TotalCost,
                            //    UserBillId = x.UserBillId,
                            //    VehicleType = x.VehicleType,

                            //})
                            //.AsQueryable();

                            foreach (var apt in apartments)
                            {
                                var paid = new BillStatistic()
                                {
                                    ApartmentCode = apt.ApartmentCode,
                                    BuildingId = apt.BuildingId,
                                    UrbanId = apt.UrbanId,
                                    TenantId = tenantItem.Id,
                                    Period = period,
                                };

                                var apartmentPayments = listPayments.Where(x => x.UrbanId == apt.UrbanId && x.BuildingId == apt.BuildingId && x.ApartmentCode == apt.ApartmentCode);

                                paid.TotalManagementPaid = apartmentPayments.Where(x => x.BillType == BillType.Manager).Sum(x => x.PayAmount ?? 0);
                                paid.TotalElectrictPaid = apartmentPayments.Where(x => x.BillType == BillType.Electric).Sum(x => x.PayAmount ?? 0);
                                paid.TotalWaterPaid = apartmentPayments.Where(x => x.BillType == BillType.Water).Sum(x => x.PayAmount ?? 0);
                                paid.TotalParkingPaid = apartmentPayments.Where(x => x.BillType == BillType.Parking).Sum(x => x.PayAmount ?? 0);
                                paid.TotalResidenceBillPaid = apartmentPayments.Where(x => x.BillType == BillType.Residence).Sum(x => x.PayAmount ?? 0);
                                paid.TotalOtherBillPaid = apartmentPayments.Where(x => x.BillType == BillType.Other).Sum(x => x.PayAmount ?? 0);

                                //var parkingBillIds = apartmentPayments.Where(x => x.BillType == BillType.Parking).Select(x => x.UserBillId).ToList();
                                paid.TotalCarPaid = apartmentPayments.Where(x => x.BillType == BillType.Parking).Sum(x => x.CarPrice ?? 0);
                                paid.TotalMotorPaid = apartmentPayments.Where(x => x.BillType == BillType.Parking).Sum(x => x.MotorPrice ?? 0);
                                paid.TotalBikePaid = apartmentPayments.Where(x => x.BillType == BillType.Parking).Sum(x => x.BikePrice ?? 0);

                                await _billStatisticRepos.InsertAsync(paid);
                            }

                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                    }

                }

            });

        }

        public static IEnumerable<(int Month, int Year)> MonthsBetween(
        DateTime startDate,
        DateTime endDate)
        {
            DateTime iterator;
            DateTime limit;

            if (endDate > startDate)
            {
                iterator = new DateTime(startDate.Year, startDate.Month, 1);
                limit = endDate;
            }
            else
            {
                iterator = new DateTime(endDate.Year, endDate.Month, 1);
                limit = startDate;
            }
            while (iterator <= limit)
            {
                yield return (
                    iterator.Month,
                    iterator.Year
                );

                iterator = iterator.AddMonths(1);
            }
        }
        #endregion
    }


}
