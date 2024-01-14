using Yootek.Common;
using static Yootek.YootekServiceBase;

namespace Yootek.Services.Dto
{
    public class GetAllApartmentTypeInput : CommonInputDto
    {
        public OrderByApartmentType? OrderBy { get; set; }
    }

    public enum OrderByApartmentType
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("Code")]
        CODE = 2
    }
}
