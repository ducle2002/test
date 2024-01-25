using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(PhieuGiaoTaiSan))]
    public class PhieuGiaoTaiSanDto : PhieuGiaoTaiSan
    {
        public string KhoTaiSanText { get; set; }
        public string NguoiLapPhieuText { get; set; }
        public string KeToanText { get; set; }
        public string ThuKhoText { get; set; }
        public List<PhieuGiaoToTaiSan> TaiSans { get; set; }
    }
    public class GetAllPhieuGiaoTaiSanInputDto : CommonInputDto
    {
        public string Code { get; set; }
        public long KhoTaiSanId { get; set; }
        public long ThuKhoId { get; set; }
        public int TrangThai { get; set; }
    }
}
