using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(Province))]
    public class ProvinceDto : Province
    {

    }

    [AutoMap(typeof(District))]
    public class DistrictDto : District
    {

    }

    [AutoMap(typeof(Ward))]
    public class WardDto : Ward
    {

    }
}
