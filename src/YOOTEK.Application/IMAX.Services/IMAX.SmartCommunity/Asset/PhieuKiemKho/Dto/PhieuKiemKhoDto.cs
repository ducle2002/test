using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
namespace IMAX.Services
{
    [AutoMap(typeof(PhieuKiemKho))]
    public class PhieuKiemKhoDto : PhieuKiemKho
    {
        public string KhoTaiSanText { get; set; }
        public string NguoiLapPhieuText { get; set; }
        public string NguoiXacNhanText { get; set; }
        public List<PhieuKiemKhoToTaiSan> TaiSans { get; set; }
    }
    public class GetAllPhieuKiemKhoInputDto : CommonInputDto
    {
        public string Code { get; set; }
        public long KhoTaiSanId { get; set; }
    }
}
