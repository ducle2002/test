using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    public class ListHotlineInputDto : CommonInputDto
    {
        //public long Id { get; set; }
        public long? OrganizationUnitId { get; set; }
        public OrderByHotline? OrderBy { get; set; }
        //public DateTime? FromDay { get; set; }
        //public DateTime? ToDay { get; set; }
    }

    public class ListHotlineInputForUserDto : CommonInputDto
    {
        public long UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public OrderByHotline? OrderBy { get; set; }
    }

    public enum OrderByHotline
    {
        [FieldName("Name")]
        NAME = 1
    }

    [AutoMap(typeof(Hotlines))]
    public class HotlineOutputDto : Hotlines
    {
    }
    [AutoMap(typeof(Hotlines))]
    public class GetInputHotline : Hotlines
    {
    }
    [AutoMap(typeof(Hotlines))]
    public class HotlineInputDto : Hotlines
    {

    }

    public class HotlineDto
    {
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
