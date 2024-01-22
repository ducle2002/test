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
    public class GetAllDetailPagesInput : CommonInputDto
    {
        public string Language { get; set; }

    }
    [AutoMap(typeof(DetailPage))]
    public class DetailPageInput : DetailPage
    {

    }
}
