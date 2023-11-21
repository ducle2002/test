using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
using IMAX.Services.Dto;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services
{
    [AutoMap(typeof(PhieuNhapKho))]
    public class PhieuNhapKhoDto : PhieuNhapKho
    {
        public string KhoTaiSanText { get; set; }
        public string NguoiLapPhieuText { get; set; }
        public string KeToanText { get; set; }
        public string ThuKhoText { get; set; }
        public List<PhieuNhapKhoToTaiSan> TaiSans { get; set; }
    }
    public class GetAllPhieuNhapKhoInputDto : CommonInputDto
    {
        public string Code { get; set; }
        public long KhoTaiSanId { get; set; }
        public long NguoiLapPhieuId { get; set; }
        public long KeToanId { get; set; }
        public long ThuKhoId { get; set; }
        public int TrangThai { get; set; }
        public FieldSortPhieuNhapKho? OrderBy { get; set; }
    }

    public enum FieldSortPhieuNhapKho
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
