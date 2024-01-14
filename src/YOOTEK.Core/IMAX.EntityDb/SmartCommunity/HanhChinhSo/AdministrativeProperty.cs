using Abp.Domain.Entities;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    public enum ADPropertyType
    {
        STRING = 1,
        HMTL = 2,
        NUMBER = 3,
        BOOL = 4,
        DATETIME = 5,
        TIME = 6,
        OBJECT = 7,
        OPTION = 8,
        TABLE = 9,
        PHONENUMBER = 10,
        OPTIONCHECKBOX = 11,
        OPTIONMULTIPLE = 12,
        CHECKNOTE = 13,
        IMAGEFILE = 14,

    }

    [Table("AdministrativeProperty")]
    public class AdministrativeProperty : Entity<long>, IMayHaveTenant
    {
        public string Value { get; set; }
        [StringLength(1000)]
        public string Key { get; set; }
        public long? TypeId { get; set; }
        public ADPropertyType Type { get; set; }
        public long? ConfigId { get; set; }
        [StringLength(2000)]
        public string DisplayName { get; set; }
        public long? ParentId { get; set; }
        public bool? IsDelete { get; set; }
        public int? TenantId { get; set; }
        public bool? IsTableColumn { get; set; } 
    }
}
