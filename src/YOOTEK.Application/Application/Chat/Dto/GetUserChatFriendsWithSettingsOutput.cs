using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Castle.Components.DictionaryAdapter;
using Yootek.Dto.Interface;
using Yootek.Friendships.Dto;

namespace Yootek.Chat.Dto
{
    public class GetUserChatFriendsWithSettingsOutput
    {
        public DateTime ServerTime { get; set; }

        public List<ChatFriendOrRoomDto> Friends { get; set; }
        public long SenderId { get; set; }
        public GetUserChatFriendsWithSettingsOutput()
        {
            Friends = new EditableList<ChatFriendOrRoomDto>();
        }
    }
}