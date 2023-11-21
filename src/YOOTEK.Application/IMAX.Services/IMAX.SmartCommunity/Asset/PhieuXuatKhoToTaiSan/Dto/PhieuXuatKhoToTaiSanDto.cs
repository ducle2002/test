using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
namespace IMAX.Services
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
