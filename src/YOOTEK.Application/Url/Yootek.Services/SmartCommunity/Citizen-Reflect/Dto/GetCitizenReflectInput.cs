using Yootek.Common;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    public class GetCitizenReflectInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int FormId { get; set; }
        public int? State { get; set; }
        public long UserId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string Phone { get; set; }
        public string NameFeeder { get; set; }
        public string? ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; } //Thêm input UrbanId
        public OrderByCitizenReflect? OrderBy { get; set; }
    }

    public enum OrderByCitizenReflect
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("UserName")]
        USERNAME = 2,
        [FieldName("CreationTime")]
        CREATION_TIME = 3,
    }

    public class GetCommentCitizenReflectInput : CommonInputDto
    {
        public int CitizenReflectId { get; set; }
    }



    public class GetAllCitizenReflectByMonth
    {
        public int NumberMonth { get; set; }
    }
    public enum QueryCaseStatistics
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4,
        ByHours = 5,
        ByStar = 6,
    }

    public enum FormGetReflectId
    {
        GetAll = 1,
        GetCompleted = 2
    }
    public class GetStatisticFeedbackInput : IMayHaveUrban, IMayHaveBuilding
    {
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public QueryDateTime QueryDateTime { get; set; }
        public QueryScope QueryScope { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [CanBeNull] public string ApartmentCode { get; set; }
    }

    public enum QueryDateTime
    {
        YEAR = 1,
        QUARTER = 2,
        MONTH = 3,
        WEEK = 4,
        DAY = 5,
    }

    public enum QueryScope
    {
        TENANT = 1,
        URBAN = 2,
        BUILDING = 3,
        APARTMENT = 4,
    }

    public class StatisticsUserCitizenReflectInput
    {
        public long? OrganizationUnitId { get; set; }
        public int NumberRange { get; set; }
        public QueryCaseStatistics QueryCase { get; set; }
        public FormGetReflectId FormId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? UrbanId { get; set; }
        public int? BuildingId { get; set; }

    }
    public class StatisticsUserCitizenReflectCommentInput
    {
        public long? OrganizationUnitId { get; set; }
        public int NumberMonth { get; set; }
    }

    public class CitizenReflectUserInput : CommonInputDto
    {
        //public int? State { get; set; }
        public int FormId { get; set; }
        public string? ApartmentCode { get; set; }
    }

    public class RateCitizenReflectDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public static partial class CommonENumCitizenReflectUser
    {
        public enum STATE_CitizenReflect
        {
            PENDING = 1, //chwof xử lý
            HANDLING = 2, // đang xử lý
            ADMIN_CONFIRMED = 3, // admin xác nhận đã xử lý xong
            USER_CONFIRMED = 4, //người dùng xác nhận đã xử lý
            USER_RATE_CitizenReflect = 5, //người dùng đánh giá về CitizenReflect

        }
        public enum FORM_ID_CitizenReflect
        {
            [EnumDisplayString("Form user get all CitizenReflect")]
            FORM_USER_GET_CitizenReflect_GETALL = 11
            ,
            [EnumDisplayString("Form user get new CitizenReflects")]
            FORM_USER_GET_CitizenReflect_PENDING = 21
            ,
            [EnumDisplayString("Form user get pending/handlding CitizenReflects")]
            FORM_USER_GET_CitizenReflect_HANDLING = 22
            ,
            [EnumDisplayString("Form user get handled CitizenReflects")]
            FORM_USER_GET_CitizenReflect_HANDLED = 23
            ,
        }

    }

    public class EnumDisplayString : Attribute
    {
        public string DisplayString;

        public EnumDisplayString(string text)
        {
            this.DisplayString = text;
        }
    }
}
