

using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.RealTime;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Citizen_Reflect.ExportData;
using Yootek.Notifications;
using Yootek.Organizations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.UserFeedbackCommentEnum;
using static Yootek.Common.Enum.UserFeedbackEnum;

namespace Yootek.Services
{

    public interface IAdminGovernmentReflectAppService : IApplicationService
    {
        Task<object> GetAllGovernmentReflect(GetAllGovernmentReflectInput input);
        Task<object> UpdateStateGovernmentReflect(UpdateStateReflectGovernmentInput input);
        Task<DataResult> DeleteGovernmentReflect(long id);

    }

    public class AdminGovernmentReflectAppService : YootekAppServiceBase, IAdminGovernmentReflectAppService
    {
        private readonly IRepository<CitizenReflect, long> _citizenReflectRepos;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly INotificationCommunicator _notificationCommunicator;
        private readonly IRepository<CitizenReflectComment, long> _citizenReflectCommentRepos;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly IGovReflectExcelExporter _excelExporter;


        public AdminGovernmentReflectAppService(
            IRepository<CitizenReflect, long> citizenReflectRepos,
            IOnlineClientManager onlineClientManager,
            IRepository<CitizenReflectComment, long> citizenReflectCommentRepos,
            INotificationCommunicator notificationCommunicator,
            IRepository<UserOrganizationUnit, long> userOrganizationRepos,
            IRepository<User, long> userRepo,
            IAppNotifier appNotifier,
            IRepository<Citizen, long> citizenRepos,
            IRepository<AppOrganizationUnit, long> organizationRepos,
            IGovReflectExcelExporter excelExporter
            )
        {
            _citizenReflectRepos = citizenReflectRepos;
            _citizenReflectCommentRepos = citizenReflectCommentRepos;
            _onlineClientManager = onlineClientManager;
            _notificationCommunicator = notificationCommunicator;
            _userOrganizationRepos = userOrganizationRepos;
            _userRepos = userRepo;
            _appNotifier = appNotifier;
            _citizenRepos = citizenRepos;
            _organizationRepos = organizationRepos;
            _excelExporter = excelExporter;
        }

        protected IQueryable<CitizenReflectDto> QueryDataCitizenReflect()
        {
            return (from fb in _citizenReflectRepos.GetAll()
                    join us in _userRepos.GetAll() on fb.CreatorUserId equals us.Id into tb_us
                    from us in tb_us.DefaultIfEmpty()
                    join hd in _userRepos.GetAll() on fb.HandleUserId equals hd.Id into tb_hd
                    from hd in tb_hd.DefaultIfEmpty()
                    join og in _organizationRepos.GetAll() on fb.HandleOrganizationUnitId equals og.Id into tb_og
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
                        HandlerName = hd.FullName,
                        FinishTime = fb.FinishTime,
                        TenantId = fb.TenantId,
                        UserName = us != null ? us.UserName : null,
                        FullName = us != null ? us.FullName : null,
                        Address = !string.IsNullOrWhiteSpace(fb.AddressFeeder) ? fb.AddressFeeder : (us != null ? us.HomeAddress : null),
                        ImageUrl = us != null ? us.ImageUrl : null,
                        Phone = fb.CheckVerify != null && fb.CheckVerify.Value ? us.PhoneNumber ?? fb.Phone : fb.Phone,
                        NameFeeder = !string.IsNullOrWhiteSpace(fb.NameFeeder) ? fb.NameFeeder : (fb.CheckVerify != null && fb.CheckVerify.Value ? us.FullName ?? fb.NameFeeder : fb.NameFeeder),
                        HandleUserId = fb.HandleUserId,
                        HandleOrganizationUnitId = fb.HandleOrganizationUnitId,
                        ReflectReport = fb.ReflectReport,
                        ReportName = fb.ReportName,
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
                        UrbanId = fb.UrbanId,
                        BuildingId = fb.BuildingId,

                    });
        }
        protected IQueryable<CitizenReflectDto> QueryGetAllGovernmentReflect(GetAllGovernmentReflectInput input)
        {
            DateTime fromDay = new DateTime(), toDay = new DateTime();
            if (input.FromDay.HasValue)
            {
                fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

            }
            if (input.ToDay.HasValue)
            {
                toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

            }

            var query = QueryDataCitizenReflect()
                         .WhereIf(input.OrganizationUnitId.HasValue, x => (x.Type.HasValue && x.Type == input.OrganizationUnitId)
                         || (x.OrganizationUnitId.HasValue && x.OrganizationUnitId == input.OrganizationUnitId))
                         .WhereIf(input.FromDay.HasValue, x => (x.Type.HasValue && x.OrganizationUnitId.HasValue && x.CreationTime.Date >= fromDay))
                         .WhereIf(input.ToDay.HasValue, x => x.Type.HasValue && x.OrganizationUnitId.HasValue && x.CreationTime <= toDay)
                         .WhereIf(input.Keyword != null, x => x.Type.HasValue
                         && x.OrganizationUnitId.HasValue
                         && (x.Name.ToLower().Contains(input.Keyword.ToLower())
                         || (x.UserName != null && x.UserName.ToLower().Contains(input.Keyword.ToLower()))
                         || (x.Data.ToLower().Contains(input.Keyword.ToLower()))))
                         .AsQueryable();
            return query;
        }

        public async Task<object> GetAllGovernmentReflect(GetAllGovernmentReflectInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var query = QueryGetAllGovernmentReflect(input);

                var orIds = await _userOrganizationRepos.GetAll().Where(x => x.UserId == AbpSession.UserId).Select(x => x.OrganizationUnitId).ToListAsync();

                if (orIds != null)
                {
                    //query = query.Where(x => orIds.Contains(x.Type.Value));
                    query = query.Where(x => orIds.Contains(x.OrganizationUnitId.Value));
                }

                #region Truy van tung Form

                switch (input.FormId)
                {
                    //phản ánh mới
                    case ReflectFormId.NEW:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.PENDING);
                        break;
                    case ReflectFormId.ASSIGNED:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.ASSIGNED || x.State == (int)STATE_FEEDBACK.ASSIGNEDHANDLER);
                        break;
                    case ReflectFormId.HANDLING:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.HANDLING || x.State == (int)STATE_FEEDBACK.DECLINED);
                        break;
                    case ReflectFormId.ADMIN_CONFIRMED:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.USER_CONFIRMED || x.State == (int)STATE_FEEDBACK.ADMIN_CONFIRMED);
                        break;
                    case ReflectFormId.RATED:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.USER_RATE_FEEDBACK);
                        break;
                    case ReflectFormId.GETALL:
                        break;
                    default:
                        query = null;
                        break;
                }

                #endregion

                if (query != null)
                {
                    //   query = query.OrderByDescending(u => u.LastModificationTime.HasValue ? u.LastModificationTime : u.CreationTime);

                    var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

                    var totalRecs = query.Count();

                    mb.statisticMetris(t1, 0, "get_government_reflect");

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

        public async Task<DataResult> GetGovernmentReflectById(long id)
        {
            try
            {
                var result = await QueryDataCitizenReflect().Where(x => x.Id == id).FirstOrDefaultAsync();

                return DataResult.ResultSuccess(result, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> AssignGovernmentReflect(AssignReflectInput input)
        {
            try
            {
                var reflect = _citizenReflectRepos.FirstOrDefault(input.Id);
                if (reflect == null) throw new UserFriendlyException("Reflect not found !");
                reflect.HandleOrganizationUnitId = input.HandleOrganizationUnitId;
                reflect.HandleUserId = input.HandleUserId;
                reflect.State = (int)STATE_FEEDBACK.ASSIGNED;
                // if(input.HandleUserId > 0) reflect.State = (int)STATE_FEEDBACK.ASSIGNEDHANDLER;
                await _citizenReflectRepos.UpdateAsync(reflect);
                return DataResult.ResultSuccess("Update success!");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateGovernmentReflect(CitizenReflectDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateInput = await _citizenReflectRepos.GetAsync(input.Id);
                    input.MapTo(updateInput);
                    await _citizenReflectRepos.UpdateAsync(updateInput);
                    mb.statisticMetris(t1, 0, "Ud_reflect");

                    var data = DataResult.ResultSuccess(updateInput, "Update success !");
                    return data;
                }
                else
                {
                    input.State = (int)STATE_FEEDBACK.ASSIGNED;
                    input.OrganizationUnitId = input.HandleOrganizationUnitId;
                    var insertData = input.MapTo<CitizenReflect>();
                    var id = await _citizenReflectRepos.InsertAndGetIdAsync(insertData);
                    insertData.Id = id;
                    mb.statisticMetris(t1, 0, "Insert_reflect");
                    var data = DataResult.ResultSuccess(insertData, "Insert success!");
                    return data;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> UpdateStateGovernmentReflect(UpdateStateReflectGovernmentInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var updateData = await _citizenReflectRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    updateData.State = input.State;
                    //call back
                    await _citizenReflectRepos.UpdateAsync(updateData);
                    var feedbackComment = new CitizenReflectComment();
                    feedbackComment.FeedbackId = updateData.Id;
                    feedbackComment.TenantId = AbpSession.TenantId;
                    switch (input.State)
                    {
                        case (int)STATE_FEEDBACK.DECLINED:
                            feedbackComment.TypeComment = TYPE_COMMENT_FEEDBACK.STATE_DECLINED;
                            feedbackComment.Comment = input.Note;
                            feedbackComment.FileUrl = input.FileOfNote;
                            break;
                        case (int)STATE_FEEDBACK.HANDLING:
                            feedbackComment.TypeComment = TYPE_COMMENT_FEEDBACK.STATE_HANDLING;
                            break;
                        case (int)STATE_FEEDBACK.PENDING:
                            feedbackComment.TypeComment = TYPE_COMMENT_FEEDBACK.STATE_PENDING;
                            break;
                        case (int)STATE_FEEDBACK.USER_CONFIRMED:
                            feedbackComment.TypeComment = TYPE_COMMENT_FEEDBACK.STATE_USER_CONFIRMED;
                            break;
                        case (int)STATE_FEEDBACK.USER_RATE_FEEDBACK:
                            feedbackComment.TypeComment = TYPE_COMMENT_FEEDBACK.STATE_USER_RATE_FEEDBACK;
                            break;
                        case (int)STATE_FEEDBACK.ADMIN_CONFIRMED:
                            feedbackComment.TypeComment = TYPE_COMMENT_FEEDBACK.STATE_ADMIN_CONFIRMED;
                            break;
                    }

                    feedbackComment.ReadState = 1;
                    feedbackComment.OrganizationUnitId = updateData.OrganizationUnitId;
                    await _citizenReflectCommentRepos.InsertAndGetIdAsync(feedbackComment);
                    var user = new UserIdentifier(updateData.TenantId, updateData.CreatorUserId.Value);
                    var clients = await _onlineClientManager.GetAllByUserIdAsync(user);
                    await NotifierStateCitizenReflect(updateData);
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

        public async Task<object> UpdateHandleReportReflect(UpdateHandleReportnput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var updateData = await _citizenReflectRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    updateData.ReportName = input.ReportName;
                    updateData.ReflectReport = input.ReflectReport;
                    //updateData.State = (int)STATE_FEEDBACK.UPLOADREPORT;
                    //call back
                    await _citizenReflectRepos.UpdateAsync(updateData);

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

        public async Task<DataResult> DeleteGovernmentReflect(long id)
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

        public async Task<object> DeleteMultiple([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = DeleteGovernmentReflect(id);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<ListResultDto<CitizenReflectCommentDto>> GetAllCommnetByCitizenReflect(GetCommentCitizenReflectInput input)
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
                Logger.Fatal(e.Message);
                return null;
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


        private async Task NotifierStateCitizenReflect(CitizenReflect reflect)
        {
            var detailUrlApp = $"yoolife://app/feedback/detail?id={reflect.Id}";
            
            switch (reflect.State)
            {
                case (int?)STATE_FEEDBACK.DECLINED:
                    var detailUrlWADeclined = $"/feedbacks?formId=13&id={reflect.Id}";
                    var messageDeclined = new UserMessageNotificationDataBase(
                            AppNotificationAction.StateReflectCitizen,
                            AppNotificationIcon.StateReflectCitizenDenied,
                            TypeAction.Detail,
                            $"Phản ánh {reflect.Name} của bạn đã bị từ chối. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWADeclined
                            );
                    await _appNotifier.SendUserMessageNotifyFullyAsync(
                        "Yoolife phản ánh số !",
                        $"Phản ánh {reflect.Name} của bạn đã bị từ chối. Nhấn để xem chi tiết !",
                        detailUrlApp,
                        detailUrlWADeclined,
                        new UserIdentifier[] { new UserIdentifier(reflect.TenantId, reflect.CreatorUserId.HasValue ? reflect.CreatorUserId.Value : 0) },
                        messageDeclined);
                    break;

                case (int?)STATE_FEEDBACK.HANDLING:
                    var detailUrlWAHandling = $"/feedbacks?formId=12&id={reflect.Id}";
                    var messageHandling = new UserMessageNotificationDataBase(
                           AppNotificationAction.StateReflectCitizen,
                           AppNotificationIcon.StateReflectCitizenHandling,
                           TypeAction.Detail,
                           $"Phản ánh {reflect.Name} của bạn đã được tiếp nhận !",
                           detailUrlApp,
                           detailUrlWAHandling
                           );
                    await _appNotifier.SendUserMessageNotifyFullyAsync(
                        "Yoolife phản ánh số !",
                        $"Phản ánh {reflect.Name} của bạn đã được tiếp nhận !",
                        detailUrlApp,
                        detailUrlWAHandling,
                        new UserIdentifier[] { new UserIdentifier(reflect.TenantId, reflect.CreatorUserId.HasValue ? reflect.CreatorUserId.Value : 0) },
                        messageHandling);
                    break;

                case (int?)STATE_FEEDBACK.ADMIN_CONFIRMED:
                    var detailUrlConfirm = $"/feedbacks?formId=14&id={reflect.Id}";
                    var messageConfirm = new UserMessageNotificationDataBase(
                          AppNotificationAction.StateReflectCitizen,
                          AppNotificationIcon.StateReflectCitizenConfirmed,
                          TypeAction.Detail,
                          $"Phản ánh {reflect.Name} của bạn đã được hoàn thành !",
                          detailUrlApp,
                          detailUrlConfirm
                          );
                    await _appNotifier.SendUserMessageNotifyFullyAsync(
                         "Yoolife phản ánh số !",
                        $"Phản ánh {reflect.Name} của bạn đã được hoàn thành !",
                        detailUrlApp,
                        detailUrlConfirm,
                        new UserIdentifier[] { new UserIdentifier(reflect.TenantId, reflect.CreatorUserId.HasValue ? reflect.CreatorUserId.Value : 0) },
                        messageConfirm);
                    break;
            }

        }
        public async Task<object> ExportGovernmentReflectToExcel(ExportGovReflectInputDto input)
        {
            try
            {
                var query = QueryDataCitizenReflect()
                    .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id));
                var orIds = await _userOrganizationRepos.GetAll()
                    .Where(x => x.UserId == AbpSession.UserId)
                    .Select(x => x.OrganizationUnitId).ToListAsync();

                if (orIds != null)
                {
                    //query = query.Where(x => orIds.Contains(x.Type.Value));
                    query = query.Where(x => x.OrganizationUnitId.HasValue).Where(x => orIds.Contains(x.OrganizationUnitId.Value));
                }

                #region Truy van tung Form

                switch (input.FormId)
                {
                    //phản ánh mới
                    case ReflectFormId.NEW:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.PENDING);
                        break;
                    case ReflectFormId.ASSIGNED:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.ASSIGNED || x.State == (int)STATE_FEEDBACK.ASSIGNEDHANDLER);
                        break;
                    case ReflectFormId.HANDLING:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.HANDLING || x.State == (int)STATE_FEEDBACK.DECLINED);
                        break;
                    case ReflectFormId.ADMIN_CONFIRMED:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.USER_CONFIRMED || x.State == (int)STATE_FEEDBACK.ADMIN_CONFIRMED);
                        break;
                    case ReflectFormId.RATED:
                        query = query.Where(x => x.State == (int)STATE_FEEDBACK.USER_RATE_FEEDBACK);
                        break;
                    case ReflectFormId.GETALL:
                        break;
                    default:
                        query = null;
                        break;
                }

                #endregion

                var result = query.ToList();

                var totalRecs = query.Count();

                var res = _excelExporter.ExportToExcel(result);

                return DataResult.ResultSuccess(res, "Get success!");


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
