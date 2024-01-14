using Yootek.Common;
using Yootek.Organizations.Interface;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    public class GetAllBlockTowerInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public OrderByBlockTower? OrderBy { get; set; }
    }

    public enum OrderByBlockTower
    {
        [FieldName("DisplayName")]
        DISPLAY_NAME = 1,
        [FieldName("Code")]
        CODE = 2
    }
}