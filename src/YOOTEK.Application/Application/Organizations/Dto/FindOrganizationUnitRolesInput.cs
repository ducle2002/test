using IMAX.Core.Dto;
using IMAX.Dto;

namespace IMAX.Organizations.Dto
{
    public class FindOrganizationUnitRolesInput : PagedAndFilteredInputDto
    {
        public long OrganizationUnitId { get; set; }
    }
}