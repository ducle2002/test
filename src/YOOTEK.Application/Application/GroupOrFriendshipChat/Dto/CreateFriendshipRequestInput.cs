using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Yootek.Common;

namespace Yootek.Friendships.Dto
{
    public class CreateFriendshipRequestInput
    {
        [Range(1, long.MaxValue)]
        public long UserId { get; set; }

        public int? TenantId { get; set; }
    }

    public class GetAllFriendInput : CommonInputDto
    {

    }
}