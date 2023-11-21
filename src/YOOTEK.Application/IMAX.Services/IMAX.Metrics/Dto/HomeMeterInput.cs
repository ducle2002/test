using IMAX.Common;
using IMAX.IMAX.EntityDb.IMAX.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.Metrics.Dto
{
    public class HomeMeterInputDto: HomeMeter
    {
    }

    public class HomeMeterQueryDto: CommonInputDto
    {
        public int? Type { get; set; }
        public string ApartmentCode { get; set; }
    }
}
