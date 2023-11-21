using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Common.DataResult;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using IMAX.MultiTenancy;
using IMAX.Organizations;
using IMAX.Services.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public interface IStatisticBillAppService
    {
        DataResult GetStatisticBill(GetStatisticBillInput input);
        DataResult GetTotalStatisticUserBill(GetTotalStatisticUserBillInput input);
        DataResult GetStatisticBillUrban();
        DataResult GetStatisticBillBuilding(GetStatisticBillBuildingInput input);
        List<DataStatisticUserBill> QueryStatisticBill(GetStatisticBillInput input);
        DataStatisticBillTenantDto QueryBillMonthlyStatistics(GetTotalStatisticUserBillInput input);
    }
    [AbpAuthorize]
    public class StatisticBillAppService : IMAXAppServiceBase, IStatisticBillAppService
    {
        private readonly IRepository<UserBill, long> _userBillRepository;
        private readonly IRepository<BillDebt, long> _billDebtRepository;
        private readonly IRepository<UserBillPayment, long> _userBillPaymentRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<Tenant, int> _tenantRepository;

        public StatisticBillAppService(
            IRepository<UserBill, long> userBillRepo,
            IRepository<BillDebt, long> billDebtRepository,
            IRepository<UserBillPayment, long> userBillPaymentRepository,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<Tenant, int> tenantRepository
            )
        {
            _userBillRepository = userBillRepo;
            _billDebtRepository = billDebtRepository;
            _userBillPaymentRepository = userBillPaymentRepository;
            _organizationUnitRepository = organizationUnitRepository;
            _tenantRepository = tenantRepository;
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
                throw new UserFriendlyException(ex.Message);
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
                throw new UserFriendlyException(ex.Message);
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
                throw new UserFriendlyException(ex.Message);
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
                throw new UserFriendlyException(ex.Message);
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
                throw new UserFriendlyException(ex.Message);
            }
        }

        [RemoteService(false)]
        public DataStatisticBillTenantDto QueryBillMonthlyStatistics(GetTotalStatisticUserBillInput input)
        {
            try
            {
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
                        .WhereIf(input.FromDay.HasValue, u => u.CreationTime >= fromDay)
                        .WhereIf(input.ToDay.HasValue, u => u.CreationTime <= toDay)
                        .WhereIf(!input.FromDay.HasValue && !input.ToDay.HasValue, x => x.Period.Value.Year == yearCurrent & x.Period.Value.Month == monthCurrent)
                        .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                        .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId);

                IQueryable<UserBillPayment> queryBillPayment = _userBillPaymentRepository.GetAll()
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
                    TotalCostOther = queryUserBill.Where(x => x.BillType == BillType.Orther).Sum(x => x.LastCost) ?? 0,


                    TotalDebt = queryUserBill.Where(x => x.Status == UserBillStatus.Debt).Sum(x => x.DebtTotal) ?? 0,
                    TotalDebtElectric = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Electric).Sum(x => x.DebtTotal) ?? 0,
                    TotalDebtWater = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Water).Sum(x => x.DebtTotal) ?? 0,
                    TotalDebtParking = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Parking).Sum(x => x.DebtTotal) ?? 0,
                    TotalDebtManager = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Manager).Sum(x => x.DebtTotal) ?? 0,
                    TotalDebtResidence = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Residence).Sum(x => x.DebtTotal) ?? 0,
                    TotalDebtOther = queryUserBill.Where(x => x.Status == UserBillStatus.Debt && x.BillType == BillType.Orther).Sum(x => x.DebtTotal) ?? 0,


                    TotalPaid = queryUserBill.Where(x => x.Status == UserBillStatus.Paid).Sum(x => x.LastCost) ?? 0,
                    TotalPaidElectric = queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Electric).Sum(x => x.LastCost) ?? 0,
                    TotalPaidWater = queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Water).Sum(x => x.LastCost) ?? 0,
                    TotalPaidParking = queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Parking).Sum(x => x.LastCost) ?? 0,
                    TotalPaidManager = queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Manager).Sum(x => x.LastCost) ?? 0,
                    TotalPaidResidence = queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Residence).Sum(x => x.LastCost) ?? 0,
                    TotalPaidOther = queryUserBill.Where(x => x.Status == UserBillStatus.Paid && x.BillType == BillType.Orther).Sum(x => x.LastCost) ?? 0,

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
                                var result = await QueryStatisticsByMonth(input, i, currentYear);
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
                throw new UserFriendlyException(ex.Message);
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


            result.SumPaid = await query.Where(x => x.Status == UserBillStatus.Paid)
                .Where(x => x.Period.Month == i && x.Period.Year == currentYear).Select(x => x.LastCost).SumAsync();
            result.SumUnpaid = await query
                .Where(x => x.Status == UserBillStatus.WaitForConfirm
                            || x.Status == UserBillStatus.WaitForConfirm
                            || x.Status == UserBillStatus.Pending)
                .Where(x => x.Period.Month == i && x.Period.Year == currentYear)
                .Select(x => x.LastCost).SumAsync();
            var debtSum = await query
                     .Where(x => x.Status == UserBillStatus.Debt)
                     .Where(x => x.Period.Month == i && x.Period.Year == currentYear)
                     .Select(x => x.DebtTotal).SumAsync();
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
    }
}
