using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    public class GetAllDocumentsInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public bool? Arrange { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? DocumentTypeId { get; set; }
        public DocumentScope? Scope { get; set; }
        public OrderByDocument? OrderBy { get; set; }

    }
    public enum OrderByDocument
    {
        [FieldName("DisplayName")]
        DISPLAY_NAME = 1,
        [FieldName("BuildingId")]
        BUILDING_ID = 2,
    }
    public class GetAllDocumentsByUserInput : CommonInputDto
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? TypeId { get; set; }
        public OrderByDocument? OrderBy { get; set; }
    }

}
