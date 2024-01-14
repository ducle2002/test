using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.Application;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Clb.Projects;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface IPostReactionAppService : IApplicationService
    {
        Task<object> CreateReactionAsync(CreateReactionDto dto);
        Task<object> UpdateReactionAsync(UpdateReactionDto input);
        Task<object> GetAllPostReactionAsync(GetAllReactionDto input);
        Task<object> DeleteReactionAsync(long id);
    }

    [AbpAuthorize]
    public class PostReactionAppService : YootekAppServiceBase, IPostReactionAppService
    {
        private readonly IRepository<ForumPostReaction, long> _reactionRepos;
        private readonly IRepository<Member, long> _memberRepos;
        private readonly IRepository<ForumPost, long> _postRepos;
        

        public PostReactionAppService(
          IRepository<ForumPost, long> postRepos,
          IRepository<ForumPostReaction, long> reactionRepos, IRepository<Member, long> memberRepos)
        {
            _postRepos = postRepos;
            _reactionRepos = reactionRepos;
            _memberRepos = memberRepos;
        }

        #region Project
        public async Task<object> CreateReactionAsync(CreateReactionDto dto)
        {
            try
            {
                var reaction = ObjectMapper.Map<ForumPostReaction>(dto);
                reaction.TenantId = AbpSession.TenantId;
                
                var post = await _postRepos.FirstOrDefaultAsync(x => x.Id == dto.PostId);
                if(post == null)
                    throw new UserFriendlyException("Not found !");
                
                await _reactionRepos.InsertAsync(reaction);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> UpdateReactionAsync(UpdateReactionDto input)
        {
            try
            {
                var reaction = await _reactionRepos.FirstOrDefaultAsync(x => x.Id == input.Id)
                    ?? throw new UserFriendlyException("Not found !");
                ObjectMapper.Map(input, reaction);
                
                await _reactionRepos.UpdateAsync(reaction);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllPostReactionAsync(GetAllReactionDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from pr in _reactionRepos.GetAll()
                        join member in _memberRepos.GetAll() on pr.CreatorUserId equals member.AccountId
                        select new PostReactionDto()
                        {
                            Id = pr.Id,
                            TenantId = pr.TenantId,
                            PostId = pr.PostId,
                            CommentId = pr.CommentId,
                            Type = pr.Type,
                            Creator = new ShortenedUserDto()
                            {
                                Id = member.Id,
                                FullName = member.FullName,
                                ImageUrl = member.ImageUrl,
                                EmailAddress = member.Email,
                            },
                            CreationTime = pr.CreationTime,
                            LastModificationTime = pr.LastModificationTime
                        })
                    .Where(x => x.PostId == input.PostId)
                    .WhereIf(input.Type.HasValue, x => x.Type == input.Type);

                if (input.CommentId.HasValue)
                {
                    query = query.Where(x => x.CommentId == input.CommentId);
                }
                else
                {
                    query = query.Where(x => x.CommentId == null);
                }
                    
                var result = await query.PageBy(input).ToListAsync();
                
                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                mb.statisticMetris(t1, 0, "get_all_forum_post_reaction");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        #endregion
        
        public async Task<object> DeleteReactionAsync(long id)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var project = await _reactionRepos.FirstOrDefaultAsync(x => x.Id == id)
                              ?? throw new UserFriendlyException("Not found !");
                    
                    await _reactionRepos.DeleteAsync(project);
                    return DataResult.ResultSuccess(true, "Delete success !");
                }
            }

            catch (Exception e)
            {
                DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
