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
    [AutoMap(typeof(NewsWebImax))]
    public class NewsWebImaxDto : NewsWebImax
    {
    }
    public class GetAllNewsWebImaxInput : CommonInputDto
    {
        public int? Category { get; set; }
        public int? State { get; set; }
    }
}
