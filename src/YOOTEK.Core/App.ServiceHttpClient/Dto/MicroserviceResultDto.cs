using Abp.Application.Services.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using System;

namespace Yootek.App.ServiceHttpClient.Dto
{
    public class MicroserviceResultDto<T>
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public T Result { get; set; }

        public static implicit operator MicroserviceResultDto<T>(MicroserviceResultDto<PagedResultDto<WorkDto>> v)
        {
            throw new NotImplementedException();
        }
    }
}