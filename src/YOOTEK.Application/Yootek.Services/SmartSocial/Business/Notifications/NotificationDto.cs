using Yootek.Common;
using JetBrains.Annotations;
using System;

namespace Yootek.Services.Notifications.Dto
{
    public class GetAllNotificationsInputDto : CommonInputDto
    {
        public int? State { get; set; }
        public int? AppType { get; set; }
        public int? FormId { get; set; }
        public long? ProviderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public enum FormIdNotification
    {
        NOTIFICATION_PARTNER = 1,
        NOTIFICATION_USER_SHOPPING = 2,
        NOTIFICATION_USER_BOOKING = 3,
        NOTIFICATION_USER_FORUM = 4,
    }
    public class GetNotificationByIdInputDto
    {
        public Guid Id { get; set; }
    }
    public class SetNotificationAsReadInputDto
    {
        public Guid Id { get; set; }
    }
    public class SetNotificationAsUnreadInputDto
    {
        public Guid Id { get; set; }
    }
    public class SetAllNotificationAsReadInputDto
    {
        public int? AppType { get; set; }
        public int? FormId { get; set; }
        public long? ProviderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    /*public class CreateNotificationToUserInputDto
    {
        public int? TenantId { get; set; }
        public string NotificationName { get; set; }
        public DataNotification Data { get; set; }
        public long UserId { get; set; }
        public int AppType { get; set; }
        public int TabId { get; set; }
    }
    public class CreateNotificationToTopicInputDto
    {
        public int? TenantId { get; set; }
        public string NotificationName { get; set; }
        public DataNotification Data { get; set; }
        public int AppType { get; set; }
    }*/
    public class UpdateNotificationInputDto
    {
        public Guid Id { get; set; }
        public int? TenantId { get; set; }
        public string NotificationName { get; set; }
        public string Data { get; set; }
        public string DataTypeName { get; set; }
        public string EntityTypeName { get; set; }
        public string EntityTypeAssemblyQualifiedName { get; set; }
        public string EntityId { get; set; }
        public int Severity { get; set; }
        public long UserId { get; set; }
        public string ExcludedUserIds { get; set; }
    }
    public class DeleteNotificationInputDto
    {
        public Guid Id { get; set; }
    }

    public class DeleteAllNotificationsInputDto
    {
        public int? State { get; set; }
        public int? AppType { get; set; }
        public int? FormId { get; set; }
        public long? ProviderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    #region FcmToken
    public class CreateFcmTokenInputDto
    {
        public string Token { get; set; }
        [CanBeNull] public string DeviceId { get; set; }
        public int? DeviceType { get; set; }
        public int? TenantId { get; set; }
        public int? AppType { get; set; }
    }
    public class LogoutFcmTokenInputDto
    {
        public string Token { get; set; }
    }
    #endregion
}
