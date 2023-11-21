using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
namespace IMAX.Services
{
    [AutoMap(typeof(NhaSanXuat))]
    public class NhaSanXuatDto : NhaSanXuat
    {
    }    
    public class GetAllNhaSanXuatInputDto : CommonInputDto
    {
    }
}
