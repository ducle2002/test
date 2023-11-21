using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace IMAX.Core.Dto
{
    public class PagedAndFilteredInputDto : IPagedResultRequest
    {
        [Range(1, IMAXConsts.MaxPageSize)]
        public int MaxResultCount { get; set; }

        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }

        public string Filter { get; set; }

        public PagedAndFilteredInputDto()
        {
            MaxResultCount = IMAXConsts.DefaultPageSize;
        }
    }
}