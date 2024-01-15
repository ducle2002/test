using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    [AutoMap(typeof(PhieuXuatKho))]
    public class PhieuXuatKhoDto : PhieuXuatKho
    {
        public string KhoTaiSanText { get; set; }
        public string NguoiLapPhieuText { get; set; }
        public string KeToanText { get; set; }
        public string ThuKhoText { get; set; }
        public List<PhieuXuatKhoToTaiSan> TaiSans { get; set; }
    }
    public class GetAllPhieuXuatKhoInputDto : CommonInputDto
    {
        public string Code { get; set; }
        public long KhoTaiSanId { get; set; }
        public long ThuKhoId { get; set; }
        public int TrangThai { get; set; }
        public FieldSortPhieuXuatKho? OrderBy { get; set; }
    }

    public enum FieldSortPhieuXuatKho
    {
        [FieldName("Id")]
        ID = 1,
        [FieldName("Code")]
        CODE = 2,
        [FieldName("KhoTaiSanId")]
        KHOTAISANID = 3,
        [FieldName("TrangThai")]
        TRANGTHAI = 4,
        [FieldName("CreationTime")]
        CREATION_TIME = 5,
    }
}
