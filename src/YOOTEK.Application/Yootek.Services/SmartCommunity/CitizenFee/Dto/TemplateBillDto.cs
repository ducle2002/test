using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.Common;
using Yootek.Yootek.EntityDb.SmartCommunity.Phidichvu;
using Microsoft.AspNetCore.Http;
using static Yootek.YootekServiceBase;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto
{
    [AutoMap(typeof(TemplateBill))]
    public class TemplateBillDto : Entity<long>
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string? BuildingName { get; set; }
        public string? UrbanName { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int? TenantId { get; set; }
        public ETemplateBillType Type { get; set; }
        public string Code { get; set; }
    }
    public class GetTemplateOfTenantInput
    {
        public int? TenantId { get; set; }
        public ETemplateBillType? Type { get; set; }
    }
    public class GetAllTemplateBillWithoutTenantInput
    {
        public ETemplateBillType? Type { get; set; }
    }
    public class GetAllTemplateBillInput : CommonInputDto
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public int? TenantId { get;set; }
        public ETemplateBillType? Type { get; set; }
        public OrderByTemplateBill? OrderBy { get; set; }
    }

    public enum OrderByTemplateBill
    {
        [FieldName("Name")]
        NAME = 1
    }

    [AutoMap(typeof(TemplateBill))]
    public class CreateTemplateBillInput
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public ETemplateBillType Type { get; set; }
        public string Name { get; set; }
        public int? TenantId { get; set; }
        public IFormFile FileHTML { get; set; }
        public string Code { get; set; } 
    }
    [AutoMap(typeof(TemplateBill))]
    public class UpdateTemplateBillInput
    {
        public long Id { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public ETemplateBillType Type { get; set; }
        public string Name { get; set; }
        public int? TenantId { get; set; }
        public IFormFile FileHTML { get; set; }
        public string Code { get; set; }
    }
}
