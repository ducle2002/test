using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace Yootek.EntityDb
{
    public class ItemType : Entity<long>
    {
        [StringLength(2000)]
        public string Key { get; set; }
        public bool? Required { get; set; }
        public int? Type { get; set; }
        public int? InputType { get; set; }
        [StringLength(1000)]
        public string Value { get; set; }
        [StringLength(1000)]
        public string ListUnit { get; set; }
    }
}
