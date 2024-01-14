using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Yootek.EntityDb.Clb.Hotlines;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    public class ListClbHotlineInputDto : CommonInputDto
    {
        public OrderByClbHotline? OrderBy { get; set; }
    }

    public enum OrderByClbHotline
    {
        [FieldName("Name")]
        NAME = 1
    }

    [AutoMap(typeof(ClbHotlines))]
    public class ClbHotlineOutputDto : ClbHotlines
    {
    }
    [AutoMap(typeof(ClbHotlines))]
    public class ClbGetInputHotline : ClbHotlines
    {
    }
    [AutoMap(typeof(ClbHotlines))]
    public class ClbHotlineInputDto : ClbHotlines
    {

    }

    public class ClbHotlineDto
    {
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}