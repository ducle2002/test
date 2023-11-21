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
    public class GetAllDetailPagesInput : CommonInputDto
    {
        public string Language { get; set; }

    }
    [AutoMap(typeof(DetailPage))]
    public class DetailPageInput : DetailPage
    {

    }
}
