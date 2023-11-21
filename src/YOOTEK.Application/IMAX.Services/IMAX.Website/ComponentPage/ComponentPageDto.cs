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
