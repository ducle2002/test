using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Application;
using IMAX.Authorization;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using IMAX.IMAX.EntityDb.Forum;
using IMAX.QueriesExtension;
using IMAX.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface IAdminForumAppService : IApplicationService { }
    public class AdminForumAppService : IMAXAppServiceBase, IAdminForumAppService
    {
        private readonly IRepository<Forum, long> _forumRepos;
        private readonly IRepository<ForumComment, long> _forumCommentRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<ForumTopic, long> _topicRepos;
        public AdminForumAppService(
            IRepository<Forum, long> forumRepos,
            IRepository<ForumComment, long> forumCommentRepos,
            IRepository<User, long> userRepos,
            IRepository<ForumTopic, long> topicRepos
            )
        {
            _forumCommentRepos = forumCommentRepos;
            _forumRepos = forumRepos;
            _userRepos = userRepos;
            _topicRepos = topicRepos;
        }
        protected IQueryable<ForumDto> QueryData(GetAllForumInput input)
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
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = (from fr in _forumRepos.GetAll()
                         join us in _userRepos.GetAll() on fr.CreatorUserId equals us.Id into tb_us
                         from us in tb_us.DefaultIfEmpty()
                         select new ForumDto
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
                             IsAdminAnswered = ((from cm in _forumCommentRepos.GetAll() where (cm.ForumId == fr.Id && cm.IsAdmin.Value) select cm).Count()) > 0,
                             TopicId = fr.TopicId
                         })
                        // .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                         .ApplySearchFilter(input.Keyword, x => x.ThreadTitle)
                         .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
                         .WhereIf(input.IsAdminAnswered.HasValue, x => x.IsAdminAnswered == input.IsAdminAnswered)
                         .WhereIf(input.FromDay.HasValue && !input.ToDay.HasValue, x => x.CreationTime.Date == fromDay.Date)
                         .WhereIf(input.FromDay.HasValue && input.ToDay.HasValue, x => x.CreationTime.Date >= fromDay.Date && x.CreationTime.Date <= toDay.Date)
                         .AsQueryable();
            #region Truy van tung Form
            switch (input.FormId.Value)
            {
                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_ACCEPT:
                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT);
                    break;
                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_NEW:
                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.NEW);
                    break;
                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL_DISABLE:
                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.DISABLE);
                    break;
                case (int)CommonENumForum.FORM_ID_FORUM.ADMIN_GETALL:
                    break;
                case (int)CommonENumForum.FORM_ID_FORUM.USER_GETALL:
                    query = query.Where(x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT);
                    break;
                case (int)CommonENumForum.FORM_ID_FORUM.CREATOR_GETALL:
                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId);
                    break;
                default:
                    query = query.Take(0);
                    break;
            }
            #endregion
            return query;
        }
        public async Task<object> GetAllForumSocialAsync(GetAllForumInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var query = QueryData(input);
                    var list = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByForum.CREATION_TIME, SortBy.DESC)
                        .PageBy(input).ToListAsync();

                    var data = DataResult.ResultSuccess(list, "Get success", query.Count());
                    mb.statisticMetris(t1, 0, "gall_forum");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> GetByIdAsync(long id)
        {
            try
            {
                var data = await _forumRepos.GetAsync(id);
                var data1 = data.MapTo<ForumDto>();
                data1.CommentCount = (from cm in _forumCommentRepos.GetAll()
                                      where cm.ForumId == data1.Id
                                      select cm).Count();
                return DataResult.ResultSuccess(data1, "Success!");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<object> GetAllCommentByForumAsync(GetAllCommentForumSocialInput input)
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
                                 select new CommentForumDto
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
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> GetForumById(GetAllForumInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var result = QueryData(input).FirstOrDefaultAsync().Result;

                    var data = DataResult.ResultSuccess(result, "Get success");
                    mb.statisticMetris(t1, 0, "gall_forum");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateComment(CommentForumDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _forumCommentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        updateData.IsAdmin = true;
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
                    var insertInput = input.MapTo<ForumComment>();
                    insertInput.IsAdmin = true;
                    long id = await _forumCommentRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "insert_forum");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }


    }
}
