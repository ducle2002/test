using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(PhieuNhapKhoToTaiSan))]
    public class PhieuNhapKhoToTaiSanDto : PhieuNhapKhoToTaiSan
    {
        public string PhieuNhapKhoText { get; set; }
        public string TaiSanText { get; set; }
    }
    public class GetAllPhieuNhapKhoToTaiSanInputDto : CommonInputDto
    {
        public long PhieuNhapKhoId { get; set; }
        public long TaiSanId { get; set; }
    }
}
