using System.ComponentModel;
using Abp.Application.Services.Dto;
using Yootek.Common;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class CategoryDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [NotNull]
        public string Name { get; set; }
        public long? ParentId { get; set; }
        public bool HasChildren { get; set; }
        [Range(1, 10000)]
        public long BusinessType { get; set; }
        [NotNull]
        public string? IconUrl { get; set; }
    }
    public class CreateCategoryDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public long? ParentId { get; set; }
        public bool? HasChildren { get; set; }
        public long BusinessType { get; set; }
        public string? IconUrl { get; set; }
    }
    public class DeleteCategoryDto : EntityDto<long>
    {
    }
    public class GetAllCategoriesDto : CommonInputDto
    {
        public long? BusinessType { get; set; }
        public long? ParentId { get; set; }
        public int? OrderBy { get; set; }
    }

    public enum CategoryOrderBy
    {
        [Description("Name")] Name = 1,
        [Description("ParentId")] ParentId = 2,
        [Description("HasChildren")] HasChildren = 3,
        [Description("BusinessType")] BusinessType = 4,
        [Description("IconUrl")] IconUrl = 5,
        [Description("Id")] Id = 6
    }
    public class GetCategoryByIdDto : EntityDto<long>
    {
    }
    public class GetListCategoryFromChildrenDto : EntityDto<long>
    {
    }
    public class GetListCategoryIdInvestmentDto
    {
    }
    public class UpdateCategoryDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string? Name { get; set; }
        public long? ParentId { get; set; }
        public bool? HasChildren { get; set; }
        public long BusinessType { get; set; }
        public string? IconUrl { get; set; }
    }
}
