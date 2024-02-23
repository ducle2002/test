using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace Yootek.Friendships
{
    [Table("AppFriendships")]
    public class Friendship : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long FriendUserId { get; set; }

        public int? FriendTenantId { get; set; }

        [Required]
        [MaxLength(AbpUserBase.MaxUserNameLength)]
        public string FriendUserName { get; set; }

        public string FriendTenancyName { get; set; }
        public string FriendImageUrl { get; set; }
        public bool? IsSender { get; set; }
        public bool? IsOrganizationUnit { get; set; }

        public FriendshipState State { get; set; }
        public FollowState? FollowState { get; set; }
        public DateTime CreationTime { get; set; }

        public Friendship(UserIdentifier user, UserIdentifier probableFriend, string probableFriendTenancyName, string probableFriendUserName, string friendImageUrl, FriendshipState state, FollowState? followState = null, bool isSender = false, bool isOrganizationUnit = false)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (probableFriend == null)
            {
                throw new ArgumentNullException(nameof(probableFriend));
            }

            if (!Enum.IsDefined(typeof(FriendshipState), state))
            {
                throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(FriendshipState));
            }
            

            UserId = user.UserId;
            TenantId = user.TenantId;
            FriendUserId = probableFriend.UserId;
            FriendTenantId = probableFriend.TenantId;
            FriendTenancyName = probableFriendTenancyName;
            FriendUserName = probableFriendUserName;
            State = state;
            FollowState = followState;
            FriendImageUrl = friendImageUrl;
            IsSender = isSender;
            CreationTime = Clock.Now;
            IsOrganizationUnit = isOrganizationUnit;
        }

        protected Friendship()
        {

        }
    }
}
