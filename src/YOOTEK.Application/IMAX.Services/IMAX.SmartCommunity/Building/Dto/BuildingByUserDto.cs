using Abp.AutoMapper;
using IMAX.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.SmartCommunity.Building.Dto
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class BuildingByUserDto : AppOrganizationUnit
    {
        public BuildingByUserDto Urban { get; set; }
        public long? UrbanId { get; set; }

    }

}
