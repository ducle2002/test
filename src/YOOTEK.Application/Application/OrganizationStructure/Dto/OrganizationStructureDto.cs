using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.Common;
using Yootek.Organizations.OrganizationStructure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Yootek.Application.OrganizationStructure.Dto
{
    public class OrganizationStructureDto
    {
    }
    public class OrganizationStructureDeptOutputDto : OrganizationStructureDept
    {
        public string UnitName { get; set; }
        public int UserCount { get; set; }
        [CanBeNull] public List<long> UnitIds { get; set; }
    }
    public class OrganizationStructureUnitOutputDto : OrganizationStructureUnit
    {
        public List<long> ParentUnitIds { get; set; }
    }
    public class OrganizationStructureInputDto : CommonInputDto
    {
    }
    public class OrganizationStructureDeptUsersDto : CommonInputDto
    {
        public long Id { get; set; }
    }
    [AutoMap(typeof(OrganizationStructureDept))]
    public class OrganizationStructureDeptInputDto : OrganizationStructureDept
    {
        [FromBody] public List<long> UnitIds { get; set; }
    }
    [AutoMap(typeof(OrganizationStructureUnit))]
    public class OrganizationStructureUnitInputDto : OrganizationStructureUnit
    {
        [FromBody] public List<long> ParentUnitIds { get; set; }
    }
    public class OrganizationStructureInsertUsersDto
    {
        public long Id { get; set; }
        [FromBody]
        public List<long> UserIds { get; set; }
    }
    public class OrganizationStructureFindUsersDto : CommonInputDto
    {
        public long Id { get; set; }
    }
    public class GetDepartmentOrganizationUnitInputDto : CommonInputDto
    {
        public long Id { get; set; }
    }
    public class AddDepartmentToOrganizationUnitDto
    {
        public long Id { get; set; }
        public List<long> DeptIds { get; set; }
    }
    public class OrganizationStructureFindDepartmentDto : CommonInputDto
    {
        public long Id { get; set; }
    }
    public class DepartmentUserDto
    {
        public long DepartmentId { get; set; }
        public List<OrganizationStructureDeptUser> ListStaff { get; set; }
    }
    public class DepartmentDto : Entity<long>
    {
        public long? ParentId { get; set; }
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<DepartmentChildDto> Childs { get; set; }
    }

    public class DepartmentChildDto : Entity<long>
    {
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<DepartmentStaffDto> Staffs { get; set; }
    }
    public class DepartmentStaffDto : Entity<long>
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
    public class AccountDepartmentDto : Entity<long>
    {
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public DateTime AddedTime { get; set; }
    }
}
