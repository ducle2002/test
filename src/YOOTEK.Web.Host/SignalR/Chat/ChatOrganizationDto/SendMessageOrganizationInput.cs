using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Chat
{
    public class SendMessageOrganizationInput
    {
        public int? TenantId { get; set; }
        public long? MessageRepliedId { get; set; }
        public long UserId { get; set; }

        public long SenderId { get; set; }

        public string UserName { get; set; }

        public string TenancyName { get; set; }

        public string SenderImageUrl { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public int TypeMessage { get; set; }
        public bool IsAdmin { get; set; }
    }
}
