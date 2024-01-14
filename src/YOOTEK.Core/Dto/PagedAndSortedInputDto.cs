using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Core.Dto
{
    public class PagedAndSortedInputDto : IPagedResultRequest, ISortedResultRequest
    {
        public string Sorting { get; set; }
        [Range(1, YootekConsts.MaxPageSize)]
        public int MaxResultCount { get; set; }

        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }
        public PagedAndSortedInputDto()
        {
            MaxResultCount = YootekConsts.DefaultPageSize;
        }
    }
}