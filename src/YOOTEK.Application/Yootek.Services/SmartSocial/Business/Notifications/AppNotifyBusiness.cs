using System.Collections.Generic;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using System.Threading.Tasks;
using JetBrains.Annotations;
using static Yootek.Services.Notifications.AppNotifyBusiness;

namespace Yootek.Services.Notifications
{
    public interface IAppNotifyBusiness
    {
        Task SendNotifyFullyToUser(MessageNotifyToOneUserDto input);
        Task SendNotifyFullyToListUser(MessageNotifyToListUserDto input);

        Task SendNotifyFirebaseToUser(MessageNotifyToOneUserDto input);
        Task SendNotifyDatabaseToUser(MessageNotifyToOneUserDto input);
        Task SendNotifyFullyToTopic(MessageNotifyToTopicDto input);
        Task SendNotifyFirebaseToTopic(MessageNotifyToTopicDto input);
        Task SendNotifyDatabaseToTopic(MessageNotifyToTopicDto input);
    }
    public class AppNotifyBusiness : IAppNotifyBusiness
    {
        private readonly IHttpNotificationService _httpNotificationService;
        public AppNotifyBusiness(IHttpNotificationService httpNotificationService)
        {
            _httpNotificationService = httpNotificationService;
        }

        #region Class Dto 
        public class MessageNotifyToOneUserDto
        {
            public int? TenantId { get; set; }
            public string Title { get; set; } // title
            public string Message { get; set; } // body
            public string Action { get; set; }
            public string Icon { get; set; }
            public string? ImageUrl { get; set; }
            public int? AppType { get; set; }
            public int Type { get; set; }
            public long UserId { get; set; }
            public long? ProviderId { get; set; }
            public int? PageId { get; set; } // màn chứa thông báo
            public long? TransactionId { get; set; } // đại diện cho đăng ký, order, booking, ...
            public long? OrderId { get; set; }
            public long? BookingId { get; set; }
            public int? TransactionType { get; set; }
        }

        public class MessageNotifyToListUserDto
        {
            public int? TenantId { get; set; }
            public string Title { get; set; } // title
            public string Message { get; set; } // body
            public string Action { get; set; }
            public string Icon { get; set; }
            public string? ImageUrl { get; set; }
            public int? AppType { get; set; }
            public int Type { get; set; }
            public List<long?> UserId { get; set; }
            public long? ProviderId { get; set; }
            public int? PageId { get; set; } // màn chứa thông báo
            public long? TransactionId { get; set; } // đại diện cho đăng ký, order, booking, ...
            public long? OrderId { get; set; }
            public long? BookingId { get; set; }
            
            public int? TransactionType { get; set; }
        }
        
        public class NotificationMessage
        {
            public long? UserId { get; set; }
            public long? PartnerId { get; set; }
            public int? TenantIdUser { get; set; }
            public int? TenantIdPartner { get; set; }
            public string? ImageUrl { get; set; }
            public long? ProviderId { get; set; }
            public string? ProviderName { get; set; }
            public long? TransactionId { get; set; }
            public string? TransactionCode { get; set; }
            public string? TransactionName { get; set; }
            public long? OrderId { get; set; }
            public string? OrderCode { get; set; }
            public long? BookingId { get; set; }
            public string? BookingCode { get; set; }
            public string? BookingName { get; set; }
        }
        
        public class NotificationMessageToListUser
        {
            [CanBeNull] public List<long?> UserId { get; set; }
            public long? PartnerId { get; set; }
            public int? TenantIdUser { get; set; }
            public int? TenantIdPartner { get; set; }
            public string? ImageUrl { get; set; }
            public long? ProviderId { get; set; }
            public string? ProviderName { get; set; }
            public long? TransactionId { get; set; }
            public string? TransactionCode { get; set; }
            public string? TransactionName { get; set; }
            public long? OrderId { get; set; }
            public string? OrderCode { get; set; }
            public long? BookingId { get; set; }
            public string? BookingCode { get; set; }
            public string? BookingName { get; set; }
        }

        public class MessageNotifyToTopicDto
        {
            public int TenantId { get; set; }
            public string Title { get; set; } // title
            public string Message { get; set; } // body
            public string Action { get; set; }
            public string Icon { get; set; }
            public string ImageUrl { get; set; }
            public int? AppType { get; set; }
            public int Type { get; set; }
            public string TopicName { get; set; }
            public long? ProviderId { get; set; }
            public int? PageId { get; set; } // màn chứa thông báo
            public long? TransactionId { get; set; }
            public long? OrderId { get; set; }
            public long? BookingId { get; set; }
        }
        public enum TYPE_NOTIFICATION
        {
            SOCIAL = 1,
            OTHER = 2,
        }
        
        public enum TYPE_TRANSACTION
        {
            ORDER = 1,
            BOOKING = 2,
            ACTIVITY = 3, 
            REGISTER = 4,
        }
        #endregion

        #region fully
        public async Task SendNotifyFullyToUser(MessageNotifyToOneUserDto input)
        {
            await _httpNotificationService.CreateNotificationToOneUserAsync(new CreateNotificationToOneUserDto()
            {
                TenantId = input.TenantId,
                NotificationName = input.Title,
                UserId = input.UserId,
                AppType = input.AppType,
                Type = input.Type,
                Data = new DataNotification()
                {
                    Action = input.Action,
                    Description = string.Empty,
                    Icon = input.Icon,
                    ImageUrl = input.ImageUrl ?? string.Empty,
                    Message = input.Message,
                    TypeAction = 0,
                    PageId = input.PageId,
                    ProviderId = input.ProviderId,
                    OrderId = input.OrderId,
                    BookingId = input.BookingId,
                    TransactionId = input.TransactionId,
                    TransactionType = input.TransactionType,
                },
            });
            
        }
        
        public async Task  SendNotifyFullyToListUser(MessageNotifyToListUserDto input)
        {
            await _httpNotificationService.CreateNotificationToListUserAsync(new CreateNotificationToListUserDto()
            {
                TenantId = input.TenantId,
                NotificationName = input.Title,
                UserId = input.UserId,
                AppType = input.AppType,
                Type = input.Type,
                Data = new DataNotification()
                {
                    Action = input.Action,
                    Description = string.Empty,
                    Icon = input.Icon,
                    ImageUrl = input.ImageUrl ?? string.Empty,
                    Message = input.Message,
                    TypeAction = 0,
                    PageId = input.PageId,
                    ProviderId = input.ProviderId,
                    OrderId = input.OrderId,
                    BookingId = input.BookingId,
                    TransactionId = input.TransactionId,
                    TransactionType = input.TransactionType,
                },
            });
            
        }
        public async Task SendNotifyFullyToTopic(MessageNotifyToTopicDto input)
        {
            await _httpNotificationService.CreateNotificationTopicAsync(new CreateNotificationTopicDto()
            {
                TenantId = input.TenantId,
                NotificationName = input.Title,
                AppType = input.AppType,
                Type = input.Type,
                Topic = input.TopicName,
                Data = new DataNotification()
                {
                    Action = input.Action,
                    Description = string.Empty,
                    Icon = input.Icon,
                    ImageUrl = input.ImageUrl ?? "",
                    Message = input.Message,
                    TypeAction = 0,
                    PageId = input.PageId,
                    ProviderId = input.ProviderId,
                    BookingId = input.BookingId,
                    OrderId = input.OrderId,
                    TransactionId = input.TransactionId,
                },
            });
            
        }
        #endregion 

        #region only firebase
        public async Task SendNotifyFirebaseToUser(MessageNotifyToOneUserDto input)
        {
            await _httpNotificationService.CreateNotificationToOneUserAsync(new CreateNotificationToOneUserDto()
            {
                TenantId = input.TenantId,
                NotificationName = input.Title,
                UserId = input.UserId,
                AppType = input.AppType,
                Type = input.Type,
                Data = new DataNotification()
                {
                    Action = input.Action,
                    Description = string.Empty,
                    Icon = input.Icon,
                    ImageUrl = input.ImageUrl ?? string.Empty,
                    Message = input.Message,
                    TypeAction = 0,
                    PageId = input.PageId,
                    ProviderId = input.ProviderId,
                    OrderId = input.OrderId,
                    BookingId = input.BookingId,
                    TransactionId = input.TransactionId,
                },
            });
            
        }
        public async Task SendNotifyFirebaseToTopic(MessageNotifyToTopicDto input)
        {
            await _httpNotificationService.CreateNotificationTopicAsync(new CreateNotificationTopicDto()
            {
                TenantId = input.TenantId,
                NotificationName = input.Title,
                AppType = input.AppType,
                Type = input.Type,
                Topic = input.TopicName,
                Data = new DataNotification()
                {
                    Action = input.Action,
                    Description = string.Empty,
                    Icon = input.Icon,
                    ImageUrl = input.ImageUrl ?? "",
                    Message = input.Message,
                    TypeAction = 0,
                    PageId = input.PageId,
                    ProviderId = input.ProviderId,
                    BookingId = input.BookingId,
                    OrderId = input.OrderId,
                    TransactionId = input.TransactionId,
                },
            });
            
        }
        #endregion 

        #region only database
        public async Task SendNotifyDatabaseToUser(MessageNotifyToOneUserDto input)
        {
            await _httpNotificationService.CreateNotificationToOneUserAsync(new CreateNotificationToOneUserDto()
            {
                TenantId = input.TenantId,
                NotificationName = input.Title,
                UserId = input.UserId,
                AppType = input.AppType,
                Type = input.Type,
                Data = new DataNotification()
                {
                    Action = input.Action,
                    Description = string.Empty,
                    Icon = input.Icon,
                    ImageUrl = input.ImageUrl ?? string.Empty,
                    Message = input.Message,
                    TypeAction = 0,
                    PageId = input.PageId,
                    ProviderId = input.ProviderId,
                    OrderId = input.OrderId,
                    BookingId = input.BookingId,
                    TransactionId = input.TransactionId,
                },
            });
            
        }
        public async Task SendNotifyDatabaseToTopic(MessageNotifyToTopicDto input)
        {
            await _httpNotificationService.CreateNotificationTopicAsync(new CreateNotificationTopicDto()
            {
                TenantId = input.TenantId,
                NotificationName = input.Title,
                AppType = input.AppType,
                Type = input.Type,
                Topic = input.TopicName,
                Data = new DataNotification()
                {
                    Action = input.Action,
                    Description = string.Empty,
                    Icon = input.Icon,
                    ImageUrl = input.ImageUrl ?? "",
                    Message = input.Message,
                    TypeAction = 0,
                    PageId = input.PageId,
                    ProviderId = input.ProviderId,
                    BookingId = input.BookingId,
                    OrderId = input.OrderId,
                    TransactionId = input.TransactionId,
                },
            });
            
        }
        #endregion 
    }
}
