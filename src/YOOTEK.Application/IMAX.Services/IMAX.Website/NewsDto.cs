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
