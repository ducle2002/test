using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class CreateItemAttributeDto
    {
        public int? TenantId { get; set; }
        public long CategoryId { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemAttributeDataType DataType { get; set; }
        public ItemAttributeInputType InputType { get; set; }
        public bool IsRequired { get; set; } = false;
        public List<string> UnitList { get; set; } = new List<string>();
        public List<string> ValueList { get; set; } = new List<string>();
    }
    public class CreateListItemAttributesDto
    {
        public List<CreateItemAttributeDto> ItemAttributes { get; set; }
    }
    public class DeleteItemAttributeDto : EntityDto<long>
    {
    }
    public class GetAllItemAttributesDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? CategoryId { get; set; }
    }
    public class GetItemAttributeByIdDto : EntityDto<long>
    {
    }
    public class ItemAttributeDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [NotNull]
        public long CategoryId { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public ItemAttributeDataType DataType { get; set; }
        public ItemAttributeInputType InputType { get; set; }
        [NotNull]
        public bool IsRequired { get; set; }
        public List<string>? UnitList { get; set; }
        public List<string>? ValueList { get; set; }
    }
    public class UpdateItemAttributeDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long CategoryId { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemAttributeDataType DataType { get; set; }
        public ItemAttributeInputType InputType { get; set; }
        public bool IsRequired { get; set; } = false;
        public List<string> UnitList { get; set; } = new List<string>();
        public List<string> ValueList { get; set; } = new List<string>();
    }
    public class UpdateListItemAttributesDto
    {
        public List<UpdateItemAttributeDto> ItemAttributes { get; set; }
    }

    [Table("ItemAttributes")]
    public class ItemAttribute : Entity<long>, IDeletionAudited, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [NotNull]
        public long CategoryId { get; set; }
        [NotNull]
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public ItemAttributeDataType DataType { get; set; }
        public ItemAttributeInputType InputType { get; set; }
        [NotNull]
        public bool IsRequired { get; set; }
        public List<string>? UnitList { get; set; }
        public List<string>? ValueList { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public long? DeleterUserId { get; set; }
        [NotMapped]
        public bool IsChooseToDelete { get; set; }
    }

    public enum ItemAttributeInputType
    {
        Text = 1,
        TextArea = 2,
        Number = 3,
        Date = 4,
        Select = 5,
        MultiSelect = 6,
        Checkbox = 7,
        Radio = 8,
        Editor = 9,
    }
    public enum ItemAttributeDataType
    {
        String = 1,
        Int = 2,
        Float = 3,
        Date = 4,
        Boolean = 5,
        Array = 6,
    }
}
