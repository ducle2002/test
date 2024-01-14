using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Hotlines")]
    public class Hotlines : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        public string Properties { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
