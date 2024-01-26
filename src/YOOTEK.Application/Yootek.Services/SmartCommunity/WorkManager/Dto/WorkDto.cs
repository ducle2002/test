using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.Common;

namespace Yootek.Services
{
    public class GetListWorkInput : CommonInputDto
    {
        public EWorkStatus? Status { get; set; }
        public FormIdWork? FormId { get; set; }
        public long? WorkTypeId { get; set; }
    }
}
