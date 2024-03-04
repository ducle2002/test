using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common;

namespace YOOTEK.Application.OrganizationUnitChat.Dto
{
    public class GetOrganizationUnitIdByUserInput : CommonInputDto
    {
        public long? UrbanId { get; set; }
    }
}
