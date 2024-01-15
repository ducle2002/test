using Yootek.Common;

namespace Yootek.Services.SmartSocial.Advertisements 
{ 
    public class GetAllAmenitiesAttributeInput : CommonInputDto
    {
        public int? BusinessType { get; set; }
        public bool IsPagination { get; set; }

    }
}
