using Abp.Domain.Entities;
using Abp.Organizations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("AdministrativeValue")]
    public class AdministrativeValue : Entity<long>, IMayHaveTenant
    {
        public string Value { get; set; }
        [StringLength(1000)]
        public string Key { get; set; }
        public long? AdministrativeId { get; set; }
        public long? ParentId { get; set; }
        public int? TenantId { get; set; }
        public long TypeId { get; set; }
    }
}
