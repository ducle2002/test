using Abp;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using Yootek.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Yootek.GroupChats
{

    [Table("AppGroupChats")]
    public class GroupChat : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string Name { get; set; }
        [StringLength(256)]
        public string GroupChatCode { get; set; }

        public string GroupImageUrl { get; set; }

        public DateTime CreationTime { get; set; }

        public GroupChat(UserIdentifier user, string groupName, string groupImageUrl)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            TenantId = user.TenantId;
            GroupImageUrl = groupImageUrl;
            Name = groupName;
            CreationTime = Clock.Now;
        }

        protected GroupChat()
        {

        }
    }
}
