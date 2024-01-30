using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yootek.Yootek.EntityDb.Clb.Votes;
using Yootek.Yootek.Services.Yootek.Clb.Dto;

namespace Yootek.Services
{
    public interface IClbUserCityVoteAppService : IApplicationService
    {
        Task<object> CreateOrUpdateUserVote(ClbUserVoteDto input);
        Task<object> CreateUserVote(CreateClbVoteInput input);
        Task<object> UpdateUserVote(UpdateClbVoteInput input);
        Task<object> GetAllCityVotes(GetAllCityVoteInput input);
        Task<object> GetVoteById(GetVoteByIdInput input);
    }

    [AbpAuthorize]
    public class ClbUserCityVoteAppService : YootekAppServiceBase, IClbUserCityVoteAppService
    {
        private readonly IRepository<ClbCityVote, long> _cityVoteRepos;
        private readonly IRepository<ClbUserVote, long> _userVoteRepos;
        private readonly IRepository<User, long> _userRepos;

        public ClbUserCityVoteAppService(
            IRepository<ClbCityVote, long> cityVoteRepos,
            IRepository<ClbUserVote, long> userVoteRepos,
            IRepository<User, long> userRepos
            )
        {
            _cityVoteRepos = cityVoteRepos;
            _userVoteRepos = userVoteRepos;
            _userRepos = userRepos;
        }

        #region Vote
        public async Task<object> CreateOrUpdateUserVote(ClbUserVoteDto input)
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

                    var insertInput = input.MapTo<ClbUserVote>();
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
        public async Task<object> CreateUserVote(CreateClbVoteInput input)
        {
            try
            {
                var userVoteOrg = await _userVoteRepos.FirstOrDefaultAsync(x =>
                    x.CityVoteId == input.CityVoteId && x.CreatorUserId == AbpSession.UserId);
                if (userVoteOrg != null) throw new UserFriendlyException("User is voted !");

                var userVoteInsert = input.MapTo<ClbUserVote>();
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
        public async Task<object> UpdateUserVote(UpdateClbVoteInput input)
        {
            try
            {
                var userVoteUpdate = await _userVoteRepos.FirstOrDefaultAsync(input.Id)
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
                var cityVote = await _cityVoteRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("CityVote not found");
                long totalUsers = _userRepos.GetAll().Count();
                var cityVoteDto = cityVote.MapTo<ClbCityVoteDto>();
                cityVoteDto.TotalVotes = _userVoteRepos.GetAll().Where(x => x.CityVoteId == input.Id).Count();
                cityVoteDto.UserIsVoted = _userVoteRepos.FirstOrDefault(x => x.CityVoteId == input.Id && x.CreatorUserId == AbpSession.UserId);
                cityVoteDto.TotalUsers = totalUsers > 0 ? totalUsers : 1;

                var voteOptions = JsonConvert.DeserializeObject<List<ClbVoteOption>>(cityVote.Options);
                if (voteOptions is { Count: > 0 })
                {
                    foreach (var voteOption in voteOptions)
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
                            select new ClbCityVoteDto()
                             {
                                 CreatorUserId = cv.CreatorUserId,
                                 CreationTime = cv.CreationTime,
                                 Options = cv.Options,
                                 Name = cv.Name,
                                 Id = cv.Id,
                                 StartTime = cv.StartTime,
                                 FinishTime = cv.FinishTime,
                                 TenantId = cv.TenantId,
                                 LastModificationTime = cv.LastModificationTime,
                                 LastModifierUserId = cv.LastModifierUserId,
                                 TotalVotes = (from sv in _userVoteRepos.GetAll()
                                               where sv.CityVoteId == cv.Id
                                               select sv).Count(),
                                 UserIsVoted = (from sv in _userVoteRepos.GetAll()
                                                where sv.CityVoteId == cv.Id && sv.CreatorUserId == AbpSession.UserId
                                                select sv).First(),
                                 TotalUsers = totalUsers,
                                 IsShowNumbersVote = cv.IsShowNumbersVote
                             })
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
                            item.VoteOptions = JsonConvert.DeserializeObject<List<ClbVoteOption>>(item.Options);
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
