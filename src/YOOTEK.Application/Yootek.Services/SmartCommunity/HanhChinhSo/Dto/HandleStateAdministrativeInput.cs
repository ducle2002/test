using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class HandleStateAdministrativeInput
    {
        public long Id { get; set; }
        public AdministrativeState State { get; set; }
		[StringLength(1000)]
		public string? DeniedReason { get; set; }

    }
}
