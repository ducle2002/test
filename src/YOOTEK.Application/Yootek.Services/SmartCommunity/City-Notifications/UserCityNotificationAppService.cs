using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Yootek.Authorization.Users;
using Yootek.Chat;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp;
using Abp.Authorization.Users;
using Abp.Linq.Extensions;
using Abp.UI;
using Abp.Runtime.Session;

namespace Yootek.Services
{
    public interface IUserCityNotificationAppService : IApplicationService
    {
        Task<object> GetAllNotificationUserTenant(NotificationInput input);
        Task<object> CreatOrUpdateCommentAsync(CommentDto input);
        Task SetCommentFeedbackAsRead(long feedbackId);
        Task<object> GetNotificationUserOffline();
        Task<object> GetAllFeedbackUser(FeedbackUserInput input);
        Task<object> GetAllNotificationUser();
        Task<object> CreatOrUpdateFollowAsync(CommentDto input);
        Task<object> GetAllCommentAsync(GetCommentInput input);
        Task<object> CreateOrUpdateFeedback(UserFeedbackDto input);
        Task<object> DeleteFeedback(long id);
        Task<object> GetAllCommnetByFeedback(GetCommentFeedbackInput input);
        Task<object> CreateOrUpdateFeedbackComment(UserFeedbackCommentDto input);
        Task<object> DeleteFeedbackComment(long id);
        Task<object> RateFeedbackUser(RateFeedbackDto input);
        Task<object> GetCityNotificationById(long id);
    }

    [AbpAuthorize]
    public class UserCityNotificationAppService : YootekAppServiceBase, IUserCityNotificationAppService
    {
        private readonly IRepository<CityNotification, long> _cityNotificationRepos;
        private readonly IRepository<Post, long> _postRepos;
        private readonly IRepository<CityNotificationComment, long> _commentRepos;
        private readonly IRepository<CitizenReflect, long> _userFeedbackRepos;
        private readonly IRepository<CitizenReflectComment, long> _userFeedbackCommentRepos;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly INotificationCommunicator _notificationCommunicator;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly ITenantCache _tenantCache;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly UserManager _userManager;
        private readonly UserStore _store;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserCityNotification, long> _userCityNotificationRepos;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepos;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepos;
        private readonly IAppNotifier _appNotifier;
        public UserCityNotificationAppService(
            IRepository<CityNotification, long> cityNotificationRepos,
            IRepository<Post, long> postRepos,
            IRepository<CityNotificationComment, long> commentRepos,
            IRepository<Citizen, long> citizenRepos,
            IRepository<User, long> userRepos,

            IRepository<UserCityNotification, long> userCityNotificationRepos,
            IRepository<CitizenReflectComment, long> userFeedbackCommentRepos,
            IChatCommunicator chatCommunicator,
            IOnlineClientManager onlineClientManager,
            INotificationCommunicator notificationCommunicator,
            ITenantCache tenantCache,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<CitizenReflect, long> userFeedbackRepos,
            UserManager userManager,
            UserStore store,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepos,
            IAppNotifier appNotifier
            )
        {
            _cityNotificationRepos = cityNotificationRepos;
            _postRepos = postRepos;
            _notificationCommunicator = notificationCommunicator;
            _onlineClientManager = onlineClientManager;
            _userFeedbackCommentRepos = userFeedbackCommentRepos;
            _tenantCache = tenantCache;
            _commentRepos = commentRepos;
            _unitOfWorkManager = unitOfWorkManager;
            _userManager = userManager;
            _citizenRepos = citizenRepos;
            _chatCommunicator = chatCommunicator;
            _userFeedbackRepos = userFeedbackRepos;
            _store = store;
            _userRepos = userRepos;
            _userCityNotificationRepos = userCityNotificationRepos;
            _appOrganizationUnitRepos = appOrganizationUnitRepos;
            _userOrganizationUnitRepos = userOrganizationUnitRepos;
            _appNotifier = appNotifier;
        }


        public async Task<object> GetAllOrganizationUnitCityNotification(long? orgId)
        {
            try
            {
                var org = new AppOrganizationUnit();
                if (orgId.HasValue)
                {
                    org = await _appOrganizationUnitRepos.FirstOrDefaultAsync(orgId.Value);
                }
                var result = (from apm in _appOrganizationUnitRepos.GetAll()
                              where apm.Type == APP_ORGANIZATION_TYPE.NOTIFICATION
                              select new
                              {
                                  OrganizationUnitId = apm.ParentId,
                                  Name = apm.DisplayName,
                                  ImageUrl = apm.ImageUrl,
                                  Type = apm.Type,
                                  Description = apm.Description,
                                  Code = apm.Code

                              })
                               .WhereIf(org != null && org.Id > 0, x => x.Code.StartsWith(org.Code))
                            .ToList();
                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }
        public async Task<object> CreateOrUpdateFeedback(UserFeedbackDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var feedbackComment = new CitizenReflectComment();
                feedbackComment.TenantId = AbpSession.TenantId;
                feedbackComment.ReadState = 1;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _userFeedbackRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        if (input.IsPublic != updateData.IsPublic)
                        {
                            var post = await _postRepos.FirstOrDefaultAsync(x => x.FeedbackId == input.Id);
                            if (post != null)
                            {
                                post.State = input.IsPublic.Value ? post.State : 1;
                                await _postRepos.UpdateAsync(post);
                            }
                        }
                        input.MapTo(updateData);
                        //if (input.State == 4) updateData.IsRated = true;
                        //else updateData.IsRated = false;
                        //call back
                        await _userFeedbackRepos.UpdateAsync(updateData);
                        feedbackComment.FeedbackId = updateData.Id;
                        switch (input.State)
                        {
                            case (int?)UserFeedbackEnum.STATE_FEEDBACK.DECLINED:
                                feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_DECLINED;
                                feedbackComment.Comment = input.Note;
                                feedbackComment.FileUrl = input.FileOfNote;
                                break;
                            case (int?)UserFeedbackEnum.STATE_FEEDBACK.HANDLING:
                                feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_HANDLING;
                                break;
                            case (int?)UserFeedbackEnum.STATE_FEEDBACK.PENDING:
                                feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_PENDING;
                                break;
                            case (int?)UserFeedbackEnum.STATE_FEEDBACK.USER_CONFIRMED:
                                feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_USER_CONFIRMED;
                                break;
                            case (int?)UserFeedbackEnum.STATE_FEEDBACK.USER_RATE_FEEDBACK:
                                feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_USER_RATE_FEEDBACK;
                                break;
                            case (int?)UserFeedbackEnum.STATE_FEEDBACK.ADMIN_CONFIRMED:
                                feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_ADMIN_CONFIRMED;
                                break;
                        }
                        await _userFeedbackCommentRepos.InsertAndGetIdAsync(feedbackComment);
                    }
                    mb.statisticMetris(t1, 0, "Ud_feedback");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    input.State = (int)UserFeedbackEnum.STATE_FEEDBACK.PENDING;
                    var insertInput = input.MapTo<CitizenReflect>();
                    long id = await _userFeedbackRepos.InsertAndGetIdAsync(insertInput);
                    if (string.IsNullOrEmpty(insertInput.Name))
                    {
                        insertInput.Name = "PA0" + id.ToString();
                    }
                    insertInput.Id = id;

                    //if (input.IsPublic.Value)
                    //{
                    //    var arr = new string[1];
                    //    arr[0] = insertInput.FileUrl;
                    //    var post = new Post()
                    //    {
                    //        ContentPost = insertInput.Data,
                    //        FeedbackId = id,
                    //        ImageContent = arr[0] != null ? string.Join(",", arr) : null,
                    //        State = 1,
                    //        TenantId = AbpSession.TenantId
                    //    };
                    //    var insertPost = post.MapTo<Post>();
                    //    await _postRepos.InsertAsync(insertPost);
                    //}

                    feedbackComment.FeedbackId = id;
                    feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_PENDING;
                    await _userFeedbackCommentRepos.InsertAndGetIdAsync(feedbackComment);
                    await CurrentUnitOfWork.SaveChangesAsync();
                    var admins = await _store.GetUserByOrganizationUnitIdAsync((int)input.Type.Value, AbpSession.TenantId);

                    //var clients = _onlineClientManager.GetAllClients()
                    //    .Where(c => c.TenantId == AbpSession.TenantId)
                    //    .ToImmutableList();

                    if (admins != null && admins.Any())
                    {
                        _notificationCommunicator.SendNotificationToAdminTenant(admins, insertInput);
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
        public async Task<object> CreateOrUpdateFeedbackComment(UserFeedbackCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var feedback = await _userFeedbackRepos.GetAsync(input.FeedbackId);
                if (feedback != null)
                {
                    if (input.Id > 0)
                    {
                        //update
                        var updateData = await _userFeedbackCommentRepos.GetAsync(input.Id);
                        if (updateData != null)
                        {
                            input.MapTo(updateData);
                            await _userFeedbackCommentRepos.UpdateAsync(updateData);
                        }
                        mb.statisticMetris(t1, 0, "Ud_fbcomment");

                        var data = DataResult.ResultSuccess(updateData, "Update success !");
                        return data;
                    }
                    else
                    {

                        var insertInput = input.MapTo<CitizenReflectComment>();
                        insertInput.ReadState = 1;
                        long id = await _userFeedbackCommentRepos.InsertAndGetIdAsync(insertInput);
                        insertInput.Id = id;

                        mb.statisticMetris(t1, 0, "is_noti");
                        var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                        return data;
                    }
                }
                else
                {
                    mb.statisticMetris(t1, 0, "is_noti");
                    var data = DataResult.ResultError(null, "Feedback not found !");
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
        public async Task SetCommentFeedbackAsRead(long feedbackId)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    var setItems = await _userFeedbackCommentRepos.GetAllListAsync(
                            un => un.CreatorUserId != AbpSession.UserId
                            && (un.ReadState == 1 || un.ReadState == null)
                        );

                    foreach (var it in setItems)
                    {
                        it.ReadState = 2;
                    }

                    await _unitOfWorkManager.Current.SaveChangesAsync();
                });
                mb.statisticMetris(t1, 0, "SetCommentFeedbackAsRead");

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }
        }
        public async Task<object> GetAllCommentAsync(GetCommentInput input)
        {
            try
            {
                var id = AbpSession.UserId;
                var query = (from cm in _commentRepos.GetAll()
                             join us in _userRepos.GetAll() on cm.CreatorUserId equals us.Id into tb_us
                             from us in tb_us.DefaultIfEmpty()
                             where !cm.IsLike.HasValue
                             select new CommentDto()
                             {
                                 CreationTime = cm.CreationTime,
                                 CreatorUserId = cm.CreatorUserId,
                                 Id = cm.Id,
                                 Type = cm.Type,
                                 Comment = cm.Comment,
                                 IsLike = cm.IsLike,
                                 FullName = us != null ? us.UserName : null,
                                 Avatar = us != null ? us.ImageUrl : null,
                                 CityNotificationId = cm.CityNotificationId

                             })
                            .WhereIf(input.NotificationId > 0, u => u.CityNotificationId == input.NotificationId)
                            .AsQueryable();


                var result = query.PageBy(input).ToList();

                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }
        public async Task<object> CreatOrUpdateFollowAsync(CommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = await _commentRepos.FirstOrDefaultAsync(x => x.CreatorUserId == AbpSession.UserId && x.IsLike.HasValue && x.Comment == null && x.CityNotificationId == input.CityNotificationId);

                if (updateData != null)
                {
                    //update
                    updateData.IsLike = input.IsLike;
                    await _commentRepos.UpdateAsync(updateData);
                    mb.statisticMetris(t1, 0, "Ud_ad_follow");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CityNotificationComment>();
                    insertInput.TenantId = AbpSession.TenantId;
                    insertInput.CreatorUserId = AbpSession.UserId;
                    long id = await _commentRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_ad_follow");
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
        public async Task<object> CreatOrUpdateCommentAsync(CommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _commentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _commentRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_comment");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CityNotificationComment>();
                    long id = await _commentRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;

                    var userComment = UserManager.GetUser(AbpSession.ToUserIdentifier());
                    if(userComment != null) await NotifierNewCommentUser(insertInput, userComment);
                    mb.statisticMetris(t1, 0, "is_comment");
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
        private async Task NotifierNewCommentUser(CityNotificationComment data, User userComment)
        {
            var detailUrlApp = $"yoolife://app/notification/comments?id={data.Id}";
            var detailUrlWA = $"/notices?id={data.Id}";
            var creatorNotification = new List<UserIdentifier>
            {
                new UserIdentifier(data.TenantId, data.CreatorUserId ?? 0)
            };

            var messageDeclined = new NotificationWithContentIdDatabase(
                data.Id,
                AppNotificationAction.CityNotificationComment,
                AppNotificationIcon.CityNotificationCommentIcon,
                TypeAction.Detail,
                  $"{userComment.UserName} đã bình luận một bài thông báo số của bạn. Nhấn để xem chi tiết !",
                detailUrlApp,
                detailUrlWA
                );
            await _appNotifier.SendMessageNotificationInternalAsync(
                    "Yoolife thông báo số!",
                    $"{userComment.UserName} đã bình luận một bài thông báo số của bạn. Nhấn để xem chi tiết !",
                    detailUrlApp,
                    detailUrlWA,
                    creatorNotification.ToArray(),
                    messageDeclined,
                    AppType.USER
                    );

        }
        public async Task<object> DeleteFeedback(long id)
        {
            try
            {

                await _userFeedbackRepos.DeleteAsync(id);
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
        public async Task<object> DeleteFeedbackComment(long id)
        {
            try
            {

                await _userFeedbackCommentRepos.DeleteAsync(id);
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
        public async Task<object> GetAllCommnetByFeedback(GetCommentFeedbackInput input)
        {
            try
            {
                var result = await _userFeedbackCommentRepos.GetAll()
                    .Where(x => x.FeedbackId == input.FeedbackId)
                    .OrderByDescending(m => m.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetAllFeedbackUser(FeedbackUserInput input)
        {

            try
            {
                var query = (from f in _userFeedbackRepos.GetAll()
                             select new UserFeedbackDto
                             {
                                 Id = f.Id,
                                 Name = f.Name,
                                 Data = f.Data,
                                 FileUrl = f.FileUrl,
                                 Type = f.Type,
                                 TenantId = f.TenantId,
                                 FinishTime = f.FinishTime,
                                 State = f.State,
                                 IsPublic = f.IsPublic,
                                 IsDeleted = f.IsDeleted,
                                 DeleterUserId = f.DeleterUserId,
                                 DeletionTime = f.DeletionTime,
                                 LastModificationTime = f.LastModificationTime,
                                 LastModifierUserId = f.LastModifierUserId,
                                 CreationTime = f.CreationTime,
                                 CreatorUserId = f.CreatorUserId,
                                 Rating = f.Rating,
                                 RatingContent = f.RatingContent
                             }).Where(x => x.CreatorUserId == AbpSession.UserId).AsQueryable();
                query = QueryFormId(query, input, input.FormId);
                if (query != null)
                {
                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                    var totalRecs = query.Count();
                    return DataResult.ResultSuccess(result, "Get success!", totalRecs);
                }
                else
                {
                    var result = new List<UserFeedbackDto>();
                    return DataResult.ResultSuccess(result, "Get success!", 0);

                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        protected IQueryable<UserFeedbackDto> QueryFormId(IQueryable<UserFeedbackDto> query, FeedbackUserInput input, int formId)
        {

            switch (formId)
            {   //tab 1
                case (int)UserFeedbackEnum.FORM_ID_FEEDBACK.FORM_USER_GET_FEEDBACK_PENDING:
                    query = query.Where(x => x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.PENDING).OrderByDescending(x => x.CreationTime);
                    break;
                //tab 2
                case (int)UserFeedbackEnum.FORM_ID_FEEDBACK.FORM_USER_GET_FEEDBACK_HANDLING:
                    query = query.Where(x => x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.DECLINED
                                            || x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.HANDLING).OrderByDescending(x => x.CreationTime);
                    break;
                //tab 3
                case (int)UserFeedbackEnum.FORM_ID_FEEDBACK.FORM_USER_GET_FEEDBACK_HANDLED:
                    query = query.Where(x => x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.USER_CONFIRMED
                                            || x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.USER_RATE_FEEDBACK
                                            || x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.ADMIN_CONFIRMED).OrderByDescending(x => x.CreationTime);
                    break;
                case (int)UserFeedbackEnum.FORM_ID_FEEDBACK.FORM_USER_GET_FEEDBACK_GETALL:
                    query = query.OrderByDescending(x => x.CreationTime);
                    break;
                default:
                    query = null;
                    break;
            }

            return query;
        }
        public async Task<object> RateFeedbackUser(RateFeedbackDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var feedback = await _userFeedbackRepos.GetAsync(input.Id);
                if (feedback != null && input.Id > 0)
                {
                    feedback.State = (int)UserFeedbackEnum.STATE_FEEDBACK.USER_RATE_FEEDBACK;
                    feedback.Rating = input.Rating;
                    feedback.RatingContent = input.Comment;
                    await _userFeedbackRepos.UpdateAsync(feedback);
                    var data = DataResult.ResultSuccess(feedback, "Success!");
                    return data;
                }
                else
                {
                    var data = DataResult.ResultFail("Feedback not exist!");
                    return data;
                }
            }
            catch (Exception e)
            {
                var result = DataResult.ResultError(e.Message, "Failed!");
                Logger.Fatal(e.StackTrace);
                return result;
            }
        }

        public async Task<object> GetAllNotificationUserTenant(NotificationInput input)
        {
            try
            {
                var citizen = _citizenRepos.FirstOrDefault(x => x.State == STATE_CITIZEN.ACCEPTED && x.AccountId == AbpSession.UserId);
                //var citizen = _citizenRepos.FirstOrDefault(x => x.AccountId == AbpSession.UserId);
                var query = (from noti in _cityNotificationRepos.GetAll()
                             join og in _appOrganizationUnitRepos.GetAll() on noti.OrganizationUnitId equals og.Id into tb_og
                             from og in tb_og.DefaultIfEmpty()
                             where noti.Type == input.Type
                             select new CityNotificationDto()
                             {
                                 CreationTime = noti.CreationTime,
                                 CreatorUserId = noti.CreatorUserId,
                                 Data = noti.Data,
                                 FileUrl = noti.FileUrl,
                                 //Follow = noti.Follow,
                                 Id = noti.Id,
                                 Name = noti.Name,
                                 Type = noti.Type,
                                 //CountComment = (from com in _commentRepos.GetAll()
                                 //                where com.CityNotificationId == noti.Id
                                 //                && !com.IsLike.HasValue
                                 //                select com).AsQueryable().Count(),
                                 //CountFollow = (from com in _commentRepos.GetAll()
                                 //               where com.CityNotificationId == noti.Id
                                 //               && com.IsLike == true
                                 //               select com).AsQueryable().Count(),
                                 ReceiverGroupCode = noti.ReceiverGroupCode,
                                 IsReceiveAll = noti.IsReceiveAll,
                                 DepartmentCode = noti.DepartmentCode,
                                 State = noti.State,
                                 OrganizationUnitId = noti.OrganizationUnitId,
                                 //IsAllowComment = noti.IsAllowComment,
                                 OrganizationUnitName = og.DisplayName,
                                 UrbanId = noti.UrbanId,
                                 BuildingId = noti.BuildingId,
                                 AttachUrls = noti.AttachUrls
                             })
                             .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                             .WhereIf(input.Type == (int)NotificationEnum.NOTIFICATION_TYPE.TENANT_NOTIFICATION, u => u.State == (int)NotificationEnum.NOTIFICATION_STATE.PUSH_NOTIFICATION)
                             .WhereIf(input.Type == (int)NotificationEnum.NOTIFICATION_TYPE.TENANT_DAILYLIFE, u => u.Type == (int)NotificationEnum.NOTIFICATION_TYPE.TENANT_DAILYLIFE)
                             .WhereIf(input.Keyword != null, x => (x.Name != null && x.Name.ToLower().Contains(input.Keyword)))

                            // .Where(u => u.ReceiverGroupCode == null || (citizen != null && !string.IsNullOrEmpty(u.ReceiverGroupCode) && u.ReceiverGroupCode.Contains(citizen.ApartmentCode)))
                             .WhereIf(input.UrbanId!=null && input.BuildingId!=null, u => u.UrbanId == null ||
                                         (u.UrbanId ==  input.UrbanId && (u.BuildingId == null || u.BuildingId == input.BuildingId)))
                             .WhereIf(input.ReceiverGroupCode != null , u =>
                                 string.IsNullOrEmpty(u.ReceiverGroupCode) || (!string.IsNullOrEmpty(u.ReceiverGroupCode) &&
                                                                           u.ReceiverGroupCode.Contains(input.ReceiverGroupCode)))
                             .Where(u => u.DepartmentCode == null || (citizen != null && !string.IsNullOrEmpty(u.DepartmentCode) && u.DepartmentCode.Contains(citizen.ApartmentCode)))
                             .OrderByDescending(x => x.CreationTime)
                             .AsQueryable();
                var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> GetAllNotificationUser()
        {
            try
            {
                var result = await (from noti in _cityNotificationRepos.GetAll()
                                    join userNoti in _userCityNotificationRepos.GetAll() on AbpSession.UserId equals userNoti.UserId into tb_user_noti
                                    from userNoti in tb_user_noti.DefaultIfEmpty()
                                    where noti.Id == userNoti.CityNotificationId
                                    select noti)
                    .OrderByDescending(m => m.CreationTime)
                    .ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> GetNotificationUserOffline()
        {
            try
            {

                var result = await _cityNotificationRepos.GetAllListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetCityNotificationById(long id)
        {
            try
            {
                var query = (from noti in _cityNotificationRepos.GetAll()
                                 //join cm in _commentRepos.GetAll() on noti.Id equals cm.CityNotificationId into tb_cm
                                 //from cm in tb_cm.DefaultIfEmpty()
                             where noti.Id == id
                             select new CityNotificationDto()
                             {
                                 CreationTime = noti.CreationTime,
                                 CreatorUserId = noti.CreatorUserId,
                                 Data = noti.Data,
                                 FileUrl = noti.FileUrl,
                                 //Follow = noti.Follow,
                                 Id = noti.Id,
                                 Name = noti.Name,
                                 Type = noti.Type,
                                 //  Department = noti.Department,
                                 //
                                 //OrganizationUnitId = noti.OrganizationUnitId,
                                 //  DepartmentCode = noti.DepartmentCode,
                                 CountComment = (from com in _commentRepos.GetAll()
                                                 where com.CityNotificationId == noti.Id
                                                 && !com.IsLike.HasValue
                                                 select com).AsQueryable().Count(),
                                 //CountFollow = (from com in _commentRepos.GetAll()
                                 //               where com.CityNotificationId == noti.Id
                                 //               && com.IsLike == true
                                 //               select com).AsQueryable().Count(),
                                 ReceiverGroupCode = noti.ReceiverGroupCode,
                                 //State = noti.State,
                                 OrganizationUnitId = noti.OrganizationUnitId,
                                 IsAllowComment = noti.IsAllowComment,
                                 AttachUrls = noti.AttachUrls
                             }).AsQueryable();
                var result = query.FirstOrDefault();
                var count = query.Count();
                var data = DataResult.ResultSuccess(result, "Get success!", count);
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

    }
}
