using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(PhieuGiaoToTaiSan))]
    public class PhieuGiaoToTaiSanDto : PhieuGiaoToTaiSan
    {
        		public string PhieuGiaoTaiSanText { get; set; }
		public string TaiSanText { get; set; }
    }    
    public class GetAllPhieuGiaoToTaiSanInputDto : CommonInputDto
    {
		public  long PhieuGiaoTaiSanId { get; set; }
		public  long TaiSanId { get; set; }
    }
}
