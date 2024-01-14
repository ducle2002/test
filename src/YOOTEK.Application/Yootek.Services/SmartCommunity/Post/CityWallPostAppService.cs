

using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Common.Enum.PostEnums;
using Yootek.EntityDb;
using Yootek.Friendships;
using Yootek.Notifications;
using Yootek.Services.Dto;
using Yootek.Users.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Yootek.Services
{
    public interface ICityWallPostAppService : IApplicationService
    {
        #region Post
        Task<object> CreateOrUpdatePostAsync(PostDto input);
        Task<object> DeletePost(long id);
        Task<object> GetAllPostAsync(PostInput input);
        #endregion

        #region CommentPost
        Task<object> CreateOrUpdateCommentPostAsync(CommentPostDto input);
        Task<object> DeleteCommentPost(long id);
        Task<object> GetCommentPostAsync(long PostId, long? parentCommentId, PostInput input);
        #endregion

        #region LikePostAndComment
        Task<object> GetLikeAsync(long? PostId, long? CommentId, PostInput input);
        Task<object> CreateOrUpdateLikePostAsync(LikePostDto input);
        Task<object> DeleteLikePost(long id);
        #endregion

        #region User
        Task<object> GetUserInformation(long Id);
        #endregion
    }

    [AbpAuthorize]
    public class CityWallPostAppService : YootekAppServiceBase, ICityWallPostAppService
    {
        private readonly IRepository<Post, long> _postAdminRepos;
        private readonly IRepository<PostComment, long> _postCommentRepos;
        private readonly IRepository<LikePost, long> _likePostRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<Friendship, long> _friendShipRepos;
        private readonly IRepository<HomeMember, long> _homeMemberRepos;
        private readonly IRepository<ReportComment, long> _reportCommentRepos;
        private readonly IAppNotifier _appNotifier;

        public CityWallPostAppService(
            IRepository<Post, long> postAdminRepos,
            IRepository<PostComment, long> postCommentRepos,
            IRepository<LikePost, long> likePostRepos,
            IRepository<Friendship, long> friendShipRepos,
            IRepository<HomeMember, long> homeMemberRepos,
            IRepository<User, long> userRepos,
            IRepository<ReportComment, long> reportCommentRepo,
            IAppNotifier appNotifier)

        {
            _postAdminRepos = postAdminRepos;
            _postCommentRepos = postCommentRepos;
            _likePostRepos = likePostRepos;
            _userRepos = userRepos;
            _friendShipRepos = friendShipRepos;
            _homeMemberRepos = homeMemberRepos;
            _reportCommentRepos = reportCommentRepo;
            _appNotifier = appNotifier;
        }

        #region User
        public async Task<object> GetUserInformation(long Id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = Enumerable.Empty<object>().AsQueryable();

                query = (from user in _userRepos.GetAll()
                         where user.Id == Id
                         select new UserDto()
                         {
                             UserName = user.UserName,
                             Name = user.Name,
                             Surname = user.Surname,
                             EmailAddress = user.EmailAddress,
                             PhoneNumber = user.PhoneNumber,
                             HomeAddress = user.HomeAddress,
                             AddressOfBirth = user.AddressOfBirth,
                             Nationality = user.Nationality,
                             Id = user.Id,
                             FullName = user.FullName,
                             DateOfBirth = user.DateOfBirth,
                             ImageUrl = user.ImageUrl,
                             Gender = user.Gender,

                         });
                var result = query.First();
                var data = DataResult.ResultSuccess(result, "Get Info Success", 0);
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        #region Post

      
        public async Task<object> CreateOrUpdatePostAsync(PostDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                input.Type = (int)POST_TYPE_ENUM.USER;
                input.Status = (int)EStatusPost.Normal;
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
                    mb.statisticMetris(t1, 0, "user_ud_post");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    if(AbpSession.TenantId == 62)
                    {
                        input.State = (int)CommonENumPost.STATE_POST.NEW;
                    }else
                    {
                        input.State = (int)CommonENumPost.STATE_POST.ACTIVE;
                    }
                    var insertInput = input.MapTo<Post>();
                    long id = await _postAdminRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "user_is_post");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
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

                //var friendOrFamilyIds = await GetAllFriendOrFamilyId();
                long t1 = TimeUtils.GetNanoseconds();
                var query = Enumerable.Empty<object>().AsQueryable();

                DateTime fromDay = new DateTime(), toDay = new DateTime();
                if (input.FromDay.HasValue)
                {
                    fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

                }
                if (input.ToDay.HasValue)
                {
                    toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

                }

                query = (from post in _postAdminRepos.GetAll()
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
                             EmotionState = post.EmotionState,
                             CreatorUserId = post.CreatorUserId,
                             LastModificationTime = post.LastModificationTime,
                             LastModifierUserId = post.LastModifierUserId,
                             NameUserCreate = creator.UserName,
                             ImageAvatarUserCreate = creator.ImageUrl,
                             FullName = creator.FullName,
                             Status = post.Status,
                             CountComment = (from comment in _postCommentRepos.GetAll()
                                             where comment.PostId == post.Id
                                             select comment.Id).AsParallel().AsQueryable().Count(),
                             CountLike = (from like in _likePostRepos.GetAll()
                                          where like.PostId == post.Id
                                          select like.Id).AsParallel().AsQueryable().Count(),
                             Like = (from like in _likePostRepos.GetAll()
                                     where like.PostId == post.Id && like.CreatorUserId == AbpSession.UserId
                                     select like).FirstOrDefault(),
                         })
                          .Where(x => x.State == (int)CommonENumPost.STATE_POST.ACTIVE)
                          .WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) || (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay))
                          .WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime <= toDay) || (!u.LastModificationTime.HasValue && u.CreationTime <= toDay))
                          .WhereIf(input.CreatorUserId != 0 && input.CreatorUserId != null, x => x.CreatorUserId == input.CreatorUserId)
                          .OrderByDescending(x => x.LastModificationTime)
                          .OrderByDescending(x => x.CreationTime)
                          .AsQueryable();

                var result = query.PageBy(input).ToList();
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
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

                var post = _postAdminRepos.FirstOrDefault(input.PostId??0);

                if(post == null) { throw new Exception("Post not found !"); }

                if (post.Status == (int)EStatusPost.BlockComment ||
                    post.Status == (int)EStatusPost.BlockCommentAndShare)
                {
                    throw new Exception("Post is block comment !");
                }

                input.TenantId = AbpSession.TenantId;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _postCommentRepos.GetAsync(input.Id);

                    if (updateData != null)
                    {
                        input.MapTo(updateData);

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

                    var userReact = _userRepos.FirstOrDefault(AbpSession.UserId??0);

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
                if(input.id < 1)
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
            catch (Exception e)
            {
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

        #region Common
        public async Task<List<long>> GetAllFriendOrFamilyId()
        {
            var friends = (from fr in _friendShipRepos.GetAll()
                           where fr.UserId == AbpSession.UserId
                           select fr.FriendUserId).ToList();
            var families = (from fm in _homeMemberRepos.GetAll()
                            join fm2 in _homeMemberRepos.GetAll() on fm.SmartHomeCode equals fm2.SmartHomeCode
                            where fm.UserId == AbpSession.UserId && fm2.UserId != AbpSession.UserId
                            select fm2.UserId.Value).ToList();
            var result = new List<long>();
            result = result.Concat(friends).ToList();
            result = result.Concat(families).ToList();
            return result;
        }
        #endregion
    }
}
