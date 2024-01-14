using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.Guides
{
    [AutoMap(typeof(Handbook))]
    public class HandbookDto : Handbook
    {
    }

    public class FindInHandbook : CommonInputDto
    {
        public long? CreatorUserId { get; set; }
        public int? FormId { get; set; }
        public long OrganizationUnitId { get; set; }
    }
}
