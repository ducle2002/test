using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Apartments")]
    public class Apartment : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(2000)]
        public string ImageUrl { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        [StringLength(256)]
        public string Properties { get; set; }
        public decimal? Area { get; set; }
        public long? FloorId { get; set; }
        public long? BlockId { get; set; }
        public long? TypeId { get; set; }
        public long? StatusId { get; set; }
        public string Description { get; set; }
        [StringLength(100)]
        public string ProvinceCode { get; set; }
        [StringLength(100)]
        public string DistrictCode { get; set; }
        [StringLength(100)]
        public string WardCode { get; set; }
        public string Address { get; set; }
        public string BillConfig { get; set; }
    }

    public enum StatusApartment
    {
        // căn hộ
        RENTED = 1,   // đã cho thuê
        AVAILABLE = 2,    // sẵn sàng cho thuê hoặc bán
        REPAIRS = 3,    // đang sửa chữa
        SOLD = 4,    // đã bán
        UNAVAILABLE = 5,     // tạm thời không thể sử dụng 


        IN_USE = 13,  // đang được sử dụng
        // mảnh đất
        /*UNDEVELOPED = 11,   // chưa được sử dụng hoặc phát triển
        UNDER_DEVELOPMENT = 12,    // đang được phát triển
        IN_USE = 13,    // ĐANG ĐƯỢC SỬ DỤNG*/
    }
    public enum ClassifyApartment
    {
        APARTMENT = 1,  // Căn hộ
        OFFICE = 2,   // Văn phòng
        SHOPPINGMALL = 3,
        SHOPHOUSE = 4,
        OTHER = 5,
    }
}
