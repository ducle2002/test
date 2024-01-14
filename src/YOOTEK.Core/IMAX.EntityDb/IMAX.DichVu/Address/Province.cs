using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    public class Province : Entity<string>
    {
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(256)]
        public string Type { get; set; }
    }
}
