using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(LoaiTaiSan))]
    public class LoaiTaiSanDto : LoaiTaiSan
    {
    }    
    public class GetAllLoaiTaiSanInputDto : CommonInputDto
    {
    }
}
