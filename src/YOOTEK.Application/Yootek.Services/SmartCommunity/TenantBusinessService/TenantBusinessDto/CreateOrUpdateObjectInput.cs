using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [AutoMap(typeof(ObjectPartner))]
    public class CreateOrUpdateObjectInput : ObjectPartner
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public string QueryKey { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PropertyHistories { get; set; }
        public string Properties { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public int? State { get; set; }
        public string StateProperties { get; set; }
        public bool? IsAdminCreate { get; set; }
        public string Address { get; set; }
       // public long? CreatorUserId { get; set; }
    }
}
