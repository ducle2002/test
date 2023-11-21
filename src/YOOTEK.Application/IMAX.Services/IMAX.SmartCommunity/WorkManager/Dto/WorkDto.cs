using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos;
using IMAX.Common;

namespace IMAX.Services
{
    public class GetListWorkInput : CommonInputDto
    {
        public EWorkStatus? Status { get; set; }
        public FormIdWork? FormId { get; set; }
        public long? WorkTypeId { get; set; }
    }
}
