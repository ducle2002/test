using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Chat;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.Organizations.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.UserFeedbackCommentEnum;

namespace Yootek.Services
{
    public interface IUserCitizenReflectAppService : IApplicationService
    {
        Task SetCommentCitizenReflectAsRead(long reflectId);
        Task<object> GetNotificationUserOffline();
        Task<object> GetAllCitizenReflect(CitizenReflectUserInput input);
        Task<object> CreateOrUpdateCitizenReflect(CreateOrUpdateCitizenReflectInput input);
        Task<object> CreateCitizenReflect(CreateCitizenReflectInput input);
        Task<object> DeleteCitizenReflect(long id);
        Task<object> GetAllCommnetByCitizenReflect(GetCommentCitizenReflectInput input);
        Task<object> CreateOrUpdateCitizenReflectComment(CitizenReflectCommentDto input);
        Task<object> DeleteCitizenReflectComment(long id);
        Task<object> RateCitizenReflect(RateCitizenReflectDto input);
    }

    [AbpAuthorize]
    public class UserCitizenReflectAppService : YootekAppServiceBase, IUserCitizenReflectAppService
    {
        private readonly IRepository<CityNotification, long> _cityNotificationRepos;
        private readonly IRepository<Post, long> _postRepos;
        private readonly IRepository<CityNotificationComment, long> _commentRepos;
        private readonly IRepository<CitizenReflect, long> _citizenReflectRepos;
        private readonly IRepository<CitizenReflectComment, long> _citizenReflectCommentRepos;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly INotificationCommunicator _notificationCommunicator;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly ITenantCache _tenantCache;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly UserManager _userManager;
        private readonly UserStore _store;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<UserAccount, long> _userAccountRepos;
        private readonly IRepository<UserCityNotification, long> _userCityNotificationRepos;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepos;
        private readonly IAppNotifier _appNotifier;


        public UserCitizenReflectAppService(
            IRepository<CityNotification, long> cityNotificationRepos,
            IRepository<Post, long> postRepos,
            IRepository<CityNotificationComment, long> commentRepos,
            IRepository<Citizen, long> citizenRepos,
            IRepository<User, long> userRepos,
            IRepository<UserAccount, long> userAccountRepos,
            IRepository<UserCityNotification, long> userCityNotificationRepos,
            IRepository<CitizenReflectComment, long> citizenReflectCommentRepos,
            IChatCommunicator chatCommunicator,
            IOnlineClientManager onlineClientManager,
            INotificationCommunicator notificationCommunicator,
            ITenantCache tenantCache,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<CitizenReflect, long> citizenReflectRepos,
            UserManager userManager,
            UserStore store,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepos,
            IAppNotifier appNotifier
            )
        {
            _cityNotificationRepos = cityNotificationRepos;
            _postRepos = postRepos;
            _notificationCommunicator = notificationCommunicator;
            _onlineClientManager = onlineClientManager;
            _citizenReflectCommentRepos = citizenReflectCommentRepos;
            _tenantCache = tenantCache;
            _commentRepos = commentRepos;
            _unitOfWorkManager = unitOfWorkManager;
            _userManager = userManager;
            _citizenRepos = citizenRepos;
            _chatCommunicator = chatCommunicator;
            _citizenReflectRepos = citizenReflectRepos;
            _store = store;
            _userRepos = userRepos;
            _userAccountRepos = userAccountRepos;
            _userCityNotificationRepos = userCityNotificationRepos;
            _appOrganizationUnitRepos = appOrganizationUnitRepos;
            _appNotifier = appNotifier;
        }


        public async Task<DataResult> GetAllOrganizationUnitCitizenReflect(long? orgId)
        {
            try
            {
                var org = new AppOrganizationUnit();
                if (orgId.HasValue)
                {
                    org = await _appOrganizationUnitRepos.FirstOrDefaultAsync(orgId.Value);
                }

                var result = (from apm in _appOrganizationUnitRepos.GetAll()
                              where apm.Type == APP_ORGANIZATION_TYPE.FEEDBACK
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
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> CreateOrUpdateCitizenReflect(CreateOrUpdateCitizenReflectInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var feedbackComment = new CitizenReflectComment();
                feedbackComment.TenantId = AbpSession.TenantId;
                feedbackComment.ReadState = 1;
                if (input.Type > 0) input.OrganizationUnitId = input.Type;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _citizenReflectRepos.GetAsync(input.Id);
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
                        await _citizenReflectRepos.UpdateAsync(updateData);
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
                        await _citizenReflectCommentRepos.InsertAndGetIdAsync(feedbackComment);
                    }
                    mb.statisticMetris(t1, 0, "Ud_feedback");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    input.State = (int)UserFeedbackEnum.STATE_FEEDBACK.PENDING;
                    var insertInput = input.MapTo<CitizenReflect>();
                    long id = await _citizenReflectRepos.InsertAndGetIdAsync(insertInput);
                    if (string.IsNullOrEmpty(insertInput.Name))
                    {
                        insertInput.Name = "PA0" + id.ToString();
                    }
                    insertInput.Id = id;
                    long createtorId = (long)insertInput.CreatorUserId;

                    var queryCitizen = (from us in _citizenRepos.GetAll()
                                        where createtorId == us.AccountId
                                        select us).FirstOrDefault();

                    if (queryCitizen != null)
                    {
                        insertInput.Phone = queryCitizen.State == STATE_CITIZEN.ACCEPTED ? queryCitizen.PhoneNumber : input.Phone;
                        insertInput.NameFeeder = queryCitizen.State == STATE_CITIZEN.ACCEPTED ? queryCitizen.FullName : input.NameFeeder;
                    }

                    feedbackComment.FeedbackId = id;
                    feedbackComment.TypeComment = UserFeedbackCommentEnum.TYPE_COMMENT_FEEDBACK.STATE_PENDING;
                    await _citizenReflectCommentRepos.InsertAndGetIdAsync(feedbackComment);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    var organizationUnits = await _appOrganizationUnitRepos.GetAllListAsync(x => x.Type == APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME && x.ParentId == input.UrbanId);
                    var organizationUnitTypes = _appOrganizationUnitRepos.GetAllList(x => x.ParentId != null && x.Type != APP_ORGANIZATION_TYPE.REPRESENTATIVE_NAME)
                        .GroupBy(x => x.ParentId)
                        .Select(ou => new
                        {
                            parentId = ou.Key,
                            types = ou.Select(x => x.Type).Distinct().ToArray()
                        }).ToDictionary(x => x.parentId, y => y.types);

                    var listOrganizationUnitTypes = new ListResultDto<OrganizationUnitDto>(
                        organizationUnits.Select(ou =>
                        {
                            var organizationUnitDto = ObjectMapper.Map<OrganizationUnitDto>(ou);
                            organizationUnitDto.Types = organizationUnitTypes.ContainsKey(ou.Id) ? organizationUnitTypes[ou.Id] : null;
                            return organizationUnitDto;
                        }).ToList());

                    var unitChargeFeedbacks = listOrganizationUnitTypes.Items.Where(ou => ou.Types != null && ou.Types.Contains(APP_ORGANIZATION_TYPE.FEEDBACK)).ToList();

                    var memberUnitCharge = unitChargeFeedbacks
                        .SelectMany(ou => _userManager.GetUsersInOrganizationUnitAsync(ObjectMapper.Map<AppOrganizationUnit>(ou)).Result)
                        .Select(a => new UserIdentifier(a.TenantId, a.Id))
                        .ToArray();

                    var user = _userRepos.FirstOrDefault(AbpSession.UserId ?? 0);

                    if (memberUnitCharge != null && memberUnitCharge.Any())
                    {
                        await NotifierNewNotificationReflect(insertInput, memberUnitCharge, user?.FullName);
                    }



                    //var admins = await _store.GetUserByOrganizationUnitIdAsync((int)input.Type.Value, AbpSession.TenantId);

                    ////var clients = _onlineClientManager.GetAllClients()
                    ////    .Where(c => c.TenantId == AbpSession.TenantId)
                    ////    .ToImmutableList();

                    //if (admins != null && admins.Any())
                    //{
                    //    _notificationCommunicator.SendNotificationToAdminTenant(admins, insertInput);
                    //}

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
        public async Task<object> GetUserCitizenReflectById(long id)
        {
            try
            {
                var result = await _citizenReflectRepos.GetAsync(id);
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

        public async Task<object> CreateCitizenReflect(CreateCitizenReflectInput input)
        {
            try
            {
                CitizenReflect citizenInsert = input.MapTo<CitizenReflect>();
                citizenInsert.TenantId = AbpSession.TenantId;
                citizenInsert.State = (int)UserFeedbackEnum.STATE_FEEDBACK.PENDING;
                long id = await _citizenReflectRepos.InsertAndGetIdAsync(citizenInsert);
                citizenInsert.Name = "PA0" + id.ToString();
                citizenInsert.Id = id;

                Citizen? citizen = (from us in _citizenRepos.GetAll()
                                    where AbpSession.UserId == us.AccountId
                                    select us).FirstOrDefault();
                if (citizen != null)
                {
                    citizenInsert.Phone = citizen.PhoneNumber;
                    citizenInsert.NameFeeder = citizen.FullName;
                }

                CitizenReflectComment feedbackComment = new()
                {
                    FeedbackId = id,
                    TenantId = AbpSession.TenantId,
                    ReadState = (int?)STATE_READ_COMMENT_FEEDBACK.UN_READ,
                    TypeComment = TYPE_COMMENT_FEEDBACK.STATE_PENDING,
                };
                await _citizenReflectCommentRepos.InsertAndGetIdAsync(feedbackComment);
                await CurrentUnitOfWork.SaveChangesAsync();

                List<User>? admins = await _store.GetUserByOrganizationUnitIdAsync(0, AbpSession.TenantId);

                if (admins != null && admins.Any())
                {
                    _notificationCommunicator.SendNotificationToAdminTenant(admins, citizenInsert);
                }
                return DataResult.ResultSuccess(citizenInsert, "Insert success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> CreateOrUpdateCitizenReflectComment(CitizenReflectCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var feedback = await _citizenReflectRepos.GetAsync(input.FeedbackId);
                if (feedback != null)
                {
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

                        var admins = await _store.GetAllChatCitizenManagerTenantAsync(feedback.UrbanId, AbpSession.TenantId);
                        var adminIds = admins.Select(a => new UserIdentifier(a.TenantId, a.Id)).ToArray();
                        if (admins != null && admins.Any())
                        {
                            _notificationCommunicator.SendCommentFeedbackToAdminTenant(admins, insertInput);
                            await NotifierCommentCitizenReflect(insertInput, feedback.Name, adminIds);                                                                                    //  var adminIdentifiers = admins.Select(u => new UserIdentifier(u.TenantId, u.Id)).ToList();
                        }
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

        public async Task SetCommentCitizenReflectAsRead(long reflectId)
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

                    await _unitOfWorkManager.Current.SaveChangesAsync();
                });
                mb.statisticMetris(t1, 0, "SetCommentCitizenReflectAsRead");

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }
        }

        public async Task<object> DeleteCitizenReflect(long id)
        {
            try
            {

                await _citizenReflectRepos.DeleteAsync(id);
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

        public async Task<object> GetAllCommnetByCitizenReflect(GetCommentCitizenReflectInput input)
        {
            try
            {

                var result = await _citizenReflectCommentRepos.GetAll()
                    .Where(x => x.FeedbackId == input.CitizenReflectId)
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

        public async Task<object> GetAllCitizenReflect(CitizenReflectUserInput input)
        {

            try
            {
                IQueryable<CitizenReflectDto> query = (from f in _citizenReflectRepos.GetAll()
                                                       select new CitizenReflectDto
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
                                                           RatingContent = f.RatingContent,
                                                           OrganizationUnitId = f.OrganizationUnitId,
                                                           ReflectReport = f.ReflectReport,
                                                           ReportName = f.ReportName,
                                                           ApartmentCode = f.ApartmentCode,
                                                       })
                     .Where(x => x.CreatorUserId == AbpSession.UserId)
                     .WhereIf(input.ApartmentCode != null, x => x.ApartmentCode == input.ApartmentCode).AsQueryable();
                query = QueryFormId(query, input, input.FormId);

                if (query != null)
                {
                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                    var totalRecs = query.Count();
                    return DataResult.ResultSuccess(result, "Get success!", totalRecs);
                }
                else
                {
                    var result = new List<CitizenReflectDto>();
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
        protected IQueryable<CitizenReflectDto> QueryFormId(IQueryable<CitizenReflectDto> query, CitizenReflectUserInput input, int formId)
        {
            switch (formId)
            {   //tab 1
                case (int)UserFeedbackEnum.FORM_ID_FEEDBACK.FORM_USER_GET_FEEDBACK_PENDING:
                    query = query.Where(x => x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.PENDING || x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.ASSIGNEDHANDLER || x.State == (int)UserFeedbackEnum.STATE_FEEDBACK.ASSIGNED).OrderByDescending(x => x.CreationTime);
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

        public async Task<object> RateCitizenReflect(RateCitizenReflectDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var feedback = await _citizenReflectRepos.GetAsync(input.Id);
                if (feedback != null && input.Id > 0)
                {
                    feedback.State = (int)UserFeedbackEnum.STATE_FEEDBACK.USER_RATE_FEEDBACK;
                    feedback.Rating = input.Rating;
                    feedback.RatingContent = input.Comment;
                    await _citizenReflectRepos.UpdateAsync(feedback);
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

        public async Task<object> GetUnitPosition()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var result = await _cityNotificationRepos.GetAllListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!");
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
        
        #region common
        private async Task NotifierCommentCitizenReflect(CitizenReflectComment comment, string reflectName, UserIdentifier[] admin)
        {
            var detailUrlApp = $"yooioc://app/feedback/comment?id={comment.FeedbackId}";
            var detailUrlWA = $"/feedbacks/comment?id={comment.FeedbackId}";
            var messageDeclined = new NotificationWithContentIdDatabase(
            comment.Id,
            AppNotificationAction.CommentReflectCitizen,
            AppNotificationIcon.CommentReflectCitizenSuccessIcon,
            TypeAction.Detail,
            $"Bạn có 1 tin nhắn phản ánh {reflectName} mới. Nhấn để xem chi tiết !",
            detailUrlApp,
            detailUrlWA
            );
            await _appNotifier.SendMessageNotificationInternalAsync(
                "Tin nhắn phản ánh số!",
                $"Bạn có 1 tin nhắn phản ánh {reflectName} mới. Nhấn để xem chi tiết !",
                detailUrlApp,
                detailUrlWA,
                admin.ToArray(),
                messageDeclined,
                AppType.IOC);

        }
        private async Task NotifierNewNotificationReflect(CitizenReflect data, UserIdentifier[] admin, string creatorName)
        {
            var detailUrlApp = $"yooioc://app/feedback/detail?id={data.Id}";
            var detailUrlWA = $"/feedbacks?id={data.Id}";
            var messageDeclined = new NotificationWithContentIdDatabase(
                            data.Id,
                            AppNotificationAction.ReflectCitizenNew,
                            AppNotificationIcon.ReflectCitizenNewIcon,
                            TypeAction.Detail,
                            $"{creatorName} đã tạo một phản ánh mới. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWA
                            );

            await _appNotifier.SendMessageNotificationInternalAsync(
                "Yoolife phản ánh số!",
                $"{creatorName} đã tạo một phản ánh mới. Nhấn để xem chi tiết !",
                detailUrlApp,
                detailUrlWA,
                admin.ToArray(),
                messageDeclined,
                AppType.IOC
                );

        }
        #endregion
    }
}
