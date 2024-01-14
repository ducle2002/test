using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("ApartmentStatuses")]
    public class ApartmentStatus : Entity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(512)]
        public string Name { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        [StringLength(20)]
        public string ColorCode { get; set; }
    }
}
