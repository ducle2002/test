using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Yootek.EntityDb
{
    [Table("UserParking")]
    public class UserParking : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long UserId { get; set; }
        public long ParkingId { get; set; }
        [CanBeNull] public string Properties { get; set; }
        [CanBeNull] public string ImageUrls { get; set; }
        public long? VehicleId { get; set; }

        [NotMapped]
        [CanBeNull]
        public string[] ListImageUrl
        {
            get
            {
                if (ImageUrls != null) return JsonConvert.DeserializeObject<string[]>(ImageUrls);
                return new string[] { };
            }
            set => ImageUrls = JsonConvert.SerializeObject(value);
        }
    }
}