using Abp.AutoMapper;
using Yootek.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class BuildingByUserDto : AppOrganizationUnit
    {
        public BuildingByUserDto Urban { get; set; }
        public long? UrbanId { get; set; }

    }

}
