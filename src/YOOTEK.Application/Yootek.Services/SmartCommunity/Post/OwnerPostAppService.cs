

using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Common.Enum.PostEnums;
using Yootek.EntityDb;
using Yootek.Friendships;
using Yootek.Services.Dto;
using Yootek.Users.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Yootek.Services
{
    public interface IOwnerPostAppService : IApplicationService
    {

    }

    [AbpAuthorize]
    public class OwnerPostAppService : YootekAppServiceBase, IOwnerPostAppService
    {
        private readonly IRepository<Post, long> _postAdminRepos;
        private readonly IRepository<PostComment, long> _postCommentRepos;
        private readonly IRepository<LikePost, long> _likePostRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<Friendship, long> _friendShipRepos;
        private readonly IRepository<HomeMember, long> _homeMemberRepos;

        public OwnerPostAppService(
            IRepository<Post, long> postAdminRepos,
            IRepository<PostComment, long> postCommentRepos,
            IRepository<LikePost, long> likePostRepos,
            IRepository<Friendship, long> friendShipRepos,
            IRepository<HomeMember, long> homeMemberRepos,
            IRepository<User, long> userRepos)

        {
            _postAdminRepos = postAdminRepos;
            _postCommentRepos = postCommentRepos;
            _likePostRepos = likePostRepos;
            _userRepos = userRepos;
            _friendShipRepos = friendShipRepos;
            _homeMemberRepos = homeMemberRepos;
        }

        #region Common
        private async Task<List<long>> GetAllFriendOrFamilyId()
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
