using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
namespace IMAX.Services
{
    [AutoMap(typeof(LoaiTaiSan))]
    public class LoaiTaiSanDto : LoaiTaiSan
    {
    }    
    public class GetAllLoaiTaiSanInputDto : CommonInputDto
    {
    }
}
