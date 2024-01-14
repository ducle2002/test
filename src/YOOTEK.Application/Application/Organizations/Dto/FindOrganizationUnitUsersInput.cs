using Yootek.Core.Dto;
using Yootek.Dto;

namespace Yootek.Organizations.Dto
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
