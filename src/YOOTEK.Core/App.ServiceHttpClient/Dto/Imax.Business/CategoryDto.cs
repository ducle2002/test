using Abp.Application.Services.Dto;
using IMAX.Common;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace IMAX.App.ServiceHttpClient.Dto.Imax.Business
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
