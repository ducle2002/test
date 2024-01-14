using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Organizations;
using static Yootek.YootekServiceBase;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Building.Dto
{
    [AutoMap(typeof(AppOrganizationUnit))]
    public class TenantProjectDto : AppOrganizationUnit
    {
    }
    public class GetAllBuildingsInput : CommonInputDto
    {
        public long? UrbanId { get; set; }
        public OrderByBuilding? OrderBy { get; set; }
    }
    public enum OrderByBuilding
    {
        [FieldName("DisplayName")]
        DISPLAY_NAME = 1,
        [FieldName("ProjectCode")]
        PROJECT_CODE = 2
    }
    public class BuildingBasicDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? ImageUrl { get; set; }
        public long? UrbanId { get; set; }
        public string? UrbanName { get; set; }
        public string? UrbanCode { get; set; }
        public string? UrbanImageUrl { get; set; }
    }
}
