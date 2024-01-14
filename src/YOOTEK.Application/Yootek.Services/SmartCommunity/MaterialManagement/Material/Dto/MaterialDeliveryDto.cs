using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Organizations.Interface;
using Yootek.SmartCommunity;
using System;
using System.Collections.Generic;

namespace Yootek.Services
{
    [AutoMap(typeof(MaterialDeliveryDto))]
    public class MaterialDeliveryViewDto : IMayHaveUrban, IMayHaveBuilding
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
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
        public string Description { get; set; }
        public string Code { get; set; }
        public bool IsDelivery { get; set; }
        public int? TotalPrice { get; set; }
        public Guid Key { get; set; }
        public string UnitName { get; set; }
        public int? Amount { get; set; }
        public string DepartmentName { get; set; }
        public int? Price { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public long? FromInventoryId { get; set; }
        public string ApartmentCode { get; set; }
        public string CustomerName { get; set; }
        public int? RecipientType { get; set; }
        public long? StaffId { get; set; }
    }

    [AutoMap(typeof(MaterialDelivery))]
    public class MaterialDeliveryDto : IMayHaveUrban, IMayHaveBuilding
    {
        public string Id { get; set; }
        public int? TenantId { get; set; }
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
        public string Description { get; set; }
        public string Code { get; set; }
        public long? StaffId { get; set; }
        public long? FromInventoryId { get; set; }
        public bool IsDelivery { get; set; }
        public int? Amount { get; set; }
        public int? TotalAmount { get; set; }
        public int? AllPrice { get; set; }
        public int? CountMaterial { get; set; }
        public string DepartmentName { get; set; }
        public List<MaterialDto> Materials { get; set; }
        public List<MaterialDeliveryViewDto> MaterialViews { get; set; }
        public string ApartmentCode { get; set; }
        public string CustomerName { get; set; }
        public int? RecipientType { get; set; }

        public List<Guid> DeleteElements { get; set; }
        public List<Guid> UpdateElements { get; set; }
        public List<Guid> AddElements { get; set; }
    }


    public class GetAllDeliveryInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? StaffId { get; set; }
        public long? LocationId { get; set; }
        public bool IsDelivery { get; set; }
    }

    public class InventoryDeliveryExportInput
    {
        public List<string> Ids { get; set; }
        public bool IsDelivery { get; set; }
    }

    public class ApproveDeliveryInputDto
    {
        public string Id { get; set; }
        public bool IsDelivery { get; set; }
    }
}
