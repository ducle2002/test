using Abp.AutoMapper;
using IMAX.Common;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    [AutoMap(typeof(Introduce))]
    public class IntroduceDto : Introduce
    {
    }

    public class GetAllIntroduceInput : CommonInputDto
    {
        public long? Id { get; set; }
    }
    public class GetIntroduceInput
    {
        public int? TenantId { get; set; }
    }
}
