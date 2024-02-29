using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("TypeAdministrative")]
    public class TypeAdministrative : Entity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(2000)]
        public string Detail { get; set; }
        [StringLength(2000)]
        public string ImageUrl { get; set; }
        [StringLength(1000)]
        public string FileUrl { get; set; }
        public long? OrganizationUnitId { get; set; }

        public bool? Surcharge { get; set; }
        public double? Price { get; set; }
        [StringLength(1000)]
        public string PriceDetail { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
      //  public UnitCharge Unit { get; set; }
    }

    public enum UnitCharge
    {

    }
}
