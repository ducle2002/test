using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.Dto
{
    public class DistrictInputDto
    {
        public string? ProvinceId { get; set; }
    }

    public class WardInputDto
    {
        public string? DistrictId { get; set; }
    }
}
