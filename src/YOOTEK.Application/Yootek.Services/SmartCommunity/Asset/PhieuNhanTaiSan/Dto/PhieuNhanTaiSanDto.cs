using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(PhieuNhanTaiSan))]
    public class PhieuNhanTaiSanDto : PhieuNhanTaiSan
    {
        public string KhoTaiSanText { get; set; }
        public string NguoiLapPhieuText { get; set; }
        public string KeToanText { get; set; }
        public string ThuKhoText { get; set; }
        public List<PhieuNhanToTaiSan> TaiSans { get; set; }
    }
    public class GetAllPhieuNhanTaiSanInputDto : CommonInputDto
    {
        public string Code { get; set; }
        public long KhoTaiSanId { get; set; }
        public long ThuKhoId { get; set; }
        public int TrangThai { get; set; }
    }
}
