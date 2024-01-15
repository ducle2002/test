using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Forum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yootek.Yootek.Services.Yootek.Clb.Clb.Forum.Dto;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Services
{
    public interface IAdminForumAppService : IApplicationService
    {
    }

    public class AdminForumAppService : YootekAppServiceBase, IAdminForumAppService
    {
        private readonly IRepository<ForumPost, long> _forumRepos;
        private readonly IRepository<ForumComment, long> _forumCommentRepos;
        private readonly IRepository<Member, long> _memberRepos;
        private readonly IRepository<ForumTopic, long> _topicRepos;
        private readonly IRepository<Tag, long> _tagRepos;
        private readonly IRepository<ForumPostReaction, long> _reactionRepos;

        public AdminForumAppService(
            IRepository<ForumPost, long> forumRepos,
            IRepository<ForumComment, long> forumCommentRepos,
            IRepository<ForumTopic, long> topicRepos,
            IRepository<Tag, long> tagRepos,
            IRepository<Member, long> memberRepos, IRepository<ForumPostReaction, long> reactionRepos)
        {
            _forumCommentRepos = forumCommentRepos;
            _forumRepos = forumRepos;
            _topicRepos = topicRepos;
            _tagRepos = tagRepos;
            _memberRepos = memberRepos;
            _reactionRepos = reactionRepos;
        }

        protected IQueryable<ForumPostDto> QueryData(GetAllForumInput input)
        {
            DateTime fromDay = new DateTime(), toDay = new DateTime();
            if (input.FromDay.HasValue)
            {
                fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0,
                    0, 0);
            }

            if (input.ToDay.HasValue)
            {
                toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59,
                    59);
            }

            var query = (from fr in _forumRepos.GetAll()
                    join us in _memberRepos.GetAll() on fr.CreatorUserId equals us.AccountId into tbUs
                    from us in tbUs.DefaultIfEmpty()
                    select new ForumPostDto
                    {
                        Id = fr.Id,
                        ImageUrls = fr.ImageUrls,
                        LinkUrls = fr.LinkUrls,
                        Content = fr.Content,
                        State = fr.State,
                        CreationTime = fr.CreationTime,
                        CreatorAvatar = us.ImageUrl,
                        CreatorName = us.FullName,
                        CreatorUserId = fr.CreatorUserId,
                        LastModificationTime = fr.LastModificationTime,
                        LastModifierUserId = fr.LastModifierUserId,
                        TenantId = fr.TenantId,
                        ThreadTitle = fr.ThreadTitle,
                        CommentCount = (from cm in _forumCommentRepos.GetAll()
                            where cm.ForumPostId == fr.Id
                            select cm).Count(),
                        LikeCount = (from lk in _reactionRepos.GetAll()
                            where lk.PostId == fr.Id && lk.Type == EForumReactionType.Like
                            select lk).Count(),
                        DislikeCount = (from dl in _reactionRepos.GetAll()
                            where dl.PostId == fr.Id && dl.Type == EForumReactionType.Dislike
                            select dl).Count(),
                        ReactionType = (from rt in _reactionRepos.GetAll()
                            where rt.PostId == fr.Id && rt.CreatorUserId == AbpSession.UserId
                            select rt.Type).FirstOrDefault(),
                        Code = fr.Code,
                        TopicId = fr.TopicId,
                        Type = fr.Type,
                        GroupId = fr.GroupId,
                    })
                // .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                .ApplySearchFilter(input.Keyword, x => x.ThreadTitle)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
                .WhereIf(input.TopicId.HasValue, x => x.TopicId == input.TopicId.Value)
                .WhereIf(input.FromDay.HasValue && !input.ToDay.HasValue,
                    x => x.CreationTime.Date == fromDay.Date)
                .WhereIf(input.FromDay.HasValue && input.ToDay.HasValue,
                    x => x.CreationTime.Date >= fromDay.Date && x.CreationTime.Date <= toDay.Date)
                .WhereIf(input.GroupId.HasValue, x => x.GroupId == input.GroupId.Value)
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type.Value)
                .WhereIf(input.UserId.HasValue, x => x.CreatorUserId == input.UserId.Value)
                .AsQueryable();

            #region Truy van tung Form

            if (input.FormId == null) return query;
            {
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
            }

            #endregion

            return query;
        }

        public async Task<object> GetAllForumPostSocialAsync(GetAllForumInput input)
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
                throw;
            }
        }

        public async Task<object> GetAllTopics(GetAllTopic input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var t1 = TimeUtils.GetNanoseconds();
                    var query = (from tp in _topicRepos.GetAll()
                        select new ForumTopicDto
                        {
                            Id = tp.Id,
                            Name = tp.Name,
                            ImageUrl = tp.ImageUrl,
                            Description = tp.Description,
                            PostCount = (from fr in _forumRepos.GetAll()
                                where fr.TopicId == tp.Id
                                select fr).Count(),
                            TenantId = tp.TenantId,
                        }).ApplySearchFilter(input.Keyword, x=>x.Name);

                    var count = query.Count();
                    var list = await query
                        .PageBy(input)
                        .ToListAsync();

                    var data = DataResult.ResultSuccess(list, "Get success", count);
                    mb.statisticMetris(t1, 0, "gall_topics");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> GetAllTags()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var query = _tagRepos.GetAll();
                    var count = query.Count();
                    var list = await query.ToListAsync();

                    var data = DataResult.ResultSuccess(list, "Get success", count);
                    mb.statisticMetris(t1, 0, "gall_tags");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> CreateOrUpdateTopicAsync(ForumTopicDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var insertTopic = ObjectMapper.Map<ForumTopic>(input);
                var id = await _topicRepos.InsertAndGetIdAsync(insertTopic);
                insertTopic.Id = id;
                
                mb.statisticMetris(t1, 0, "insert_topic");
                var result = DataResult.ResultSuccess(insertTopic, "Insert success!");
                return result;
            }
            catch (Exception e)
            {
                return new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> UpdateTopicAsync(UpdateForumTopicDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var updateTopic = await _topicRepos.GetAsync(input.Id) ??
                                  throw new UserFriendlyException("Not found!");
            
                ObjectMapper.Map(input, updateTopic);
                await _topicRepos.UpdateAsync(updateTopic);

                mb.statisticMetris(t1, 0, "update_topic");
                var result = DataResult.ResultSuccess(updateTopic, "Update success!");
                return result;
            }
            catch (Exception e)
            {
                return new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> CreateTagsAsync(CreateTagDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var tag = input.MapTo<Tag>();
                var id = await _tagRepos.InsertAndGetIdAsync(tag);
                mb.statisticMetris(t1, 0, "insert_tag");
                var result = DataResult.ResultSuccess(tag, "Insert success!");
                return result;
            }
            catch (Exception e)
            {
                return new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> UpdateTagsAsync(UpdateTagDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var tag = await _tagRepos.GetAsync(input.Id);
                if (tag != null)
                {
                    input.MapTo(tag);
                    await _tagRepos.UpdateAsync(tag);
                }

                mb.statisticMetris(t1, 0, "update_tag");
                var result = DataResult.ResultSuccess(tag, "Update success!");
                return result;
            }
            catch (Exception e)
            {
                return new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteTagsAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _tagRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "del_tag");
                return data;
            }
            catch (Exception e)
            {
                return new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteTopicAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _topicRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "del_topic");
                return data;
            }
            catch (Exception e)
            {
                return new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> CloseThreadAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var forum = await _forumRepos.GetAsync(id);
                if (forum != null)
                {
                    forum.State = (int)CommonENumForum.FORUM_STATE.CLOSE;
                    await _forumRepos.UpdateAsync(forum);
                }

                mb.statisticMetris(t1, 0, "close_thread");
                var data = DataResult.ResultSuccess("Close Success");
                return data;
            }
            catch (Exception e)
            {
                return new UserFriendlyException(e.Message);
            }
        }
    }
}