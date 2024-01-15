using Yootek.Core.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Application.Organizations.Dto
{
    public class FindUrbanInput : PagedAndFilteredInputDto
    {
        public long OrganizationUnitId { get; set; }
        public long ParentId { get; set; }
        public int Type { get; set; }
    }
    public class BuildingToOrganizationUnitInput
    {
        public long[] BuildingIds { get; set; }

        [Range(1, long.MaxValue)]
        public long OrganizationUnitId { get; set; }
    }
    public class DeleteBuildingFromOrganizationInput
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public long BuildingId { get; set; }
    }
}
