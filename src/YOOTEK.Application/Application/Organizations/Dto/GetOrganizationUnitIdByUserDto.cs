using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.Dto
{
    public class GetOrganizationUnitIdByUserDto
    {
        public long OrganizationUnitId { get; set; }
        public int TypeBuilding { get; set; }
        public string BuildingCode { get; set; }
        public int TypeUrban { get; set; }
        public int? TenantCode { get; set; }
    }
}
