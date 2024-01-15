using Yootek.Common;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Friendships.Dto
{
    public class CreateFriendshipRequestByUserNameInput
    {
        [Required(AllowEmptyStrings = true)]
        public string TenancyName { get; set; }

        public string UserName { get; set; }
    }

    public class FindUserToAddFriendInput : CommonInputDto
    {
        [Required(AllowEmptyStrings = true)]
        public string TenancyName { get; set; }

        public int? TenantId { get; set; }
    }
}