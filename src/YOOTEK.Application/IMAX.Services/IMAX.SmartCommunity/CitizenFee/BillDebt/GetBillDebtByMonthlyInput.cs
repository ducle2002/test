using IMAX.Common.Enum;
using IMAX.Common;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;
using IMAX.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public class GetBillDebtByMonthlyInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public BillType? BillType { get; set; }
        public DateTime? Period { get; set; }
        public bool IsDraft { get; set; }
        public string ApartmentCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public UserBillStatus? Status { get; set; }
        public OrderByBillByMonth OrderBy { get; set; }

    }

}
