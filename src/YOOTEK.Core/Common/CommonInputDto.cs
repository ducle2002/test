using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Common
{
    public class CommonInputDto : IPagedResultRequest, IFilteredInputDto, ISortedInputDto
    {
        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }
        [Range(1, YootekConsts.MaxPageSize)]
        public int MaxResultCount { get; set; }
        [CanBeNull] public string Keyword { get; set; }
        public SortBy SortBy { get; set; }

        public CommonInputDto()
        {
            MaxResultCount = YootekConsts.DefaultPageSize;
            SortBy = SortBy.ASC;
        }
    }
}
