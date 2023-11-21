using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Common
{
    public class CommonInputDto : IPagedResultRequest, IFilteredInputDto, ISortedInputDto
    {
        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }
        [Range(1, IMAXConsts.MaxPageSize)]
        public int MaxResultCount { get; set; }
        [CanBeNull] public string Keyword { get; set; }
        public SortBy SortBy { get; set; }

        public CommonInputDto()
        {
            MaxResultCount = IMAXConsts.DefaultPageSize;
            SortBy = SortBy.ASC;
        }
    }
}
