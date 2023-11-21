using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace IMAX.Core.Dto
{
    public class PagedAndSortedInputDto : IPagedResultRequest, ISortedResultRequest
    {
        public string Sorting { get; set; }
        [Range(1, IMAXConsts.MaxPageSize)]
        public int MaxResultCount { get; set; }

        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }
        public PagedAndSortedInputDto()
        {
            MaxResultCount = IMAXConsts.DefaultPageSize;
        }
    }
}