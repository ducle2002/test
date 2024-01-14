
namespace Yootek.Application.Notifications.Dto
{
    public class GetAllProviderInput
    {
        public int? Type { get; set; }
        public int? GroupType { get; set; }
    }

    public class GetAllCitizenInput
    {
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

    public class GetBuildingInput
    {
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
    }
}
