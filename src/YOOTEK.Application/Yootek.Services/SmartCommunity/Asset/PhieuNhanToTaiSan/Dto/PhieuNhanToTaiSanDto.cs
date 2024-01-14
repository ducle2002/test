using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(PhieuNhanToTaiSan))]
    public class PhieuNhanToTaiSanDto : PhieuNhanToTaiSan
    {
        		public string PhieuNhanTaiSanText { get; set; }
		public string TaiSanText { get; set; }
    }    
    public class GetAllPhieuNhanToTaiSanInputDto : CommonInputDto
    {
		public  long PhieuNhanTaiSanId { get; set; }
		public  long TaiSanId { get; set; }
    }
}
