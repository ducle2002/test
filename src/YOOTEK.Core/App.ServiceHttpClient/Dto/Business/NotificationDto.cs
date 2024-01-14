using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class GetAllNotificationsDto : CommonInputDto
    {
        public int? State { get; set; }
        public int? AppType { get; set; }
        public int? FormId { get; set; }
        public long? ProviderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    [Table("Notifications")]
    public class NotificationEntity : FullAuditedEntity<Guid>
    {
        public string? TenantIds { get; set; }
        public string NotificationName { get; set; }
        public string Data { get; set; }
        public string DataTypeName { get; set; }
        public string EntityTypeName { get; set; }
        public string EntityTypeAssemblyQualifiedName { get; set; }
        public string EntityId { get; set; }
        public int Severity { get; set; }
        public int Type { get; set; }
        public string ExcludedUserIds { get; set; }
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string? TenantIds { get; set; }
        public string NotificationName { get; set; }
        public string Data { get; set; }
        public string DataTypeName { get; set; }
        public string EntityTypeName { get; set; }
        public string EntityTypeAssemblyQualifiedName { get; set; }
        public string EntityId { get; set; }
        public int Severity { get; set; }
        public int Type { get; set; }
        public string ExcludedUserIds { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public int? State { get; set; }
        public int? AppType { get; set; }
        public int? PageId { get; set; }
        public long? ProviderId { get; set; }
    }
    public class GetNotificationByIdDto
    {
        public Guid Id { get; set; }
    }
    public class SetNotificationAsReadDto
    {
        public Guid Id { get; set; }
    }
    public class SetNotificationAsUnreadDto
    {
        public Guid Id { get; set; }
    }
    public class SetAllNotificationsAsReadDto
    {
        public int? AppType { get; set; }
        public int? FormId { get; set; }
        public long? ProviderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class CreateNotificationToOneUserDto
    {
        public int? TenantId { get; set; }
        public string NotificationName { get; set; }
        public DataNotification Data { get; set; }
        public long UserId { get; set; }
        public int Type { get; set; }
        public int? AppType { get; set; }
    }
    
    public class CreateNotificationToListUserDto
    {
        public int? TenantId { get; set; }
        public string NotificationName { get; set; }
        public DataNotification Data { get; set; }
        public List<long?> UserId { get; set; }
        public int Type { get; set; }
        public int? AppType { get; set; }
    }
    public class CreateNotificationTopicDto
    {
        public int TenantId { get; set; }
        public string NotificationName { get; set; } // title
        public DataNotification Data { get; set; }
        public int Type { get; set; }
        public int? AppType { get; set; }
        public string Topic { get; set; }
    }
    public class DataNotification
    {
        public string Action { get; set; }
        public string Icon { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public int TypeAction { get; set; }
        public string ImageUrl { get; set; }
        public long? ProviderId { get; set; }
        public int? PageId { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
        public long? TransactionId { get; set; }
        
        public int? TransactionType { get; set; }
    }
    public class UpdateNotificationDto
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

    public class DeleteNotificationDto
    {
        public Guid Id { get; set; }
    }
    public class DeleteAllNotificationsDto
    {
        public int? State { get; set; }
        public int? AppType { get; set; }
        public int? FormId { get; set; }
        public long? ProviderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public enum TypeNotification
    {
        SOCIAL = 1,
        OTHER = 2,
    }

    public class ResponseGetAllNotifications
    {
        public long TotalCount { get; set; }
        public long TotalUnread { get; set; }
        public List<NotificationDto> Items { get; set; }
    }

    #region FcmToken
    [Table("FcmTokens")]
    public class FcmToken : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Token { get; set; }
        [CanBeNull] public string DeviceId { get; set; }
        public int? DeviceType { get; set; }
        public int? TenantId { get; set; }
        public int? AppType { get; set; }
    }
    public class FcmTokenDto : FcmToken
    {
    }
    public class GetAllFcmTokensDto : CommonInputDto
    {
        public long? UserId { get; set; }
        public int? TenantId { get; set; }
        public int? AppType { get; set; }
    }
    public class CreateFcmTokenDto
    {
        public string Token { get; set; }
        [CanBeNull] public string DeviceId { get; set; }
        public int? DeviceType { get; set; }
        public int? TenantId { get; set; }
        public int? AppType { get; set; }
    }
    public class DeleteFcmTokenDto
    {
        public string Token { get; set; }
    }
    #endregion

    #region FcmGroup
    public class AddUserToFcmGroup
    {
        public long UserId { get; set; }
        public int TenantId { get; set; }
    }

    public class RemoveUserFromFcmGroup
    {
        public long UserId { get; set; }
        public int TenantId { get; set; }
    }
    #endregion
}
