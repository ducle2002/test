

using Yootek.Common;

namespace Yootek.Services.SmartSocial.Advertisements
{
    public class GetAllAmenitiesItemInput : CommonInputDto
    {
        public int ProviderId { get; set; }
        public GetAmenitiesItemFormId FormId { get; set; }
        public int BusinessType { get; set; }
        public bool IsPagination { get; set; }

    }

    public enum GetAmenitiesItemFormId
    {

    }

}
