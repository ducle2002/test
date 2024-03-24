using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using static Yootek.YootekServiceBase;
using Abp.Domain.Entities;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using Abp.Organizations;


namespace Yootek.Services
{
    public class ADOptionPropertyDto
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
    }

    [AutoMap(typeof(AdministrativeProperty))]
    public class ADPropetyInput : AdministrativeProperty
    {
        public long Id { get; set; }
        public List<AdministrativePropertyDto> TableColumn { get; set; }
        public List<AdministrativePropertyDto> OptionValues { get; set; }
        public string StateRowFe { get; set; }
    }

    [AutoMap(typeof(AdministrativeProperty))]
    public class ADPropetyDto
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public long? TypeId { get; set; }
        public ADPropertyType Type { get; set; }
        public string DisplayName { get; set; }
        public long? ConfigId { get; set; }
        public long? ParentId { get; set; }
        public int? TenantId { get; set; }

        // là column hiển thị trên bảng web quan trị
        public bool? IsTableColumn { get; set; }
        public List<AdministrativeProperty> TableColumn { get; set; }
        public List<AdministrativeProperty> OptionValues { get; set; }
    }

    [AutoMap(typeof(AdministrativeProperty))]
    public class AdministrativePropertyDto : AdministrativeProperty
    {
        public string StateRowFe { get; set; }
    }

    [AutoMap(typeof(TypeAdministrative))]
    public class TypeAdministrativeDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding, IMayHaveOrganizationUnit
    {
        public List<ADPropetyDto> Properties { get; set; }
        public string OrganizationUnitName { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }
        public string ImageUrl { get; set; }
        public string FileUrl { get; set; }
        public long? OrganizationUnitId { get; set; }
        public double? Price { get; set; }
        public string PriceDetail { get; set; }
        public bool? Surcharge { get; set; }
    }

    public class GetAllTypeAdministrativeInput : CommonInputDto
    {
        public OrderByTypeAdministrative? OrderBy { get; set; }
    }

    public enum OrderByTypeAdministrative
    {
        [FieldName("Name")]
        NAME = 1
    }

    [AutoMap(typeof(Administrative))]
    public class AdministrativeDto : Administrative
    {
        public string Name { get; set; }
        public long? BuildingId { get; set; }
        public string ApartmentCode { get; set; }
        public string CreatorUserName { get; set; }
        public string CreatorUserAvatar { get; set; }
    }

    public class GetAllAdministrativeInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
    }

    public class GetListAdministrativeByOwnerInput : CommonInputDto
    {
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }

    public class AdministrativeOutputDto : Administrative
    {
        public string? NameTypeAdministrative { get; set; }
    }

    public class CancelAdministrativeByUserInput
    {
        public long Id { get; set; }
        public string DeniedReason { get; set; }
    }
    [AutoMap(typeof(AdministrativeValue))]
    public class ValueAdministrativeDto : AdministrativeValue
    {
    }

    //[AutoMap(typeof(AdministrativeProperty))]
    //public class AdministrativePropertyDto : AdministrativeProperty
    //{

    //}
}