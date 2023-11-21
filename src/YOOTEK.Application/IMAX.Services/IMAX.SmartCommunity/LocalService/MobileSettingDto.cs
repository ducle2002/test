using Abp.AutoMapper;
using IMAX.Common;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    [AutoMap(typeof(LocalService))]
    public class LocalServiceDto : LocalService
    {
    }
    public class GetAllLocalServiceInput : CommonInputDto
    {
        
        public long? OrganizationUnitId { get; set; }
        public GroupType? GroupType { get; set; }

        public GetAllLocalServiceInput()
        {
            MaxResultCount = 1000;
        }
    }
}
