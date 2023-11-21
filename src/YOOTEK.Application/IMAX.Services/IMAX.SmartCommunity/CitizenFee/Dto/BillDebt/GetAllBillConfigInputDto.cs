using IMAX.Common;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public class GetAllBillDebtInputDto : CommonInputDto
    {
        public DateTime? Period { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string ApartmentCode { get; set; }
        
    }

    public class UserGetBillDebtInputDto :CommonInputDto
    {
        public DateTime? Period { get; set; }
        public long OrganizationUnitId { get; set; }
        public string ApartmentCode { get; set; }
    }
}
