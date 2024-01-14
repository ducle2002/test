using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.Chat.BusinessChat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.BusinessChat.Dto
{
    [AutoMapFrom(typeof(UserProviderFriendship))]
    public class ProviderFriendshipDto: EntityDto<long>
    {
        public long UserId { get; set; }
        public long FriendUserId { get; set; }
        public long ProviderId { get; set; }
        public string FriendName { get; set; }
        public int? FriendTenantId { get; set; }
        public int? TenantId { get; set; }
        public string FriendImageUrl { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastMessageDate { get; set; }
        public bool IsOnline { get; set; }
        public int UnreadMessageCount { get; set; }
        public BusinessChatMessage LastMessage { get; set; }
    }


    [AutoMapFrom(typeof(UserProviderFriendship))]
    public class UserFriendshipDto : EntityDto<long>
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public int? TenantId { get; set; }
        public string UserImageUrl { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastMessageDate { get; set; }
        public bool IsOnline { get; set; }
        public int UnreadMessageCount { get; set; }
        public BusinessChatMessage LastMessage { get; set; }
    }
}
