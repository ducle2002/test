using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(PhieuXuatKhoToTaiSan))]
    public class PhieuXuatKhoToTaiSanDto : PhieuXuatKhoToTaiSan
    {
        public string PhieuXuatKhoText { get; set; }
        public string TaiSanText { get; set; }
    }
    public class GetAllPhieuXuatKhoToTaiSanInputDto : CommonInputDto
    {
        public long PhieuXuatKhoId { get; set; }
        public long TaiSanId { get; set; }
    }
}
