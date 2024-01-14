using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("SmartHomes")]
    public class SmartHome : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(2000)]
        public string SmartHomeCode { get; set; }
        [StringLength(2000)]
        public string ImageUrl { get; set; }
        public int? Status { get; set; }
        public string PropertiesHistory { get; set; }
        public string Properties { get; set; }
        public string HouseDetail { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public int? TenantId { get; set; }
        public string BuildingCode { get; set; }
        public string UrbanCode { get; set; }
        public decimal? ApartmentAreas { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }

    }
}
