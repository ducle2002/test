using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.SmartCommunity
{
    public enum DeliveryState
    {
        NEW = 0,
        APPROVED = 1
    }

    public class MaterialDelivery : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        public long MaterialId { get; set; }
        public int? Amount { get; set; }
        public DeliveryState? State { get; set; }
        public long DeliverStaffId1 { get; set; }
        public long ReceiverStaffId1 { get; set; }
        public long? DeliverStaffId2 { get; set; }
        public long? ReceiverStaffId2 { get; set; }
        public long? DeliverStaffId3 { get; set; }
        public long? ReceiverStaffId3 { get; set; }
        public long DeliverDepartmentId { get; set; }
        public long ReceiverDepartmentId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        public bool IsDelivery { get; set; }
        public Guid Key { get; set; }

        public long? StaffId { get; set; }
        public long? FromInventoryId { get; set; }
        public string ApartmentCode { get; set; }
        public string CustomerName { get; set; }
        public int? RecipientType { get; set; }
    }
}
