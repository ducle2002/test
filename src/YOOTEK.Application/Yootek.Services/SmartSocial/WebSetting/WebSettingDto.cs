using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [AutoMap(typeof(Introduce))]
    public class IntroduceDto
    {
        public long? Id { get; set; }
        public int? TenantId { get; set; }
        public string Detail { get; set; }
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
