using System;
using System.Collections.Generic;
using System.Linq;
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using static Yootek.YootekServiceBase;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;

namespace Yootek.Services.Dto
{
    public class GetAllBillTypesInputDto : FilteredInputDto
    {
        public long? Id { get; set; }
        public long? TenantId { get; set; }
    }

    public class CreateOrUpdateBillTypeInputDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string? Properties { get; set; }
    }

    public class GetAllBillConfigInputDto : CommonInputDto
    {
        public long? Id { get; set; }
        public long? TenantId { get; set; }
        public BillType? BillType { get; set; }
        public long? ParentId { get; set; }
        public int? Level { get; set; }
        public OrderByBill? OrderBy { get; set; }
    }
    public enum OrderByBill
    {
        [FieldName("Title")]
        TITLE = 1,
    }

    public class GetAllBillEmailDto
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }

    }

    [AutoMap(typeof(BillConfig))]
    public class CreateOrUpdateBillConfigInputDto : IMayHaveUrban, IMayHaveBuilding
    {
        public long? Id { get; set; }
        public int? TenantId { get; set; }
        public BillType? BillType { get; set; }
        public string Title { get; set; }
        public long? ParentId { get; set; }
        public BillConfigPricesType? PricesType { get; set; }
        public string? Properties { get; set; }
        public string AppendToBillConfigIds { get; set; }
        public bool? IsDefault { get; set; }
        public VehicleType? VehicleType { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? ParkingId { get; set; }
        public bool? IsPrivate { get; set; }
        public string? Code { get; set; }
        public long? ApartmentTypeId { get; set; }
    }

    public class AdminGetAllUserBillsInputDto : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? Id { get; set; }
        public BillType? BillType { get; set; }

        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [CanBeNull] public string ApartmentCode { get; set; }
        public DateTime? Period { get; set; }
        public UserBillStatus? Status { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    public class GetStatisticYearInput
    {
        public int Year { get; set; }
        public List<string> ApartmentCodes { get; set; }
        public IQueryable<UserBill> QueryUserBill { get; set; }
    }
    public class GetStatisticBillInput : IMayHaveUrban, IMayHaveBuilding
    {
        public DateTime? DateFrom { get; set; } = DateTime.Now;
        public DateTime? DateTo { get; set; } = DateTime.Now;
        public FormIdStatisticDateTime? FormIdDateTime { get; set; } = FormIdStatisticDateTime.MONTH;
        public FormIdStatisticScope? FormIdScope { get; set; } = FormIdStatisticScope.BUILDING;
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

    public class DataStatisticUserBill
    {
        public string? NameArea { get; set; } // export excel
        public string TenantName { get; set; }
        public long UrbanId { get; set; }
        public long BuildingId { get; set; }
        public string UrbanName { get; set; }
        public string BuildingName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public double Total { get; set; }
        public double TotalPaid { get; set; }
        public double TotalUnpaid { get; set; }
        public double TotalDebt { get; set; }
        public int Count { get; set; }
        public int CountUnpaid { get; set; }
        public int CountDebt { get; set; }
        public int CountPaid { get; set; }
    }
    public enum FormIdStatisticDateTime
    {
        YEAR = 1,  // các năm
        MONTH = 2,  // các tháng trong năm 
        WEEK = 3,  // các tuần trong tháng
        DAY = 4,  // các ngày trong tháng
    }

    public enum FormIdStatisticScope
    {
        TENANT = 1,
        URBAN = 2,
        BUILDING = 3,
    }

    public class CreateOrUpdateUserBillInputDto
    {
        public long Id { get; set; }
        [CanBeNull] public string Code { get; set; }
        public int? TenantId { get; set; }

        // public long BillConfigId { get; set; }
        public BillType? BillType { get; set; }
        public string ApartmentCode { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
        public double? LastCost { get; set; }
        public UserBillStatus? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? Period { get; set; }
        public string? Properties { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }
        public int? ECarNumber { get; set; }
        public int? EMotorNumber { get; set; }
        public int? EBikeNumber { get; set; }
        public long? BillConfigId { get; set; }
        public int? MonthNumber { get; set; }

    }

    [AutoMap(typeof(UserBill))]
    public class CreateUserBillByAdminInput
    {
        public BillType? BillType { get; set; }
        public string ApartmentCode { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
        public double? LastCost { get; set; }
        public UserBillStatus? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? Period { get; set; }
        public string? Properties { get; set; }

        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }
    }

    [AutoMap(typeof(UserBill))]
    public class UpdateUserBillByAdminInput
    {
        public long Id { get; set; }
        [CanBeNull] public string Code { get; set; }
        public BillType? BillType { get; set; }
        public string ApartmentCode { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
        public double? LastCost { get; set; }
        public UserBillStatus? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? Period { get; set; }
        public string? Properties { get; set; }

        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }
    }

    public class BillConfigPropertiesDto
    {
        public PriceDto[] Prices { get; set; }
    }

    public class PriceDto
    {
        public int? From { get; set; }
        public int? To { get; set; }
        public double Value { get; set; }
    }

    public class BillPropertiesDto
    {
        public BillPriceDto[] Prices { get; set; }
    }


    public class BillPriceDto
    {
        public int From { get; set; }
        public int? To { get; set; }
        public decimal Value { get; set; }
    }

    public class UserBillPropertiesDto
    {
        public UserBillSurcharge[]? Surcharges { get; set; }
    }

    public class CalculateUserBillDto
    {
        public UserBillSurcharge[]? Surcharges { get; set; }
        public double Cost { get; set; }
        public double LastCost { get; set; }
    }

    public class UploadBillsExcelInputDto
    {
        public IFormFile File { get; set; }
    }

    public class SendNotifyBillManualDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string[] UserIds { get; set; }
    }

    public class DownloadBillsExcelInputDto
    {
        [CanBeNull] public List<long> Ids { get; set; }
    }

    public class DownloadMonthlyInvoiceExcelInputDto : FilteredInputDto
    {
        [CanBeNull] public DateTime? Period { get; set; }
        [CanBeNull] public string ApartmentCode { get; set; }
        [CanBeNull] public long? OrganizationUnitId { get; set; }
    }

    public class DownloadAllBillsExcelInputDto
    {
        public BillType? BillType { get; set; }
    }

    public class GetAllAdminUserBillPaymentDto : CommonInputDto
    {
        [CanBeNull] public DateTime? Period { get; set; }
        public UserBillPaymentStatus? Status { get; set; }
        public UserBillPaymentMethod? Method { get; set; }
        public string ApartmentCode { get; set; }
        public bool IsAdvanced { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public OrderByBillPayment? OrderBy { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public DateTime? InDay { get; set; }
    }
    public enum OrderByBillPayment
    {
        [FieldName("Title")]
        TITLE = 1,
        [FieldName("ApartmentCode")]
        APARTMENT_CODE = 2,
        [FieldName("PaymentCode")]
        PAYMENT_CODE = 3,
        [FieldName("CreationTime")]
        CREATION_TIME = 4,
    }

    public class InternalSetBillPaymentStatusInput
    {
        public int TenantId { get; set; }
        public long Id { get; set; }
        public UserBillPaymentStatus Status { get; set; }
    }

    public class SetBillPaymentStatusDto
    {
        public string UserBillIds { get; set; }
        public string BillDebtIds { get; set; }
        public TypePayment? TypePayment { get; set; }
        public long Id { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        public UserBillPaymentMethod Method { get; set; }
    }

    public class ViewBillsInPaymentDto
    {
        public string userBillIds { get; set; }
    }

    public class BillQueryInput : IMayHaveBuilding, IMayHaveUrban
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public int NumberRange { get; set; }
        public QueryCaseBillStatistics QueryCase { get; set; }
        public BillType? Type { get; set; }
    }

    public class GetCountUserBill
    {
        public DateTime? Period { get; set; }
        public QueryCaseBillStatistics QueryCase { get; set; }
    }

    public class BillStatisticsQueryOutput
    {
        public int CountPending { get; set; }
        public int CountPaid { get; set; }
        public int CountDebt { get; set; }
        public int CountWaiting { get; set; }
        public double? SumPaid { get; set; }
        public double? SumUnpaid { get; set; }
        public double? SumDebt { get; set; }

    }

    public enum QueryCaseBillStatistics
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4,
    }


    public class GetAllBillsByMonthDto : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public BillType? BillType { get; set; }
        public DateTime? Period { get; set; }
        public string ApartmentCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public UserBillStatus? Status { get; set; }
        public OrderByBillByMonth OrderBy { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }

    }
    public enum OrderByBillByMonth
    {
        [FieldName("Title")]
        TITLE = 1,
        [FieldName("ApartmentCode")]
        APARTMENT_CODE = 2,
        [FieldName("Period")]
        PERIOD = 3
    }

    public class QueryMonthlyBill
    {
        [CanBeNull] public string ApartmentCode { get; set; }
        [CanBeNull] public string Title { get; set; }
        //        public DateTime Month { get; set; }
        public List<UserBill> BillDetail { get; set; }
        /*public DateTime DueDate { get; set; }
        public double Total { get; set; }
        public DateTime FirstCreationDate { get; set; }
        public string CustomerName { get; set; }
        public UserBillStatus Status { get; set; }
        
        public DateTime? Period { get; set; }*/
    }

    public class CreateOrUpdateMonthlyInvoice : IMayHaveUrban, IMayHaveBuilding
    {
        [CanBeNull] public string Title { get; set; }
        [CanBeNull] public string CustomerName { get; set; }
        public long? UrbanAreaCode { get; set; }
        public long? BuildingCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string ApartmentCode { get; set; }
        public DateTime? Period { get; set; }
        public DateTime? DueDate { get; set; }
        public List<CreateOrUpdateUserBillInputDto> BillDetail { get; set; }
        public int? TenantId { get; set; }
        public UserBillStatus? Status { get; set; }
        public List<long> DeleteList { get; set; }
        public long CitizenTempId { get; set; }
    }

    public class GetAllBillConfigDto
    {

        public BillType? BillType { get; set; }
        public BillProperites? Properties { get; set; }
    }
}