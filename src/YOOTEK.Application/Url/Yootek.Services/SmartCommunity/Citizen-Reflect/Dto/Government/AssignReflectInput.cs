using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class AssignReflectInput: EntityDto<long>
    {
        public long HandleOrganizationUnitId { get; set; }
        public long? HandleUserId { get; set; }
    }
}
