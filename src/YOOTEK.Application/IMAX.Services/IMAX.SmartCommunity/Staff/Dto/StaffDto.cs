using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.Common;
using IMAX.EntityDb;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services
{
    [AutoMap(typeof(Staff))]
    public class StaffDto : EntityDto<long>
    {
        public string Specialize { get; set; }
        public int? TenantId { get; set; }
        public int PositionId { get; set; }
        public long? OrganizationUnitId { get; set; }

        public long UserId { get; set; }
        public StaffDto(long id) : base(id)
        {
        }
    }
    [AutoMap(typeof(Staff))]
    public class StaffInput : Staff
    {
        public string FullName { get { return this.Surname + " " + this.Name; } }
        //public bool IsActive { get; set; }
        public string? PositionName { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string? DepartmentUnitName { get; set; }
        public long? UrbanId { get; set; }
        public string[] RoleNames { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public string? AddressOfBirth { get; set; }
        public DateTime? DateOfBirth { get; set; }


    }
    [AutoMap(typeof(Staff))]
    public class CreateStaffDto : Staff
    {
        public string[] RoleNames { get; set; }
    }
    public class GetStaffInput : CommonInputDto
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? DepartmentUnitId { get; set; }
        public long? UrbanId { get; set; }
        public int? Type { get; set; }
        public OrderByStaff? OrderBy { get; set; }
    }

    public enum OrderByStaff
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("AccountName")]
        ACCOUNT_NAME = 2
    }
    public class UpdateInput : StaffInput
    {
        //public string[] RoleNames { get; set; }
        //public int PositionId { get; set; }
        //public long OrganizationUnitId { get; set; }
        //public DateTime? DateOfBirth { get; set; }

    }
    public class StaffExportDto
    {
        [CanBeNull] public List<long> Ids { get; set; }
    }
    public class ImportStaffExcelDto
    {
        public IFormFile Form { get; set; }
    }
    public class GetStaffUnitInput
    {
        public int Type { get; set; }
    }
}
