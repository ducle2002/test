using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Application.Services.Dto;

namespace Yootek.Services.Dto
{
    public class UserBillDto
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public DateTime? Period { get; set; }
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public double Amount { get; set; }
        public UserBillStatus Status { get; set; }
        public double? LastCost { get; set; }
        public long? CitizenTempId { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public UserBillPaymentMethod? paymentMethod { get; set; }
        public string Properties { get; set; }
    }

    public class UserGetBillByMonthInput : CommonInputDto
    {
        public BillType? BillType { get; set; }
        public UserBillFormId? FormId { get; set; }
        public string ApartmentCode { get; set; }
    }

    public class GetAllUserBillsInputDto : CommonInputDto
    {
        public long[]? Ids { get; set; }
        public BillType? BillType { get; set; }
        public string? ApartmentCode { get; set; }

        //public UserBillStatus? Status { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public UserBillFormId? FormId { get; set; }
    }

    public class GetUserBillTemplateInput
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }

        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
    }

    public class HandlePayUserBillInputDto
    {
        public int ResultCode { get; set; }
        public long UserId { get; set; }
        public long PaymentId { get; set; }
        public int TenantId { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        public double Amount { get; set; }
        public UserBillPaymentMethod PaymentMethod { get; set; }
        public TypePayment TypePayment { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public long[]? BillDebtIds { get; set; }
        public Guid? RequestPaymentId { get; set; }
        public string ApartmentCode { get; set; }
    }

    [AutoMap(typeof(MappingRequestPaymentDto))]
    public class HandlePayUserBillDirectInputDto
    {
        public string? Title { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        public double Amount { get; set; }
        public DateTime? Period { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        [CanBeNull] public string ImgUrls { get; set; }
        [CanBeNull] public string FileUrls { get; set; }
        [CanBeNull] public string Description { get; set; }
        public long[]? BillDebtIds { get; set; }
        public TypePayment? TypePayment { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    [AutoMap(typeof(MappingRequestPaymentDto))]
    public class RequestPaymentInputDto
    {
        public int? TenantId { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        [CanBeNull] public string Title { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public DateTime? Period { get; set; }
        public long[]? BillDebtIds { get; set; }
        public TypePayment? TypePayment { get; set; }
        public Guid? RequestPaymentId { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    public class MappingRequestPaymentDto
    {
        public int? TenantId { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        [CanBeNull] public string Title { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public DateTime? Period { get; set; }
        public long[]? BillDebtIds { get; set; }
        public TypePayment? TypePayment { get; set; }
        public Guid? RequestPaymentId { get; set; }
        public long? OrganizationUnitId { get; set; }

        public UserBill UserBill { get; set; }

        [CanBeNull] public string ImgUrls { get; set; }
        [CanBeNull] public string FileUrls { get; set; }
        [CanBeNull] public string Description { get; set; }
    }

    public class GetMonthPrepaymentDto
    {
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public BillType BillType { get; set; }
    }

    public class GetPaymentHistoriesInputDto : CommonInputDto
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public UserBillPaymentStatus? Status { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
    }

    public enum TypeHandlePayment
    {
        Normal = 1,
        TenantServiceBooking = 2
    }

    public class ApartmentDetailtDto
    {
        public string ApartmentCode {  get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string UrbanName { get; set; }
        public string BuildingName { get; set; }
        public string ApartmentTypeName { get; set; }
        public decimal Area { get; set; }
    }

    public class UserBillVehicleInfoDto
    {
        public int? TenantId { get; set; }
        public long UserBillId { get; set; }
        public VehicleType VehicleType { get; set; }
        public int NumberVehicle { get; set; }
        public double TotalCost { get; set; }
        public BillConfigPricesType PriceType { get; set; }
        public string Detail { get; set; }
    }

    public class ApartmentPaymentBillDetailDto
    {
        public string ApartmentCode { get; set; }
        public decimal Area { get; set; }
        public string BuildingName { get; set; }
        public string UrbanName { get; set; }
        public string ApartmentTypeName { get; set; }
        public DateTime Period { get; set; }

        public decimal ManagementDebtAll { get; set; }
        public double ManagementCostAll { get; set; }
        public double ManagementTotalAll { get; set; }
        public double ManagementPaidAll { get; set; }
        public double ManagementPrepayAll { get; set; }
        public double ManagementBalanceAll { get; set; }

        public decimal ManagementDebt {  get; set; }
        public double ManagementCost { get; set; }
        public double ManagementTotal { get; set; }
        public double ManagementPaid { get; set; }
        public double ManagementPrepay { get; set; }
        public double ManagementBalance { get; set; }

        public decimal ElectricDebt { get; set; }
        public double ElectricCost { get; set; }
        public double ElectricTotal { get; set; }
        public double ElectricPaid { get; set; }
        public double ElectricPrepay { get; set; }
        public double ElectricBalance { get; set; }

        public decimal WaterDebt { get; set; }
        public double WaterCost { get; set; }
        public double WaterTotal { get; set; }
        public double WaterPaid { get; set; }
        public double WaterPrepay { get; set; }
        public double WaterBalance { get; set; }

        public decimal ParkingDebt { get; set; }
        public double ParkingCost { get; set; }
        public double ParkingTotal { get; set; }
        public double ParkingPaid { get; set; }
        public double ParkingPrepay { get; set; }
        public double ParkingBalance { get; set; }

        public double CarDebt { get; set; }
        public double CarCost { get; set; }
        public double CarTotal { get; set; }
        public double CarPaid { get; set; }
        public double CarPrepay { get; set; }
        public double CarBalance { get; set; }

        public double MotorDebt { get; set; }
        public double MotorCost { get; set; }
        public double MotorTotal { get; set; }
        public double MotorPaid { get; set; }
        public double MotorPrepay { get; set; }
        public double MotorBalance { get; set; }

        public double BikeDebt { get; set; }
        public double BikeCost { get; set; }
        public double BikeTotal { get; set; }
        public double BikePaid { get; set; }
        public double BikePrepay { get; set; }
        public double BikeBalance { get; set; }

        public List<CellBillPaidMonthlyDto> ManagementCellPaidMonths { get; set; }
        public List<CellBillPaidMonthlyDto> ElectricCellPaidMonths { get; set; }
        public List<CellBillPaidMonthlyDto> WaterCellPaidMonths { get; set; }
        public List<CellBillPaidMonthlyDto> CarCellPaidMonths { get; set; }
        public List<CellBillPaidMonthlyDto> MotorCellPaidMonths { get; set; }
        public List<CellBillPaidMonthlyDto> BikeCellPaidMonths { get; set; }
        public List<CellBillPaidMonthlyDto> ParkingCellPaidMonths { get; set; }
    }

    public class CellBillPaidMonthlyDto
    {
        public double TotalPaid { get; set; }
        public DateTime Period { get; set; }
    }


    public class ApartmentBillGetAllDto : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public DateTime Period { get; set; }
        public DateTime DueDate { get; set; }
        public UserBillStatus? Status { get; set; }
        public BillType BillType { get; set; }
        public double LastCost { get; set; }
        public decimal DebtTotal { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public bool? IsPrepayment { get; set; }
    }


    public class ApartmentPaymentGetAllDto 
    {
        public string UserBillIds { get; set; }
        public string UserBillDebtIds { get; set; }
        public string UserBillPrepaymentIds { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        public double Amount { get; set; }
        public DateTime Period { get; set; }
        public DateTime CreationTime { get; set; }
        public int? TenantId { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        public TypePayment? TypePayment { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public bool? IsRecover { get; set; }
        public string BillPaymentInfo { get; set; }
        public string CustomerName { get; set; }
    }

    public class ApartmentPaymentHistoryDto
    {
        public long UserBillId { get; set; }
        public DateTime? Period { get; set; }
        public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public UserBillStatus Status { get; set; }
        public double? LastCost { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }
        public DateTime CreationTime { get; set; }
        public double? CarPrice { get; set; }
        public double? MotorPrice { get; set; }
        public double? BikePrice { get; set; }
        public double? OtherVehiclePrice { get; set; }
        public double? PayAmount { get; set; }
        public decimal? DebtTotal { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }


    [AutoMap(typeof(BillConfig))]
    public class BillConfigDto : BillConfig
    {
        public List<BillConfig> ListPrivates { get; set; }
    }
}