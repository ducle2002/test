using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Common.Enum.PostEnums;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Services
{
    public interface IAdminPostAppService : IApplicationService
    {
        #region Post
        Task<object> CreateOrUpdatePostAsync(PostDto input);
        Task<object> DeletePost(long id);
        Task<object> GetAllPostAsync(PostInput input);
        #endregion

        #region CommentPost
        Task<object> CreateOrUpdateCommentPostAsync(CommentPostDto input);
        Task<object> DeleteCommentPost(long id);
        #endregion

        #region LikePostAndComment
        Task<object> CreateOrUpdateLikePostAsync(LikePostDto input);
        Task<object> DeleteLikePost(long id);
        #endregion
    }

    [AbpAuthorize]
    public class AdminPostAppService : YootekAppServiceBase, IAdminPostAppService
    {
        private readonly IRepository<Post, long> _postAdminRepos;
        private readonly IRepository<PostComment, long> _postCommentRepos;
        private readonly IRepository<LikePost, long> _likePostRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<ReportComment, long> _reportCommentRepos;
        private readonly IAppNotifier _appNotifier;

        public AdminPostAppService(
            IRepository<Post, long> postAdminRepos,
            IRepository<PostComment, long> postCommentRepos,
            IRepository<LikePost, long> likePostRepos,
            IRepository<User, long> userRepos,
            IRepository<ReportComment, long> reportCommentRepo,
            IAppNotifier appNotifier
            )
        {
            _postAdminRepos = postAdminRepos;
            _postCommentRepos = postCommentRepos;
            _likePostRepos = likePostRepos;
            _userRepos = userRepos;
            _reportCommentRepos = reportCommentRepo;
            _appNotifier = appNotifier;
        }



        #region Post

        public async Task<object> CreateOrUpdatePostAsync(PostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                input.Type = (int)POST_TYPE_ENUM.ADMIN;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _postAdminRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _postAdminRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_post");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    input.State = (int)CommonENumPost.STATE_POST.NEW;
                    //Insert
                    input.State = input.TenantId == null ? (int)CommonENumPost.STATE_POST.ACTIVE : (int)CommonENumPost.STATE_POST.NEW;
                    var insertInput = input.MapTo<Post>();
                    long id = await _postAdminRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_post");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> CreatePostAsync(PostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                input.Type = (int)POST_TYPE_ENUM.ADMIN;

                input.State = (int)CommonENumPost.STATE_POST.NEW;
                //Insert
                input.State = input.TenantId == null ? (int)CommonENumPost.STATE_POST.ACTIVE : (int)CommonENumPost.STATE_POST.NEW;
                var insertInput = input.MapTo<Post>();
                long id = await _postAdminRepos.InsertAndGetIdAsync(insertInput);

                mb.statisticMetris(t1, 0, "admin_is_post");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> UpdatePostAsync(PostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                input.Type = (int)POST_TYPE_ENUM.ADMIN;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _postAdminRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _postAdminRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_post");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    return "post không tồn tại";
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> DeletePost(long id)
        {

            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _postAdminRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_post");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllPostAsync(PostInput input)
        {

            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                DateTime fromDay = new DateTime(), toDay = new DateTime();
                if (input.FromDay.HasValue)
                {
                    fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

                }
                if (input.ToDay.HasValue)
                {
                    toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

                }

                IQueryable<PostDto> query = (from post in _postAdminRepos.GetAll()
                                             join user in _userRepos.GetAll() on post.CreatorUserId equals user.Id into tb_user
                                             from creator in tb_user.DefaultIfEmpty()
                                             select new PostDto()
                                             {
                                                 Id = post.Id,
                                                 CreationTime = post.CreationTime,
                                                 TenantId = post.TenantId,
                                                 ContentPost = post.ContentPost,
                                                 State = post.State,
                                                 ImageContent = post.ImageContent,
                                                 VideoContent = post.VideoContent,
                                                 CreatorUserId = post.CreatorUserId,
                                                 LastModificationTime = post.LastModificationTime,
                                                 LastModifierUserId = post.LastModifierUserId,
                                                 NameUserCreate = creator.UserName,
                                                 FullName = creator.FullName,
                                                 ImageAvatarUserCreate = creator.ImageUrl,
                                                 Status = post.Status,
                                                 CountComment = (from comment in _postCommentRepos.GetAll()
                                                                 where comment.PostId == post.Id
                                                                 select comment).AsQueryable().Count(),
                                                 CountLike = (from like in _likePostRepos.GetAll()
                                                              where like.PostId == post.Id
                                                              select like).AsQueryable().Count(),
                                                 Like = (from like in _likePostRepos.GetAll()
                                                         where like.PostId == post.Id && like.CreatorUserId == AbpSession.UserId
                                                         select like).FirstOrDefault(),
                                                 // Comments = (from comment in _postCommentRepos.GetAll()
                                                 //             join userCreate in _userRepos.GetAll() on comment.CreatorUserId equals userCreate.Id into tb_user
                                                 //             from userCreate in tb_user.DefaultIfEmpty()
                                                 //             where comment.PostId == post.Id
                                                 //             select new CommentPostDto()
                                                 //             {
                                                 //                 Id = comment.Id,
                                                 //                 CreationTime = comment.CreationTime,
                                                 //                 TenantId = comment.TenantId,
                                                 //                 Comment = comment.Comment,
                                                 //                 ParentCommentId = comment.ParentCommentId,
                                                 //                 CountLike = (from like in _likePostRepos.GetAll()
                                                 //                              where like.CommentId == comment.Id
                                                 //                              select like).AsQueryable().Count(),
                                                 //                 Like = (from like in _likePostRepos.GetAll()
                                                 //                         where like.CommentId == comment.Id && like.CreatorUserId == AbpSession.UserId
                                                 //                         select like).FirstOrDefault(),
                                                 //                 NameUserCreate = userCreate.UserName,
                                                 //                 ImageAvatarUserCreate = userCreate.ImageUrl,
                                                 //                 CreatorUserId = comment.CreatorUserId,
                                                 //                 FullName = userCreate.FullName,
                                                 //                 PostId = comment.PostId
                                                 //             }).ToList()
                                             })
                          // .Where(x => x.State != (int)UserFeedbackEnum.STATE_POST.ADMIN_DELETE)
                          .WhereIf(input.PostState.HasValue, u => (u.State == input.PostState))
                          .WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) || (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay))
                          .WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime <= toDay) || (!u.LastModificationTime.HasValue && u.CreationTime <= toDay))
                          .ApplySearchFilter(input.Keyword, x => x.ContentPost)
                          .ApplySort(input.OrderBy, input.SortBy)
                          .ApplySort(OrderByPost.CREATION_TIME, SortBy.DESC)
                          .AsQueryable();
                int count = query.Count();
                var result = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success", count);
                mb.statisticMetris(t1, 0, "gall_post");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> ChangeStatePost(StateDto input)
        {

            try
            {
                var updateData = await _postAdminRepos.GetAsync(input.id);
                if (updateData == null)
                {
                    return "Post không tồn tại";
                }
                updateData.State = input.state;
                //call back
                await _postAdminRepos.UpdateAsync(updateData);
                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        #endregion

        #region Comment
        public async Task<object> GetCommentPostAsync(long PostId, long? parentCommentId, PostInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = Enumerable.Empty<object>().AsQueryable();
                query = (from comment in _postCommentRepos.GetAll()
                         join userCreate in _userRepos.GetAll() on comment.CreatorUserId equals userCreate.Id into tb_user
                         from userCreate in tb_user.DefaultIfEmpty()
                         select new CommentPostDto()
                         {
                             Id = comment.Id,
                             CreationTime = comment.CreationTime,
                             Comment = comment.Comment,
                             FullName = userCreate.FullName,
                             PostId = comment.PostId,
                             ParentCommentId = comment.ParentCommentId,
                             ImageAvatarUserCreate = userCreate.ImageUrl,
                             CreatorUserId = userCreate.Id,
                             CountComments = (from c in _postCommentRepos.GetAll()
                                              where c.ParentCommentId == comment.Id
                                              select c).AsQueryable().Count(),
                             CountLike = (from like in _likePostRepos.GetAll()
                                          where like.CommentId == comment.Id
                                          select like).AsQueryable().Count(),
                             Like = (from like in _likePostRepos.GetAll()
                                     where like.CommentId == comment.Id && like.CreatorUserId == AbpSession.UserId
                                     select like).FirstOrDefault()
                         })
                         .Where(comment => comment.PostId == PostId && (comment.ParentCommentId == parentCommentId || (comment.ParentCommentId == 0 && parentCommentId == null)))
                          .OrderByDescending(x => x.CreationTime)
                          .AsQueryable();
                var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                var data = DataResult.ResultSuccess(result, "get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_comment");
                return data;

            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<object> CreateOrUpdateCommentPostAsync(CommentPostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var post = _postAdminRepos.FirstOrDefault(input.PostId ?? 0);

                if (post == null) { throw new Exception("Post not found !"); }

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _postCommentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _postCommentRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_post_cmt");
                    var data = DataResult.ResultSuccess(updateData, "Insert success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<PostComment>();

                    insertInput.Id = await _postCommentRepos.InsertAndGetIdAsync(insertInput);

                    var userReact = _userRepos.FirstOrDefault(AbpSession.UserId ?? 0);

                    await _appNotifier.SendCommentPostMessageNotifyAsync(post, insertInput, userReact);

                    mb.statisticMetris(t1, 0, "admin_is_post_cmt");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<object> CreateCommentPostAsync(CommentPostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var post = _postAdminRepos.FirstOrDefault(input.PostId ?? 0);

                if (post == null) { throw new Exception("Post not found !"); }

                input.TenantId = AbpSession.TenantId;


                //Insert
                var insertInput = input.MapTo<PostComment>();

                insertInput.Id = await _postCommentRepos.InsertAndGetIdAsync(insertInput);

                var userReact = _userRepos.FirstOrDefault(AbpSession.UserId ?? 0);

                await _appNotifier.SendCommentPostMessageNotifyAsync(post, insertInput, userReact);

                mb.statisticMetris(t1, 0, "admin_is_post_cmt");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;

            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<object> UpdateCommentPostAsync(CommentPostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var post = _postAdminRepos.FirstOrDefault(input.PostId ?? 0);

                if (post == null) { throw new Exception("Post not found !"); }

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _postCommentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _postCommentRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_post_cmt");
                    var data = DataResult.ResultSuccess(updateData, "Insert success !");
                    return data;
                }
                else
                {
                    return "comment không tồn tại";
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<object> DeleteCommentPost(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _postCommentRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_post_cmt");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> CreateOrUpdateReportCommentAsync(ReportCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var comment = _postCommentRepos.FirstOrDefault(input.CommentId);

                if (comment == null) { throw new Exception("Comment not found !"); }

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _reportCommentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _reportCommentRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_post_cmt");
                    var data = DataResult.ResultSuccess(updateData, "Insert success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<ReportComment>();

                    insertInput.State = (int)CommonENumPost.STATE_REPORT_COMMENT.NEW;

                    insertInput.Id = await _reportCommentRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_post_cmt");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<object> CreateReportCommentAsync(ReportCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var comment = _postCommentRepos.FirstOrDefault(input.CommentId);

                if (comment == null) { throw new Exception("Comment not found !"); }

                input.TenantId = AbpSession.TenantId;

                //Insert
                var insertInput = input.MapTo<ReportComment>();

                insertInput.State = (int)CommonENumPost.STATE_REPORT_COMMENT.NEW;

                insertInput.Id = await _reportCommentRepos.InsertAndGetIdAsync(insertInput);

                mb.statisticMetris(t1, 0, "admin_is_post_cmt");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> UpdateReportCommentAsync(ReportCommentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var comment = _postCommentRepos.FirstOrDefault(input.CommentId);

                if (comment == null) { throw new Exception("Comment not found !"); }

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _reportCommentRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _reportCommentRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_post_cmt");
                    var data = DataResult.ResultSuccess(updateData, "Insert success !");
                    return data;
                }
                else
                {
                    return "comment không tồn tại";
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<object> GetAllReportCommentAsync(ReportCommentInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = Enumerable.Empty<object>().AsQueryable();
                query = (from reportComment in _reportCommentRepos.GetAll()
                         where (reportComment.State != (int)CommonENumPost.STATE_REPORT_COMMENT.ADMIN_DELETE)
                         join comment in _postCommentRepos.GetAll() on reportComment.CommentId equals comment.Id
                         join userCreate in _userRepos.GetAll() on comment.CreatorUserId equals userCreate.Id into tb_user
                         from userCreate in tb_user.DefaultIfEmpty()
                         select new ReportCommentDto()
                         {
                             Id = reportComment.Id,
                             CreationTime = reportComment.CreationTime,
                             CommentId = reportComment.CommentId,
                             Comment = comment.Comment,
                             FullName = userCreate.FullName,
                             PostId = comment.PostId,
                             ParentCommentId = comment.ParentCommentId,
                             ImageAvatarUserCreate = userCreate.ImageUrl,
                             CreatorUserId = userCreate.Id,
                         })
                          .OrderByDescending(x => x.CreationTime)
                          .AsQueryable();
                var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                var data = DataResult.ResultSuccess(result, "get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_comment");
                return data;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> ChangeStateReportComment(StateDto input)
        {

            try
            {
                if (input.id < 1)
                {
                    return "Comment không tồn tại";
                }
                var updateData = await _reportCommentRepos.GetAsync(input.id);
                if (updateData == null)
                {
                    return "Comment không bị report";
                }
                updateData.State = input.state;
                //call back
                if (input.state == (int)CommonENumPost.STATE_REPORT_COMMENT.ADMIN_DELETE)
                {
                    //Xoa cmt
                    var originComment = await _postCommentRepos.GetAsync(updateData.CommentId);
                    if (originComment == null)
                    {
                        return "Comment không tồn tại";
                    }

                    await DeleteCommentPost(updateData.CommentId);
                }
                await _reportCommentRepos.UpdateAsync(updateData);
                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteReportComment(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _reportCommentRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_like_post");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

        #endregion

        #region LikePostAndComment
        public async Task<object> GetLikeAsync(long? PostId, long? CommentId, PostInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = Enumerable.Empty<object>().AsQueryable();
                query = (from like in _likePostRepos.GetAll()
                         join userCreate in _userRepos.GetAll() on like.CreatorUserId equals userCreate.Id into tb_user
                         from userCreate in tb_user.DefaultIfEmpty()
                         where (PostId != null && PostId == like.PostId) || (CommentId != null && like.CommentId == CommentId)
                         select new LikeDto()
                         {
                             Id = like.Id,
                             CreatorUserId = like.CreatorUserId,
                             StateLike = like.StateLike,
                             FullName = userCreate.FullName,
                             Avatar = userCreate.ImageUrl
                         }
                    );
                var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                var data = DataResult.ResultSuccess(result, "get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_comment");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> CreateOrUpdateLikePostAsync(LikePostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var post = _postAdminRepos.FirstOrDefault(input.PostId ?? 0);

                input.TenantId = AbpSession.TenantId;

                var test = AbpSession.UserId;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _likePostRepos.GetAsync(input.Id);

                    if (updateData.StateLike == input.StateLike)
                    {
                        await _likePostRepos.DeleteAsync(updateData.Id);

                        var data = DataResult.ResultSuccess("Delete Success");

                        mb.statisticMetris(t1, 0, "admin_del_like_post");

                        return data;
                    }
                    else
                    {
                        if (updateData != null)
                        {
                            input.MapTo(updateData);
                            //call back
                            await _likePostRepos.UpdateAsync(updateData);
                        }
                        mb.statisticMetris(t1, 0, "admin_ud_like_post");

                        var data = DataResult.ResultSuccess(updateData, "Update success !");

                        return data;
                    }
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<LikePost>();

                    insertInput.Id = await _likePostRepos.InsertAndGetIdAsync(insertInput);

                    var userReact = _userRepos.FirstOrDefault(AbpSession.UserId ?? 0);

                    await _appNotifier.SendLikePostMessageNotifyAsync(post, insertInput, userReact);

                    mb.statisticMetris(t1, 0, "admin_is_like_post");

                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");

                    return data;
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<object> DeleteLikePost(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _likePostRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_like_post");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }
        #endregion
    }
}
