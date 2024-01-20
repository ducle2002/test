using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{
    [AutoMap(typeof(CityNotification))]
    public class CityNotificationDto : CityNotification
    {
        public long CountComment { get; set; }
        public long CountFollow { get; set; }
        public IEnumerable<UserCityNotificationDto> Users { get; set; }
        public string UsersId { get; set; }
        public bool isSendByGroup { get; set; }
        public string OrganizationUnitName { get; set; }
    }


    [AutoMap(typeof(CityNotificationComment))]
    public class CommentDto : CityNotificationComment
    {
        public string FullName { get; set; }
        public string Avatar { get; set; }
    }

    [AutoMap(typeof(UserCityNotification))]
    public class UserCityNotificationDto : UserCityNotification
    {
        public string FullName { get; set; }
        public string Avatar { get; set; }
        //  public long NotificationId { get; set; }
        //  public long UserId { get; set; }
    }

    public class UserOffline
    {
        public long Id { get; set; }
        public bool IsOnline { get; set; }
    }

    public class NotificationInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public int Type { get; set; }
        public string Code { get; set; }
        public string? ReceiverGroupCode { get; set; }
        public int? State { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public RECEIVE_TYPE? ReceiveAll { get; set; }

        public OrderByNotification OrderBy { get; set; }
        public List<string>? AttachUrls { get; set; }

    }
    public enum OrderByNotification
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("ReceiverGroupCode")]
        RECEIVER_GROUP_CODE = 2
    }
    /* */
    public class NotificationOutput
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public string Data { get; set; }
        public string FileUrl { get; set; }
        public int? Type { get; set; }
        public int? TenantId { get; set; }
        public long? DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
        public int? Follow { get; set; }
        public object? Department { get; set; }

        // public object? OrganizationUnit { get; set; }
        public long? UserId { get; set; }
        public long CountComment { get; set; }
        public long CountFollow { get; set; }
        public IEnumerable<UserCityNotificationDto> Users { get; set; }
        public bool isSendByGroup { get; set; }
        public DateTime? CreationTime { get; set; }
        public long? CreatorUserId { get; set; }


    }





    /* */

    public class GetCommentInput : CommonInputDto
    {
        public long NotificationId { get; set; }
        public int Type { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public OrderByComment? OrderBy { get; set; }
    }

    public enum OrderByComment
    {
        [FieldName("FullName")]
        FULL_NAME = 1
    }


    public class LikeDto : CitizenReflectLike
    {
        public String FullName { get; set; }
        public String Avatar { get; set; }
    }
}
