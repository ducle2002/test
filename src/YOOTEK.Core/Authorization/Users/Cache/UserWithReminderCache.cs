using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Authorization.Users.Cache
{
    public class UserWithReminderCacheItem
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }

        public List<ReminderCacheItem> Reminders { get; set; }
    }
}
