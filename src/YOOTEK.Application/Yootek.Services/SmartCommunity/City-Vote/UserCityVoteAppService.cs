using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IUserCityVoteAppService : IApplicationService
    {
        Task<object> CreateOrUpdateUserVote(UserVoteDto input);
        Task<object> CreateUserVote(CreateVoteInput input);
        Task<object> UpdateUserVote(UpdateVoteInput input);
        Task<object> GetAllCityVotes(GetAllCityVoteInput input);
        Task<object> GetVoteById(GetVoteByIdInput input);
    }

    [AbpAuthorize]
    public class UserCityVoteAppService : YootekAppServiceBase, IUserCityVoteAppService
    {
        private readonly IRepository<CityVote, long> _cityVoteRepos;
        private readonly IRepository<UserVote, long> _userVoteRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;

        public UserCityVoteAppService(
            IRepository<CityVote, long> cityVoteRepos,
            IRepository<UserVote, long> userVoteRepos,
            IRepository<User, long> userRepos,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository
            )
        {
            _cityVoteRepos = cityVoteRepos;
            _userVoteRepos = userVoteRepos;
            _userRepos = userRepos;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
        }

        #region Vote

        public async Task<object> GetAllOrganizationVote(long? orgId)
        {

            try
            {
                var org = new AppOrganizationUnit();
                if (orgId.HasValue)
                {
                    org = await _appOrganizationUnitRepository.FirstOrDefaultAsync(orgId.Value);
                }

                var result = (from apm in _appOrganizationUnitRepository.GetAll()
                              where apm.Type == APP_ORGANIZATION_TYPE.VOTE
                              select new
                              {
                                  OrganizationUnitId = apm.ParentId,
                                  Name = apm.DisplayName,
                                  ImageUrl = apm.ImageUrl,
                                  Type = apm.Type,
                                  Description = apm.Description,
                                  Code = apm.Code
                              })
                              .WhereIf(org != null && org.Id > 0, x => x.Code.StartsWith(org.Code))
                              .ToList();
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



        public async Task<object> CreateOrUpdateUserVote(UserVoteDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var check = await _userVoteRepos.FirstOrDefaultAsync(x => x.CityVoteId == input.CityVoteId && x.CreatorUserId == AbpSession.UserId);

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0 || check != null)
                {
                    //update
                    var updateData = await _userVoteRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _userVoteRepos.UpdateAsync(updateData);
                    }
                    else
                    {
                        input.MapTo(check);

                        //call back
                        await _userVoteRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_uservote");

                    var data = DataResult.ResultSuccess(input, "Update success !");
                    return data;
                }
                else if (check == null)
                {

                    var insertInput = input.MapTo<UserVote>();
                    long id = await _userVoteRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_uservote");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
                return DataResult.ResultFail("User is voted !"); ;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> CreateUserVote(CreateVoteInput input)
        {
            try
            {
                UserVote? userVoteOrg = await _userVoteRepos.FirstOrDefaultAsync(x =>
                    x.CityVoteId == input.CityVoteId && x.CreatorUserId == AbpSession.UserId);
                if (userVoteOrg != null) throw new UserFriendlyException("User is voted !");

                UserVote userVoteInsert = input.MapTo<UserVote>();
                userVoteInsert.TenantId = AbpSession.TenantId;
                long id = await _userVoteRepos.InsertAndGetIdAsync(userVoteInsert);
                userVoteInsert.Id = id;
                return DataResult.ResultSuccess(userVoteInsert, "Insert success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> UpdateUserVote(UpdateVoteInput input)
        {
            try
            {
                UserVote? userVoteUpdate = await _userVoteRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("Vote not found!");
                if (userVoteUpdate.CreatorUserId != AbpSession.UserId)
                    throw new UserFriendlyException("You don't have permission");
                input.MapTo(userVoteUpdate);
                await _userVoteRepos.UpdateAsync(userVoteUpdate);
                return DataResult.ResultSuccess(userVoteUpdate, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetVoteById(GetVoteByIdInput input)
        {
            try
            {
                CityVote? cityVote = await _cityVoteRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("CityVote not found");
                long totalUsers = _userRepos.GetAll().Count();
                CityVoteDto cityVoteDto = cityVote.MapTo<CityVoteDto>();
                cityVoteDto.TotalVotes = _userVoteRepos.GetAll().Where(x => x.CityVoteId == input.Id).Count();
                cityVoteDto.UserIsVoted = _userVoteRepos.FirstOrDefault(x => x.CityVoteId == input.Id && x.CreatorUserId == AbpSession.UserId);
                cityVoteDto.TotalUsers = totalUsers > 0 ? totalUsers : 1;
                cityVoteDto.OrganizationName = _appOrganizationUnitRepository.GetAll()
                    .Where(x => x.Id == cityVoteDto.OrganizationUnitId).Select(x => x.DisplayName).FirstOrDefault();

                List<VoteOption> voteOptions = JsonConvert.DeserializeObject<List<VoteOption>>(cityVote.Options);
                if (voteOptions != null && voteOptions.Count > 0)
                {
                    foreach (VoteOption voteOption in voteOptions)
                    {
                        voteOption.CountVote = await _userVoteRepos.GetAll().Where(x => x.CityVoteId == input.Id && x.OptionVoteId == voteOption.Id).CountAsync();
                        voteOption.Percent = totalUsers > 0 ? (float)Math.Round((float)((float)voteOption.CountVote / (float)totalUsers), 3) : 0;
                    }
                }
                cityVoteDto.VoteOptions = voteOptions;

                return DataResult.ResultSuccess(cityVoteDto, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> GetAllCityVotes(GetAllCityVoteInput input)
        {
            try
            {
                var totalUsers = _userRepos.GetAll().Count();
                totalUsers = totalUsers > 0 ? totalUsers : 1;
                var query = (from cv in _cityVoteRepos.GetAll()
                             join ou in _appOrganizationUnitRepository.GetAll() on cv.OrganizationUnitId equals ou.Id into tb_ou
                             from ou in tb_ou.DefaultIfEmpty()
                                 //join sv in _userVoteRepos.GetAll() on cv.Id equals sv.CityVoteId into tb_sv
                                 //from ci in tb_sv.DefaultIfEmpty()
                             select new CityVoteDto()
                             {
                                 CreatorUserId = cv.CreatorUserId,
                                 CreationTime = cv.CreationTime,
                                 Options = cv.Options,
                                 Name = cv.Name,
                                 OrganizationUnitId = cv.OrganizationUnitId,
                                 Id = cv.Id,
                                 StartTime = cv.StartTime,
                                 FinishTime = cv.FinishTime,
                                 TenantId = cv.TenantId,
                                 LastModificationTime = cv.LastModificationTime,
                                 LastModifierUserId = cv.LastModifierUserId,
                                 TotalVotes = (from sv in _userVoteRepos.GetAll()
                                               where sv.CityVoteId == cv.Id
                                               select sv).Count(),
                                 //UserVotes = (from sv in _userVoteRepos.GetAll()
                                 //             where sv.CityVoteId == cv.Id
                                 //             select sv).ToList(),
                                 UserIsVoted = (from sv in _userVoteRepos.GetAll()
                                                where sv.CityVoteId == cv.Id && sv.CreatorUserId == AbpSession.UserId
                                                select sv).First(),
                                 TotalUsers = totalUsers,
                                 OrganizationName = ou.DisplayName,
                                 IsShowNumbersVote = cv.IsShowNumbersVote,
                                 IsOptionOther = cv.IsOptionOther
                             })
                             .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                             .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                             .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                             .OrderByDescending(x => x.CreationTime)
                             .AsQueryable()

                             ;
                switch (input.State)
                {
                    //new
                    case STATE_GET_VOTE.NEW:
                        query = query.Where(x => x.FinishTime >= DateTime.Now && x.UserVotes.FirstOrDefault(u => u.CreatorUserId == AbpSession.UserId) == null);
                        break;
                    // voted
                    case STATE_GET_VOTE.NEWVOTED:
                        query = query.Where(x => x.FinishTime >= DateTime.Now && x.UserVotes.FirstOrDefault(u => u.CreatorUserId == AbpSession.UserId) != null);
                        break;
                    case STATE_GET_VOTE.FINISH:
                        query = query.Where(x => x.FinishTime < DateTime.Now);
                        break;
                    //con han
                    case STATE_GET_VOTE.UNEXPIRED:
                        query = query.Where(x => x.FinishTime >= DateTime.Now);
                        break;

                    //het han
                    case STATE_GET_VOTE.EXPIRED:
                        query = query.Where(x => x.FinishTime < DateTime.Now);
                        break;
                    default:
                        break;
                }

                var dataGrids = query.PageBy(input).ToList();
                if (dataGrids != null)
                {
                    foreach (var item in dataGrids)
                    {
                        try
                        {
                            item.VoteOptions = JsonConvert.DeserializeObject<List<VoteOption>>(item.Options);
                        }
                        catch (Exception ex)
                        {

                        }
                        if (item.VoteOptions != null)
                        {
                            foreach (var opt in item.VoteOptions)
                            {
                                opt.CountVote = await _userVoteRepos.GetAll().Where(x => x.CityVoteId == item.Id && x.OptionVoteId == opt.Id).CountAsync();
                                opt.Percent = totalUsers > 0 ? (float)Math.Round((float)((float)opt.CountVote / (float)totalUsers), 3) : 0;

                            }
                        }
                        //if (!string.IsNullOrWhiteSpace(item.OrganizationName))
                        //{
                        //    item.Name = $"( {item.OrganizationName} ) " + item.Name;
                        //}
                    }
                }
                var data = DataResult.ResultSuccess(dataGrids, "Get success!", query.Count());
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
    }
}
