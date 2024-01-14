using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(PhieuKiemKhoToTaiSan))]
    public class PhieuKiemKhoToTaiSanDto : PhieuKiemKhoToTaiSan
    {
        		public string PhieuKiemKhoText { get; set; }
		public string TaiSanText { get; set; }
    }    
    public class GetAllPhieuKiemKhoToTaiSanInputDto : CommonInputDto
    {
		public  long PhieuKiemKhoId { get; set; }
		public  long TaiSanId { get; set; }
    }
}
