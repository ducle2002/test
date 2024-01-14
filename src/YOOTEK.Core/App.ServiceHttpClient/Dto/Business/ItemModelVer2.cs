using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class ItemModelVer2 : Entity<long>, IDeletionAudited, IMayHaveTenant
    {
        public string Name { get; set; }
        public int? TenantId { get; set; }
        public long ItemId { get; set; }
        public bool IsDefault { get; set; }
        public string? Sku { get; set; }
        public int Stock { get; set; }
        public int Sales { get; set; }
        public double OriginalPrice { get; set; }
        public double CurrentPrice { get; set; }
        public string? ImageUrl { get; set; }
        public List<int> TierIndex { get; set; } = new List<int>();
        // 3 trường ko map sang PItemMOdel
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public long? DeleterUserId { get; set; }
    }
}