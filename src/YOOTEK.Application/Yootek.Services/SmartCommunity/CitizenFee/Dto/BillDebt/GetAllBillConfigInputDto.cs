using Yootek.Common;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class GetAllBillDebtInputDto : CommonInputDto
    {
        public DateTime? Period { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string ApartmentCode { get; set; }
        
    }

    public class UserGetBillDebtInputDto :CommonInputDto
    {
        public DateTime? Period { get; set; }
        public long OrganizationUnitId { get; set; }
        public string ApartmentCode { get; set; }
    }
}
