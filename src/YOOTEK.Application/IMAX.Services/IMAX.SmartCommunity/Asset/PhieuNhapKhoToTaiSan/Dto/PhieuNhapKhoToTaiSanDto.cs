using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
namespace IMAX.Services
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
