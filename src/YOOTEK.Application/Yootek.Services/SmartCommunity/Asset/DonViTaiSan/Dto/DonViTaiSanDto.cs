using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
namespace Yootek.Services
{
    [AutoMap(typeof(DonViTaiSan))]
    public class DonViTaiSanDto : DonViTaiSan
    {
    }    
    public class GetAllDonViTaiSanInputDto : CommonInputDto
    {
    }
}
