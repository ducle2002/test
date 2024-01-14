using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Authorization.Users.Cache
{
    [AutoMapFrom(typeof(Reminder))]
    public class ReminderCacheItem
    {
        public const string GetAll = "GetAllReminderCacheItem";
        public const string CacheName = "ReminderCacheItem";

        public long Id { get; set; }
        public DateTime TimeFinish { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int State { get; set; }
        public int? Level { get; set; }
        public bool? IsNotify { get; set; }
        public bool? IsLoop { get; set; }
        public string LoopDay { get; set; }
        public List<int> LoopDays { get; set; } = new List<int>();
        public int? TenantId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }

    }
}
