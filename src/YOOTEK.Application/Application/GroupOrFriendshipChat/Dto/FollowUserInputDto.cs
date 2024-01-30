using System.ComponentModel.DataAnnotations;

namespace YOOTEK.Application.GroupOrFriendshipChat.Dto;

public class FollowUserInputDto
{
    [Range(1, long.MaxValue)]
    public long UserId { get; set; }

    public int? TenantId { get; set; }
}