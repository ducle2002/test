using Yootek.Common;
using Yootek.Yootek.EntityDb.Yootek.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.Metrics.Dto
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
