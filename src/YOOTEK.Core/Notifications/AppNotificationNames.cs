namespace Yootek.Notifications
{
    /// <summary>
    /// Constants for notification names used in this application.
    /// </summary>
    public static class AppNotificationNames
    {
        public const string SimpleMessage = "Thông báo mới !";
        public const string WelcomeToTheApplication = "Chào mừng bạn đến với Yoolife !";
        public const string NewUserRegistered = "App.NewUserRegistered";
        public const string NewTenantRegistered = "App.NewTenantRegistered";
        public const string GdprDataPrepared = "App.GdprDataPrepared";
        public const string TenantsMovedToEdition = "App.TenantsMovedToEdition";
        public const string DownloadInvalidImportUsers = "App.DownloadInvalidImportUsers";

        // smart-social
        public const string UserOrderService = "App.UserOrderService";
        public const string UserBookingService = "App.UserBookingService";
    }

    public static class AppNotificationAction
    {
        public const string UserWelcomeApp = "UserWelcomeApp";

        public const string AdminSendNotification = "AdminSendNotification";

        public const string StateVerifyCitizen = "App.StateVerifyCitizen";
        public const string StateReflectCitizen = "App.StateReflectCitizen";

        public const string CityVoteNew = "App.CityVoteNew";
        public const string CityNotificationNew = "App.CityNotificationNew";

        public const string AdministrativeNew = "App.AdministrativeNew";

        public const string CityNotificationComment = "App.CityNotificationComment";

        //Administrative
        public const string StateAdministrative = "App.StateAdministrative";

        public const string ChatMessage = "App.ChatMessage";
        public const string BillPaymentSuccess = "App.BillPaymentSuccess";
        public const string BillPaymentCancel = "App.BillPaymentCancel";
        public const string UserBill = "App.UserBill";

        public const string CommentReflectCitizen = "App.CommentReflectCitizen";
        public const string ReflectCitizenNew = "App.ReflectCitizenNew";
        public const string WorkNotification = "App.WorkNotification";

        // smart-social
        public const string CreateOrderSuccess = "App.CreateOrderSuccessAction";
        public const string ShippingOrder = "App.ShippingOrderAction";
        public const string ShipperCompleted = "App.ShipperCompletedAction";
        public const string UserCompleted = "App.UserCompletedAction";
        public const string UserCancel = "App.UserCancelAction";
        public const string UserRequestCancel = "App.UserRequestCancelAction";
        public const string PartnerConfirmCancel = "App.PartnerConfirmCancelAction";
        public const string PartnerCancelOrder = "App.PartnerCancelOrder";
        public const string PartnerCancel = "App.PartnerCancel";
        public const string CreateBookingSuccess = "App.CreateBookingSuccessAction";

        // eco-farm
        public const string RegisterPackageSuccess = "App.RegisterPackageSuccessAction";
        public const string CancelPackageSuccess = "App.CancelPackageSuccessAction";
        public const string CreatePackageActivitySuccess = "App.CreatePackageActivitySuccessAction";


        // citizen
        public const string CitizenVerify = "App.CitizenVerify";

        // post wall
        public const string PostCommentAction = "PostCommentAction";
        public const string PostReactAction = "PostReactAction";

        // digital service order
        public const string DigitalServiceOrder = "App.DigitalServiceOrderAction";

        // friend chat
        public const string FriendRequest = "App.FriendRequest";
    }

    public static class AppNotificationIcon
    {
        public const string UserWelcomeApp = "UserWelcomeApp";
        public const string AdminSendNotification = "AdminSendNotification";

        public const string StateVerifyCitizenSuccess = "App.StateVerifyCitizen.Accepted";
        public const string StateVerifyCitizenRefuse = "App.StateVerifyCitizen.Refuse";
        public const string StateVerifyCitizenDenied = "App.StateVerifyCitizen.Denied";


        public const string StateReflectCitizenDenied = "App.StateReflectCitizen.Denied";
        public const string StateReflectCitizenHandling = "App.StateReflectCitizen.Handling";
        public const string StateReflectCitizenConfirmed = "App.StateReflectCitizen.Confirmed";

        public const string CityVoteNewIcon = "App.CityVoteNewIcon";
        public const string CityNotificationNewIcon = "App.CityNotificationNewIcon";

        public const string AdministrativeNewIcon = "App.AdministrativeNewIcon";

        public const string CityNotificationCommentIcon = "App.CityNotificationCommentIcon";

        public const string ChatMessageIcon = "App.ChatMessageIcon";
        public const string StateAdministrativeIcon = "App.StateAdministrativeIcon";
        public const string BillPaymentSuccessIcon = "App.BillPaymentSuccessIcon";
        public const string BillPaymentCancelIcon = "App.BillPaymentCancelIcon";

        public const string UserBill = "App.UserBill";


        public const string CommentReflectCitizenSuccessIcon = "App.CommentReflectCitizenSuccessIcon";
        public const string ReflectCitizenNewIcon = "App.ReflectCitizenNewIcon";
        public const string WorkNotificationIcon = "App.WorkNotificationIcon";

        // smart-social
        public const string CreateOrderSuccessIcon = "App.CreateOrderSuccessIcon";
        public const string ShippingOrderIcon = "App.ShippingOrderIcon";
        public const string ShipperCompletedIcon = "App.ShipperCompletedIcon";
        public const string UserCompletedIcon = "App.UserCompletedIcon";
        public const string UserCancelIcon = "App.UserCancelIcon";
        public const string UserRequestCancelIcon = "App.UserRequestCancelIcon";
        public const string PartnerConfirmCancelIcon = "App.PartnerConfirmCancelIcon";
        public const string PartnerCancelOrderIcon = "App.PartnerCancelOrderIcon";
        public const string PartnerCancelIcon = "App.PartnerCancelIcon";
        public const string CreateBookingSuccessIcon = "App.CreateBookingSuccessIcon";

        // eco-farm
        public const string RegisterPackageSuccessIcon = "App.RegisterPackageSuccessIcon";
        public const string CancelPackageSuccessIcon = "App.CancelPackageSuccessIcon";
        public const string CreatePackageActivitySuccessIcon = "App.CreatePackageActivitySuccessIcon";


        // citizen
        public const string CitizenVerifyIcon = "App.CitizenVerifyIcon";

        // Post wall
        public const string PostCommentIcon = "App.PostCommentIcon";
        public const string PostReactIcon = "App.PostReactIcon";

        // digital service order
        public const string DigitalServiceOrder = "App.DigitalServiceOrderIcon";

        // friend chat
        public const string FriendShipIcon = "App.FriendShipIcon";

        // friend chat
        public const string TenantBusinessIcon = "App.TenantBusinessIcon";
    }
}
