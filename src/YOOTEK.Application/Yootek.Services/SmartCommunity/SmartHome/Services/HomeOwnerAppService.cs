using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Yootek.Authorization.Users;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class HomeMemberDto : HomeMember
    {
        public string OwnerName { get; set; }
    }

    public interface IHomeOwnerAppService : IApplicationService
    {
        Task<object> GetAll();
        Task<object> Create(HomeMember createInput);
        Task<object> Update(HomeMember updateInput);
    }

    [AbpAuthorize]
    public class HomeOwnerAppService : YootekAppServiceBase, IHomeOwnerAppService
    {
        private readonly IRepository<HomeMember, long> _homeMemberRepos;
        private readonly IRepository<User, long> _userRepos;
        public HomeOwnerAppService(
            IRepository<HomeMember, long> homeMemberRepos,
            IRepository<User, long> userRepos)
        {
            _homeMemberRepos = homeMemberRepos;
            _userRepos = userRepos;
        }
        public async Task<object> GetAll()
        {
            try
            {
                var owner = from HomeMember in _homeMemberRepos.GetAll()
                            join user in _userRepos.GetAll()
                            on HomeMember.UserId equals user.Id
                            select new HomeMemberDto()
                            {
                                Id = HomeMember.Id,
                                IsVoter = HomeMember.IsVoter,
                                UserId = HomeMember.UserId,
                                OwnerName = user.Name
                            };
                return owner.ToList();
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message, e);
                return null;
            }
        }

        public async Task<object> Create(HomeMember createInput)
        {
            try
            {
                return await _homeMemberRepos.InsertAsync(createInput);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message, e);
                return null;
            }
        }

        public async Task<object> Update(HomeMember updateInput)
        {
            try
            {
                return await _homeMemberRepos.UpdateAsync(updateInput);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message, e);
                return null;
            }
        }
    }
}
