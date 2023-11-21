using Abp.AutoMapper;
using IMAX.Chat;
using IMAX.Chat.Dto;
using IMAX.Dto.Interface;
using IMAX.EntityDb;
using IMAX.Friendships;
using IMAX.Friendships.Cache;
using System;


namespace IMAX.Friendships.Dto
{
    [AutoMapFrom(typeof(FriendCacheItem), typeof(Friendship))]
    public class FriendDto : ChatFriendOrRoomDto
    {
        public long FriendUserId { get; set; }
        public int? FriendTenantId { get; set; }
        public string FriendUserName { get; set; }
        public string FriendTenancyName { get; set; }
        public string FriendProfilePictureId { get; set; }
        public FriendshipState State { get; set; }
        public int StateAddFriend { get; set; }
        public ChatMessage LastMessage { get; set; }
        public bool? IsOrganizationUnit { get; set; }
        public Citizen FriendInfo { get; set; }
    }
}
