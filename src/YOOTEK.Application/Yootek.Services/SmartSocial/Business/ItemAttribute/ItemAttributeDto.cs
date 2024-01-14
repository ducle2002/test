using Yootek.Common;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using System.Collections.Generic;

namespace Yootek.Services.SmartSocial.ItemAttributes.Dto
{
    public class GetItemAttributesInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? CategoryId { get; set; }
        public string Search { get; set; }
    }
    public class CreateItemAttributeInputDto
    {
        public int? TenantId { get; set; }
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public ItemAttributeDataType DataType { get; set; }
        public ItemAttributeInputType InputType { get; set; }
        public bool? IsRequired { get; set; }
        public List<string> UnitList { get; set; }
        public List<string> ValueList { get; set; }
    }
    public class UpdateItemAttributeInputDto : CreateItemAttributeInputDto
    {
        public long Id { get; set; }
        public bool? IsChooseToDelete { get; set; }
    }

    #region admin
    public class GetItemAttributesByAdminInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? CategoryId { get; set; }
        public string Search { get; set; }
    }
    #endregion
}
