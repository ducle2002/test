using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Authorization.Permissions
{
    [Table("PermissionsTenants")]
    public class PermissionTenant : Entity<long>
    {
        public int TenantId { get; set; }

        [StringLength(1000)]
        public string Name { get; set; }
    }
}
