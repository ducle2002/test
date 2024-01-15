using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.EntityDb;
using Yootek.Common;

namespace Yootek.Services
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
