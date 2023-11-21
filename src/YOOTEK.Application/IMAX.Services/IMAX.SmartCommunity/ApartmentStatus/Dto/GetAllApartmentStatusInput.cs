using IMAX.Common;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services.Dto
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
