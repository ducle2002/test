using Yootek.Common;
using Yootek.Organizations.Interface;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    public class GetAllFloorInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public OrderByFloor? OrderBy { get; set; }
    }
    public enum OrderByFloor
    {
        [FieldName("DisplayName")]
        DISPLAY_NAME = 1,
        [FieldName("CreationTime")]
        CREATION_TIME = 2
    }
}
