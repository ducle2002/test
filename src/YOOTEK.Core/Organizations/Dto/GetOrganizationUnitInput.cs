using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.Dto
{
    public class GetOrganizationUnitInput
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }
    }
}
