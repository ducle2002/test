// ReSharper disable All

namespace Yootek.Authorization
{
    public static class PermissionNames
    {
        // Phân quyền dữ liệu
        public const string Data = "Data";
        // Quyền xem toàn bộ dữ liệu
        public const string Data_Admin = "Data.Admin";

       
        public const string Pages = "Pages";
        // [Begin] Tenant -> Khu đô thị, Tòa nhà, Block, Tầng, Căn hộ, Loại mặt bằng, Trạng thái mặt bằng

        // Quyền Smart social
        public const string Pages_SmartSocial = "Pages.SmartSocial";

        public const string Pages_SmartSocial_Setting = "Pages.SmartSocial.Setting";
        public const string Pages_SmartSocial_AppFeedback = "Pages.SmartSocial.AppFeedback";
        public const string Pages_SmartSocial_VerifyProvider = "Pages.SmartSocial.VerifyProvider";
        public const string Pages_SmartSocial_Notification = "Pages.SmartSocial.Notification";

        public const string Pages_TenantName = "Pages.TenantName";
     
        public const string Pages_TenantName_About = "Pages.TenantName.About";
        public const string Pages_TenantName_About_Get = "Pages.TenantName.About.Get";
        public const string Pages_TenantName_About_Edit = "Pages.TenantName.About.Edit";

        public const string Pages_TenantName_Urbans = "Pages.TenantName.Urbans";
        public const string Pages_TenantName_Urbans_GetAll = "Pages.TenantName.Urbans.GetAll";
        public const string Pages_TenantName_Urbans_GetDetail = "Pages.TenantName.Urbans.GetDetail";
        public const string Pages_TenantName_Urbans_Create = "Pages.TenantName.Urbans.Create";
        public const string Pages_TenantName_Urbans_Edit = "Pages.TenantName.Urbans.Edit";
        public const string Pages_TenantName_Urbans_Delete = "Pages.TenantName.Urbans.Delete";

        public const string Pages_TenantName_Buildings = "Pages.TenantName.Buildings";
        public const string Pages_TenantName_Buildings_GetAll = "Pages.TenantName.Buildings.GetAll";
        public const string Pages_TenantName_Buildings_GetDetail = "Pages.TenantName.Buildings.GetDetail";
        public const string Pages_TenantName_Buildings_Create = "Pages.TenantName.Buildings.Create";
        public const string Pages_TenantName_Buildings_Edit = "Pages.TenantName.Buildings.Edit";
        public const string Pages_TenantName_Buildings_Delete = "Pages.TenantName.Buildings.Delete";

        public const string Pages_TenantName_Blocks = "Pages.TenantName.Blocks";
        public const string Pages_TenantName_Blocks_GetAll = "Pages.TenantName.Blocks.GetAll";
        public const string Pages_TenantName_Blocks_GetDetail = "Pages.TenantName.Blocks.GetDetail";
        public const string Pages_TenantName_Blocks_Create = "Pages.TenantName.Blocks.Create";
        public const string Pages_TenantName_Blocks_Edit = "Pages.TenantName.Blocks.Edit";
        public const string Pages_TenantName_Blocks_Delete = "Pages.TenantName.Blocks.Delete";

        public const string Pages_TenantName_Floors = "Pages.TenantName.Floors";
        public const string Pages_TenantName_Floors_GetAll = "Pages.TenantName.Floors.GetAll";
        public const string Pages_TenantName_Floors_GetDetail = "Pages.TenantName.Floors.GetDetail";
        public const string Pages_TenantName_Floors_Create = "Pages.TenantName.Floors.Create";
        public const string Pages_TenantName_Floors_Edit = "Pages.TenantName.Floors.Edit";
        public const string Pages_TenantName_Floors_Delete = "Pages.TenantName.Floors.Delete";

        public const string Pages_TenantName_Apartments = "Pages.TenantName.Apartments";
        public const string Pages_TenantName_Apartments_GetAll = "Pages.TenantName.Apartments.GetAll";
        public const string Pages_TenantName_Apartments_GetDetail = "Pages.TenantName.Apartments.GetDetail";
        public const string Pages_TenantName_Apartments_Create = "Pages.TenantName.Apartments.Create";
        public const string Pages_TenantName_Apartments_Edit = "Pages.TenantName.Apartments.Edit";
        public const string Pages_TenantName_Apartments_Delete = "Pages.TenantName.Apartments.Delete";

        public const string Pages_TenantName_ApartmentTypes = "Pages.TenantName.ApartmentTypes";
        public const string Pages_TenantName_ApartmentTypes_GetAll = "Pages.TenantName.ApartmentTypes.GetAll";
        public const string Pages_TenantName_ApartmentTypes_GetDetail = "Pages.TenantName.ApartmentTypes.GetDetail";
        public const string Pages_TenantName_ApartmentTypes_Create = "Pages.TenantName.ApartmentTypes.Create";
        public const string Pages_TenantName_ApartmentTypes_Edit = "Pages.TenantName.ApartmentTypes.Edit";
        public const string Pages_TenantName_ApartmentTypes_Delete = "Pages.TenantName.ApartmentTypes.Delete";

        public const string Pages_TenantName_ApartmentStatuses = "Pages.TenantName.ApartmentStatuses";
        public const string Pages_TenantName_ApartmentStatuses_GetAll = "Pages.TenantName.ApartmentStatuses.GetAll";
        public const string Pages_TenantName_ApartmentStatuses_GetDetail = "Pages.TenantName.ApartmentStatuses.GetDetail";
        public const string Pages_TenantName_ApartmentStatuses_Create = "Pages.TenantName.ApartmentStatuses.Create";
        public const string Pages_TenantName_ApartmentStatuses_Edit = "Pages.TenantName.ApartmentStatuses.Edit";

        public const string Pages_TenantName_ApartmentStatuses_Delete = "Pages.TenantName.ApartmentStatuses.Delete";
        // [End]


        // [Begin] Quản lý cư dân -> Cư dân, xác minh cư dân

        public const string Pages_Citizens = "Pages.Citizens";

        public const string Pages_Citizens_List = "Pages.Citizens.List";
        public const string Pages_Citizens_List_GetAll = "Pages.Citizens.List.GetAll";
        public const string Pages_Citizens_List_GetDetail = "Pages.Citizens.List.GetDetail";
        public const string Pages_Citizens_List_Create = "Pages.Citizens.List.Create";
        public const string Pages_Citizens_List_Edit = "Pages.Citizens.List.Edit";
        public const string Pages_Citizens_List_Delete = "Pages.Citizens.List.Delete";

        public const string Pages_Citizens_Verifications = "Pages.Citizen.Verifications";
        public const string Pages_Citizens_Verifications_GetAll = "Pages.Citizen.Verifications.GetAll";
        public const string Pages_Citizens_Verifications_GetDetail = "Pages.Citizen.Verifications.GetDetail";
        public const string Pages_Citizens_Verifications_Create = "Pages.Citizen.Verifications.Create";
        public const string Pages_Citizens_Verifications_Edit = "Pages.Citizen.Verifications.Edit";
        public const string Pages_Citizens_Verifications_Delete = "Pages.Citizen.Verifications.Delete";
        public const string Pages_Citizens_Verifications_Approve = "Pages.Citizen.Verifications.Approve";
        public const string Pages_Citizens_Verifications_Request = "Pages.Citizen.Verifications.Request";
        public const string Pages_Citizens_Verifications_Decline = "Pages.Citizen.Verifications.Decline";

        // [End]

        // [Begin] Quản lý công dân (Chính phủ số)

        //public const string Pages_Citizens = "Pages.Citizens";

        //public const string Pages_Citizens_List = "Pages.Citizens.List";
        //public const string Pages_Citizens_List_GetAll = "Pages.Citizens.List.GetAll";
        //public const string Pages_Citizens_List_GetDetail = "Pages.Citizens.List.GetDetail";
        //public const string Pages_Citizens_List_Create = "Pages.Citizens.List.Create";
        //public const string Pages_Citizens_List_Edit = "Pages.Citizens.List.Edit";
        //public const string Pages_Citizens_List_Delete = "Pages.Citizens.List.Delete";

        //public const string Pages_Citizens_Verifications = "Pages.Citizen.Verifications";
        //public const string Pages_Citizens_Verifications_GetAll = "Pages.Citizen.Verifications.GetAll";
        //public const string Pages_Citizens_Verifications_GetDetail = "Pages.Citizen.Verifications.GetDetail";
        //public const string Pages_Citizens_Verifications_Create = "Pages.Citizen.Verifications.Create";
        //public const string Pages_Citizens_Verifications_Edit = "Pages.Citizen.Verifications.Edit";
        //public const string Pages_Citizens_Verifications_Delete = "Pages.Citizen.Verifications.Delete";
        //public const string Pages_Citizens_Verifications_Approve = "Pages.Citizen.Verifications.Approve";
        //public const string Pages_Citizens_Verifications_Request = "Pages.Citizen.Verifications.Request";
        //public const string Pages_Citizens_Verifications_Decline = "Pages.Citizen.Verifications.Decline";

        // [End]

        // [Begin] Ban quản lý số -> Thông báo số, Phản ánh số, Khảo sát số, Quản trị bảng tin, Giao tiếp số, Hỏi đáp, Đường dây nóng, Kinh tế số

        public const string Pages_Digitals = "Pages.Digitals";

        public const string Pages_Digitals_Notifications = "Pages.Digitals.Notifications";
        public const string Pages_Digitals_Notifications_GetAll = "Pages.Digitals.Notifications.GetAll";
        public const string Pages_Digitals_Notifications_GetDetail = "Pages.Digitals.Notifications.GetDetail";
        public const string Pages_Digitals_Notifications_Create = "Pages.Digitals.Notifications.Create";
        public const string Pages_Digitals_Notifications_Edit = "Pages.Digitals.Notifications.Edit";
        public const string Pages_Digitals_Notifications_Delete = "Pages.Digitals.Notifications.Delete";

        public const string Pages_Digitals_Reflects = "Pages.Digitals.Reflects";
        public const string Pages_Digitals_Reflects_GetAll = "Pages.Digitals.Reflects.GetAll";
        public const string Pages_Digitals_Reflects_GetDetail = "Pages.Digitals.Reflects.GetDetail";
        public const string Pages_Digitals_Reflects_Create = "Pages.Digitals.Reflects.Create";
        public const string Pages_Digitals_Reflects_Edit = "Pages.Digitals.Reflects.Edit";
        public const string Pages_Digitals_Reflects_Delete = "Pages.Digitals.Reflects.Delete";

        public const string Pages_Digitals_Surveys = "Pages.Digitals.Surveys";
        public const string Pages_Digitals_Surveys_GetAll = "Pages.Digitals.Surveys.GetAll";
        public const string Pages_Digitals_Surveys_GetDetail = "Pages.Digitals.Surveys.GetDetail";
        public const string Pages_Digitals_Surveys_Create = "Pages.Digitals.Surveys.Create";
        public const string Pages_Digitals_Surveys_Edit = "Pages.Digitals.Surveys.Edit";
        public const string Pages_Digitals_Surveys_Delete = "Pages.Digitals.Surveys.Delete";

        public const string Pages_Digitals_Forums = "Pages.Digitals.Forums";
        public const string Pages_Digitals_Forums_GetAll = "Pages.Digitals.Forums.GetAll";
        public const string Pages_Digitals_Forums_GetDetail = "Pages.Digitals.Forums.GetDetail";
        public const string Pages_Digitals_Forums_Create = "Pages.Digitals.Forums.Create";
        public const string Pages_Digitals_Forums_Edit = "Pages.Digitals.Forums.Edit";
        public const string Pages_Digitals_Forums_Approve = "Pages.Digitals.Forums.Approve";
        public const string Pages_Digitals_Forums_Delete = "Pages.Digitals.Forums.Delete";

        public const string Pages_Digitals_Communications = "Pages.Digitals.Communications";

        public const string Pages_Digitals_QnA = "Pages.Digitals.QnA";
        public const string Pages_Digitals_QnA_GetAll = "Pages.Digitals.QnA.GetAll";
        public const string Pages_Digitals_QnA_GetDetail = "Pages.Digitals.QnA.GetDetail";
        public const string Pages_Digitals_QnA_Create = "Pages.Digitals.QnA.Create";
        public const string Pages_Digitals_QnA_Edit = "Pages.Digitals.QnA.Edit";
        public const string Pages_Digitals_QnA_Approve = "Pages.Digitals.QnA.Approve";
        public const string Pages_Digitals_QnA_Decline = "Pages.Digitals.QnA.Decline";
        public const string Pages_Digitals_QnA_Delete = "Pages.Digitals.QnA.Delete";

        public const string Pages_Digitals_Hotline = "Pages.Digitals.Hotline";
        public const string Pages_Digitals_Hotline_GetAll = "Pages.Digitals.Hotline.GetAll";
        public const string Pages_Digitals_Hotline_GetDetail = "Pages.Digitals.Hotline.GetDetail";
        public const string Pages_Digitals_Hotline_Create = "Pages.Digitals.Hotline.Create";
        public const string Pages_Digitals_Hotline_Edit = "Pages.Digitals.Hotline.Edit";
        public const string Pages_Digitals_Hotline_Delete = "Pages.Digitals.Hotline.Delete";

        public const string Pages_Digitals_Economic = "Pages.Digitals.Economic";
        public const string Pages_Digitals_Economic_Create_Store = "Pages.Digitals.Economic.Create_Store";
        public const string Pages_Digitals_Economic_Approve_Store = "Pages.Digitals.Economic.Approve_Store";
        public const string Pages_Digitals_Economic_Shopping = "Pages.Digitals.Economic.Shopping";
        public const string Pages_Digitals_Economic_Work = "Pages.Digitals.Economic.Work";
        public const string Pages_Digitals_Economic_Repair = "Pages.Digitals.Economic.Repair";
        public const string Pages_Digitals_Economic_Health_Care = "Pages.Digitals.Economic.Health_Care";
        public const string Pages_Digitals_Economic_Entertainment = "Pages.Digitals.Economic.Entertainment";
        public const string Pages_Digitals_Economic_Report_User = "Pages.Digitals.Economic.Report_User";

        // [End]

        // [Begin] Quản lý hành chính/dịch vụ -> Cài đặt cấu hình, Quản lý xe/phương tiện, Vị trí đỗ xe

        public const string Pages_AdministrationService = "Pages.AdministrationService";

        public const string Pages_AdministrationService_Configurations = "Pages.AdministrationService.Configurations";

        public const string Pages_AdministrationService_Configurations_GetAll =
            "Pages.AdministrationService.Configurations.GetAll";

        public const string Pages_AdministrationService_Configurations_GetDetail =
            "Pages.AdministrationService.Configurations.GetDetail";

        public const string Pages_AdministrationService_Configurations_Create =
            "Pages.AdministrationService.Configurations.Create";

        public const string Pages_AdministrationService_Configurations_Edit =
            "Pages.AdministrationService.Configurations.Edit";

        public const string Pages_AdministrationService_Configurations_Delete =
            "Pages.AdministrationService.Configurations.Delete";

        public const string Pages_AdministrationService_Register_Vehicles = "Pages.AdministrationService.Register.Vehicles";
        public const string Pages_AdministrationService_Register_Vehicles_GetAll = "Pages.AdministrationService.Register.Vehicles.GetAll";

        public const string Pages_AdministrationService_Register_Vehicles_GetDetail =
            "Pages.AdministrationService.Register.Vehicles.GetDetail";

        public const string Pages_AdministrationService_Register_Vehicles_Create = "Pages.AdministrationService.Register.Vehicles.Create";
        public const string Pages_AdministrationService_Register_Vehicles_Edit = "Pages.AdministrationService.Register.Vehicles.Edit";
        public const string Pages_AdministrationService_Register_Vehicles_Delete = "Pages.AdministrationService.Register.Vehicles.Delete";

        public const string Pages_AdministrationService_Vehicles = "Pages.AdministrationService.Vehicles";
        public const string Pages_AdministrationService_Vehicles_GetAll = "Pages.AdministrationService.Vehicles.GetAll";

        public const string Pages_AdministrationService_Vehicles_GetDetail =
            "Pages.AdministrationService.Vehicles.GetDetail";

        public const string Pages_AdministrationService_Vehicles_Create = "Pages.AdministrationService.Vehicles.Create";
        public const string Pages_AdministrationService_Vehicles_Edit = "Pages.AdministrationService.Vehicles.Edit";
        public const string Pages_AdministrationService_Vehicles_Delete = "Pages.AdministrationService.Vehicles.Delete";

        public const string Pages_AdministrationService_ParkingSpots = "Pages.AdministrationService.ParkingSpots";

        public const string Pages_AdministrationService_ParkingSpots_GetAll =
            "Pages.AdministrationService.ParkingSpots.GetAll";

        public const string Pages_AdministrationService_ParkingSpots_GetDetail =
            "Pages.AdministrationService.ParkingSpots.GetDetail";

        public const string Pages_AdministrationService_ParkingSpots_Create =
            "Pages.AdministrationService.ParkingSpots.Create";

        public const string Pages_AdministrationService_ParkingSpots_Edit =
            "Pages.AdministrationService.ParkingSpots.Edit";

        public const string Pages_AdministrationService_ParkingSpots_Delete =
            "Pages.AdministrationService.ParkingSpots.Delete";

        // [End]

        // [Begin] Hóa đơn/Chi phí số -> Hóa đơn hàng tháng, Quản lý thanh toán, Công nợ, Phí dịch vụ, Tài khoản nhận thanh toán, Đối soát sao kê

        public const string Pages_Invoices = "Pages.Invoices";

        public const string Pages_Invoices_Monthly = "Pages.Invoices.Monthly";
        public const string Pages_Invoices_Monthly_GetAll = "Pages.Invoices.Monthly.GetAll";
        public const string Pages_Invoices_Monthly_GetDetail = "Pages.Invoices.Monthly.GetDetail";
        public const string Pages_Invoices_Monthly_Create = "Pages.Invoices.Monthly.Create";
        public const string Pages_Invoices_Monthly_Edit = "Pages.Invoices.Monthly.Edit";
        public const string Pages_Invoices_Monthly_Delete = "Pages.Invoices.Monthly.Delete";

        public const string Pages_Invoices_Payments = "Pages.Invoices.Payments";
        public const string Pages_Invoices_Payments_GetAll = "Pages.Invoices.Payments.GetAll";
        public const string Pages_Invoices_Payments_GetDetail = "Pages.Invoices.Payments.GetDetail";
        public const string Pages_Invoices_Payments_Create = "Pages.Invoices.Payments.Create";
        public const string Pages_Invoices_Payments_Edit = "Pages.Invoices.Payments.Edit";
        public const string Pages_Invoices_Payments_Delete = "Pages.Invoices.Payments.Delete";
        public const string Pages_Invoices_Payments_Advanced = "Pages.Invoices.Payments.Advanced";

        public const string Pages_Invoices_Debt = "Pages.Invoices.Debt";
        public const string Pages_Invoices_Debt_GetAll = "Pages.Invoices.Debt.GetAll";
        public const string Pages_Invoices_Debt_GetDetail = "Pages.Invoices.Debt.GetDetail";
        public const string Pages_Invoices_Debt_Create = "Pages.Invoices.Debt.Create";
        public const string Pages_Invoices_Debt_Edit = "Pages.Invoices.Debt.Edit";
        public const string Pages_Invoices_Debt_Delete = "Pages.Invoices.Debt.Delete";

        public const string Pages_Invoices_ServiceFees = "Pages.Invoices.ServiceFees";
        public const string Pages_Invoices_ServiceFees_GetAll = "Pages.Invoices.ServiceFees.GetAll";
        public const string Pages_Invoices_ServiceFees_GetDetail = "Pages.Invoices.ServiceFees.GetDetail";
        public const string Pages_Invoices_ServiceFees_Create = "Pages.Invoices.ServiceFees.Create";
        public const string Pages_Invoices_ServiceFees_Edit = "Pages.Invoices.ServiceFees.Edit";
        public const string Pages_Invoices_ServiceFees_Delete = "Pages.Invoices.ServiceFees.Delete";

        public const string Pages_Invoices_PaymentAccounts = "Pages.Invoices.PaymentAccounts";
        public const string Pages_Invoices_PaymentAccounts_GetAll = "Pages.Invoices.PaymentAccounts.GetAll";
        public const string Pages_Invoices_PaymentAccounts_GetDetail = "Pages.Invoices.PaymentAccounts.GetDetail";
        public const string Pages_Invoices_PaymentAccounts_Create = "Pages.Invoices.PaymentAccounts.Create";
        public const string Pages_Invoices_PaymentAccounts_Edit = "Pages.Invoices.PaymentAccounts.Edit";
        public const string Pages_Invoices_PaymentAccounts_Delete = "Pages.Invoices.PaymentAccounts.Delete";

        public const string Pages_Invoices_StatementVerification = "Pages.Invoices.StatementVerification";
        public const string Pages_Invoices_StatementVerification_GetAll = "Pages.Invoices.StatementVerification.GetAll";

        public const string Pages_Invoices_StatementVerification_GetDetail =
            "Pages.Invoices.StatementVerification.GetDetail";

        public const string Pages_Invoices_StatementVerification_Create = "Pages.Invoices.StatementVerification.Create";
        public const string Pages_Invoices_StatementVerification_Edit = "Pages.Invoices.StatementVerification.Edit";
        public const string Pages_Invoices_StatementVerification_Delete = "Pages.Invoices.StatementVerification.Delete";

        // [End]

        // [Begin] Quản lý tài sản/vật tư -> Danh mục tài sản/vật tư, Thiết lập thông số tài sản

        public const string Pages_Assets = "Pages.Assets";

        public const string Pages_Assets_AssetCatalog = "Pages.Assets.AssetCatalog";
        public const string Pages_Assets_AssetCatalog_GetAll = "Pages.Assets.AssetCatalog.GetAll";
        public const string Pages_Assets_AssetCatalog_GetDetail = "Pages.Assets.AssetCatalog.GetDetail";
        public const string Pages_Assets_AssetCatalog_Create = "Pages.Assets.AssetCatalog.Create";
        public const string Pages_Assets_AssetCatalog_Edit = "Pages.Assets.AssetCatalog.Edit";
        public const string Pages_Assets_AssetCatalog_Delete = "Pages.Assets.AssetCatalog.Delete";

        public const string Pages_Assets_AssetParameters = "Pages.Assets.AssetParameters";
        public const string Pages_Assets_AssetParameters_GetAll = "Pages.Assets.AssetParameters.GetAll";
        public const string Pages_Assets_AssetParameters_GetDetail = "Pages.Assets.AssetParameters.GetDetail";
        public const string Pages_Assets_AssetParameters_Create = "Pages.Assets.AssetParameters.Create";
        public const string Pages_Assets_AssetParameters_Edit = "Pages.Assets.AssetParameters.Edit";
        public const string Pages_Assets_AssetParameters_Delete = "Pages.Assets.AssetParameters.Delete";

        // [End]

        // [Begin] Quản lý tiện ích nội khu -> Danh mục tiện ích nội khu, Cấu hình tiện ích nội khu

        public const string Pages_LocalAmenities = "Pages.LocalAmenities";

        public const string Pages_LocalAmenities_List = "Pages.LocalAmenities.List";

        public const string Pages_LocalAmenities_List_GetAll =
            "Pages.LocalAmenities.List.GetAll";

        public const string Pages_LocalAmenities_List_GetDetail =
            "Pages.LocalAmenities.List.GetDetail";

        public const string Pages_LocalAmenities_List_Create =
            "Pages.LocalAmenities.List.Create";

        public const string Pages_LocalAmenities_List_Edit = "Pages.LocalAmenities.List.Edit";

        public const string Pages_LocalAmenities_List_Delete =
            "Pages.LocalAmenities.List.Delete";

        public const string Pages_LocalAmenities_ServiceConfigurations =
            "Pages.LocalAmenities.ServiceConfigurations";

        public const string Pages_LocalAmenities_ServiceConfigurations_GetAll =
            "Pages.LocalAmenities.ServiceConfigurations.GetAll";

        public const string Pages_LocalAmenities_ServiceConfigurations_GetDetail =
            "Pages.LocalAmenities.ServiceConfigurations.GetDetail";

        public const string Pages_LocalAmenities_ServiceConfigurations_Create =
            "Pages.LocalAmenities.ServiceConfigurations.Create";

        public const string Pages_LocalAmenities_ServiceConfigurations_Edit =
            "Pages.LocalAmenities.ServiceConfigurations.Edit";

        public const string Pages_LocalAmenities_ServiceConfigurations_Delete =
            "Pages.LocalAmenities.ServiceConfigurations.Delete";

        // [End]

        // [Begin] Vận hành ban quản lý -> Cơ cấu tổ chức, Quản lý nhân sự, Giao việc/Quản lý công việc

        public const string Pages_Operations = "Pages.Operations";

        public const string Pages_Operations_OrganizationStructure = "Pages.Operations.OrganizationStructure";
        public const string Pages_Operations_OrganizationStructure_GetAll =
            "Pages.Operations.OrganizationStructure.GetAll";
        public const string Pages_Operations_OrganizationStructure_GetDetail =
            "Pages.Operations.OrganizationStructure.GetDetail";
        public const string Pages_Operations_OrganizationStructure_Create =
            "Pages.Operations.OrganizationStructure.Create";
        public const string Pages_Operations_OrganizationStructure_Edit = "Pages.Operations.OrganizationStructure.Edit";
        public const string Pages_Operations_OrganizationStructure_Delete =
            "Pages.Operations.OrganizationStructure.Delete";

        public const string Pages_Operations_OrganizationUnits = "Pages.Operations.OrganizationUnits";
        public const string Pages_Operations_OrganizationUnits_GetAll =
            "Pages.Operations.OrganizationUnits.GetAll";
        public const string Pages_Operations_OrganizationUnits_GetDetail =
            "Pages.Operations.OrganizationUnits.GetDetail";
        public const string Pages_Operations_OrganizationUnits_Create =
            "Pages.Operations.OrganizationUnits.Create";
        public const string Pages_Operations_OrganizationUnits_Edit = "Pages.Operations.OrganizationUnits.Edit";
        public const string Pages_Operations_OrganizationUnits_Delete =
            "Pages.Operations.OrganizationUnits.Delete";

        public const string Pages_Operations_Personnel = "Pages.Operations.Personnel";
        public const string Pages_Operations_Personnel_GetAll = "Pages.Operations.Personnel.GetAll";
        public const string Pages_Operations_Personnel_GetDetail = "Pages.Operations.Personnel.GetDetail";
        public const string Pages_Operations_Personnel_Create = "Pages.Operations.Personnel.Create";
        public const string Pages_Operations_Personnel_Edit = "Pages.Operations.Personnel.Edit";
        public const string Pages_Operations_Personnel_Delete = "Pages.Operations.Personnel.Delete";

        public const string Pages_Operations_TaskManagement = "Pages.Operations.TaskManagement";
        public const string Pages_Operations_TaskManagement_GetAll = "Pages.Operations.TaskManagement.GetAll";
        public const string Pages_Operations_TaskManagement_GetDetail = "Pages.Operations.TaskManagement.GetDetail";
        public const string Pages_Operations_TaskManagement_Create = "Pages.Operations.TaskManagement.Create";
        public const string Pages_Operations_TaskManagement_Edit = "Pages.Operations.TaskManagement.Edit";
        public const string Pages_Operations_TaskManagement_Delete = "Pages.Operations.TaskManagement.Delete";

        public const string Pages_Operations_TaskTypeManagement = "Pages.Operations.TaskTypeManagement";
        public const string Pages_Operations_TaskTypeManagement_GetAll = "Pages.Operations.TaskTypeManagement.GetAll";
        public const string Pages_Operations_TaskTypeManagement_GetDetail = "Pages.Operations.TaskTypeManagement.GetDetail";
        public const string Pages_Operations_TaskTypeManagement_Create = "Pages.Operations.TaskTypeManagement.Create";
        public const string Pages_Operations_TaskTypeManagement_Edit = "Pages.Operations.TaskTypeManagement.Edit";
        public const string Pages_Operations_TaskTypeManagement_Delete = "Pages.Operations.TaskTypeManagement.Delete";

        // [End]

        //[Begin] Đồng hồ đo điện nước
        public const string Pages_MeterManagement = "Pages.Meter";
        public const string Pages_MeterManagement_List = "Pages.Meter.List";
        public const string Pages_MeterManagement_List_GetAll = "Pages.Meter.List.GetAll";
        public const string Pages_MeterManagement_List_Edit = "Pages.Meter.List.Edit";
        public const string Pages_MeterManagement_List_Delete = "Pages.Meter.List.Delete";
        public const string Pages_MeterManagement_List_Create = "Pages.Meter.List.Create";

        public const string Pages_MeterManagement_Type = "Pages.Meter.Type";
        public const string Pages_MeterManagement_Type_GetAll = "Pages.Meter.Type.GetAll";
        public const string Pages_MeterManagement_Type_Create = "Pages.Meter.Type.Create";
        public const string Pages_MeterManagement_Type_Edit = "Pages.Meter.Type.Edit";
        public const string Pages_MeterManagement_Type_Delete = "Pages.Meter.Type.Delete";

        public const string Pages_MeterManagement_Monthly = "Pages.Meter.Monthly";
        public const string Pages_MeterManagement_Monthly_GetAll = "Pages.Meter.Monthly.GetAll";
        public const string Pages_MeterManagement_Monthly_Create = "Pages.Meter.Monthly.Create";
        public const string Pages_MeterManagement_Monthly_Edit = "Pages.Meter.Monthly.Edit";
        public const string Pages_MeterManagement_Monthly_Delete = "Pages.Meter.Monthly.Delete";
        //[End]
        
        
        
        // [Begin] Quản trị hệ thống -> Quản lý tài khoản, Phân quyền, Quản lý Tenant
        public const string Pages_SystemAdministration = "Pages.SystemAdministration";

        public const string Pages_SystemAdministration_AccountManagement =
            "Pages.SystemAdministration.AccountManagement";

        public const string Pages_SystemAdministration_AccountManagement_GetAll =
            "Pages.SystemAdministration.AccountManagement.GetAll";
        public const string Pages_SystemAdministration_AccountManagement_GetDetail =
            "Pages.SystemAdministration.AccountManagement.GetDetail";
        public const string Pages_SystemAdministration_AccountManagement_Create =
            "Pages.SystemAdministration.AccountManagement.Create";
        public const string Pages_SystemAdministration_AccountManagement_Edit =
            "Pages.SystemAdministration.AccountManagement.Edit";
        public const string Pages_SystemAdministration_AccountManagement_Delete =
            "Pages.SystemAdministration.AccountManagement.Delete";
        public const string Pages_SystemAdministration_Roles = "Pages.SystemAdministration.Roles";
        public const string Pages_SystemAdministration_Roles_GetAll =
            "Pages.SystemAdministration.Roles.GetAll";
        public const string Pages_SystemAdministration_Roles_GetDetail =
            "Pages.SystemAdministration.Roles.GetDetail";
        public const string Pages_SystemAdministration_Roles_Create =
            "Pages.SystemAdministration.Roles.Create";
        public const string Pages_SystemAdministration_Roles_Edit = "Pages.SystemAdministration.Roles.Edit";
        public const string Pages_SystemAdministration_Roles_Delete =
            "Pages.SystemAdministration.Roles.Delete";

        // [End]

        // [Begin] Báo cáo/Thống kê

        public const string Pages_Reporting = "Pages.Reporting";

        public static string Pages_Reporting_Overview = "Pages.Reporting.Overview";
        public static string Pages_Reporting_Overview_CitizenReflect = "Pages.Reporting.Overview.CitizenReflect";
        public static string Pages_Reporting_Overview_CitizenChat = "Pages.Reporting.Overview.CitizenChat";
        public static string Pages_Reporting_Overview_CitizenVote = "Pages.Reporting.Overview.CitizenVote";
        public static string Pages_Reporting_Overview_CitizenUseApp = "Pages.Reporting.Overview.CitizenUseApp";
        public static string Pages_Reporting_Overview_Account = "Pages.Reporting.Overview.Account";

        public static string Pages_Reporting_Invoice = "Pages.Reporting.Invoice";
        public static string Pages_Reporting_Asset = "Pages.Reporting.Asset";
        public static string Pages_Reporting_CitizenReflect = "Pages.Reporting.CitizenReflect";
        public static string Pages_Reporting_Citizen = "Pages.Reporting.Citizen";
        public static string Pages_Reporting_WorkManagement = "Pages.Reporting.WorkManagement";

        // [End]

        // [Begin] Quản lý tài liệu

        public const string Pages_DocumentManagement = "Pages.DocumentManagement";

        public const string Pages_DocumentManagement_List = "Pages.DocumentManagement.List";
        public const string Pages_DocumentManagement_List_GetAll = "Pages.DocumentManagement.List.GetAll";
        public const string Pages_DocumentManagement_List_GetDetail = "Pages.DocumentManagement.List.GetDetail";
        public const string Pages_DocumentManagement_List_Create = "Pages.DocumentManagement.List.Create";
        public const string Pages_DocumentManagement_List_Edit = "Pages.DocumentManagement.List.Edit";
        public const string Pages_DocumentManagement_List_Delete = "Pages.DocumentManagement.List.Delete";

        public const string Pages_DocumentManagement_Type = "Pages.DocumentManagement.Type";
        public const string Pages_DocumentManagement_Type_GetAll = "Pages.DocumentManagement.Type.GetAll";
        public const string Pages_DocumentManagement_Type_GetDetail = "Pages.DocumentManagement.Type.GetDetail";
        public const string Pages_DocumentManagement_Type_Create = "Pages.DocumentManagement.Type.Create";
        public const string Pages_DocumentManagement_Type_Edit = "Pages.DocumentManagement.Type.Edit";
        public const string Pages_DocumentManagement_Type_Delete = "Pages.DocumentManagement.Type.Delete";

        // [End]

        public const string Pages_Users = "Pages.Users";
        public const string Pages_User_Detail = "Pages.User.Detail";
        public const string Pages_Users_Activation = "Pages.Users.Activation";

        // Root Roles
        public const string Pages_Roles = "Pages.Roles";


        // Resident: công dân số
        public const string Pages_Residents = "Pages.Resident";
        public const string Pages_Residents_Verification = "Pages.Residents.Verification";
        public const string Pages_Residents_Information = "Pages.Residents.Information";


        // Chính phủ số
        public const string Pages_Government = "Pages.Government";

        // Chat (chính phủ)
        public const string Pages_Government_ChatCitizen = "Pages.Government.ChatCitizen";

        // Phản ánh (chính phủ)
        public const string Pages_Government_Citizens_Reflects = "Pages.Government.Citizens.Reflects";
        public const string Pages_Government_Citizens_Reflects_Create = "Pages.Government.Citizens.Reflects.Create";
        public const string Pages_Government_Citizens_Reflects_Update = "Pages.Government.Citizens.Reflects.Update";
        public const string Pages_Government_Citizens_Reflects_View = "Pages.Government.Citizens.Reflects.View";
        public const string Pages_Government_Citizens_Reflects_Delete = "Pages.Government.Citizens.Reflects.Delete";

        public const string Pages_Government_Citizens_Reflects_AssignDepartment =
            "Pages.Government.Citizens.Reflects.AssignDepartment";

        public const string Pages_Government_Citizens_Reflects_AssignHandler =
            "Pages.Government.Citizens.Reflects.AssignHandler";

        public const string Pages_Government_Citizens_Reflects_Chat = "Pages.Government.Citizens.Reflects.Chat";
        public const string Pages_Government_Citizens_Reflects_Handle = "Pages.Government.Citizens.Reflects.Handle";

        // Khảo sát (chính phủ)
        public const string Pages_Government_Citizens_Vote = "Pages.Government.Citizens.Vote";

        // Thông báo số (bảng tin trên app) (chính phủ)
        public const string Pages_Government_Digital_Notices = "Pages.Government.Digital.Notices";

        // Cổng thông tin điện tử
        public const string Pages_Government_Portal = "Pages.Government.Portal";

        public const string Pages_Government_Question_Answer = "Pages.Government.Question_Answer";
        public const string Pages_Government_DigitalDaily = "Pages.Government.DigitalDaily";


        // quản trị
        public const string Pages_Admin = "Pages.Admin";
        public const string Pages_Admin_Type_Local_Service = "Pages.Admin.Type_Local_Service";
        public const string Pages_Admin_Organization_Units = "Pages.Admin.Organization_Units";
        public const string Pages_Admin_Type_Administrative = "Pages.Admin.Type_Administrative";
        public const string Pages_Admin_Tenant_Settings = "Pages.Admin.Tenant_Settings";
        public const string Pages_Admin_Organization_Structure = "Pages.Admin.Organization_Structure";

        public const string Pages_Settings = "Pages.Settings";
        public const string Pages_Settings_Images = "Pages.Settings.Images";
        public const string Pages_Settings_Images_GetAll = "Pages.Settings.Images.GetAll";
        public const string Pages_Settings_Images_GetDetail = "Pages.Settings.Images.GetDetail";
        public const string Pages_Settings_Images_Create = "Pages.Settings.Images.Create";
        public const string Pages_Settings_Images_Edit = "Pages.Settings.Images.Edit";
        public const string Pages_Settings_Images_Delete = "Pages.Settings.Images.Delete";


        //UI
        public const string Pages_DemoUiComponents = "Pages.DemoUiComponents";
        public const string Pages_Administration = "Pages.Administration";
        public const string Pages_Administration_Roles = "Pages.Administration.Roles";
        public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
        public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
        public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";

        public const string Pages_Administration_Users = "Pages.Administration.Users";
        public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
        public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
        public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";

        public const string Pages_Administration_Users_ChangePermissions =
            "Pages.Administration.Users.ChangePermissions";

        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";
        public const string Pages_Administration_Users_Unlock = "Pages.Administration.Users.Unlock";

        public const string Pages_Administration_Languages = "Pages.Administration.Languages";
        public const string Pages_Administration_Languages_Create = "Pages.Administration.Languages.Create";
        public const string Pages_Administration_Languages_Edit = "Pages.Administration.Languages.Edit";
        public const string Pages_Administration_Languages_Delete = "Pages.Administration.Languages.Delete";
        public const string Pages_Administration_Languages_ChangeTexts = "Pages.Administration.Languages.ChangeTexts";

        public const string Pages_Administration_Languages_ChangeDefaultLanguage =
            "Pages.Administration.Languages.ChangeDefaultLanguage";

        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";

        public const string Pages_Administration_OrganizationUnits = "Pages.Administration.OrganizationUnits";

        public const string Pages_Administration_OrganizationUnits_ManageOrganizationTree =
            "Pages.Administration.OrganizationUnits.ManageOrganizationTree";

        public const string Pages_Administration_OrganizationUnits_ManageMembers =
            "Pages.Administration.OrganizationUnits.ManageMembers";

        public const string Pages_Administration_OrganizationUnits_ManageRoles =
            "Pages.Administration.OrganizationUnits.ManageRoles";

        public const string Pages_Administration_HangfireDashboard = "Pages.Administration.HangfireDashboard";

        public const string Pages_Administration_UiCustomization = "Pages.Administration.UiCustomization";

        public const string Pages_Administration_WebhookSubscription = "Pages.Administration.WebhookSubscription";

        public const string Pages_Administration_WebhookSubscription_Create =
            "Pages.Administration.WebhookSubscription.Create";

        public const string Pages_Administration_WebhookSubscription_Edit =
            "Pages.Administration.WebhookSubscription.Edit";

        public const string Pages_Administration_WebhookSubscription_ChangeActivity =
            "Pages.Administration.WebhookSubscription.ChangeActivity";

        public const string Pages_Administration_WebhookSubscription_Detail =
            "Pages.Administration.WebhookSubscription.Detail";

        public const string Pages_Administration_Webhook_ListSendAttempts =
            "Pages.Administration.Webhook.ListSendAttempts";

        public const string Pages_Administration_Webhook_ResendWebhook = "Pages.Administration.Webhook.ResendWebhook";

        public const string Pages_Administration_DynamicProperties = "Pages.Administration.DynamicProperties";

        public const string Pages_Administration_DynamicProperties_Create =
            "Pages.Administration.DynamicProperties.Create";

        public const string Pages_Administration_DynamicProperties_Edit = "Pages.Administration.DynamicProperties.Edit";

        public const string Pages_Administration_DynamicProperties_Delete =
            "Pages.Administration.DynamicProperties.Delete";

        public const string Pages_Administration_DynamicPropertyValue = "Pages.Administration.DynamicPropertyValue";

        public const string Pages_Administration_DynamicPropertyValue_Create =
            "Pages.Administration.DynamicPropertyValue.Create";

        public const string Pages_Administration_DynamicPropertyValue_Edit =
            "Pages.Administration.DynamicPropertyValue.Edit";

        public const string Pages_Administration_DynamicPropertyValue_Delete =
            "Pages.Administration.DynamicPropertyValue.Delete";

        public const string Pages_Administration_DynamicEntityProperties =
            "Pages.Administration.DynamicEntityProperties";

        public const string Pages_Administration_DynamicEntityProperties_Create =
            "Pages.Administration.DynamicEntityProperties.Create";

        public const string Pages_Administration_DynamicEntityProperties_Edit =
            "Pages.Administration.DynamicEntityProperties.Edit";

        public const string Pages_Administration_DynamicEntityProperties_Delete =
            "Pages.Administration.DynamicEntityProperties.Delete";

        public const string Pages_Administration_DynamicEntityPropertyValue =
            "Pages.Administration.DynamicEntityPropertyValue";

        public const string Pages_Administration_DynamicEntityPropertyValue_Create =
            "Pages.Administration.DynamicEntityPropertyValue.Create";

        public const string Pages_Administration_DynamicEntityPropertyValue_Edit =
            "Pages.Administration.DynamicEntityPropertyValue.Edit";

        public const string Pages_Administration_DynamicEntityPropertyValue_Delete =
            "Pages.Administration.DynamicEntityPropertyValue.Delete";
        //TENANT-SPECIFIC PERMISSIONS

        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";

        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";

        public const string Pages_Administration_Tenant_SubscriptionManagement =
            "Pages.Administration.Tenant.SubscriptionManagement";

        //HOST-SPECIFIC PERMISSIONS

        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";
        public const string Pages_Editions_MoveTenantsToAnotherEdition = "Pages.Editions.MoveTenantsToAnotherEdition";

        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Create = "Pages.Tenants.Create";
        public const string Pages_Tenants_Edit = "Pages.Tenants.Edit";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";
        public const string Pages_Tenants_Delete = "Pages.Tenants.Delete";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";

        public const string Pages_Administration_Host_Maintenance = "Pages.Administration.Host.Maintenance";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";


        // Thiết bị thông minh
        public const string Pages_Smart_Device = "Pages.SmartDevice";
        public const string Pages_Parking_Management = "Pages.ParkingManagement";
    }
}