using Abp.AutoMapper;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;

namespace Yootek.Services
{
    [AutoMap(typeof(BillDebt))]
    public class UserBillDebtDto: IMayHaveBuilding, IMayHaveUrban
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long? UserId { get; set; }
        public long? CitizenTempId { get; set; }
        public string ApartmentCode { get; set; }
        public double? DebtTotal { get; set; }
        public double? PaidAmount { get; set; }
        public DebtState? State { get; set; }
        public long? BillPaymentId { get; set; }
        public int? TenantId { get; set; }
        public DateTime Period { get; set; }
        public DateTime CreationTime { get; set; }
        public string UserBillIds { get; set; }
        public long? OrganizationUnitId { get; set; }
        public List<UserBill> BillList { get; set; }
        public string CitizenName { get; set; }
        public string CitizenPhone { get; set; }
        public UserBillPayment BillPayment { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
    }

        [AutoMap(typeof(BillDebt))]
    public class BillDebtUserBillDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long? CitizenTempId { get; set; }
        public string ApartmentCode { get; set; }
        public double? DebtTotal { get; set; }
        public double? PaidAmount { get; set; }
        public int? TenantId { get; set; }
        public DateTime Period { get; set; }
        public DateTime CreationTime { get; set; }
        public string UserBillIds { get; set; }
        public long? OrganizationUnitId { get; set; }
        public List<UserBill> BillList { get; set; }
        public string CitizenName { get; set; }
        public string CitizenPhone { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string Properites { get; set; }
    }

    [AutoMap(typeof(UserBill))]
    public class DebtUserBillDto: Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string Code { get; set; }
        public DateTime? Period { get; set; }
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public long BillConfigId { get; set; }
        public BillType BillType { get; set; }
        public string Properties { get; set; }
        public UserBillStatus Status { get; set; }
        public int? TenantId { get; set; }
        public DateTime? DueDate { get; set; }
        public double? LastCost { get; set; }
        public decimal? DebtTotal { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? CitizenTempId { get; set; }
        public string CitizenName { get; set; }
        public string CitizenPhone { get; set; }
        public DateTime CreationTime { get; set; }
        public string Properites { get; set; }
    }
}
