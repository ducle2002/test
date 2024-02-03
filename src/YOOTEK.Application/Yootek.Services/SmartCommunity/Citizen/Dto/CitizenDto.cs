using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using static Yootek.YootekServiceBase;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(Citizen))]
    public class CitizenDto : Citizen
    {
        [CanBeNull] public string BuildingName { get; set; }
        [CanBeNull] public string ApartmentName { get; set; }

        [CanBeNull] public string UrbanName { get; set; }

        public long? ApartmentId { get; set; }
    }

    [AutoMap(typeof(Citizen))]
    public class CreateOrUpdateInput : IMayHaveUrban, IMayHaveBuilding
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public long? AccountId { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ApartmentCode { get; set; }
        public STATE_CITIZEN? State { get; set; }
        public string BuildingCode { get; set; }
        public int? Type { get; set; }
        public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string UrbanCode { get; set; }
        public long? CitizenTempId { get; set; }
        public string? CitizenCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }

    public class CitizenInfoDto
    {
        public List<ApartmentInfoDto> ApartmentCodes { get; set; }
        public Citizen CitizenInfo { get; set; }
    }

    public class ApartmentInfoDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ApartmentCode { get; set; }
    }

    [AutoMap(typeof(Citizen))]
    public class CreateCitizenByAdminInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public long? AccountId { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ApartmentCode { get; set; }
        public string BuildingCode { get; set; }
        public int? Type { get; set; }
        public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string UrbanCode { get; set; }
        public long? CitizenTempId { get; set; }
        public string? CitizenCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
        public STATE_CITIZEN? State { get; set; }

    }

    [AutoMap(typeof(Citizen))]
    public class UpdateCitizenByAdminInput
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public long? AccountId { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ApartmentCode { get; set; }
        public STATE_CITIZEN? State { get; set; }
        public string BuildingCode { get; set; }
        public int? Type { get; set; }
        public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string UrbanCode { get; set; }
        public long? CitizenTempId { get; set; }
        public string? CitizenCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }

    [AutoMap(typeof(Citizen))]
    public class CreateCitizenByUserInput
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ApartmentCode { get; set; }
        public string BuildingCode { get; set; }
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string UrbanCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }

    [AutoMap(typeof(Citizen))]
    public class UpdateCitizenByUserInput
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ApartmentCode { get; set; }
        public STATE_CITIZEN? State { get; set; }
        public string BuildingCode { get; set; }
        public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string UrbanCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }

    [AutoMap(typeof(CitizenTemp))]
    public class CitizenTempDto : CitizenTemp, IMayHaveUrban, IMayHaveBuilding
    {
        public string UrbanName { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public DateTime? AccountDOB { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string AccountEmail { get; set; }
    }
    [AutoMap(typeof(CitizenTemp))]
    public class CitizenTempVerificationDto : CitizenTemp
    {
        public string BuildingName { get; set; }
    }

    [AutoMap(typeof(CitizenTemp))]
    public class UpdateOrCreateCitizenTemp
    {
        public long Id { get; set; }
        [CanBeNull] public string UrbanName { get; set; }
        [CanBeNull] public string UserName { get; set; }
        [CanBeNull] public string FirstName { get; set; }
        [CanBeNull] public string Surname { get; set; }
        public DateTime? AccountDOB { get; set; }
        [CanBeNull] public string AccountEmail { get; set; }
        [CanBeNull] public string FullName { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string Nationality { get; set; }
        [CanBeNull] public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        [CanBeNull] public string PhoneNumber { get; set; }
        [CanBeNull] public string Email { get; set; }
        [CanBeNull] public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? IsVoter { get; set; }
        [CanBeNull] public string ApartmentCode { get; set; }
        [CanBeNull] public string BuildingCode { get; set; }
        public int? Type { get; set; }
        public bool? IsStayed { get; set; }
        [CanBeNull] public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        [CanBeNull] public string UrbanCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [CanBeNull] public string TaxCode { get; set; }
        [CanBeNull] public string CitizenCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        [CanBeNull] public string Career { get; set; }
        public int? OwnerGeneration { get; set; }
        public long? OwnerId { get; set; }
        public int? CareerCategoryId { get; set; }
    }
    public class AccountDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string? Name { get; set; }
        public string FullName { get; set; }
        public string ImageUrl { get; set; }
        public STATE_CITIZEN? State { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }

    public class GetAllUserTenantVertifyInput : CommonInputDto
    {
        public OrderByUserTenantVertify? OrderBy { get; set; }
    }

    public enum OrderByUserTenantVertify
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("UserName")]
        USERNAME = 2
    }

    public class GetAllUserTenantNotVertifyInput : CommonInputDto
    {
        public OrderByUserTenantNotVertify? OrderBy { get; set; }
    }

    public enum OrderByUserTenantNotVertify
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("UserName")]
        USERNAME = 2
    }

    public class SmarthomeTenantDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string SmarthomeCode { get; set; }
        public string ImageUrl { get; set; }
    }

    public class GetAllCitizenInput : CommonInputDto
    {
        public int FormId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public STATE_CITIZEN? State { get; set; }
        public OrderByCitizen? OrderBy { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public bool? IsStayed { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string? ApartmentCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? FromAge { get; set; }
        public int? ToAge { get; set; }
        public string BuildingCode { get; set; }
        public string Sorting { get; set; }
        public int? CareerCategoryId { get; set; }
    }

    public class ExportCitizenInput : GetAllCitizenInput
    {
        [CanBeNull] public List<long> Ids;
    }

    public class GetStatisticCitizenInput
    {
        public long? OrganizationUnitId { get; set; }
        public int NumberRange { get; set; }
        public QueryCaseCitizenStatistics QueryCase { get; set; }
        public FormGetCitizenId FormId { get; set; }
        public int? Sex { get; set; }
    }

    public enum QueryCaseCitizenStatistics
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4,
        ByAgeAndSex = 5,
        ByCareer = 6,
    }

    public enum FormGetCitizenId
    {
        GetAll = 1,
        GetAccepted = 2,
        GetRejected = 3,
        GetNew = 4,
    }

    public class ResultStatisticCitizen
    {
        public int CountTotal { get; set; }
        public int CountNew { get; set; }
        public int CountAccepted { get; set; }
        public int CountRejected { get; set; }
    }

    public class CitizenRole
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public string Nationality { get; set; }
        public string Contact { get; set; }
        public int? OwnerGeneration { get; set; }
        public long? OwnerId { get; set; }
    }

    public class GetAllSmarthomeDto
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string ApartmentCode { get; set; }
        public long? OrganizationUnitId { get; set; }
        public decimal? ApartmentAreas { get; set; }
    }

    public class SmarthomeByTenantDto : IMayHaveUrban, IMayHaveBuilding
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string ApartmentCode { get; set; }
        public string HouseDetail { get; set; }
        public string UrbanName { get; set; }
        public string UrbanCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string BuildingCode { get; set; }
        public long? OrganizationUnitId { get; set; }
        public List<CitizenRole> Citizens { get; set; }
        public AppOrganizationUnit Building { get; set; }
        public decimal? ApartmentAreas { get; set; }

        public int? CurrentCitizenCount { get; set; }
    }
    public enum OrderByCitizen
    {

        [FieldName("FullName")]
        FULLNAME = 1,
        [FieldName("State")]
        STATE = 2,
        [FieldName("Type")]
        TYPE = 3,
        [FieldName("ApartmentCode")]
        APARTMENT_CODE = 4,
        [FieldName("CreationTime")]
        CREATION_TIME = 5,
    }
    public enum STATE_GET_VOTE
    {

        NEW = 1,
        NEWVOTED = 2,
        FINISH = 3,
        ALL = 4,
        DISABLE = 5,
        UNEXPIRED = 6,
        EXPIRED = 7,
        COMING = 8,
        IN_PROGRESS = 9
    }

    public class GetVoteByIdInput
    {
        public long Id { get; set; }
    }
    public class GetAllCitizenApartment
    {
        public int? State { get; set; }
    }
    public class GetAllCityVoteInput : CommonInputDto
    {
        public int Type { get; set; }
        public STATE_GET_VOTE? State { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public STATUS_VOTE? Status { get; set; }
        public OrderByCityVote? OrderBy { get; set; }
    }

    public enum OrderByCityVote
    {
        [FieldName("Name")]
        NAME = 1
    }

    public class CityVoteExcelExportInput
    {
        [CanBeNull] public List<long> Ids { get; set; }
        public STATE_GET_VOTE State { get; set; }
    }

    public class GetCitizenVerifiedDto
    {
        public CitizenDto Citizen { get; set; }
        public CitizenTempDto CitizenTemp { get; set; }
        public CitizenVerificationDto CitizenVerification { get; set; }
    }

    [AutoMap(typeof(CitizenVerification))]
    public class CitizenVerificationDto : CitizenVerification
    {

    }

    public class CitizenOutput
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public long? AccountId { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ApartmentCode { get; set; }
        public int? State { get; set; }
        public string BuildingCode { get; set; }
        public int? Type { get; set; }
        public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string UrbanCode { get; set; }
        public long? CitizenTempId { get; set; }

        public string? CitizenCode { get; set; }

        public bool IsVoter { get; set; }
        public string Career { get; set; }

    }

    public class ExportCitizenTempDto
    {
        [CanBeNull] public List<long> Ids { get; set; }
        public bool? IsStayed { get; set; }
    }

    public class GetFamilyInfoByApartmentCode : CommonInputDto
    {
        public string ApartmentCode { get; set; }
    }
    public class ImportCitizenInput
    {
        public IFormFile File { get; set; }

    }
    public class UserTenantVertifyInput : FilteredInputDto
    {
    }
}
