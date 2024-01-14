using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    [Table("Introduce")]
    public class Introduce : Entity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Detail { get; set; }
    }
}
