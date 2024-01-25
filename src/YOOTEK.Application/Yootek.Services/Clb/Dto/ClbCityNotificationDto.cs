using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Clb.City_Notification;
using Yootek.Organizations.Interface;
using Yootek.Services;

namespace Yootek.Yootek.Services.Yootek.Clb.Dto
{
    public class CityNotificationDto
    {
    }
    
    #region MyRegion

        [AutoMap(typeof(ClbCityNotification))]
        public class ClbCityNotificationDto : ClbCityNotification
        {
            public long CountComment { get; set; }
            public long CountFollow { get; set; }
            public IEnumerable<ClbUserCityNotificationDto> Users { get; set; }
            public string UsersId { get; set; }
            public bool isSendByGroup { get; set; }
        }


        [AutoMap(typeof(ClbCityNotificationComment))]
        public class ClbCommentDto : ClbCityNotificationComment
        {
            public string FullName { get; set; }
            public string Avatar { get; set; }
        }

        [AutoMap(typeof(ClbUserCityNotification))]
        public class ClbUserCityNotificationDto : UserCityNotification
        {
            public string FullName { get; set; }

            public string Avatar { get; set; }
            //  public long NotificationId { get; set; }
            //  public long UserId { get; set; }
        }

        public class ClbUserOffline
        {
            public long Id { get; set; }
            public bool IsOnline { get; set; }
        }

        public class ClbNotificationInput : CommonInputDto
        {
            public int Type { get; set; }
            public string Code { get; set; }
            public string? ReceiverGroupCode { get; set; }
            public int? State { get; set; }
            public RECEIVE_TYPE? ReceiveAll { get; set; }

            public OrderByNotification OrderBy { get; set; }
        }

        /* */
        public class ClbNotificationOutput
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
            public IEnumerable<ClbUserCityNotificationDto> Users { get; set; }
            public bool isSendByGroup { get; set; }
            public DateTime? CreationTime { get; set; }
            public long? CreatorUserId { get; set; }
        }


        /* */

        public class GetClbCommentInput : CommonInputDto
        {
            public long NotificationId { get; set; }
            public int Type { get; set; }
            public DateTime? FromDay { get; set; }
            public DateTime? ToDay { get; set; }
            public OrderByComment? OrderBy { get; set; }
        }

        public class ClbLikeDto : CitizenReflectLike
        {
            public String FullName { get; set; }
            public String Avatar { get; set; }
        }

        #endregion
}