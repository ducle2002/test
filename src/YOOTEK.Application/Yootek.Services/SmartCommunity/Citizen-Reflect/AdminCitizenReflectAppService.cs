using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.UI;
using Amazon.Runtime.Internal.Transform;
using DocumentFormat.OpenXml.Bibliography;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Application;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.UserFeedbackEnum;
using Newtonsoft.Json;

namespace Yootek.Services
{
    public interface IAdminCitizenReflectAppService : IApplicationService
    {
        Task<object> GetAllCitizenReflect(GetCitizenReflectInput input);
        Task<DataResult> GetCitizenReflectById(long id);
        Task<object> UpdateStateCitizenReflect(UpdateStateReflectInput input);
        Task<DataResult> CreateReflect(CreateCitizenReflectInput input);
        Task<DataResult> DeleteCitizenReflect(long id);
        Task<object> GetStatisticFeedback(GetStatisticFeedbackInput input);
        Task SetCommentCitizenReflectAsRead(long reflectId);
        Task<ListResultDto<CitizenReflectCommentDto>> GetAllCommnetByCitizenReflect(GetCommentCitizenReflectInput input);
        Task<DataResult> GetChatMessageByCitizenReflect(GetCommentCitizenReflectInput input);
        Task<object> CreateOrUpdateCitizenReflectComment(CitizenReflectCommentDto input);
        Task<object> DeleteCitizenReflectComment(long id);
        Task<object> GetStatisticsCitizenReflectComment(StatisticsUserCitizenReflectCommentInput input);
    }

    //[AbpAuthorize(PermissionNames.Pages_Digitals_Reflects,
    //    PermissionNames.Pages_Government, PermissionNames.Pages_Government_Citizens_Reflects)]
    [AbpAuthorize]
    public class AdminCitizenReflectAppService : YootekAppServiceBase, IAdminCitizenReflectAppService
    {
        private readonly IRepository<CitizenReflect, long> _citizenReflectRepos;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly INotificationCommunicator _notificationCommunicator;
        private readonly IRepository<CitizenReflectComment, long> _citizenReflectCommentRepos;
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        // private readonly IFeedbackListExcelExporter _feedbackListExcelExporter;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private static IRepository<User, long> _userRepos;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<Apartment, long> _smartHomeRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly AppOrganizationUnitManager _organizationUnitManager;


        public AdminCitizenReflectAppService(
            IRepository<CitizenReflect, long> citizenReflectRepos,
            IHttpWorkAssignmentService httpWorkAssignmentService,
            IOnlineClientManager onlineClientManager,
            IRepository<CitizenReflectComment, long> citizenReflectCommentRepos,
            INotificationCommunicator notificationCommunicator,
            // IFeedbackListExcelExporter feedbackListExcelExporter,
            IRepository<UserOrganizationUnit, long> userOrganizationRepos,
            IRepository<User, long> userRepo,
            IAppNotifier appNotifier,
            IRepository<Citizen, long> citizenRepos,
            AppOrganizationUnitManager organizationUnitManager,
            IRepository<AppOrganizationUnit, long> organizationRepos,
            IRepository<Apartment, long> smartHomeRepos
        )
        {
            _citizenReflectRepos = citizenReflectRepos;
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _citizenReflectCommentRepos = citizenReflectCommentRepos;
            _onlineClientManager = onlineClientManager;
            _notificationCommunicator = notificationCommunicator;
            _organizationUnitManager = organizationUnitManager;
            //  _feedbackListExcelExporter = feedbackListExcelExporter;
            _userOrganizationRepos = userOrganizationRepos;
            _userRepos = userRepo;
            _appNotifier = appNotifier;
            _citizenRepos = citizenRepos;
            _organizationRepos = organizationRepos;
            _smartHomeRepos = smartHomeRepos;
        }

        protected IQueryable<CitizenReflectDto> QueryDataCitizenReflect(GetCitizenReflectInput input)
        {
            DateTime? fromDay = input.FromDay?.Date;
            DateTime? toDay = input.ToDay?.Date + TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59) + TimeSpan.FromSeconds(59);
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            IQueryable<CitizenReflectDto> query = (from fb in _citizenReflectRepos.GetAll()
                                                   join us in _userRepos.GetAll() on fb.CreatorUserId equals us.Id into tb_us
                                                   from us in tb_us.DefaultIfEmpty()
                                                   join og in _organizationRepos.GetAll() on fb.OrganizationUnitId equals og.Id into tb_og
                                                   from og in tb_og.DefaultIfEmpty()
                                                   select new CitizenReflectDto()
                                                   {
                                                       Id = fb.Id,
                                                       CreationTime = fb.CreationTime,
                                                       CreatorUserId = fb.CreatorUserId,
                                                       Data = fb.Data,
                                                       FileUrl = fb.FileUrl,
                                                       Name = fb.Name,
                                                       State = fb.State,
                                                       LastModificationTime = fb.LastModificationTime,
                                                       Type = fb.Type,
                                                       OrganizationUnitId = fb.OrganizationUnitId,
                                                       OrganizationUnitName = og.DisplayName,
                                                       FinishTime = fb.FinishTime,
                                                       TenantId = fb.TenantId,
                                                       UrbanId = fb.UrbanId, // Thêm trường UrbanId
                                                       UserName = us != null ? us.UserName : null,
                                                       NameFeeder = fb.NameFeeder,
                                                       FullName = fb.NameFeeder != null ? fb.NameFeeder : us.FullName,
                                                       ImageUrl = us != null ? us.ImageUrl : null,
                                                       CountUnreadComment = (from cm in _citizenReflectCommentRepos.GetAll()
                                                                             where fb.Id == cm.FeedbackId && cm.ReadState == 1 && cm.CreatorUserId != AbpSession.UserId
                                                                             select cm).Count(),
                                                       CountAllComment = (from cm in _citizenReflectCommentRepos.GetAll()
                                                                          where fb.Id == cm.FeedbackId
                                                                          select cm).Count(),
                                                       Rating = fb.Rating,
                                                       RatingContent = fb.RatingContent,
                                                       ApartmentCode = (from ctz in _citizenRepos.GetAll()
                                                                        where fb.CreatorUserId == ctz.AccountId
                                                                        select ctz.ApartmentCode).FirstOrDefault(),
                                                       BuildingId = fb.BuildingId,
                                                       BuildingName = _organizationRepos.GetAll().Where(o => o.Id == fb.BuildingId)
                                                             .Select(b => b.DisplayName).FirstOrDefault(),
                                                       UrbanName = _organizationRepos.GetAll().Where(o => o.Id == fb.UrbanId)
                                                             .Select(b => b.DisplayName).FirstOrDefault(),
                                                       ListHandleUserIds = fb.ListHandleUserIds,
                                                       //HandlersName = await GetUsersOrganizationNameAsync(fb.ListHandleUserIds),
                                                   })
                .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                .WhereIf(input.OrganizationUnitId.HasValue, x => (x.Type.HasValue && x.Type == input.OrganizationUnitId)
                                                                 || (x.OrganizationUnitId.HasValue &&
                                                                     x.OrganizationUnitId == input.OrganizationUnitId))
                .WhereIf(input.ApartmentCode != null, x => x.ApartmentCode == input.ApartmentCode)
                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                .WhereIf(input.FromDay.HasValue,
                    x => (x.Type.HasValue && x.OrganizationUnitId.HasValue && x.CreationTime.Date >= fromDay))
                .WhereIf(input.ToDay.HasValue,
                    x => x.Type.HasValue && x.OrganizationUnitId.HasValue && x.CreationTime <= toDay)
                .ApplySearchFilter(input.Keyword, x => x.Name, x => x.ApartmentCode, x => x.Data, x => x.NameFeeder)
                .AsQueryable();
            return query;
        }

        public async Task<object> GetAllCitizenReflect(GetCitizenReflectInput input)
        {
            try
            {
                IQueryable<CitizenReflectDto> query = QueryDataCitizenReflect(input);

                #region Truy van tung Form
                switch (input.FormId)
                {
                    //phản ánh mới
                    case (int)FORM_ID_FEEDBACK.FORM_ADMIN_GET_FEEDBACK_PENDING:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.PENDING)
                            .OrderByDescending(u =>
                                u.LastModificationTime.HasValue ? u.LastModificationTime : u.CreationTime);
                        break;
                    // phản ánh đang xử lý: đã tiếp nhận + bị từ chối
                    case (int)FORM_ID_FEEDBACK.FORM_ADMIN_GET_FEEDBACK_HANDLING:
                        query = query
                            .Where(x => x.State == (int)STATE_FEEDBACK.HANDLING ||
                                        x.State == (int)STATE_FEEDBACK.DECLINED)
                            .OrderByDescending(u =>
                                u.LastModificationTime.HasValue ? u.LastModificationTime : u.CreationTime);
                        break;

                    //phản ánh bị từ chối
                    //case (int)FORM_ID_FEEDBACK.FORM_ADMIN_GET_FEEDBACK_DECLINED:
                    //    query = query.Where(x => x.State == (int)STATE_FEEDBACK.DECLINED).OrderByDescending(x => x.Id);
                    //    break;
                    //phản ánh đã hoàn thành, đợi người dùng chấp nhận hoặc đánh giá
                    case (int)FORM_ID_FEEDBACK.FORM_ADMIN_GET_FEEDBACK_ADMIN_CONFIRMED:
                        query = query
                            .Where(x => x.State == (int)STATE_FEEDBACK.USER_CONFIRMED ||
                                        x.State == (int)STATE_FEEDBACK.ADMIN_CONFIRMED)
                            .OrderByDescending(u =>
                                u.LastModificationTime.HasValue ? u.LastModificationTime : u.CreationTime);
                        break;
                    case (int)FORM_ID_FEEDBACK.FORM_ADMIN_GET_FEEDBACK_USER_RATING:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.USER_RATE_FEEDBACK)
                            .OrderByDescending(u =>
                                u.LastModificationTime.HasValue ? u.LastModificationTime : u.CreationTime);
                        break;
                    case (int)FORM_ID_FEEDBACK.FORM_ADMIN_GET_FEEDBACK_GETALL:
                        query = query.OrderByDescending(u =>
                            u.LastModificationTime.HasValue ? u.LastModificationTime : u.CreationTime);
                        break;
                    default:
                        query = null;
                        break;
                }
                #endregion

                if (query != null)
                {
                    var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                    foreach(var item in result)
                    {
                        item.HandlersName = await GetUsersOrganizationNameAsync(item.ListHandleUserIds);
                    }
                    return DataResult.ResultSuccess(result, "Get success!", query.Count());
                }
                else
                {
                    var result = new List<CitizenReflectDto>();
                    return DataResult.ResultSuccess(result, "Get success!", 0);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetCitizenReflectById(long id)
        {
            try
            {
                CitizenReflect citizenReflect = await _citizenReflectRepos.GetAsync(id);
                CitizenReflectDto citizenReflectDto = ObjectMapper.Map<CitizenReflectDto>(citizenReflect);
                citizenReflectDto.BuildingName = GetOrganizationName(citizenReflect.BuildingId);
                citizenReflectDto.UrbanName = GetOrganizationName(citizenReflect.UrbanId);
                citizenReflectDto.HandlersName = await GetUsersOrganizationNameAsync(citizenReflect.ListHandleUserIds);
                MicroserviceResultDto<List<WorkDto>> result = await _httpWorkAssignmentService.GetAllWorkByRelatedId(new GetListWorkByRelatedIdDto()
                {
                    RelatedId = citizenReflect.Id,
                    RelationshipType = WorkAssociationType.REFLECT,
                });
                if (result != null)
                {
                    citizenReflectDto.Works = result.Result;
                }
                return DataResult.ResultSuccess(citizenReflectDto, "Success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetReflectCountStatistic()
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var count = await _citizenReflectRepos.GetAll()
                    .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                    .Where(x => x.Type.HasValue && x.OrganizationUnitId.HasValue).CountAsync();
                return DataResult.ResultSuccess(count, "Get success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> CreateReflect(CreateCitizenReflectInput input)
        {
            try
            {
                CitizenReflect citizenReflect = ObjectMapper.Map<CitizenReflect>(input);
                citizenReflect.TenantId = AbpSession.TenantId;
                citizenReflect.State = (int)STATE_FEEDBACK.PENDING;
                citizenReflect.NameFeeder = input.FullName;
                long id = await _citizenReflectRepos.InsertAndGetIdAsync(citizenReflect);
                citizenReflect.Id = id;
                citizenReflect.Name = $"PA0{id}";
                await CurrentUnitOfWork.SaveChangesAsync();
                return DataResult.ResultSuccess(citizenReflect, "Insert success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public async Task<DataResult> UpdateReflect(UpdateCitizenReflectInput input)
        {
            try
            {
                CitizenReflect? citizenOrg = await _citizenReflectRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("Reflect not found!");

                ObjectMapper.Map(input, citizenOrg);

                await _citizenReflectRepos.UpdateAsync(citizenOrg);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteCitizenReflect(long id)
        {
            try
            {
                await _citizenReflectRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<ListResultDto<CitizenReflectCommentDto>> GetAllCommnetByCitizenReflect(
            GetCommentCitizenReflectInput input)
        {
            try
            {
                var result = await _citizenReflectCommentRepos.GetAll()
                    .Where(x => x.FeedbackId == input.CitizenReflectId)
                    .OrderByDescending(m => m.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToListAsync();

                return new ListResultDto<CitizenReflectCommentDto>(result.MapTo<List<CitizenReflectCommentDto>>());
            }
            catch (Exception e)
            {
                // var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return null;
            }
        }

        public async Task<DataResult> GetChatMessageByCitizenReflect(GetCommentCitizenReflectInput input)
        {
            try
            {

                var query = _citizenReflectCommentRepos.GetAll()
                   .Where(x => x.FeedbackId == input.CitizenReflectId)
                   .OrderByDescending(m => m.CreationTime).AsQueryable();
                var result = await query
                   .Skip(input.SkipCount)
                   .Take(input.MaxResultCount)
                   .ToListAsync();

                return DataResult.ResultSuccess(result, "Get success !", query.Count());
            }
            catch (Exception e)
            {
                // var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> CreateOrUpdateCitizenReflectComment(CitizenReflectCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _citizenReflectCommentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _citizenReflectCommentRepos.UpdateAsync(updateData);
                    }

                    mb.statisticMetris(t1, 0, "Ud_fbcomment");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CitizenReflectComment>();
                    insertInput.ReadState = 1;
                    long id = await _citizenReflectCommentRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    var citizen = new UserIdentifier(AbpSession.TenantId, input.CreatorFeedbackId);

                    var clients = await _onlineClientManager.GetAllByUserIdAsync(citizen);

                    if (clients.Any())
                    {
                        _notificationCommunicator.SendCommentFeedbackToUserTenant(clients, insertInput);
                    }

                    mb.statisticMetris(t1, 0, "is_noti");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteCitizenReflectComment(long id)
        {
            try
            {
                await _citizenReflectCommentRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> SetTimeProcessCitizenReflect(SetTimeProcessInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var updateData = await _citizenReflectRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    updateData.FinishTime = input.FinishTime;
                    await _citizenReflectRepos.UpdateAsync(updateData);
                    var feedbackComment = new CitizenReflectComment();
                    feedbackComment.FeedbackId = updateData.Id;
                    feedbackComment.TenantId = AbpSession.TenantId;
                    feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_SETTIME;
                    // await NotifierStateCitizenReflect(updateData);
                    feedbackComment.ReadState = 1;
                    await _citizenReflectCommentRepos.InsertAndGetIdAsync(feedbackComment);
                    var user = new UserIdentifier(updateData.TenantId, updateData.CreatorUserId.Value);
                    var clients = await _onlineClientManager.GetAllByUserIdAsync(user);

                    if (clients.Any())
                    {
                        _notificationCommunicator.AdminUpdateStateFeedback(clients, updateData);
                    }
                }
                mb.statisticMetris(t1, 0, "Ud_feedback");
                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;


            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> UpdateStateCitizenReflect(UpdateStateReflectInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var updateData = await _citizenReflectRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    updateData.State = (int?)input.State;

                    // Kiểm tra xem người dùng đã nhập giá trị FinishTime hay chưa
                    if (input.FinishTime.HasValue)
                    {
                        updateData.FinishTime = input.FinishTime;
                    }

                    //call back
                    await _citizenReflectRepos.UpdateAsync(updateData);
                    var feedbackComment = new CitizenReflectComment();
                    feedbackComment.FeedbackId = updateData.Id;
                    feedbackComment.TenantId = AbpSession.TenantId;
                    switch (input.State)
                    {
                        case STATE_FEEDBACK.DECLINED:
                            feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_DECLINED;
                            feedbackComment.Comment = input.Note;
                            feedbackComment.FileUrl = input.FileOfNote;
                            break;
                        case STATE_FEEDBACK.HANDLING:
                            feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_HANDLING;
                            break;
                        case STATE_FEEDBACK.PENDING:
                            feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_PENDING;
                            break;
                        case STATE_FEEDBACK.USER_CONFIRMED:
                            feedbackComment.TypeComment =
                                UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_USER_CONFIRMED;
                            break;
                        case STATE_FEEDBACK.USER_RATE_FEEDBACK:
                            feedbackComment.TypeComment =
                                UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_USER_RATE_FEEDBACK;
                            break;
                        case STATE_FEEDBACK.ADMIN_CONFIRMED:
                            feedbackComment.TypeComment =
                                UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_ADMIN_CONFIRMED;
                            break;
                    }

                    await NotifierStateCitizenReflect(updateData);
                    feedbackComment.ReadState = 1;
                    await _citizenReflectCommentRepos.InsertAndGetIdAsync(feedbackComment);
                    var user = new UserIdentifier(updateData.TenantId, updateData.CreatorUserId.Value);
                    var clients = await _onlineClientManager.GetAllByUserIdAsync(user);

                    if (clients.Any())
                    {
                        _notificationCommunicator.AdminUpdateStateFeedback(clients, updateData);
                    }
                }

                mb.statisticMetris(t1, 0, "Ud_feedback");
                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task SetCommentCitizenReflectAsRead(long feedbackId)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    var setItems = await _citizenReflectCommentRepos.GetAllListAsync(
                        un => un.CreatorUserId != AbpSession.UserId
                              && (un.ReadState == 1 || un.ReadState == null)
                    );

                    foreach (var it in setItems)
                    {
                        it.ReadState = 2;
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                });
                mb.statisticMetris(t1, 0, "SetCommentFeedbackAsRead");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }
        }


        private IQueryable<CitizenReflectDto> QueryGetAllCitizenReflect(FormGetReflectId formId)
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = (from uf in _citizenReflectRepos.GetAll()
                         select new CitizenReflectDto()
                         {
                             Id = uf.Id,
                             CreationTime = uf.CreationTime,
                             LastModificationTime = uf.LastModificationTime,
                             State = uf.State,
                             OrganizationUnitId = uf.OrganizationUnitId,
                             Rating = uf.Rating,
                             UrbanId = uf.UrbanId,
                             BuildingId = uf.BuildingId,
                         })
                         .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                         .AsQueryable();
            switch (formId)
            {
                case FormGetReflectId.GetAll:
                    break;
                case FormGetReflectId.GetCompleted:
                    query = query.Where(x => x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.USER_CONFIRMED
                                             || x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.ADMIN_CONFIRMED
                                             || x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.USER_RATE_FEEDBACK);

                    break;
            }

            return query;
        }

        public async Task<object> GetStatisticFeedback(GetStatisticFeedbackInput input)
        {
            try
            {
                IEnumerable<StatisticFeedbackDto> query = _citizenReflectRepos.GetAll()
                    .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId)
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                    .WhereIf(!input.ApartmentCode.IsNullOrEmpty(), x => x.ApartmentCode == input.ApartmentCode)
                    .WhereIf(input.DateStart.HasValue, x => x.CreationTime >= input.DateStart)
                    .WhereIf(input.DateEnd.HasValue, x => x.CreationTime < input.DateEnd)
                    .Select(x => new StatisticFeedbackDto
                    {
                        Id = x.Id,
                        BuildingId = x.BuildingId,
                        State = x.State,
                        ApartmentCode = x.ApartmentCode,
                        UrbanId = x.UrbanId,
                        Type = x.Type,
                        TenantId = x.TenantId,
                        Year = x.CreationTime.Year,
                        Month = x.CreationTime.Month,
                        Day = x.CreationTime.Day,
                        CreationTime = x.CreationTime,
                        CreatorUserId = x.CreatorUserId,
                    }).AsEnumerable();
                List<DataStatisticFeedback> result = new();
                IEnumerable<GroupFeedbackStatistic> groupedQuery = null;

                switch (input.QueryDateTime)
                {
                    case QueryDateTime.YEAR:
                        groupedQuery = query.GroupBy(x => new { x.Year }).Select(y => new GroupFeedbackStatistic()
                        {
                            Year = y.Key.Year,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryDateTime.MONTH:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month }).Select(y =>
                            new GroupFeedbackStatistic()
                            {
                                Year = y.Key.Year,
                                Month = y.Key.Month,
                                Items = y.ToList(),
                            });
                        break;
                    case QueryDateTime.DAY:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day }).Select(y =>
                            new GroupFeedbackStatistic()
                            {
                                Year = y.Key.Year,
                                Month = y.Key.Month,
                                Day = y.Key.Day,
                                Items = y.ToList(),
                            });
                        break;
                }

                if (groupedQuery != null)
                {
                    foreach (GroupFeedbackStatistic group in groupedQuery)
                    {
                        result.Add(new DataStatisticFeedback
                        {
                            Year = group.Year,
                            Month = group.Month,
                            Day = group.Day,
                            TotalCount = group.Items.Count,
                            CountPending = group.Items.Count(x => x.State == (int)STATE_FEEDBACK.PENDING),
                            CountHandling = group.Items.Count(x => x.State == (int)STATE_FEEDBACK.HANDLING
                                                                   || x.State == (int)STATE_FEEDBACK.DECLINED),
                            CountCompleted = group.Items.Count(x => x.State == (int)STATE_FEEDBACK.USER_CONFIRMED
                                                                    || x.State == (int)STATE_FEEDBACK.ADMIN_CONFIRMED),
                            CountRated = group.Items.Count(x => x.State == (int)STATE_FEEDBACK.USER_RATE_FEEDBACK),
                        });
                    }
                }

                result = result.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day).ToList();
                return DataResult.ResultSuccess(result, "Statistic feedback success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<DataResult> GetStatisticsCitizenReflect(StatisticsUserCitizenReflectInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                DateTime now = DateTime.Now;
                Dictionary<string, int> dataResult = new Dictionary<string, int>();
                DateTime monthCur = new DateTime(now.Year, now.Month, 1);
                if (input.Year.HasValue && input.Month.HasValue)
                {
                    monthCur = new DateTime(input.Year.Value, input.Month.Value, 1);
                }
                else if (input.Year.HasValue)
                {
                    monthCur = new DateTime(input.Year.Value, now.Month, 1);
                }
                else if (input.Month.HasValue)
                {
                    monthCur = new DateTime(now.Year, input.Month.Value, 1);
                }
                switch (input.QueryCase)
                {
                    case QueryCaseStatistics.ByMonth:
                        if (input.NumberRange > 12) input.NumberRange = 12;
                        for (var monthIndex = input.NumberRange - 1; monthIndex >= 0; monthIndex--)
                        {
                            DateTime cal = monthCur.AddMonths(-monthIndex);
                            var query = QueryGetAllCitizenReflect(input.FormId);
                            int count = query
                                .WhereIf(input.OrganizationUnitId.HasValue,
                                    x => x.OrganizationUnitId == input.OrganizationUnitId)
                                .Where(x => x.CreationTime.Month == cal.Month && x.CreationTime.Year == cal.Year)
                                 .WhereIf(input.BuildingId.HasValue && input.BuildingId.Value > 0, x => x.BuildingId == input.BuildingId)
                                .WhereIf(input.UrbanId.HasValue && input.UrbanId.Value > 0, x => x.UrbanId == input.UrbanId)
                                .Count();
                            dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(cal.Month), count);
                        }

                        break;
                    case QueryCaseStatistics.ByDay:
                        int days = DateTime.DaysInMonth(monthCur.Year, monthCur.Month);
                        var queryByDay = QueryGetAllCitizenReflect(input.FormId);
                        var dataCount = queryByDay
                            .WhereIf(input.OrganizationUnitId.HasValue,
                                    x => x.OrganizationUnitId == input.OrganizationUnitId)
                                .Where(x => x.CreationTime.Year == monthCur.Year && x.CreationTime.Month == monthCur.Month)
                                 .WhereIf(input.BuildingId.HasValue && input.BuildingId.Value > 0, x => x.BuildingId == input.BuildingId)
                                .WhereIf(input.UrbanId.HasValue && input.UrbanId.Value > 0, x => x.UrbanId == input.UrbanId).AsEnumerable();
                        for (var dayIndex = 1; dayIndex <= days; dayIndex++)
                        {
                            int count = dataCount.Count(x => x.CreationTime.Day == dayIndex);
                            dataResult.Add(dayIndex + "-" + monthCur.Month, count);
                        }

                        break;
                    case QueryCaseStatistics.ByHours:
                        var queryByHours = QueryGetAllCitizenReflect(input.FormId);
                        var dataCountHours = queryByHours
                            .WhereIf(input.OrganizationUnitId.HasValue,
                                    x => x.OrganizationUnitId == input.OrganizationUnitId)
                                .Where(x => x.CreationTime.Year == monthCur.Year && x.CreationTime.Month == monthCur.Month)
                                .WhereIf(input.BuildingId.HasValue && input.BuildingId.Value > 0, x => x.BuildingId == input.BuildingId)
                                .WhereIf(input.UrbanId.HasValue && input.UrbanId.Value > 0, x => x.UrbanId == input.UrbanId).AsEnumerable();
                        dataResult.Add("1 days", dataCountHours.Count(x => x.LastModificationTime.HasValue && (x.LastModificationTime.Value - x.CreationTime).Days <= 1));
                        dataResult.Add("1 days - 3 days", dataCountHours.Count(x => x.LastModificationTime.HasValue && (x.LastModificationTime.Value - x.CreationTime).Days <= 3 && (x.LastModificationTime.Value - x.CreationTime).Days > 1));
                        dataResult.Add("3 days - 5 days", dataCountHours.Count(x => x.LastModificationTime.HasValue && (x.LastModificationTime.Value - x.CreationTime).Days <= 5 && (x.LastModificationTime.Value - x.CreationTime).Days > 3));
                        dataResult.Add("> 5 days", dataCountHours.Count(x => x.LastModificationTime.HasValue && (x.LastModificationTime.Value - x.CreationTime).Days > 5));
                        break;
                    case QueryCaseStatistics.ByStar:
                        var queryByStar = QueryGetAllCitizenReflect(input.FormId);
                        var dataCountStar = queryByStar.WhereIf(input.OrganizationUnitId.HasValue,
                                    x => x.OrganizationUnitId == input.OrganizationUnitId)
                                .Where(x => x.CreationTime.Year == monthCur.Year && x.CreationTime.Month == monthCur.Month)
                                 .WhereIf(input.BuildingId.HasValue && input.BuildingId.Value > 0, x => x.BuildingId == input.BuildingId)
                                .WhereIf(input.UrbanId.HasValue && input.UrbanId.Value > 0, x => x.UrbanId == input.UrbanId).AsEnumerable();
                        dataResult.Add("1", dataCountStar.Count(x => x.Rating.HasValue && x.Rating == 1));
                        dataResult.Add("2", dataCountStar.Count(x => x.Rating.HasValue && x.Rating == 2));
                        dataResult.Add("3", dataCountStar.Count(x => x.Rating.HasValue && x.Rating == 3));
                        dataResult.Add("4", dataCountStar.Count(x => x.Rating.HasValue && x.Rating == 4));
                        dataResult.Add("5", dataCountStar.Count(x => x.Rating.HasValue && x.Rating == 5));

                        break;
                    default:
                        break;
                }

                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                mb.statisticMetris(t1, 0, "gall_statistics_object");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }


        public async Task<int> QueryGetAllCitizenReflectCommentByMonth(int month, int year, long? organizationUnitId)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    if (organizationUnitId.HasValue)
                    {
                        var query = await _citizenReflectRepos.GetAllListAsync(x =>
                            x.CreationTime.Month == month && x.CreationTime.Year == year &&
                            x.TenantId == AbpSession.TenantId && x.OrganizationUnitId == organizationUnitId);
                        int count = 0;
                        foreach (var f in query)
                        {
                            var res = await _citizenReflectCommentRepos.GetAllListAsync(x => x.FeedbackId == f.Id);
                            count = count + res.Count();
                        }

                        return count;
                    }
                    else
                    {
                        var query = await _citizenReflectRepos.GetAllListAsync(x =>
                            x.CreationTime.Month == month && x.CreationTime.Year == year &&
                            x.TenantId == AbpSession.TenantId);
                        int count = 0;
                        foreach (var f in query)
                        {
                            var res = await _citizenReflectCommentRepos.GetAllListAsync(x => x.FeedbackId == f.Id);
                            count = count + res.Count();
                        }

                        return count;
                    }
                }


                #region Data Common

                #endregion
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                return 0;
            }
        }

        public async Task<object> GetStatisticsCitizenReflectComment(StatisticsUserCitizenReflectCommentInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                DateTime now = DateTime.Now;
                int monthCurrent = now.Month;
                int yearCurrent = now.Year;

                Dictionary<string, int> dataResult = new Dictionary<string, int>();

                if (monthCurrent >= input.NumberMonth)
                {
                    for (int index = monthCurrent - input.NumberMonth + 1; index <= monthCurrent; index++)
                    {
                        int count = await QueryGetAllCitizenReflectCommentByMonth(index, yearCurrent,
                            input.OrganizationUnitId);
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), count);
                    }
                }
                else
                {
                    for (var index = 13 - (input.NumberMonth - monthCurrent); index <= 12; index++)
                    {
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index),
                            await QueryGetAllCitizenReflectCommentByMonth(index, yearCurrent - 1,
                                input.OrganizationUnitId));
                    }

                    for (var index = 1; index <= monthCurrent; index++)
                    {
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index),
                            await QueryGetAllCitizenReflectCommentByMonth(index, yearCurrent,
                                input.OrganizationUnitId));
                    }
                }

                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                mb.statisticMetris(t1, 0, "gall_statistics_object");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        #region Common

        private async Task NotifierStateCitizenReflect(CitizenReflect reflect)
        {
            string detailUrlApp = $"yoolife://app/feedback/detail?id={reflect.Id}";

            switch (reflect.State)
            {
                case (int?)UserFeedbackEnum.STATE_FEEDBACK.DECLINED:
                    // detailUrlApp = $"yoolife://app/reflect/detail?id={reflect.Id}";
                    string detailUrlWADeclined = $"/feedbacks?formId=13&id={reflect.Id}";
                    var messageDeclined = new UserMessageNotificationDataBase(
                        AppNotificationAction.StateReflectCitizen,
                        AppNotificationIcon.StateReflectCitizenDenied,
                        TypeAction.Detail,
                        $"Phản ánh {reflect.Name} của bạn đã bị từ chối. Nhấn để xem chi tiết !",
                        detailUrlApp,
                        detailUrlWADeclined,
                        "",
                        "",
                        0


                    );
                    await _appNotifier.SendUserMessageNotifyFullyAsync(
                        "Thông báo phản ánh cư dân!",
                        $"Phản ánh {reflect.Name} của bạn đã bị từ chối. Nhấn để xem chi tiết !",
                        detailUrlApp,
                        detailUrlWADeclined,
                        new UserIdentifier[]
                        {
                            new UserIdentifier(reflect.TenantId,
                                reflect.CreatorUserId.HasValue ? reflect.CreatorUserId.Value : 0)
                        },
                        messageDeclined);
                    break;

                case (int?)UserFeedbackEnum.STATE_FEEDBACK.HANDLING:
                    string detailUrlWAHandling = $"/feedbacks?formId=12&id={reflect.Id}";
                    var messageHandling = new UserMessageNotificationDataBase(
                        AppNotificationAction.StateReflectCitizen,
                        AppNotificationIcon.StateReflectCitizenHandling,
                        TypeAction.Detail,
                        $"Phản ánh {reflect.Name} của bạn đã được tiếp nhận !",
                        detailUrlApp,
                        detailUrlWAHandling,
                        "",
                        "",
                        0
                    );
                    await _appNotifier.SendUserMessageNotifyFullyAsync(
                        "Thông báo phản ánh cư dân!",
                        $"Phản ánh {reflect.Name} của bạn đã được tiếp nhận !",
                        detailUrlApp,
                        detailUrlWAHandling,
                        new UserIdentifier[]
                        {
                            new UserIdentifier(reflect.TenantId,
                                reflect.CreatorUserId.HasValue ? reflect.CreatorUserId.Value : 0)
                        },
                        messageHandling);
                    break;

                case (int?)UserFeedbackEnum.STATE_FEEDBACK.ADMIN_CONFIRMED:
                    string detailUrlWAConfirm = $"/feedbacks?formId=14&id={reflect.Id}";
                    var messageConfirm = new UserMessageNotificationDataBase(
                        AppNotificationAction.StateReflectCitizen,
                        AppNotificationIcon.StateReflectCitizenConfirmed,
                        TypeAction.Detail,
                        $"Phản ánh {reflect.Name} của bạn đã được hoàn thành !",
                        detailUrlApp,
                        detailUrlWAConfirm,
                        "",
                        "",
                        0

                    );
                    await _appNotifier.SendUserMessageNotifyFullyAsync(
                        "Thông báo phản ánh cư dân!",
                        $"Phản ánh {reflect.Name} của bạn đã được hoàn thành !",
                        detailUrlApp,
                        detailUrlWAConfirm,
                        new UserIdentifier[]
                        {
                            new UserIdentifier(reflect.TenantId,
                                reflect.CreatorUserId.HasValue ? reflect.CreatorUserId.Value : 0)
                        },
                        messageConfirm);
                    break;
            }
        }

        #endregion

        #region method helpers
        private string GetOrganizationName(long? organizationId)
        {
            return _organizationRepos.GetAll().Where(x => x.Id == (organizationId ?? 0)).Select(x => x.DisplayName).FirstOrDefault();
        }
        private async Task<string> GetUsersOrganizationNameAsync(long[]? userIds)
        {
            if (userIds == null || userIds.Length == 0)
            {
                return string.Empty;
            }

            var users = await _userRepos.GetAll().Where(x => userIds.Contains(x.Id)).ToListAsync();

            var names = users.Select(x => $"{x.Surname} {x.Name}");

            return string.Join(", ", names);
        }
        #endregion
    }
}