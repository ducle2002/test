using IMAX.Core.Dto;
using IMAX.Dto;

namespace IMAX.Organizations.Dto
{
    public class FindOrganizationUnitUsersInput : PagedAndFilteredInputDto
    {
        public long OrganizationUnitId { get; set; }
    }
    public class FindStaffForUnitInput: PagedAndFilteredInputDto
    {
        public long OrganizationUnitId { get; set; }

    }
}
