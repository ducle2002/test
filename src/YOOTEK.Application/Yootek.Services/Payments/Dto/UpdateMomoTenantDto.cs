using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.Payments.Dto
{
    public class UpdateMomoTenantDto
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string PartnerCode { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        [CanBeNull] public string StoreName { get; set; }
        [CanBeNull] public string StoreId { get; set; }
    }
}