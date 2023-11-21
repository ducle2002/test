using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services
{
    [AutoMap(typeof(NhomTaiSan))]
    public class NhomTaiSanDto : NhomTaiSan
    {
        public string ParentText { get; set; }
        public string MaHeThongText { get; set; }
    }
    public class GetAllNhomTaiSanInputDto : CommonInputDto
    {
        public long ParentId { get; set; }
        public long MaHeThongId { get; set; }
        public FieldSortNhomTaiSan? OrderBy { get; set; }
    }
    public enum FieldSortNhomTaiSan
    {
        [FieldName("Code")]
        CODE = 1,
        [FieldName("Title")]
        TITLE = 2,
        [FieldName("MaHeThongId")]
        MAHETHONG = 3,       
        [FieldName("CreationTime")]
        CREATION_TIME = 4,

    }
}
