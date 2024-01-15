using Yootek.Chat;
using Yootek.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.Chat.Dto
{
    public class TenantProjectChatDto
    {
        public string Name { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string ImageUrl { get; set; }
        public APP_ORGANIZATION_TYPE Type { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime LastMessageDate { get; set; }
        public bool IsOnline { get; set; }
        public int UnreadMessageCount { get; set; }
        public ChatMessage LastMessage { get; set; }
        public string Description { get; set; }
        public bool IsManager { get; set; }
    }
}
