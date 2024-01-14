using Yootek.Common;
using static Yootek.YootekServiceBase;

namespace Yootek.Services.Dto
{
    public class GetAllApartmentStatusInput : CommonInputDto
    {
        public OrderByApartmentStatus? OrderBy { get; set; }
    }

    public enum OrderByApartmentStatus
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("Code")]
        CODE = 2
    }
}
