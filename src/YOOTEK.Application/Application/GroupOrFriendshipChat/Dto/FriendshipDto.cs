using Abp.AutoMapper;
using Yootek.Chat;
using Yootek.Chat.Dto;
using Yootek.Dto.Interface;
using Yootek.EntityDb;
using Yootek.Friendships;
using Yootek.Friendships.Cache;
using System;


namespace Yootek.Friendships.Dto
{
    [AutoMapFrom(typeof(FriendCacheItem), typeof(Friendship))]
    public class FriendDto : ChatFriendOrRoomDto
    {
        public long FriendUserId { get; set; }
        public int? FriendTenantId { get; set; }
        public string FriendUserName { get; set; }
        public string FriendTenancyName { get; set; }
        public string FriendImageUrl { get; set; }
        public FriendshipState State { get; set; }
        public FollowState? FollowState { get; set; }
        public int StateAddFriend { get; set; }
        public ChatMessage LastMessage { get; set; }
        public bool? IsOrganizationUnit { get; set; }
        public Citizen FriendInfo { get; set; }
        public bool? IsSender { get; set; }
    }
}
