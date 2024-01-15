using Abp.Application.Services;
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
using System.Threading.Tasks;
using Yootek.Yootek.EntityDb.Forum;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Services
{
    public interface IUserForumAppService : IApplicationService
    {
    }

    public class UserForumAppService : YootekAppServiceBase, IUserForumAppService
    {
        private readonly IRepository<ForumPost, long> _forumRepos;
        private readonly IRepository<ForumComment, long> _forumCommentRepos;
        private readonly IRepository<ForumPostReaction, long> _reactionRepos;
        private readonly IRepository<Member, long> _memberRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<ForumTopic, long> _topicRepos;
        
        public UserForumAppService(IRepository<ForumPost, long> forumRepos,
            IRepository<ForumComment, long> forumCommentRepos, IRepository<User, long> userRepos, IRepository<Member, long> memberRepos, IRepository<ForumPostReaction, long> reactionRepos, IRepository<ForumTopic, long> topicRepos)
        {
            _forumRepos = forumRepos;
            _forumCommentRepos = forumCommentRepos;
            _userRepos = userRepos;
            _memberRepos = memberRepos;
            _reactionRepos = reactionRepos;
            _topicRepos = topicRepos;
        }

        protected IQueryable<ForumPostDto> QueryDataForum(GetAllForumInput input)
        {
            DateTime fromDay = new(), toDay = new();
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
                    join us in _memberRepos.GetAll() on fr.CreatorUserId equals us.AccountId into tb_us
                    from us in tb_us.DefaultIfEmpty()
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
                        LikeCount = (from react in _reactionRepos.GetAll()
                            where react.PostId == fr.Id && react.Type == EForumReactionType.Like
                            select react).Count(),
                        DislikeCount = (from react in _reactionRepos.GetAll()
                            where react.PostId == fr.Id && react.Type == EForumReactionType.Dislike
                            select react).Count(),
                        ReactionType = (from react in _reactionRepos.GetAll()
                            where react.PostId == fr.Id && react.CreatorUserId == AbpSession.UserId
                            select react.Type).FirstOrDefault(),
                        Code = fr.Code,
                        TopicId = fr.TopicId,
                        Type = fr.Type,
                        GroupId = fr.GroupId,
                    })
                //.WhereIf(input.Type.HasValue, u => u.Type == input.Type)
                .ApplySearchFilter(input.Keyword, x => x.ThreadTitle)
                .WhereIf(input.TopicId.HasValue, x => x.TopicId == input.TopicId.Value)
                .WhereIf(input.State != null, x => x.State == (int)CommonENumForum.FORUM_STATE.ACCEPT || x.State == (int)CommonENumForum.FORUM_STATE.CLOSE)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id.Value)
                .WhereIf(input.FromDay.HasValue && !input.ToDay.HasValue, x => x.CreationTime.Date == fromDay.Date)
                .WhereIf(input.FromDay.HasValue && input.ToDay.HasValue,
                    x => x.CreationTime.Date >= fromDay.Date && x.CreationTime.Date <= toDay.Date)
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type.Value)
                .WhereIf(input.GroupId.HasValue, x => x.GroupId == input.GroupId.Value)
                .WhereIf(input.UserId.HasValue, x => x.CreatorUserId == input.UserId.Value)
                .AsQueryable();

            #region Truy van tung Form

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

        public async Task<object> GetCommentAsync(GetAllCommentForumSocialInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();

                    var query = (from cm in _forumCommentRepos.GetAll()
                            join us in _userRepos.GetAll() on cm.CreatorUserId equals us.Id into tb_us
                            from us in tb_us.DefaultIfEmpty()
                            select new CommentForumDto
                            {
                                FileUrls = cm.FileUrls,
                                CreatorName = us.FullName,
                                CreatorAvatar = us.ImageUrl,
                                CreationTime = cm.CreationTime,
                                Comment = cm.Comment,
                                TenantId = cm.TenantId,
                                Id = cm.Id,
                                ForumPostId = cm.ForumPostId,
                                CreatorUserId = cm.CreatorUserId,
                                LinkUrls = cm.LinkUrls,
                                ReplyId = cm.ReplyId,
                                ReplyTo =  (from cm2 in _forumCommentRepos.GetAll()
                                    join us2 in _userRepos.GetAll() on cm2.CreatorUserId equals us2.Id into tb_us2
                                    from us2 in tb_us2.DefaultIfEmpty()
                                    where cm2.Id == cm.ReplyId
                                    select new CommentForumDto
                                    {
                                        FileUrls = cm2.FileUrls,
                                        CreatorName = us2.FullName,
                                        CreatorAvatar = us2.ImageUrl,
                                        CreationTime = cm2.CreationTime,
                                        Comment = cm2.Comment,
                                        TenantId = cm2.TenantId,
                                        Id = cm2.Id,
                                        ForumPostId = cm2.ForumPostId,
                                        CreatorUserId = cm2.CreatorUserId,
                                        LinkUrls = cm2.LinkUrls,
                                        ReplyId = cm2.ReplyId
                                    }).FirstOrDefault(),
                            })
                        .Where(x => x.ForumPostId == input.ForumPostId)
                        .WhereIf(input.ReplyId.HasValue, x => x.Id == input.ReplyId.Value);
                    var list = await query
                        .Skip(input.SkipCount)
                        .Take(input.MaxResultCount)
                        .ToListAsync();

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

        public async Task<object> CreateForumPost(CreateForumPostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                //Insert
                var insertInput = ObjectMapper.Map<ForumPost>(input);
                insertInput.Code = Guid.NewGuid();
                await _forumRepos.InsertAsync(insertInput);

                mb.statisticMetris(t1, 0, "insert_forum");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> UpdateForumPost(UpdateForumPostDto input)
        {
            long t1 = TimeUtils.GetNanoseconds();
            input.TenantId = AbpSession.TenantId;

            var updateData = await _forumRepos.GetAsync(input.Id);
            if (updateData == null)
            {
                throw new UserFriendlyException("Not found post");
            }

            ObjectMapper.Map(input, updateData);
            if (input.LinkUrls != null && input.LinkUrls.Count > 0)
            {
                updateData.LinkUrls = input.LinkUrls.ToArray();
            }

            if (input.ImageUrls != null && input.ImageUrls.Count > 0)
            {
                updateData.ImageUrls = input.ImageUrls.ToArray();
            }

            //call back
            await _forumRepos.UpdateAsync(updateData);
            mb.statisticMetris(t1, 0, "update_forum");

            var data = DataResult.ResultSuccess(updateData, "Update success !");
            return data;
        }

        public async Task<object> DeleteForum(long id)
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

        public async Task<object> CreateComment(CreateForumCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                //Insert
                var insertInput = ObjectMapper.Map<ForumComment>(input);
                await _forumCommentRepos.InsertAsync(insertInput);

                mb.statisticMetris(t1, 0, "insert_forum");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> UpdateComment(UpdateForumCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = await _forumCommentRepos.GetAsync(input.Id);
                if (updateData == null)
                {
                    throw new UserFriendlyException("Not found post");
                }

                ObjectMapper.Map(input, updateData);
                if (input.LinkUrls != null && input.LinkUrls.Count > 0)
                {
                    updateData.LinkUrls = input.LinkUrls;
                }

                if (input.FileUrls != null && input.FileUrls.Count > 0)
                {
                    updateData.FileUrls = input.FileUrls;
                }

                //call back
                await _forumCommentRepos.UpdateAsync(updateData);
                mb.statisticMetris(t1, 0, "update_comment_forum");

                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception ex)
            {
                DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
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
                throw;
            }
        }
    }
}