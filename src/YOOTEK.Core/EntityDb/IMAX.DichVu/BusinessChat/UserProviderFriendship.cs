using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.Chat.BusinessChat
{
    [Table("Business.UserProviderFriendship")]
    public class UserProviderFriendship : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public long UserId { get; set; }
        public long FriendUserId { get; set; }
        public long ProviderId { get; set; }
        [StringLength(1000)]
        public string FriendName { get; set; }
        public int? FriendTenantId { get; set; }
        public int? TenantId { get; set; }
        public string FriendImageUrl { get; set; }
        public DateTime CreationTime { get; set; }
        public bool IsShop { get; set; }
    }
}
