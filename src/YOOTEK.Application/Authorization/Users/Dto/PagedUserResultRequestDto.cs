using Abp.Application.Services.Dto;

namespace IMAX.Users.Dto
{
    //custom PagedResultRequestDto
    public class PagedUserResultRequestDto : PagedResultRequestDto
    {
        // [Range(1, IMAXConsts.MaxPageSize)]
        // public int MaxResultCount { get; set; }

        // [Range(0, int.MaxValue)]
        // public int SkipCount { get; set; }

        // public PagedUserResultRequestDto()
        // {
        //     MaxResultCount = IMAXConsts.DefaultPageSize;
        // }
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}
