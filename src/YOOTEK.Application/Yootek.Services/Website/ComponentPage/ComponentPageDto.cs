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
    [AutoMap(typeof(ComponentPage))]
    public class ComponentPageInputDto : ComponentPage
    {
    }

    public class GetAllComponentPagesDto : CommonInputDto
    {
        public string Language { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public int? Order { get; set; }
    }
}
