using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Yootek.Services
{
    [AutoMap(typeof(LocalService))]
    public class LocalServiceDto : LocalService
    {
    }
    public class GetAllLocalServiceInput : CommonInputDto
    {
        
        public long? OrganizationUnitId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public GroupType? GroupType { get; set; }

        public GetAllLocalServiceInput()
        {
            MaxResultCount = 1000;
        }
    }

    public class GetAllLocalServicesByUserInput : CommonInputDto
    {
        public long? Type { get; set; }
        public long? GroupType { get; set; }
    }
    public class GetAllLocalServiceTypeInput : CommonInputDto
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public GroupType? GroupType { get; set; }

        public GetAllLocalServiceTypeInput()
        {
            MaxResultCount = 1000;
        }
    }
}
