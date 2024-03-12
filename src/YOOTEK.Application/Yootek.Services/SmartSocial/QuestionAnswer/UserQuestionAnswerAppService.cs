using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;
using Yootek.Notifications;
using Abp;
using Yootek.Organizations;
using Abp.Runtime.Session;

namespace Yootek.Services
{
    public interface IUserQuestionAnswerAppService : IApplicationService { }
    public class UserQuestionAnswerAppService : YootekAppServiceBase, IUserQuestionAnswerAppService
    {
        private readonly IRepository<QuestionAnswer, long> _forumRepos;
        private readonly IRepository<QAComment, long> _forumCommentRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IAppNotifier _appNotifier;


        public UserQuestionAnswerAppService(
            IRepository<QuestionAnswer, long> forumRepos, 
            IRepository<QAComment, long> forumCommentRepos, 
            IRepository<User, long> userRepos,
            IAppNotifier appNotifier
            )
        {
            _forumRepos = forumRepos;
            _forumCommentRepos = forumCommentRepos;
            _userRepos = userRepos;
            _appNotifier = appNotifier;
        }

        protected IQueryable<QuestionAnswerDto> QueryDataQNA(GetAllQASocialInput input)
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

            var query = (from fr in _forumRepos.GetAll()
                         join us in _userRepos.GetAll() on fr.CreatorUserId equals us.Id into tb_us
                         from us in tb_us.DefaultIfEmpty()
                         select new QuestionAnswerDto
                         {
                             Id = fr.Id,
                             FileUrl = fr.FileUrl,
                             Type = fr.Type,
                             Content = fr.Content,
                             State = fr.State,
                             CreationTime = fr.CreationTime,
                             CreatorAvatar = us.ImageUrl,
                             CreatorName = us.FullName,
                             CreatorUserId = fr.CreatorUserId,
                             LastModificationTime = fr.LastModificationTime,
                             LastModifierUserId = fr.LastModifierUserId,
                             Tags = fr.Tags,
                             TenantId = fr.TenantId,
                             ThreadTitle = fr.ThreadTitle,
                             TypeTitle = fr.TypeTitle,
                             CommentCount = (from cm in _forumCommentRepos.GetAll()
                                             where cm.ForumId == fr.Id
                                             select cm).Count(),
                             OrganizationUnitId = fr.OrganizationUnitId,
                             IsAdminAnswered = ((from cm in _forumCommentRepos.GetAll() where (cm.ForumId == fr.Id && cm.IsAdmin.Value) select cm).Count()) > 0
                         })
                         //.WhereIf(input.Type.HasValue, u => u.Type == input.Type)
                         .WhereIf(input.State != null, x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT)
                         .WhereIf(input.Keyword != null, x => ((x.Content != null && x.Content.ToLower().Contains(input.Keyword))
                                                /*|| (x.CreatorName != null && x.CreatorName.ToLower().Contains(input.Keyword))*/
                                                || (x.ThreadTitle != null && x.ThreadTitle.ToLower().Contains(input.Keyword))
                                                ))
                         .WhereIf(input.CreatorUserId.HasValue, x => x.CreatorUserId == input.CreatorUserId.Value)
                         .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
                         .WhereIf(input.IsAdminAnswered.HasValue, x => x.IsAdminAnswered == input.IsAdminAnswered)
                         .WhereIf(input.FromDay.HasValue && !input.ToDay.HasValue, x => x.CreationTime.Date == fromDay.Date)
                         .WhereIf(input.FromDay.HasValue && input.ToDay.HasValue, x => x.CreationTime.Date >= fromDay.Date && x.CreationTime.Date <= toDay.Date)
                         .AsQueryable();
            #region Truy van tung Form
            /*switch (input.FormId.Value)
            {
                case (int)CommonENumForum.FORM_ID_FORUM.USER_GETALL:
                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT);
                    break;
                case (int)CommonENumForum.FORM_ID_FORUM.CREATOR_GETALL:
                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId);
                    break;
                default:
                    query = query.Take(0);
                    break;
            }*/
            if (input.QuestionAnswered.HasValue)
            {
                if (input.QuestionAnswered.Value) query = query.Where(x => x.CommentCount > 0);
                else query = query.Where(x => x.CommentCount == 0);
            }


            #endregion

            return query;
        }

        public async Task<object> GetQuestionById(long id)
        {
            try
            {
                var query = (from fr in _forumRepos.GetAll()
                    join us in _userRepos.GetAll() on fr.CreatorUserId equals us.Id into tb_us
                    from us in tb_us.DefaultIfEmpty()
                    select new QuestionAnswerDto
                    {
                        Id = fr.Id,
                        FileUrl = fr.FileUrl,
                        Type = fr.Type,
                        Content = fr.Content,
                        State = fr.State,
                        CreationTime = fr.CreationTime,
                        CreatorAvatar = us.ImageUrl,
                        CreatorName = us.FullName,
                        CreatorUserId = fr.CreatorUserId,
                        LastModificationTime = fr.LastModificationTime,
                        LastModifierUserId = fr.LastModifierUserId,
                        Tags = fr.Tags,
                        TenantId = fr.TenantId,
                        ThreadTitle = fr.ThreadTitle,
                        TypeTitle = fr.TypeTitle,
                        CommentCount = (from cm in _forumCommentRepos.GetAll()
                            where cm.ForumId == fr.Id
                            select cm).Count(),
                        OrganizationUnitId = fr.OrganizationUnitId,
                        IsAdminAnswered =
                            ((from cm in _forumCommentRepos.GetAll()
                                where (cm.ForumId == fr.Id && cm.IsAdmin.Value)
                                select cm).Count()) > 0
                    }).Where(x => x.Id == id).AsQueryable();
                return DataResult.ResultSuccess(query.FirstOrDefault(), "Success");
                
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> GetAllQuestionAnswerSocialAsync(GetAllQASocialInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var query = QueryDataQNA(input)
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByQuestionAnswer.CREATION_TIME, SortBy.DESC);
                    var list = await query.PageBy(input).ToListAsync();

                    var data = DataResult.ResultSuccess(list, "Get success", query.Count());
                    mb.statisticMetris(t1, 0, "gall_forum");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAnswersByQuestionAsync(GetCommentQAInput input)
        {
            try
            {

                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();

                    var query = (from cm in _forumCommentRepos.GetAll()
                                 where cm.ForumId == input.ForumId
                                 join us in _userRepos.GetAll() on cm.CreatorUserId equals us.Id into tb_us
                                 from us in tb_us.DefaultIfEmpty()
                                 select new CommentQuestionAnswerDto
                                 {
                                     FileUrl = cm.FileUrl,
                                     CreatorName = us.FullName,
                                     CreatorAvatar = us.ImageUrl,
                                     CreationTime = cm.CreationTime,
                                     Comment = cm.Comment,
                                     TenantId = cm.TenantId,
                                     Id = cm.Id,
                                     ForumId = cm.ForumId,
                                     CreatorUserId = cm.CreatorUserId,
                                     IsAdmin = cm.IsAdmin,
                                 })
                       .AsQueryable();
                    var list = await query.PageBy(input).ToListAsync();

                    var data = DataResult.ResultSuccess(list, "Get success", query.Count());
                    mb.statisticMetris(t1, 0, "gall_commentforum");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateQuestionAnswer(QuestionAnswerDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _forumRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _forumRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "update_forum");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<QuestionAnswer>();
                    insertInput.State = (int)CommonENumForum.FORUM_STATE.NEW;
                    long id = await _forumRepos.InsertAndGetIdAsync(insertInput);
                    var admins = await UserManager.GetUserOrganizationUnitByType(APP_ORGANIZATION_TYPE.VOTE);
                    var user = await UserManager.GetUserOrNullAsync(AbpSession.ToUserIdentifier());
                    await NotifierNewFaQ(insertInput, admins.ToArray(), user?.FullName ?? "Người dùng");
                    mb.statisticMetris(t1, 0, "insert_forum");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> DeleteQuestionAnswer(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _forumRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "del_forum");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateCommentQNA(CommentQuestionAnswerDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                input.CreatorUserId = AbpSession.UserId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _forumCommentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _forumCommentRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "update_forum");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<QAComment>();
                    long id = await _forumCommentRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "insert_forum");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> DeleteCommentQNA(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _forumCommentRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "del_comment_forum");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        private async Task NotifierNewFaQ(QuestionAnswer data, UserIdentifier[] admin, string creatorName)
        {
            var detailUrlApp = $"yooioc://app/faq/detail?id={data.Id}";
            var detailUrlWA = $"/faq?id={data.Id}";
            var message = new UserMessageNotificationDataBase(
                            AppNotificationAction.ReflectCitizenNew,
                            AppNotificationIcon.ReflectCitizenNewIcon,
                            TypeAction.Detail,
                            $"{creatorName} đã tạo một câu hỏi mới. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWA
                            );

            await _appNotifier.SendMessageNotificationInternalAsync(
                "Yoolife hỏi đáp số!",
                $"{creatorName} đã tạo một câu hỏi mới. Nhấn để xem chi tiết !",
                detailUrlApp,
                detailUrlWA,
                admin.ToArray(),
                message,
                AppType.IOC
                );

        }
    }
}
