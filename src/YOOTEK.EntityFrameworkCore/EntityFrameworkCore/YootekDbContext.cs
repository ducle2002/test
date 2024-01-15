using Abp.Zero.EntityFrameworkCore;
using Yootek.Authorization.Permissions;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Chat;
using Yootek.Chat.BusinessChat;
using Yootek.EntityDb;
using Yootek.Friendships;
using Yootek.Yootek.EntityDb.Clb.Hotlines;
using Yootek.Yootek.EntityDb.Clb.QnA;
using Yootek.Yootek.EntityDb.Clb.Votes;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.EntityDb.Yootek.DichVu.BusinessReg;
using Yootek.Yootek.EntityDb.Yootek.DichVu.CheckingObj;
using Yootek.Yootek.EntityDb.Yootek.Metrics;
using Yootek.Yootek.EntityDb.Yootek.MobileAppFeedback;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using Yootek.Yootek.EntityDb.SmartCommunity.Phidichvu;
using Yootek.Yootek.EntityDb.SmartCommunity.QuanLyDanCu.Citizen;
using Yootek.Yootek.EntityDb.Smarthome.Device;
using Yootek.MultiTenancy;
using Yootek.Organizations;
using Yootek.Organizations.OrganizationStructure;
using Yootek.GroupChats;
using Yootek.Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Clb.City_Notification;
using Yootek.Yootek.EntityDb.Clb.Enterprise;
using Yootek.Yootek.EntityDb.Clb.Event;
using Yootek.Yootek.EntityDb.Clb.Jobs;
using Yootek.Yootek.EntityDb.Clb.Projects;
using Yootek.SmartCommunity;
using Yootek.Storage;
using Microsoft.EntityFrameworkCore;
using ClbCityNotificationComment = Yootek.Yootek.EntityDb.Clb.City_Notification.ClbCityNotificationComment;
using Yootek.Yootek.EntityDb.Yootek.DichVu.Business;

namespace Yootek.EntityFrameworkCore
{
    public class YootekDbContext : AbpZeroDbContext<Tenant, Role, User, YootekDbContext>
    {
        #region DbSet
        public virtual DbSet<SchedulerNotification> SchedulerNotifications { get; set; }

        #region Forum

        public virtual DbSet<QuestionAnswer> QuestionAnswers { get; set; }
        public virtual DbSet<QAComment> QAComments { get; set; }
        public virtual DbSet<QATopic> QATopics { get; set; }

        public virtual DbSet<ForumPost> Forums { get; set; }
        public virtual DbSet<ForumComment> ForumComments { get; set; }
        public virtual DbSet<ForumTopic> ForumTopics { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<ForumTag> ForumTags { get; set; }

        #endregion

        #region User Config

        public virtual DbSet<Reminder> Reminders { get; set; }
        public virtual DbSet<PermissionTenant> PermissionsTenants { get; set; }

        #endregion

        #region Report store

        public virtual DbSet<ReportStore> ReportStore { get; set; }

        #endregion

        #region Chathub

        public virtual DbSet<Friendship> Friendships { get; set; }
        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<GroupChat> GroupChats { get; set; }
        public virtual DbSet<UserGroupChat> UserGroupChats { get; set; }
        public virtual DbSet<GroupMessage> GroupMessages { get; set; }
        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }
        public virtual DbSet<FcmTokens> FcmTokens { get; set; }
        public virtual DbSet<FcmGroups> FcmGroups { get; set; }

        #endregion

        #region Community

        // public virtual DbSet<ProblemSystem> ProblemSystems { get; set; }
        public virtual DbSet<CityNotification> CityNotifications { get; set; }
        public virtual DbSet<CitizenReflect> CitizenReflects { get; set; }
        public virtual DbSet<CitizenReflectComment> CitizenReflectComments { get; set; }
        public virtual DbSet<CitizenReflectLike> CitizenReflectLikes { get; set; }
        public virtual DbSet<Citizen> Citizens { get; set; }
        public virtual DbSet<CityNotificationComment> CityNotificationComments { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<PostComment> PostComments { get; set; }
        public virtual DbSet<ReportComment> ReportComments { get; set; }
        public virtual DbSet<LikePost> LikePosts { get; set; }
        public virtual DbSet<UserCityNotification> UserCityNotifications { get; set; }

        public virtual DbSet<UserVote> UserVotes { get; set; }
        public virtual DbSet<CityVote> CityVotes { get; set; }

        //fee
        public virtual DbSet<UserBill> UserBills { get; set; }
        public virtual DbSet<UserBillVehicleInfo> UserBillVehicleInfos { get; set; }
        public virtual DbSet<BillConfig> BillConfigs { get; set; }
        public virtual DbSet<UserBillPayment> UserBillPayments { get; set; }
        public virtual DbSet<UserBillPaymentHistory> UserBillPaymentHistories { get; set; }
        public virtual DbSet<BillStatistic> BillStatistics { get; set; }
        public virtual DbSet<BillDebt> BillDebts { get; set; }
        public virtual DbSet<BillEmailHistory> BillEmailHistories { get; set; }
        public virtual DbSet<BillPrepayment> BillPrepayment { get; set; }
        public virtual DbSet<TemplateBill> TemplateBill { get; set; }

        public virtual DbSet<District> District { get; set; }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<Ward> Ward { get; set; }
        public virtual DbSet<CitizenTemp> CitizenTemps { get; set; }
        public virtual DbSet<CitizenContract> CitizenContracts { get; set; }

        #endregion

        #region Staff Management

        public virtual DbSet<Position> Position { get; set; }

        public virtual DbSet<Staff> Staff { get; set; }

        #endregion

        #region News

        public virtual DbSet<News> News { get; set; }

        #endregion

        #region Images/ImageConfigs

        public virtual DbSet<Images> Images { get; set; }
        public virtual DbSet<ImageConfig> ImageConfigs { get; set; }

        #endregion

        #region Smarthome

        public virtual DbSet<HomeMember> HomeMembers { get; set; }
        public virtual DbSet<SmartHome> SmartHomes { get; set; }
        public virtual DbSet<SampleHouse> SampleHouses { get; set; }
        public virtual DbSet<HomeDevice> HomeDevices { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<HomeGateway> HomeGateways { get; set; }

        #region Business

        public virtual DbSet<BusinessChatMessage> BusinessChatMessages { get; set; }
        public virtual DbSet<UserProviderFriendship> UserProviderFriendships { get; set; }
        public virtual DbSet<Rate> Rates { get; set; }
        public virtual DbSet<ObjectPartner> ObjectPartners { get; set; }
        public virtual DbSet<ObjectType> ObjectTypes { get; set; }
        public virtual DbSet<BusinessNotify> BusinessNotifies { get; set; }
        public virtual DbSet<Items> Items { get; set; }
        public virtual DbSet<SetItems> SetItems { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<ItemType> ItemTypes { get; set; }
        public virtual DbSet<ItemViewSetting> ItemViewSettings { get; set; }
        public virtual DbSet<CheckingObject> CheckingObject { get; set; }
        public virtual DbSet<AppFeedback> GetMobileAppFeedback { get; set; }
        public virtual DbSet<Handbook> Handbooks { get; set; }
        public virtual DbSet<ItemBooking> ItemBookings { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<BookingHistory> BookingHistories { get; set; }

        #endregion

        #endregion

        public virtual DbSet<AppOrganizationUnit> AppOrganizationUnits { get; set; }
        public virtual DbSet<CitizenVerification> CitizenVerifications { get; set; }
        public virtual DbSet<TypeAdministrative> TypeAdministratives { get; set; }
        public virtual DbSet<Administrative> Administratives { get; set; }
        public virtual DbSet<AdministrativeValue> AdministrativeValues { get; set; }
        public virtual DbSet<AdministrativeProperty> AdministrativeProperties { get; set; }
        public virtual DbSet<Hotlines> Hotlines { get; set; }
        public virtual DbSet<FeedbackApp> FeedbackApps { get; set; }
        public virtual DbSet<Introduce> Introduces { get; set; }
        public virtual DbSet<LocalService> LocalServices { get; set; }
        public virtual DbSet<NewsWebImax> NewsWebImaxs { get; set; }
        public virtual DbSet<DetailPage> DetailPages { get; set; }
        public virtual DbSet<ComponentPage> ComponentPages { get; set; }
        public virtual DbSet<CitizenCard> CitizenCards { get; set; }
        public virtual DbSet<BusinessUnit> BusinessUnits { get; set; }
        public virtual DbSet<Parking> Parkings { get; set; }
        public virtual DbSet<IntegratedParkingCustomer> IntegratedParkingCustomers { get; set; }
        public virtual DbSet<UserParking> UserParkings { get; set; }
        public virtual DbSet<CitizenVehicle> CitizenVehicles { get; set; }
        public virtual DbSet<CitizenParking> CitizenParkings { get; set; }
        public virtual DbSet<UserVehicle> UserVehicles { get; set; }
        public virtual DbSet<Apartment> Apartments { get; set; }
        public virtual DbSet<ApartmentHistory> ApartmentHistories { get; set; }
        public virtual DbSet<ApartmentStatus> ApartmentStatuses { get; set; }
        public virtual DbSet<ApartmentDiscount> ApartmentDiscounts { get; set; }
        public virtual DbSet<ApartmentType> ApartmentTypes { get; set; }
        public virtual DbSet<BlockTower> BlockTowers { get; set; }
        public virtual DbSet<Floor> Floors { get; set; }
        public virtual DbSet<DocumentTypes> DocumentTypes { get; set; }
        public virtual DbSet<Documents> Documents { get; set; }
        public virtual DbSet<OrganizationStructureUnit> OrganizationStructureUnits { get; set; }
        public virtual DbSet<OrganizationStructureDept> OrganizationStructureDepts { get; set; }
        public virtual DbSet<OrganizationStructureDeptUser> OrganizationStructureDeptUsers { get; set; }
        public virtual DbSet<DeptToUnit> DeptToUnits { get; set; }
        public virtual DbSet<UnitToUnit> UnitToUnits { get; set; }
        public virtual DbSet<DepartmentOrganizationUnit> DepartmentOrganizationUnits { get; set; }
        public virtual DbSet<Guest> Guests { get; set; }
        public virtual DbSet<GuestForm> GuestForms { get; set; }

        public virtual DbSet<Meter> Meters { get; set; }
        public virtual DbSet<MeterMonthly> MeterMonthlies { get; set; }
        public virtual DbSet<MeterType> MeterTypes { get; set; }

        #endregion

        #region Clb

        public virtual DbSet<ClbHotlines> ClbHotlines { get; set; }

        // public virtual DbSet<ClbForum> ClbForums { get; set; }
        // public virtual DbSet<ClbForumComment> ClbForumComments { get; set; }
        // public virtual DbSet<ClbForumTopic> ClbForumTopics { get; set; }
        public virtual DbSet<ClbCityVote> ClbCityVotes { get; set; }
        public virtual DbSet<ClbUserVote> ClbUserVotes { get; set; }
        public virtual DbSet<ClbCityNotification> ClbCityNotifications { get; set; }
        public virtual DbSet<ClbCityNotificationComment> ClbCityNotificationComments { get; set; }
        public virtual DbSet<ClbUserCityNotification> ClbUserCityNotifications { get; set; }
        public virtual DbSet<ClbEvent> ClbEvents { get; set; }
        public virtual DbSet<ClbEventComment> ClbEventComments { get; set; }
        public virtual DbSet<ClbUserEvent> ClbUserEvents { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<Enterprises> Enterprises { get; set; }
        public virtual DbSet<UserEnterprises> UserEnterprises { get; set; }
        public virtual DbSet<BusinessField> BusinessFields { get; set; }
        public virtual DbSet<Projects> Projects { get; set; }
        public virtual DbSet<Jobs> Jobs { get; set; }
        public virtual DbSet<ForumPostReaction> ForumPostReactions { get; set; }

        #endregion

        #region qlvl

        public virtual DbSet<Material> Materials { get; set; }
        public virtual DbSet<MaintenancePlan> MaintenancePlans { get; set; }
        public virtual DbSet<MaterialCategory> MaterialCategories { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<InventoryImportExport> InventoryImportExports { get; set; }
        public virtual DbSet<MaterialDelivery> MaterialDeliveries { get; set; }

        #endregion

        #region Metrics

        public virtual DbSet<HomeMeter> HomeMeters { get; set; }

        #endregion

        #region smart social

        public virtual DbSet<Provider> Providers { get; set; }

        #endregion

        public virtual DbSet<NhomTaiSan> NhomTaiSan { get; set; }
        public virtual DbSet<LoaiTaiSan> LoaiTaiSan { get; set; }
        public virtual DbSet<DonViTaiSan> DonViTaiSan { get; set; }
        public virtual DbSet<KhoTaiSan> KhoTaiSan { get; set; }
        public virtual DbSet<NhaSanXuat> NhaSanXuat { get; set; }
        public virtual DbSet<TaiSan> TaiSan { get; set; }
        public virtual DbSet<PhieuNhapKho> PhieuNhapKho { get; set; }
        public virtual DbSet<PhieuNhapKhoToTaiSan> PhieuNhapKhoToTaiSan { get; set; }
        public virtual DbSet<PhieuGiaoTaiSan> PhieuGiaoTaiSan { get; set; }
        public virtual DbSet<PhieuGiaoToTaiSan> PhieuGiaoToTaiSan { get; set; }
        public virtual DbSet<PhieuNhanTaiSan> PhieuNhanTaiSan { get; set; }
        public virtual DbSet<PhieuNhanToTaiSan> PhieuNhanToTaiSan { get; set; }
        public virtual DbSet<PhieuXuatKho> PhieuXuatKho { get; set; }
        public virtual DbSet<PhieuXuatKhoToTaiSan> PhieuXuatKhoToTaiSan { get; set; }
        public virtual DbSet<PhieuKiemKho> PhieuKiemKho { get; set; }
        public virtual DbSet<PhieuKiemKhoToTaiSan> PhieuKiemKhoToTaiSan { get; set; }
        public virtual DbSet<MaHeThong> MaHeThong { get; set; }
        public virtual DbSet<TaiSanChiTiet> TaiSanChiTiet { get; set; }
        public virtual DbSet<NhatKyVanHanh> NhatKyVanHanh { get; set; }
        public virtual DbSet<CareerCategory> CareerCategory { get; set; }

        #region Digital Services

        public virtual DbSet<DigitalServiceOrder> DigitalServiceOrder { get; set; }
        public virtual DbSet<DigitalServiceCategory> DigitalServiceCategory { get; set; }
        public virtual DbSet<DigitalServices> DigitalServices { get; set; }
        public virtual DbSet<DigitalServiceDetails> DigitalServiceDetails { get; set; }

        #endregion

        public YootekDbContext(DbContextOptions<YootekDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // base.OnModelCreating(builder);
            /*var mutableProperties = builder.Model.GetEntityTypes()
                .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string)));*/

            base.OnModelCreating(builder);
            builder.HasDefaultSchema("public");
            /* Configure your own tables/entities inside the ConfigureMPQ method */
            //builder.Entity<RoomUserChat>()
            //    .HasKey(t => new { t.UserId, t.GroupChatId });


            //builder.Entity<RoomUserChat>()
            //    .HasOne(pt => pt.User)
            //    .WithMany(p => p.RoomUserChats)
            //    .HasForeignKey(pt => pt.UserId);

            //builder.Entity<RoomUserChat>()
            //    .HasOne(pt => pt.GroupChat)
            //    .WithMany(t => t.RoomUserChats)
            //    .HasForeignKey(pt => pt.GroupChatId);


            //builder.ConfigureMPQ();
        }
    }
}