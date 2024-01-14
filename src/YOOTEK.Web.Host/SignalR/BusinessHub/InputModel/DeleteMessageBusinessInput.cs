using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.SignalR
{
    public class DeleteMessageBusinessInput
    {
        public int? TagertTenantId { get; set; }
        public Guid SharedMessageId { get; set; }
        public long TagertUserId { get; set; }
    }
}
