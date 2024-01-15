using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    [AutoMap(typeof(MaHeThong))]
    public class MaHeThongDto : MaHeThong
    {
    }
    public class GetAllMaHeThongInputDto : CommonInputDto
    {
        public string Keyword { get; set; }
        public FieldSortMaHeThong? OrderBy { get; set; }
    }
    public enum FieldSortMaHeThong
    {
        [FieldName("Id")]
        ID = 1,
    }
}
