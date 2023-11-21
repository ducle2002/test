using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
namespace IMAX.Services
{
    [AutoMap(typeof(DonViTaiSan))]
    public class DonViTaiSanDto : DonViTaiSan
    {
    }    
    public class GetAllDonViTaiSanInputDto : CommonInputDto
    {
    }
}
