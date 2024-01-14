using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(NhaSanXuat))]
    public class NhaSanXuatDto : NhaSanXuat
    {
    }    
    public class GetAllNhaSanXuatInputDto : CommonInputDto
    {
    }
}
