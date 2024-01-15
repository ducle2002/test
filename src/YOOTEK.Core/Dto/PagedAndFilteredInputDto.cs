using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Core.Dto
{
    public class PagedAndFilteredInputDto : IPagedResultRequest
    {
        [Range(1, YootekConsts.MaxPageSize)]
        public int MaxResultCount { get; set; }

        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }

        public string Filter { get; set; }

        public PagedAndFilteredInputDto()
        {
            MaxResultCount = YootekConsts.DefaultPageSize;
        }
    }
}