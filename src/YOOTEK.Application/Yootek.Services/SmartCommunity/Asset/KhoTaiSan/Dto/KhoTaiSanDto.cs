using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(KhoTaiSan))]
    public class KhoTaiSanDto : KhoTaiSan
    {
    }    
    public class GetAllKhoTaiSanInputDto : CommonInputDto
    {
    }
}
