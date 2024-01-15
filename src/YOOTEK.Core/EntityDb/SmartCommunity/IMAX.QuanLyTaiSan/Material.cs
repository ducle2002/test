using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Materials")]
    public class Material : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(256)]
        public string MaterialName { get; set; }

        [StringLength(1000)]
        public string MaterialCode { get; set; }

        [StringLength(1000)]
        public string ImageUrl { get; set; }
        [StringLength(256)]
        public string Status { get; set; }
        public string Description { get; set; }
        public long? TypeId { get; set; }
        public long? GroupId { get; set; }
        public long? UnitId { get; set; }
        public long? ProducerId { get; set; }
        public long? LocationId { get; set; }
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string SerialNumber { get; set; }
        public string Specifications { get; set; }
        public int? Amount { get; set; }
        public int? TotalPrice { get; set; }
        [StringLength(1000)]
        public string FileUrl { get; set; }
        public int? Price { get; set; }
        public DateTime? ManufacturerDate { get; set; } //ngày sản xuất
        public int? WarrantyMonth { get; set; } // thời gian bảo hành bn tháng
        public DateTime? ExpiredDate { get; set; }
        public DateTime? UsedDate { get; set; }
        public decimal? WearRate { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
