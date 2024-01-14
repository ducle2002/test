using Abp.Domain.Entities;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum CategoryType
    {
        Group = 1, //nhóm tài sản
        Location = 2, //vị trí: tầng, mã Code, kho
        Producer = 3, // nhà sản xuất
        Type = 4, //loại tài sản
        Unit = 5, //đơn vị tài sản
        Inventory = 6 // kho
    }

    [Table("MaterialCategories")]
    public class MaterialCategory : Entity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(1000)]
        public string ImageUrl { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        [StringLength(1000)]
        public string Name { get; set; }
        public CategoryType Type { get; set; }
        public string Description { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public bool IsDelete { get; set; }
        public long? ParentId { get; set; }
    }
}
