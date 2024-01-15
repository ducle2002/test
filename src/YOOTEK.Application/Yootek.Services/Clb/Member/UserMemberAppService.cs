using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Yootek.Application;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Service;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface IUserMemberAppService : IApplicationService
    {
        Task<object> CreateMemberAsync(CreateMemberByUserDto dto);
        Task<object> UpdateMemberAsync(UpdateMemberByUserDto input);
        Task<object> GetMemberByIdAsync(long id);
        Task<object> GetUserInfo();
        Task<object> GetAllMemberAsync(GetAllMemberInput input);
    }

    [AbpAuthorize]
    public class UserMemberAppService : YootekAppServiceBase, IUserMemberAppService
    {
        private readonly IRepository<Member, long> _memberRepos;
        private readonly IRepository<User, long> _userRepos;

        public UserMemberAppService(
          IRepository<User, long> userRepos,
          IRepository<Member, long> memberRepos)
        {
            _userRepos = userRepos;
            _memberRepos = memberRepos;
        }

        #region Citizen
        public async Task<object> CreateMemberAsync(CreateMemberByUserDto dto)
        {
            try
            {
                var member = ObjectMapper.Map<Member>(dto);
                member.TenantId = AbpSession.TenantId;
                member.AccountId = AbpSession.UserId;
                member.State = ClbMemberState.New;
                member.Type = (int)ClbMemberType.Member;
                
                await _memberRepos.InsertAsync(member);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> UpdateMemberAsync(UpdateMemberByUserDto input)
        {
            try
            {
                var member = await _memberRepos.FirstOrDefaultAsync(x => x.AccountId == AbpSession.UserId)
                    ?? throw new UserFriendlyException("Người dùng chưa là hội viên");
                ObjectMapper.Map(input, member);
                
                await _memberRepos.UpdateAsync(member);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        
        public async Task<object> GetMemberByIdAsync(long id)
        {
            try
            {
                var result = await _memberRepos.FirstOrDefaultAsync(x=> x.Id == id);
                if(result == null) throw new UserFriendlyException("Hội viên không tồn tại");
                
                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetUserInfo()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _memberRepos.FirstOrDefaultAsync(x => x.AccountId == AbpSession.UserId);
                var data = DataResult.ResultSuccess(result, "Get success!");
                mb.statisticMetris(t1, 0, "get_info_citizen");
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
        
        private IQueryable<Member> QueryMember(GetAllMemberInput input)
        {
            var query = (from mem in _memberRepos.GetAll()
                         join us in _userRepos.GetAll() on mem.AccountId equals us.Id into tb_us
                         from us in tb_us.DefaultIfEmpty()
                         select new ClbMemberDto()
                         {
                             Id = mem.Id,
                             PhoneNumber = mem.PhoneNumber != null ? mem.PhoneNumber : us.PhoneNumber,
                             Nationality = mem.Nationality,
                             FullName = mem.FullName,
                             IdentityNumber = mem.IdentityNumber,
                             ImageUrl = mem.ImageUrl != null ? mem.ImageUrl : us.ImageUrl,
                             Email = mem.Email != null ? mem.Email : (us.EmailAddress.Contains("yootek") ? us.EmailAddress : null),
                             Address = mem.Address,
                             DateOfBirth = mem.DateOfBirth,
                             AccountId = mem.AccountId,
                             Gender = mem.Gender,
                             State = mem.State,
                             Type = mem.Type,
                             TenantId = mem.TenantId,
                             Career = mem.Career,
                             IdentityImageUrls = mem.IdentityImageUrls
                         })
                         .ApplySearchFilter(input.Keyword, x => x.FullName, x => x.Address, x => x.Email);

            switch (input.FormId)
            {
                case (int)GetMemberFormId.GetAll:
                    break;
                case (int) GetMemberFormId.GetVerified:
                    query = query.Where( x => x.State.Value == ClbMemberState.Accepted);
                    break;
                case (int) GetMemberFormId.GetUnVerified:
                    query = query.Where( x => x.State.Value == ClbMemberState.Refuse || x.State.Value == ClbMemberState.New);
                    break;
                case (int) GetMemberFormId.GetDisable:
                    query = query.Where( x => x.State.Value == ClbMemberState.Disable);
                    break;
                case (int) GetMemberFormId.GetNew:
                    query = query.Where( x => x.State.Value == ClbMemberState.New);
                    break;
                default:
                    break;
            }


            DateTime fromDay, toDay;

            if (input.FromDay.HasValue)
            {
                fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month,
                    input.FromDay.Value.Day, 0, 0, 0);
                query = query.WhereIf(input.FromDay.HasValue,
                    u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) ||
                         (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay));
            }

            if (input.ToDay.HasValue)
            {
                toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23,
                    59, 59);
                query = query.WhereIf(input.ToDay.HasValue,
                    u => (u.LastModificationTime.HasValue && u.LastModificationTime <= toDay) ||
                         (!u.LastModificationTime.HasValue && u.CreationTime <= toDay));
            }

            if (input.State.HasValue) query = query.Where(x => x.State.Value == input.State);
            return query;
        }

        public async Task<object> GetAllMemberAsync(GetAllMemberInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = QueryMember(input);

                    var result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .PageBy(input)
                        .ToListAsync();

                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
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
