// ReSharper disable All

using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace IMAX.Authorization
{
    public class IMAXAuthorizationProvider : AuthorizationProvider
    {
        public void createMenuPermission(IPermissionDefinitionContext context)
        {
            // Get all 
        }

        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            var dataAdmin = context.CreatePermission(PermissionNames.Data_Admin, L("Quyền dữ liệu admin"));

            var tenantName = context.CreatePermission(PermissionNames.Pages_TenantName, L("Trang chủ Tenant"));

            var tenantNameAbout = tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_About, L("Giới thiệu Tenant"));
            tenantNameAbout.CreateChildPermission(PermissionNames.Pages_TenantName_About_Get, L("Xem giới thiệu Tenant"));
            tenantNameAbout.CreateChildPermission(PermissionNames.Pages_TenantName_About_Edit, L("Chỉnh sửa giới thiệu Tenant"));

            var tenantNameUrbans = tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_Urbans, L("Quản lý khu đô thị"));
            tenantNameUrbans.CreateChildPermission(PermissionNames.Pages_TenantName_Urbans_GetAll, L("Xem danh sách khu đô thị"));
            tenantNameUrbans.CreateChildPermission(PermissionNames.Pages_TenantName_Urbans_GetDetail, L("Xem chi tiết khu đô thị"));
            tenantNameUrbans.CreateChildPermission(PermissionNames.Pages_TenantName_Urbans_Create, L("Thêm mới khu đô thị"));
            tenantNameUrbans.CreateChildPermission(PermissionNames.Pages_TenantName_Urbans_Edit, L("Chỉnh sửa khu đô thị"));
            tenantNameUrbans.CreateChildPermission(PermissionNames.Pages_TenantName_Urbans_Delete, L("Xóa khu đô thị"));

            var tenantNameBuildings = tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_Buildings, L("Quản lý toà nhà"));
            tenantNameBuildings.CreateChildPermission(PermissionNames.Pages_TenantName_Buildings_GetAll, L("Xem danh sách toà nhà"));
            tenantNameBuildings.CreateChildPermission(PermissionNames.Pages_TenantName_Buildings_GetDetail, L("Xem chi tiết toà nhà"));
            tenantNameBuildings.CreateChildPermission(PermissionNames.Pages_TenantName_Buildings_Create, L("Thêm mới tòa nhà"));
            tenantNameBuildings.CreateChildPermission(PermissionNames.Pages_TenantName_Buildings_Edit, L("Chỉnh sửa tòa nhà"));
            tenantNameBuildings.CreateChildPermission(PermissionNames.Pages_TenantName_Buildings_Delete, L("Xóa tòa nhà"));

            var tenantNameBlocks = tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_Blocks, L("Quản lý block"));
            tenantNameBlocks.CreateChildPermission(PermissionNames.Pages_TenantName_Blocks_GetAll, L("Xem danh sách block"));
            tenantNameBlocks.CreateChildPermission(PermissionNames.Pages_TenantName_Blocks_GetDetail, L("Xem chi tiết block"));
            tenantNameBlocks.CreateChildPermission(PermissionNames.Pages_TenantName_Blocks_Create, L("Thêm mới block"));
            tenantNameBlocks.CreateChildPermission(PermissionNames.Pages_TenantName_Blocks_Edit, L("Chỉnh sửa block"));
            tenantNameBlocks.CreateChildPermission(PermissionNames.Pages_TenantName_Blocks_Delete, L("Xóa block"));

            var tenantNameFloors = tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_Floors, L("Quản lý tầng"));
            tenantNameFloors.CreateChildPermission(PermissionNames.Pages_TenantName_Floors_GetAll, L("Xem danh sách tầng"));
            tenantNameFloors.CreateChildPermission(PermissionNames.Pages_TenantName_Floors_GetDetail, L("Xem chi tiết tầng"));
            tenantNameFloors.CreateChildPermission(PermissionNames.Pages_TenantName_Floors_Create, L("Thêm mới tầng"));
            tenantNameFloors.CreateChildPermission(PermissionNames.Pages_TenantName_Floors_Edit, L("Chỉnh sửa tầng"));
            tenantNameFloors.CreateChildPermission(PermissionNames.Pages_TenantName_Floors_Delete, L("Xóa tầng"));

            var tenantNameApartments = tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_Apartments, L("Quản lý căn hộ"));
            tenantNameApartments.CreateChildPermission(PermissionNames.Pages_TenantName_Apartments_GetAll, L("Xem danh sách căn hộ"));
            tenantNameApartments.CreateChildPermission(PermissionNames.Pages_TenantName_Apartments_GetDetail, L("Xem chi tiết căn hộ"));
            tenantNameApartments.CreateChildPermission(PermissionNames.Pages_TenantName_Apartments_Create, L("Thêm mới căn hộ"));
            tenantNameApartments.CreateChildPermission(PermissionNames.Pages_TenantName_Apartments_Edit, L("Chỉnh sửa căn hộ"));
            tenantNameApartments.CreateChildPermission(PermissionNames.Pages_TenantName_Apartments_Delete, L("Xóa căn hộ"));

            var tenantNameApartmentTypes = tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentTypes, L("Quản lý loại mặt bằng"));
            tenantNameApartmentTypes.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentTypes_GetAll, L("Xem danh sách loại mặt bằng"));
            tenantNameApartmentTypes.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentTypes_GetDetail, L("Xem chi tiết loại mặt bằng"));
            tenantNameApartmentTypes.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentTypes_Create, L("Thêm mới loại mặt bằng"));
            tenantNameApartmentTypes.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentTypes_Edit, L("Chỉnh sửa loại mặt bằng"));
            tenantNameApartmentTypes.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentTypes_Delete, L("Xóa loại mặt bằng"));

            var tenantNameApartmentStatuses =
                tenantName.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentStatuses, L("Quản lý trạng thái mặt bằng"));
            tenantNameApartmentStatuses.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentStatuses_GetAll, L("Xem danh sách trạng thái mặt bằng"));
            tenantNameApartmentStatuses.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentStatuses_GetDetail, L("Xem chi tiết trạng thái mặt bằng"));
            tenantNameApartmentStatuses.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentStatuses_Create, L("Thêm mới trạng thái mặt bằng"));
            tenantNameApartmentStatuses.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentStatuses_Edit, L("Chỉnh sửa trạng thái mặt bằng"));
            tenantNameApartmentStatuses.CreateChildPermission(PermissionNames.Pages_TenantName_ApartmentStatuses_Delete, L("Xóa trạng thái mặt bằng"));


            var citizens = context.CreatePermission(PermissionNames.Pages_Citizens, L("Quản lý cư dân"));

            var citizensList = citizens.CreateChildPermission(PermissionNames.Pages_Citizens_List, L("Danh sách cư dân"));
            citizensList.CreateChildPermission(PermissionNames.Pages_Citizens_List_GetAll, L("Xem danh sách cư dân"));
            citizensList.CreateChildPermission(PermissionNames.Pages_Citizens_List_GetDetail, L("Xem chi tiết cư dân"));
            citizensList.CreateChildPermission(PermissionNames.Pages_Citizens_List_Create, L("Thêm mới cư dân"));
            citizensList.CreateChildPermission(PermissionNames.Pages_Citizens_List_Edit, L("Chỉnh sửa cư dân"));
            citizensList.CreateChildPermission(PermissionNames.Pages_Citizens_List_Delete, L("Xóa cư dân"));

            var citizensVerifications = citizens.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications, L("Quản lý xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_GetAll, L("Xem danh sách xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_GetDetail, L("Xem chi tiết xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_Create, L("Thêm mới xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_Edit, L("Chỉnh sửa xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_Delete, L("Xóa xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_Approve, L("Duyệt xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_Request, L("Yêu cầu xác minh cư dân"));
            citizensVerifications.CreateChildPermission(PermissionNames.Pages_Citizens_Verifications_Decline, L("Từ chối xác minh cư dân"));


            var digitals = context.CreatePermission(PermissionNames.Pages_Digitals, L("Ban quản lý số"));

            var digitalsNotifications = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_Notifications, L("Thông báo số"));
            digitalsNotifications.CreateChildPermission(PermissionNames.Pages_Digitals_Notifications_GetAll, L("Xem danh sách thông báo số"));
            digitalsNotifications.CreateChildPermission(PermissionNames.Pages_Digitals_Notifications_GetDetail, L("Xem chi tiết thông báo số"));
            digitalsNotifications.CreateChildPermission(PermissionNames.Pages_Digitals_Notifications_Create, L("Thêm mới thông báo số"));
            digitalsNotifications.CreateChildPermission(PermissionNames.Pages_Digitals_Notifications_Edit, L("Chỉnh sửa thông báo số"));
            digitalsNotifications.CreateChildPermission(PermissionNames.Pages_Digitals_Notifications_Delete, L("Xóa thông báo số"));

            var digitalsReflects = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_Reflects, L("Phản ánh số"));
            digitalsReflects.CreateChildPermission(PermissionNames.Pages_Digitals_Reflects_GetAll, L("Xem danh sách phản ánh số"));
            digitalsReflects.CreateChildPermission(PermissionNames.Pages_Digitals_Reflects_GetDetail, L("Xem chi tiết phản ánh số"));
            digitalsReflects.CreateChildPermission(PermissionNames.Pages_Digitals_Reflects_Create, L("Thêm mới phản ánh số"));
            digitalsReflects.CreateChildPermission(PermissionNames.Pages_Digitals_Reflects_Edit, L("Chỉnh sửa phản ánh số"));
            digitalsReflects.CreateChildPermission(PermissionNames.Pages_Digitals_Reflects_Delete, L("Xóa phản ánh số"));

            var digitalsSurveys = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_Surveys, L("Khảo sát số"));
            digitalsSurveys.CreateChildPermission(PermissionNames.Pages_Digitals_Surveys_GetAll, L("Xem danh sách khảo sát số"));
            digitalsSurveys.CreateChildPermission(PermissionNames.Pages_Digitals_Surveys_GetDetail, L("Xem chi tiết khảo sát số"));
            digitalsSurveys.CreateChildPermission(PermissionNames.Pages_Digitals_Surveys_Create, L("Thêm mới khảo sát số'"));
            digitalsSurveys.CreateChildPermission(PermissionNames.Pages_Digitals_Surveys_Edit, L("Chỉnh sửa khảo sát số"));
            digitalsSurveys.CreateChildPermission(PermissionNames.Pages_Digitals_Surveys_Delete, L("Xóa khảo sát số"));

            var digitalsForums = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_Forums, L("Diễn bảng tin"));
            digitalsForums.CreateChildPermission(PermissionNames.Pages_Digitals_Forums_GetAll, L("Xem danh bảng tin"));
            digitalsForums.CreateChildPermission(PermissionNames.Pages_Digitals_Forums_GetDetail, L("Xem chi tiết bảng tin"));
            digitalsForums.CreateChildPermission(PermissionNames.Pages_Digitals_Forums_Create, L("Thêm mới bảng tin"));
            digitalsForums.CreateChildPermission(PermissionNames.Pages_Digitals_Forums_Edit, L("Chỉnh sửa bảng tin"));
            digitalsForums.CreateChildPermission(PermissionNames.Pages_Digitals_Forums_Approve, L("Duyệt bảng tin"));
            digitalsForums.CreateChildPermission(PermissionNames.Pages_Digitals_Forums_Delete, L("Xóa bảng tin"));

            var digitalsCommunications = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_Communications, L("Chuyển giao thông tin"));

            var digitalsQnA = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_QnA, L("Hỏi đáp"));
            digitalsQnA.CreateChildPermission(PermissionNames.Pages_Digitals_QnA_GetAll, L("Xem danh sách hỏi đáp"));
            digitalsQnA.CreateChildPermission(PermissionNames.Pages_Digitals_QnA_GetDetail, L("Xem chi tiết hỏi đáp"));
            digitalsQnA.CreateChildPermission(PermissionNames.Pages_Digitals_QnA_Create, L("Thêm mới hỏi đáp"));
            digitalsQnA.CreateChildPermission(PermissionNames.Pages_Digitals_QnA_Edit, L("Chỉnh sửa hỏi đáp"));
            digitalsQnA.CreateChildPermission(PermissionNames.Pages_Digitals_QnA_Approve, L("Duyệt hỏi đáp"));
            digitalsQnA.CreateChildPermission(PermissionNames.Pages_Digitals_QnA_Decline, L("Từ chối hỏi đáp"));
            digitalsQnA.CreateChildPermission(PermissionNames.Pages_Digitals_QnA_Delete, L("Xóa hỏi đáp"));

            var digitalsHotline = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_Hotline, L("Đường dây nóng"));
            digitalsHotline.CreateChildPermission(PermissionNames.Pages_Digitals_Hotline_GetAll, L("Xem danh sách đường dây nóng"));
            digitalsHotline.CreateChildPermission(PermissionNames.Pages_Digitals_Hotline_GetDetail, L("Xem chi tiết đường dây nóng"));
            digitalsHotline.CreateChildPermission(PermissionNames.Pages_Digitals_Hotline_Create, L("Thêm mới đường dây nóng"));
            digitalsHotline.CreateChildPermission(PermissionNames.Pages_Digitals_Hotline_Edit, L("Chỉnh sửa đường dây nóng"));
            digitalsHotline.CreateChildPermission(PermissionNames.Pages_Digitals_Hotline_Delete, L("Xóa đường dây nóng"));

            var digitalsEconomic = digitals.CreateChildPermission(PermissionNames.Pages_Digitals_Economic, L("Kinh tế - Dịch vụ"));
            digitalsEconomic.CreateChildPermission(PermissionNames.Pages_Digitals_Economic_Shopping, L("Mua sắm"));
            digitalsEconomic.CreateChildPermission(PermissionNames.Pages_Digitals_Economic_Work, L("Việc làm"));
            digitalsEconomic.CreateChildPermission(PermissionNames.Pages_Digitals_Economic_Repair, L("Sửa chữa"));
            digitalsEconomic.CreateChildPermission(PermissionNames.Pages_Digitals_Economic_Entertainment, L("Giải trí"));


            var administrationService = context.CreatePermission(PermissionNames.Pages_AdministrationService, L("Hành chính/Dịch vụ số"));

            var administrationServiceConfigurations =
                administrationService.CreateChildPermission(PermissionNames.Pages_AdministrationService_Configurations, L("Quản lý cấu hình"));

            var administrationServiceVehicles =
                administrationService.CreateChildPermission(PermissionNames.Pages_AdministrationService_Vehicles, L("Quản lý phương tiện"));
            administrationServiceVehicles.CreateChildPermission(PermissionNames
                .Pages_AdministrationService_Vehicles_GetAll, L("Xem danh sách phương tiện"));
            administrationServiceVehicles.CreateChildPermission(PermissionNames
                .Pages_AdministrationService_Vehicles_Create, L("Thêm mới phương tiện"));
            administrationServiceVehicles.CreateChildPermission(PermissionNames
                .Pages_AdministrationService_Vehicles_Edit, L("Chỉnh sửa phương tiện"));
            administrationServiceVehicles.CreateChildPermission(PermissionNames
                .Pages_AdministrationService_Vehicles_Delete, L("Xóa phương tiện"));

            var administrationServiceParkingSpots =
                administrationService.CreateChildPermission(PermissionNames.Pages_AdministrationService_ParkingSpots, L("Quản lý bãi đỗ xe"));


            var invoices = context.CreatePermission(PermissionNames.Pages_Invoices, L("Hóa đơn/Chi phí số"));

            var invoicesMonthly = invoices.CreateChildPermission(PermissionNames.Pages_Invoices_Monthly, L("Hóa đơn tháng"));
            invoicesMonthly.CreateChildPermission(PermissionNames.Pages_Invoices_Monthly_GetAll, L("Xem danh sách hóa đơn tháng"));
            invoicesMonthly.CreateChildPermission(PermissionNames.Pages_Invoices_Monthly_GetDetail, L("Xem chi tiết hóa đơn tháng"));
            invoicesMonthly.CreateChildPermission(PermissionNames.Pages_Invoices_Monthly_Create, L("Thêm mới hóa đơn tháng"));
            invoicesMonthly.CreateChildPermission(PermissionNames.Pages_Invoices_Monthly_Edit, L("Chỉnh sửa hóa đơn tháng"));
            invoicesMonthly.CreateChildPermission(PermissionNames.Pages_Invoices_Monthly_Delete, L("Xóa hóa đơn tháng"));

            var invoicesPayments = invoices.CreateChildPermission(PermissionNames.Pages_Invoices_Payments, L("Quản lý thanh toán"));
            invoicesPayments.CreateChildPermission(PermissionNames.Pages_Invoices_Payments_GetAll, L("Xem danh sách thanh toán"));
            invoicesPayments.CreateChildPermission(PermissionNames.Pages_Invoices_Payments_GetDetail, L("Xem chi tiết thanh toán"));
            invoicesPayments.CreateChildPermission(PermissionNames.Pages_Invoices_Payments_Create, L("Thêm mới thanh toán"));
            invoicesPayments.CreateChildPermission(PermissionNames.Pages_Invoices_Payments_Edit, L("Chỉnh sửa thanh toán"));
            invoicesPayments.CreateChildPermission(PermissionNames.Pages_Invoices_Payments_Delete, L("Xóa thanh toán"));

            var invoicesDebt = invoices.CreateChildPermission(PermissionNames.Pages_Invoices_Debt, L("Quản lý công nợ"));
            invoicesDebt.CreateChildPermission(PermissionNames.Pages_Invoices_Debt_GetAll, L("Xem danh sách công nợ"));
            invoicesDebt.CreateChildPermission(PermissionNames.Pages_Invoices_Debt_GetDetail, L("Xem chi tiết công nợ"));
            invoicesDebt.CreateChildPermission(PermissionNames.Pages_Invoices_Debt_Create, L("Thêm mới công nợ"));
            invoicesDebt.CreateChildPermission(PermissionNames.Pages_Invoices_Debt_Edit, L("Chỉnh sửa công nợ"));
            invoicesDebt.CreateChildPermission(PermissionNames.Pages_Invoices_Debt_Delete, L("Xóa công nợ"));

            var invoicesServiceFees = invoices.CreateChildPermission(PermissionNames.Pages_Invoices_ServiceFees, L("Quản lý bảng giá dịch vụ"));
            invoicesServiceFees.CreateChildPermission(PermissionNames.Pages_Invoices_ServiceFees_GetAll, L("Xem danh sách bảng giá dịch vụ"));
            invoicesServiceFees.CreateChildPermission(PermissionNames.Pages_Invoices_ServiceFees_GetDetail, L("Xem chi tiết bảng giá dịch vụ"));
            invoicesServiceFees.CreateChildPermission(PermissionNames.Pages_Invoices_ServiceFees_Create, L("Thêm mới bảng giá dịch vụ"));
            invoicesServiceFees.CreateChildPermission(PermissionNames.Pages_Invoices_ServiceFees_Edit, L("Chỉnh sửa bảng giá dịch vụ"));
            invoicesServiceFees.CreateChildPermission(PermissionNames.Pages_Invoices_ServiceFees_Delete, L("Xóa bảng giá dịch vụ"));

            var invoicesPaymentAccounts =
                invoices.CreateChildPermission(PermissionNames.Pages_Invoices_PaymentAccounts, L("Tài khoản thanh toán"));
            invoicesPaymentAccounts.CreateChildPermission(PermissionNames.Pages_Invoices_PaymentAccounts_GetAll, L("Xem danh sách tài khoản thanh toán"));
            invoicesPaymentAccounts.CreateChildPermission(PermissionNames.Pages_Invoices_PaymentAccounts_GetDetail, L("Xem chi tiết tài khoản thanh toán"));
            invoicesPaymentAccounts.CreateChildPermission(PermissionNames.Pages_Invoices_PaymentAccounts_Create, L("Thêm mới tài khoản thanh toán"));
            invoicesPaymentAccounts.CreateChildPermission(PermissionNames.Pages_Invoices_PaymentAccounts_Edit, L("Chỉnh sửa tài khoản thanh toán"));
            invoicesPaymentAccounts.CreateChildPermission(PermissionNames.Pages_Invoices_PaymentAccounts_Delete, L("Xóa tài khoản thanh toán"));

            var invoicesStatementVerification =
                invoices.CreateChildPermission(PermissionNames.Pages_Invoices_StatementVerification, L("Xác nhận sao kê"));
            invoicesStatementVerification.CreateChildPermission(PermissionNames
                .Pages_Invoices_StatementVerification_GetAll, L("Xem danh sách xác nhận sao kê"));
            invoicesStatementVerification.CreateChildPermission(PermissionNames
                .Pages_Invoices_StatementVerification_Create, L("Thêm mới xác nhận sao kê"));
            invoicesStatementVerification.CreateChildPermission(PermissionNames
                .Pages_Invoices_StatementVerification_Edit, L("Chỉnh sửa xác nhận sao kê"));
            invoicesStatementVerification.CreateChildPermission(PermissionNames
                .Pages_Invoices_StatementVerification_Delete, L("Xóa xác nhận sao kê"));


            var assets = context.CreatePermission(PermissionNames.Pages_Assets, L("Quản lý tài sản"));

            var assetsAssetCatalog = assets.CreateChildPermission(PermissionNames.Pages_Assets_AssetCatalog, L("Danh mục tài sản"));
            assetsAssetCatalog.CreateChildPermission(PermissionNames.Pages_Assets_AssetCatalog_GetAll, L("Xem danh sách danh mục tài sản"));
            assetsAssetCatalog.CreateChildPermission(PermissionNames.Pages_Assets_AssetCatalog_GetDetail, L("Xem chi tiết danh mục tài sản"));
            assetsAssetCatalog.CreateChildPermission(PermissionNames.Pages_Assets_AssetCatalog_Create, L("Thêm mới danh mục tài sản"));
            assetsAssetCatalog.CreateChildPermission(PermissionNames.Pages_Assets_AssetCatalog_Edit, L("Chỉnh sửa danh mục tài sản"));
            assetsAssetCatalog.CreateChildPermission(PermissionNames.Pages_Assets_AssetCatalog_Delete, L("Xóa danh mục tài sản"));

            var assetsAssetParameters = assets.CreateChildPermission(PermissionNames.Pages_Assets_AssetParameters, L("Thông số tài sản"));
            assetsAssetParameters.CreateChildPermission(PermissionNames.Pages_Assets_AssetParameters_GetAll, L("Xem danh sách thông số tài sản"));
            assetsAssetParameters.CreateChildPermission(PermissionNames.Pages_Assets_AssetParameters_GetDetail, L("Xem chi tiết thông số tài sản"));
            assetsAssetParameters.CreateChildPermission(PermissionNames.Pages_Assets_AssetParameters_Create, L("Thêm mới thông số tài sản"));
            assetsAssetParameters.CreateChildPermission(PermissionNames.Pages_Assets_AssetParameters_Edit, L("Chỉnh sửa thông số tài sản"));
            assetsAssetParameters.CreateChildPermission(PermissionNames.Pages_Assets_AssetParameters_Delete, L("Xóa thông số tài sản"));


            var localAmenities = context.CreatePermission(PermissionNames.Pages_LocalAmenities, L("Quản lý tiện ích nội khu"));

            var localAmenitiesList = localAmenities.CreateChildPermission(PermissionNames
                .Pages_LocalAmenities_List, L("Danh sách tiện ích nội khu"));
            localAmenitiesList.CreateChildPermission(PermissionNames.Pages_LocalAmenities_List_GetAll, L("Xem danh sách tiện ích nội khu"));
            localAmenitiesList.CreateChildPermission(PermissionNames.Pages_LocalAmenities_List_GetDetail, L("Xem chi tiết tiện ích nội khu"));
            localAmenitiesList.CreateChildPermission(PermissionNames.Pages_LocalAmenities_List_Create, L("Thêm mới danh sách tiện ích nội khu"));
            localAmenitiesList.CreateChildPermission(PermissionNames.Pages_LocalAmenities_List_Edit, L("Chỉnh sửa danh sách tiện ích nội khu"));
            localAmenitiesList.CreateChildPermission(PermissionNames.Pages_LocalAmenities_List_Delete, L("Xóa danh sách tiện ích nội khu"));


            var operations = context.CreatePermission(PermissionNames.Pages_Operations, L("Vận hành ban quản lý"));

            var operationOrganizationStructure =
                operations.CreateChildPermission(PermissionNames.Pages_Operations_OrganizationStructure, L("Cơ cấu tổ chức"));
            operationOrganizationStructure.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationStructure_GetAll, L("Xem danh sách cơ cấu tổ chức"));
            operationOrganizationStructure.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationStructure_GetDetail, L("Xem chi tiết cơ cấu tổ chức"));
            operationOrganizationStructure.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationStructure_Create, L("Thêm mới cơ cấu tổ chức"));
            operationOrganizationStructure.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationStructure_Edit, L("Chỉnh sửa cơ cấu tổ chức"));
            operationOrganizationStructure.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationStructure_Delete, L("Xóa cơ cấu tổ chức"));

            var operationOrganizationUnits =
                operations.CreateChildPermission(PermissionNames.Pages_Operations_OrganizationUnits, L("Quản lý cơ cấu dự án"));
            operationOrganizationUnits.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationUnits_GetAll, L("Xem danh sách cơ cấu dự án"));
            operationOrganizationUnits.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationUnits_GetDetail, L("Xem chi tiết cơ cấu dự án"));
            operationOrganizationUnits.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationUnits_Create, L("Thêm mới cơ cấu dự án"));
            operationOrganizationUnits.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationUnits_Edit, L("Chỉnh sửa cơ cấu dự án"));
            operationOrganizationUnits.CreateChildPermission(PermissionNames
                .Pages_Operations_OrganizationUnits_Delete, L("Xóa cơ cấu dự án"));

            var operationPersonnel = operations.CreateChildPermission(PermissionNames.Pages_Operations_Personnel, L("Quản lý nhân sự"));
            operationPersonnel.CreateChildPermission(PermissionNames.Pages_Operations_Personnel_GetAll, L("Xem danh sách nhân sự"));
            operationPersonnel.CreateChildPermission(PermissionNames.Pages_Operations_Personnel_GetDetail, L("Xem chi tiết nhân sự"));
            operationPersonnel.CreateChildPermission(PermissionNames.Pages_Operations_Personnel_Create, L("Thêm mới nhân sự"));
            operationPersonnel.CreateChildPermission(PermissionNames.Pages_Operations_Personnel_Edit, L("Chỉnh sửa nhân sự"));
            operationPersonnel.CreateChildPermission(PermissionNames.Pages_Operations_Personnel_Delete, L("xóa nhân sự"));

            var operationTaskManagement =
                operations.CreateChildPermission(PermissionNames.Pages_Operations_TaskManagement, L("Quản lý công việc"));
            operationTaskManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskManagement_GetAll, L("Xem danh sách công việc"));
            operationTaskManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskManagement_GetDetail, L("Xem chi tiết công việc"));
            operationTaskManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskManagement_Create, L("Thêm mới công việc"));
            operationTaskManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskManagement_Edit, L("Chỉnh sửa công việc"));
            operationTaskManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskManagement_Delete, L("Xóa công việc"));

            var operationTaskTypeManagement =
                operations.CreateChildPermission(PermissionNames.Pages_Operations_TaskTypeManagement, L("Quản lý loại công việc"));
            operationTaskTypeManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskTypeManagement_GetAll, L("Xem danh sách loại công việc"));
            operationTaskTypeManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskTypeManagement_GetDetail, L("Xem chi tiết loại công việc"));
            operationTaskTypeManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskTypeManagement_Create, L("Thêm mới loại công việc"));
            operationTaskTypeManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskTypeManagement_Edit, L("Chỉnh sửa loại công việc"));
            operationTaskTypeManagement.CreateChildPermission(PermissionNames.Pages_Operations_TaskTypeManagement_Delete, L("Xóa loại công việc"));


            var systemAdministration = context.CreatePermission(PermissionNames.Pages_SystemAdministration, L("Quản trị hệ thống"));

            var systemAdministrationAccountManagement =
                systemAdministration.CreateChildPermission(PermissionNames.Pages_SystemAdministration_AccountManagement, L("Quản lý tài khoản"));
            systemAdministrationAccountManagement.CreateChildPermission(PermissionNames
                .Pages_SystemAdministration_AccountManagement_GetAll, L("Xem danh sách tài khoản"));
            systemAdministrationAccountManagement.CreateChildPermission(PermissionNames
                .Pages_SystemAdministration_AccountManagement_GetDetail, L("Xem chi tiết tài khoản"));
            systemAdministrationAccountManagement.CreateChildPermission(PermissionNames
                .Pages_SystemAdministration_AccountManagement_Create, L("Thêm mới tài khoản"));
            systemAdministrationAccountManagement.CreateChildPermission(PermissionNames
                .Pages_SystemAdministration_AccountManagement_Edit, L("Chỉnh sửa tài khoản"));
            systemAdministrationAccountManagement.CreateChildPermission(PermissionNames
                .Pages_SystemAdministration_AccountManagement_Delete, L("Xóa tài khoản"));

            var systemAdministrationRoles =
                systemAdministration.CreateChildPermission(PermissionNames.Pages_SystemAdministration_Roles, L("Quản lý vai trò"));
            systemAdministrationRoles.CreateChildPermission(PermissionNames.Pages_SystemAdministration_Roles_GetAll, L("Xem danh sách vai trò"));
            systemAdministrationRoles.CreateChildPermission(PermissionNames.Pages_SystemAdministration_Roles_GetDetail, L("Xem chi tiết vai trò"));
            systemAdministrationRoles.CreateChildPermission(PermissionNames.Pages_SystemAdministration_Roles_Create, L("Thêm mới vai trò"));
            systemAdministrationRoles.CreateChildPermission(PermissionNames.Pages_SystemAdministration_Roles_Edit, L("Chỉnh sửa vai trò"));
            systemAdministrationRoles.CreateChildPermission(PermissionNames.Pages_SystemAdministration_Roles_Delete, L("Xóa vai trò"));


            var reporting = context.CreatePermission(PermissionNames.Pages_Reporting, L("Báo cáo/Thống kê"));


            var documentManagement = context.CreatePermission(PermissionNames.Pages_DocumentManagement, L("Quản lý tài liệu"));

            var documentManagementList =
                documentManagement.CreateChildPermission(PermissionNames.Pages_DocumentManagement_List, L("Danh sách tài liệu"));
            documentManagementList.CreateChildPermission(PermissionNames.Pages_DocumentManagement_List_GetAll, L("Xem danh sách tài liệu"));
            documentManagementList.CreateChildPermission(PermissionNames.Pages_DocumentManagement_List_GetDetail, L("Xem chi tiết tài liệu"));
            documentManagementList.CreateChildPermission(PermissionNames.Pages_DocumentManagement_List_Create, L("Thêm mới tài liệu"));
            documentManagementList.CreateChildPermission(PermissionNames.Pages_DocumentManagement_List_Edit, L("Chỉnh sửa tài liệu"));
            documentManagementList.CreateChildPermission(PermissionNames.Pages_DocumentManagement_List_Delete, L("Xóa tài liệu"));

            var documentManagementDocumentType =
                documentManagement.CreateChildPermission(PermissionNames.Pages_DocumentManagement_Type, L("Quản lý loại tài liệu"));
            documentManagementDocumentType.CreateChildPermission(PermissionNames.Pages_DocumentManagement_Type_GetAll, L("Xem danh sách loại tài liệu"));
            documentManagementDocumentType.CreateChildPermission(PermissionNames.Pages_DocumentManagement_Type_GetDetail, L("Xem chi tiết loại tài liệu"));
            documentManagementDocumentType.CreateChildPermission(PermissionNames.Pages_DocumentManagement_Type_Create, L("Thêm mới loại tài liệu"));
            documentManagementDocumentType.CreateChildPermission(PermissionNames.Pages_DocumentManagement_Type_Edit, L("Chỉnh sửa loại tài liệu"));
            documentManagementDocumentType.CreateChildPermission(PermissionNames.Pages_DocumentManagement_Type_Delete, L("Xóa loại tài liệu"));


            var residentmanager = context.CreatePermission(PermissionNames.Pages_Residents, L("Quản lý công dân"));
            residentmanager.CreateChildPermission(PermissionNames.Pages_Residents_Verification,
                L("Xác minh công dân"));
            residentmanager.CreateChildPermission(PermissionNames.Pages_Residents_Information,
                L("Thông tin công dân"));

            var government = context.CreatePermission(PermissionNames.Pages_Government, L("Chính phủ số"));

            government.CreateChildPermission(PermissionNames.Pages_Government_ChatCitizen, L("Chat công dân"));

            var governmentReflect = government.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects,
                L("Phản ánh công dân"));

            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_View, L("Xem phản ánh công dân"));
            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_Create,
                L("Tạo phản ánh công dân"));
            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_Update,
                L("Cập nhật phản ánh công dân"));
            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_Delete,
                L("Xóa phản ánh công dân"));
            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_Chat,
                L("Chat về phản ánh công dân"));
            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_Handle,
                L("Xử lý phản ánh công dân"));
            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_AssignDepartment,
                L("Chỉ định phòng ban xử lý phản ánh công dân"));
            governmentReflect.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Reflects_AssignHandler,
                L("'Chỉ định người xử lý phản ánh công dân"));


            government.CreateChildPermission(PermissionNames.Pages_Government_Citizens_Vote,
                L("Bình chọn công dân"));
            government.CreateChildPermission(PermissionNames.Pages_Government_Digital_Notices, L("Thông báo số"));
            government.CreateChildPermission(PermissionNames.Pages_Government_DigitalDaily, L("Nhật ký số"));
            government.CreateChildPermission(PermissionNames.Pages_Government_Question_Answer, L("Hỏi đáp"));


            var admin = context.CreatePermission(PermissionNames.Pages_Administration, L("Quản trị hệ thống"));
            admin.CreateChildPermission(PermissionNames.Pages_Users, L("Người dùng"));
            admin.CreateChildPermission(PermissionNames.Pages_Roles, L("Vai trò"));
            admin.CreateChildPermission(PermissionNames.Pages_Tenants, L("Tổ chức"),
                multiTenancySides: MultiTenancySides.Host);
            admin.CreateChildPermission(PermissionNames.Pages_Admin_Type_Local_Service, L("Loại dịch vụ địa phương"));
            admin.CreateChildPermission(PermissionNames.Pages_Admin_Organization_Units, L("Đơn vị tổ chức"));
            admin.CreateChildPermission(PermissionNames.Pages_Admin_Type_Administrative, L("Loại hành chính"));
            admin.CreateChildPermission(PermissionNames.Pages_Admin_Tenant_Settings, L("Thiết lập tổ chức"));
            admin.CreateChildPermission(PermissionNames.Pages_Admin_Organization_Structure, L("Cơ cấu tổ chức"));


            //setting
            var setting = context.CreatePermission(PermissionNames.Pages_Settings, L("Cài đặt hệ thống"));
            setting.CreateChildPermission(PermissionNames.Pages_Settings_Images, L("Cài đặt hình ảnh"),
                multiTenancySides: MultiTenancySides.Host);

            //thiet bi thong minh
            var smartDevice = context.CreatePermission(PermissionNames.Pages_Smart_Device, L("Thiết bị thông minh"));
            var parking = context.CreatePermission(PermissionNames.Pages_Parking_Management, L("Quản lý bãi đỗ xe"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, IMAXConsts.LocalizationSourceName);
        }
    }
}