using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum DebtState
    {
        DEBT = 1,
        PAIED = 2,
        REDUNDANT = 3,
        WAITFORCONFIRM = 4
    }

    [Table("BillDebts")]
    public class BillDebt : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveBuilding, IMayHaveUrban
    {
        [StringLength(1000)]
        public string Title { get; set; }
        public long? UserId { get; set; }
        public long? CitizenTempId { get; set; }
        [StringLength(1000)]
        public string ApartmentCode { get; set; }
        public double? DebtTotal { get; set; }
        public double? PaidAmount { get; set; }
        public DebtState? State { get; set; }
        public long? BillPaymentId { get; set; }
        public int? TenantId { get; set; }
        public DateTime Period { get; set; }
        [StringLength(1000)]
        public string UserBillIds { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string Properties { get; set; }
    }
}
