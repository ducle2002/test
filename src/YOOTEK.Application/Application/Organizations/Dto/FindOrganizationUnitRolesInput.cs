using Yootek.Core.Dto;
using Yootek.Dto;

namespace Yootek.Organizations.Dto
{
    public class FindOrganizationUnitRolesInput : PagedAndFilteredInputDto
    {
        public long OrganizationUnitId { get; set; }
    }
}