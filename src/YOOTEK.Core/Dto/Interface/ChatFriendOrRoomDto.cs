using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Dto.Interface
{
    public class ChatFriendOrRoomDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public bool IsGroupOrFriend { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastMessageDate { get; set; }
        public bool IsOnline { get; set; }
        public bool IsBlockOrDelete { get; set; }
        public int UnreadMessageCount { get; set; }
        public ChatFriendOrRoomDto()
        {
            IsGroupOrFriend = false;
        }
    }
}
