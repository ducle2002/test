using Yootek.Core.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.Dto
{

    public class FindUrbanInput : PagedAndFilteredInputDto
    {
        public long OrganizationUnitId { get; set; }
        public long ParentId { get; set; }
        public int Type { get; set; }
    }
    public class UrbanToOrganizationUnitInput
    {
        public long[] UrbanIds { get; set; }

        [Range(1, long.MaxValue)]
        public long OrganizationUnitId { get; set; }
    }
    public class DeleteUrbanFromOrganizationInput
    {
        public long UrbanId { get; set; }
        public long OrganizationId { get; set; }
    }  
}
