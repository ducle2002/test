using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Application;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using IMAX.Services.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface IUserForumAppService : IApplicationService { }
    public class UserForumAppService : IMAXAppServiceBase, IUserForumAppService
    {
        private readonly IRepository<Forum, long> _forumepos;
        private readonly IRepository<ForumComment, long> _forumCommentRepos;
        private readonly IRepository<User, long> _userRepos;
        public UserForumAppService(IRepository<Forum, long> forumepos, IRepository<ForumComment, long> forumCommentRepos, IRepository<User, long> userRepos)
        {
            _forumepos = forumepos;
            _forumCommentRepos = forumCommentRepos;
            _userRepos = userRepos;
        }

        protected IQueryable<ForumDto> QueryDataForum(GetAllForumInput input)
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

            var query = (from fr in _forumepos.GetAll()
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
                             IsAdminAnswered = ((from cm in _forumCommentRepos.GetAll() where (cm.ForumId == fr.Id && cm.IsAdmin.Value) select cm).Count()) > 0
                         })
                         //.WhereIf(input.Type.HasValue, u => u.Type == input.Type)
                         .WhereIf(input.State != null, x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT)
                         .WhereIf(input.Keyword != null, x => ((x.Content != null && x.Content.ToLower().Contains(input.Keyword))
                                                /*|| (x.CreatorName != null && x.CreatorName.ToLower().Contains(input.Keyword))*/
                                                || (x.ThreadTitle != null && x.ThreadTitle.ToLower().Contains(input.Keyword))
                                                ))
                         .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
                         .WhereIf(input.IsAdminAnswered.HasValue, x => x.IsAdminAnswered == input.IsAdminAnswered)
                         .WhereIf(input.FromDay.HasValue && !input.ToDay.HasValue, x => x.CreationTime.Date == fromDay.Date)
                         .WhereIf(input.FromDay.HasValue && input.ToDay.HasValue, x => x.CreationTime.Date >= fromDay.Date && x.CreationTime.Date <= toDay.Date)
                         .AsQueryable();
            #region Truy van tung Form

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
                    var query = QueryDataForum(input)
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByForum.CREATION_TIME, SortBy.DESC);
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
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> GetCommentAsync(GetAllCommentForumSocialInput input)
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

        [Obsolete]
        public async Task<object> CreateOrUpdateForum(ForumDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _forumepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _forumepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "update_forum");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Forum>();
                    insertInput.State = (int)CommonENumForum.FORUM_STATE.NEW;
                    long id = await _forumepos.InsertAndGetIdAsync(insertInput);

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

        public async Task<object> DeleteForum(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _forumepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "del_forum");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        [Obsolete]
        public async Task<object> CreateOrUpdateComment(CreateOrUpdateCommentForumDto input)
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

        public async Task<object> DeleteComment(long id)
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
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
