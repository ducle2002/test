using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMAX.EntityDb;
using IMAX.Common;

namespace IMAX.Services
{
    [AutoMap(typeof(MaintenancePlan))]
    public class PlanMaterialDto : MaintenancePlan
    {
    }


    public class GetAllPlanMaterialInput : CommonInputDto
    {
        public DateTime? DayCreat { get; set; }
        public int? State { get; set; }
    }
}
