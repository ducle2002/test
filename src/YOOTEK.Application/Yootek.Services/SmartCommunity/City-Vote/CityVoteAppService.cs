using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.City_Vote;
using Yootek.Notifications;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Yootek.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;

namespace Yootek.Services
{
    public interface ICityVoteAppService : IApplicationService
    {
        Task<object> GetStatisticCityVote(GetStatisticCityVoteInput input);
        Task<object> GetAllCityVoteAsync(GetAllCityVoteInput input);
        Task<object> CreateOrUpdateCityVote(CityVoteDto input);
        Task<object> CreateCityVote(CreateCityVoteInput input);
        Task<object> UpdateCityVote(UpdateCityVoteInput input);
        Task<object> GetVoteByIdAsync(GetVoteByIdInput id);

    }

    //[AbpAuthorize(PermissionNames.Pages_Digitals_Surveys, PermissionNames.Pages_Government, PermissionNames.Pages_Government_Citizens_Vote)]
    [AbpAuthorize]
    public class CityVoteAppService : YootekAppServiceBase, ICityVoteAppService
    {
        private readonly IRepository<CityVote, long> _cityVoteRepos;
        private readonly IRepository<UserVote, long> _userVoteRepos;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationRepos;
        private readonly ICityVoteExcelExporter _cityVoteExcelExporter;
        private readonly IBackgroundJobManager _backgroundJobManager;

        public CityVoteAppService(
            IRepository<CityVote, long> cityVoteRepos,
            IRepository<UserVote, long> userVoteRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationRepos,
            IRepository<User, long> userRepos,
            IRepository<Citizen, long> citizenRepos,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IAppNotifier appNotifier,
            IRepository<AppOrganizationUnit, long> organizationRepos,
            IBackgroundJobManager backgroundJobManager,
            ICityVoteExcelExporter cityVoteExcelExporter
            )
        {
            _cityVoteRepos = cityVoteRepos;
            _userVoteRepos = userVoteRepos;
            _userRepos = userRepos;
            _citizenRepos = citizenRepos;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _appNotifier = appNotifier;
            _userOrganizationRepos = userOrganizationRepos;
            _cityVoteExcelExporter = cityVoteExcelExporter;
            _backgroundJobManager = backgroundJobManager;
            _organizationRepos = organizationRepos;
        }


        #region Vote

        public async Task<object> GetAllCityVoteAsync(GetAllCityVoteInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var totalUsers = _userRepos.GetAll().Count();
                totalUsers = totalUsers > 0 ? totalUsers : 1;
                var query = (from cv in _cityVoteRepos.GetAll()
                             join ou in _appOrganizationUnitRepository.GetAll() on cv.OrganizationUnitId equals ou.Id into tb_ou
                             from ou in tb_ou.DefaultIfEmpty()
                                 //join ci in _citizenRepos.GetAll() on cv.CreatorUserId equals ci.AccountId into tb_ci
                                 //from ci in tb_ci.DefaultIfEmpty()
                             select new CityVoteDto()
                             {
                                 CreatorUserId = cv.CreatorUserId,
                                 CreationTime = cv.CreationTime,
                                 Options = cv.Options,
                                 Name = cv.Name,
                                 Description = cv.Description,
                                 OrganizationUnitId = cv.OrganizationUnitId,
                                 Id = cv.Id,
                                 StartTime = cv.StartTime,
                                 FinishTime = cv.FinishTime,
                                 // Properties = cv.Properties,
                                 TenantId = cv.TenantId,
                                 LastModificationTime = cv.LastModificationTime,
                                 LastModifierUserId = cv.LastModifierUserId,
                                 TotalVotes = (from sv in _userVoteRepos.GetAll()
                                               where sv.CityVoteId == cv.Id
                                               select sv).Count(),
                                 TotalUsers = totalUsers > 0 ? totalUsers : 1,
                                 UserIsVoted = (from sv in _userVoteRepos.GetAll()
                                                where sv.CityVoteId == cv.Id && sv.CreatorUserId == AbpSession.UserId
                                                select sv).First(),
                                 OrganizationName = ou.DisplayName,
                                 IsShowNumbersVote = cv.IsShowNumbersVote,
                                 Status = cv.Status,
                                 BuildingId = cv.BuildingId,
                                 UrbanId = cv.UrbanId,
                                 IsOptionOther = cv.IsOptionOther
                             })
                             .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                             .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                             .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                             .ApplySearchFilter(input.Keyword, x => x.Name)
                             .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                             .OrderByDescending(x => x.CreationTime)
                    .AsQueryable();

                var now = DateTime.Now;
                var moment = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                if (input.State.HasValue)
                {
                    switch (input.State)
                    {
                        //new
                        case STATE_GET_VOTE.COMING:
                            query = query.Where(x => x.StartTime > moment);
                            break;
                        case STATE_GET_VOTE.IN_PROGRESS:
                            query = query.Where(x => x.StartTime <= moment && x.FinishTime > moment);
                            break;
                        case STATE_GET_VOTE.NEW:
                            query = query.Where(x => x.FinishTime >= moment);
                            break;
                        // voted
                        case STATE_GET_VOTE.FINISH:
                            query = query.Where(x => x.FinishTime < moment);
                            break;

                        //con han
                        case STATE_GET_VOTE.UNEXPIRED:
                            query = query.Where(x => x.FinishTime >= moment);
                            break;

                        //het han
                        case STATE_GET_VOTE.EXPIRED:
                            query = query.Where(x => x.FinishTime < moment);
                            break;

                        case STATE_GET_VOTE.ALL:
                            break;
                    }
                }

                if (input.Status.HasValue)
                {
                    query = query.Where(x => x.Status == input.Status);
                }


                var dataGrids = await query.PageBy(input).ToListAsync();

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
                                opt.CountVote = await _userVoteRepos.GetAll()
                                    .Where(x => x.CityVoteId == item.Id && x.OptionVoteId == opt.Id).CountAsync();

                                opt.Percent = (float)Math.Round((float)((float)opt.CountVote / (float)totalUsers), 3);

                            }
                        }

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
        public async Task<object> GetVoteByIdAsync(GetVoteByIdInput input)
        {
            try
            {
                CityVote? cityVote = await _cityVoteRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("CityVote not found");
                long totalUsers = _userRepos.GetAll().Count();
                CityVoteDto cityVoteDto = cityVote.MapTo<CityVoteDto>();
                // cityVoteDto.DetailLinkApp = $"yoolife://app/evote/detail?id={cityVote.Id}";
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
        public async Task<object> CreateOrUpdateCityVote(CityVoteDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                var now = DateTime.Now;
                var moment = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _cityVoteRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        if (input.StartTime > moment)
                        {
                            input.Status = STATUS_VOTE.COMING;
                        }
                        else if (input.FinishTime < moment)
                        {
                            input.Status = STATUS_VOTE.FINISH;
                        }
                        else if (input.StartTime < moment && input.FinishTime > moment)
                        {
                            input.Status = STATUS_VOTE.IN_PROGRESS;
                        }

                        input.MapTo(updateData);
                        //call back
                        await _cityVoteRepos.UpdateAsync(updateData);
                    }

                    mb.statisticMetris(t1, 0, "Ud_cityvote");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {


                    if (input.StartTime > moment)
                    {
                        input.Status = STATUS_VOTE.COMING;
                    }
                    else if (input.FinishTime < moment)
                    {
                        input.Status = STATUS_VOTE.FINISH;
                    }
                    else if (input.StartTime < moment && input.FinishTime > moment)
                    {
                        input.Status = STATUS_VOTE.IN_PROGRESS;
                    }
                    var insertInput = input.MapTo<CityVote>();
                    long id = await _cityVoteRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;

                    var createrName = "Ban quản trị";
                    if (input.OrganizationUnitId.HasValue)
                    {
                        var org = _appOrganizationUnitRepository.FirstOrDefault(x => x.Id == input.OrganizationUnitId);
                        if (org != null) createrName = org.DisplayName;
                    }
                    await NotifierNewVoteUser(insertInput, createrName);
                    mb.statisticMetris(t1, 0, "is_cityvote");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> CreateCityVote(CreateCityVoteInput input)
        {
            try
            {
                if (input.StartTime >= input.FinishTime || input.FinishTime <= DateTime.Now)
                    throw new UserFriendlyException(400, "DateTime is invalid");
               
                //input.StartTime = Clock.Provider.Normalize(input.StartTime);
                //input.FinishTime = Clock.Provider.Normalize(input.FinishTime);

                input.StartTime = input.StartTime.AddHours(-7);
                input.FinishTime = input.FinishTime.AddHours(-7);
                CityVote cityVoteInsert = ObjectMapper.Map<CityVote>(input);

                cityVoteInsert.TenantId = AbpSession.TenantId;
                if (input.StartTime >= DateTime.Now)
                {
                    cityVoteInsert.Status = STATUS_VOTE.COMING;
                }
                else if (input.FinishTime <= DateTime.Now)
                {
                    cityVoteInsert.Status = STATUS_VOTE.FINISH;
                }
                else if (input.StartTime < DateTime.Now && input.FinishTime > DateTime.Now)
                {
                    cityVoteInsert.Status = STATUS_VOTE.IN_PROGRESS;
                }
                long id = await _cityVoteRepos.InsertAndGetIdAsync(cityVoteInsert);
                cityVoteInsert.Id = id;
                string createrName = "Ban quản trị";
                if (input.OrganizationUnitId.HasValue)
                {
                    AppOrganizationUnit? appOrganizationUnit = _appOrganizationUnitRepository.FirstOrDefault(x => x.Id == input.OrganizationUnitId);
                    if (appOrganizationUnit != null) createrName = appOrganizationUnit.DisplayName;
                }
                await NotifierNewVoteUser(cityVoteInsert, createrName);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> UpdateCityVote(UpdateCityVoteInput input)
        {
            try
            {
                CityVote? cityVoteUpdate = await _cityVoteRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException(404, "CityVote not found");
                if (input.StartTime >= DateTime.Now)
                {
                    cityVoteUpdate.Status = STATUS_VOTE.COMING;
                }
                else if (input.FinishTime <= DateTime.Now)
                {
                    cityVoteUpdate.Status = STATUS_VOTE.FINISH;
                }
                else if (input.StartTime < DateTime.Now && input.FinishTime > DateTime.Now)
                {
                    cityVoteUpdate.Status = STATUS_VOTE.IN_PROGRESS;
                }
                input.MapTo(cityVoteUpdate);
                await _cityVoteRepos.UpdateAsync(cityVoteUpdate);

                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> DeleteCityVote(long id)
        {
            try
            {
                await _cityVoteRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public Task<DataResult> DeleteMultipleCityVote([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteCityVote(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

                var data = DataResult.ResultSuccess("Delete success!");
                return Task.FromResult(data);
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }

        public async Task<object> UpdateVoteStatusByIdAsync(long id, STATUS_VOTE status)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                //update
                var updateData = await _cityVoteRepos.GetAsync(id);
                if (updateData != null)
                {
                    updateData.Status = status;
                    //call back
                    await _cityVoteRepos.UpdateAsync(updateData);
                }

                mb.statisticMetris(t1, 0, "Ud_cityvote");

                var data = DataResult.ResultSuccess(updateData, "Update success !");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetUserVoteById(long id)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var totalUsers = _userRepos.GetAll().Count();
                totalUsers = totalUsers > 0 ? totalUsers : 1;
                var query = (from uv in _userVoteRepos.GetAll()
                             join cv in _cityVoteRepos.GetAll() on uv.CityVoteId equals cv.Id
                             where id == cv.Id
                             //join us in _userRepos.GetAll() on uv.CreatorUserId equals us.Id into tb_us
                             //from us in tb_us.DefaultIfEmpty() await
                             select new UserVoteDto()
                             {
                                 CreatorUserId = uv.CreatorUserId,
                                 CreationTime = uv.CreationTime,
                                 Comment = uv.Comment,
                                 FullName = _citizenRepos.GetAll().Where(x => x.AccountId == uv.CreatorUserId).FirstOrDefault().FullName,
                                 ApartmentCode = _citizenRepos.GetAll().Where(x => x.AccountId == uv.CreatorUserId).FirstOrDefault().ApartmentCode,
                                 UrbanId = _citizenRepos.GetAll().Where(x => x.AccountId == uv.CreatorUserId).FirstOrDefault().UrbanId,
                                 BuildingId = _citizenRepos.GetAll().Where(x => x.AccountId == uv.CreatorUserId).FirstOrDefault().BuildingId,
                                 OptionOther = uv.OptionOther,
                                 TenantId = cv.TenantId,
                                 VoteOptions = cv.Options,
                                 CityVoteId = cv.Id,
                                 OptionVoteId = uv.OptionVoteId,
                                 Id = uv.Id,
                             })
                             .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                             .OrderByDescending(x => x.CreationTime)
                             .AsQueryable();

                var dataGrids = await query.ToListAsync();

                if (dataGrids != null)
                {
                    foreach (var item in dataGrids)
                    {
                        try
                        {
                            var voteOptions = JsonConvert.DeserializeObject<List<VoteOption>>(item.VoteOptions);

                            var matchedVoteOption = voteOptions?.FirstOrDefault(option => option.Id == item.OptionVoteId);
                            item.OptionValue = matchedVoteOption?.Option;
                            item.UrbanName = _organizationRepos.FirstOrDefault(x => x.Id == item.UrbanId).DisplayName;
                            item.BuildingName = _organizationRepos.FirstOrDefault(x => x.Id == item.BuildingId).DisplayName;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                var data = DataResult.ResultSuccess(dataGrids, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        #endregion

        #region Statistics
        private IQueryable<CityVoteDto> QueryGetAllCityVote()
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = (from vt in _cityVoteRepos.GetAll()
                         select new CityVoteDto()
                         {
                             Id = vt.Id,
                             CreationTime = vt.CreationTime,
                             OrganizationUnitId = vt.OrganizationUnitId,
                             UrbanId = _organizationRepos.GetAll().Where(u => u.Id == vt.OrganizationUnitId && u.ParentId == null && u.Type == 0).Select(u => u.Id).FirstOrDefault(),
                             BuildingId = _organizationRepos.GetAll().Where(u => u.Id == vt.OrganizationUnitId && u.ParentId != null && u.Type == 0).Select(u => u.Id).FirstOrDefault(),
                         })
                         .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                         .AsQueryable();

            return query;
        }

        private IQueryable<UserVote> QueryGetAllUserVote()
        {
            var query = (from vt in _userVoteRepos.GetAll()
                         select new UserVote()
                         {
                             Id = vt.Id,
                             CreationTime = vt.CreationTime
                         }).AsQueryable();
            return query;
        }
        public async Task<object> GetStatisticCityVote(GetStatisticCityVoteInput input)
        {
            try
            {
                IEnumerable<StatisticCityVoteDto> query = _cityVoteRepos.GetAll()
                    .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId)
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                    .Where(x => x.StartTime >= input.DateStart)
                    .Where(x => x.StartTime < input.DateEnd)
                    .Select(x => new StatisticCityVoteDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Year = x.StartTime.Year,
                        Month = x.StartTime.Month,
                        Day = x.StartTime.Day,
                        StartTime = x.StartTime,
                        FinishTime = x.FinishTime,
                    }).AsEnumerable();
                List<DataStatisticCityVoteDto> result = new();
                IEnumerable<GroupCityVoteStatistic> groupedQuery = null;

                switch (input.QueryDateTime)
                {
                    case QueryCaseDateTime.YEAR:
                        groupedQuery = query.GroupBy(x => new { x.Year }).Select(y => new GroupCityVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.MONTH:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month }).Select(y => new GroupCityVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.DAY:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day }).Select(y => new GroupCityVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Day = y.Key.Day,
                            Items = y.ToList(),
                        });
                        break;
                }

                if (groupedQuery != null)
                {
                    foreach (GroupCityVoteStatistic group in groupedQuery)
                    {
                        result.Add(new DataStatisticCityVoteDto
                        {
                            Year = group.Year,
                            Month = group.Month,
                            Day = group.Day,
                            TotalCount = group.Items.Count,
                            CountVoteComing = group.Items.Count(x => x.StartTime > DateTime.Now),
                            CountVoteProgressing = group.Items.Count(x => x.StartTime <= DateTime.Now
                                                                       && x.FinishTime >= DateTime.Now),
                            CountVoteFinished = group.Items.Count(x => x.FinishTime < DateTime.Now),
                        });
                    }
                }
                result = result.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day).ToList();
                return DataResult.ResultSuccess(result, "Statistic city vote success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> GetStatisticUserVote(GetStatisticUserVoteInput input)
        {
            try
            {
                IEnumerable<StatisticUserVoteDto> query = _userVoteRepos.GetAll()
                    .Where(x => x.CityVoteId == input.VoteId)
                    .Select(x => new StatisticUserVoteDto
                    {
                        Id = x.Id,
                        Year = x.CreationTime.Year,
                        Month = x.CreationTime.Month,
                        Day = x.CreationTime.Day,
                        Hour = x.CreationTime.Hour,
                        OptionVoteId = x.OptionVoteId,
                        Comment = x.Comment,
                    }).AsEnumerable();
                List<DataStatisticUserVoteDto> result = new();
                IEnumerable<GroupUserVoteStatistic> groupedQuery = null;

                switch (input.QueryDateTime)
                {
                    case QueryCaseDateTime.YEAR:
                        groupedQuery = query.GroupBy(x => new { x.Year }).Select(y => new GroupUserVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.MONTH:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month }).Select(y => new GroupUserVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.DAY:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day }).Select(y => new GroupUserVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Day = y.Key.Day,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.HOUR:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day, x.Hour }).Select(y => new GroupUserVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Day = y.Key.Day,
                            Hour = y.Key.Hour,
                            Items = y.ToList(),
                        });
                        break;
                    default:
                        break;
                }

                if (groupedQuery != null)
                {
                    foreach (GroupUserVoteStatistic group in groupedQuery)
                    {
                        result.Add(new DataStatisticUserVoteDto
                        {
                            Year = group.Year,
                            Month = group.Month,
                            Day = group.Day,
                            Hour = group.Hour,
                            CountUserVote = group.Items.Count,
                        });
                    }
                }
                result = result.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day).ThenBy(x => x.Hour).ToList();
                return DataResult.ResultSuccess(result, "Statistic user vote success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<DataResult> GetStatisticsCityVote(StatisticsGetCityVoteInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var totalUsers = _userRepos.GetAll().AsQueryable().Count();
                totalUsers = totalUsers > 0 ? totalUsers : 1 > 0 ? totalUsers : 1;
                DateTime now = DateTime.Now;
                int monthCurrent = now.Month;
                int yearCurrent = now.Year;

                Dictionary<string, ResultStatisticsCityVote> dataResult = new Dictionary<string, ResultStatisticsCityVote>();

                switch (input.QueryCase)
                {
                    case QueryCaseCityVote.ByMonth:
                        if (monthCurrent >= input.NumberRange)
                        {
                            for (int index = monthCurrent - input.NumberRange + 1; index <= monthCurrent; index++)
                            {
                                var result = new ResultStatisticsCityVote();
                                var query = QueryGetAllCityVote();
                                result.CountVotes = query.WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                    .Count();

                                result.CountUserVotes = QueryGetAllUserVote()
                                     .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                     .Count();
                                result.TotalUsers = totalUsers > 0 ? totalUsers : 1;
                                result.PercentUserVotes = result.CountVotes > 0 ? (float)Math.Round((float)((float)result.CountUserVotes / (float)totalUsers / result.CountVotes), 3) : 0;

                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                        }
                        else
                        {
                            for (var index = 13 - (input.NumberRange - monthCurrent); index <= 12; index++)
                            {
                                var result = new ResultStatisticsCityVote();
                                var query = QueryGetAllCityVote();
                                result.CountVotes = query.WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent - 1)
                                    .Count();

                                result.CountUserVotes = QueryGetAllUserVote()
                                     .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                     .Count();
                                result.TotalUsers = totalUsers > 0 ? totalUsers : 1;
                                result.PercentUserVotes = result.CountVotes > 0 ? (float)Math.Round((float)((float)result.CountUserVotes / (float)totalUsers / result.CountVotes), 3) : 0;

                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                            for (var index = 1; index <= monthCurrent; index++)
                            {
                                var result = new ResultStatisticsCityVote();
                                var query = QueryGetAllCityVote();
                                result.CountVotes = query.WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                    .Count();

                                result.CountUserVotes = QueryGetAllUserVote()
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                    .Count();
                                result.TotalUsers = totalUsers > 0 ? totalUsers : 1;
                                result.PercentUserVotes = result.CountVotes > 0 ? (float)Math.Round((float)((float)result.CountUserVotes / (float)totalUsers / result.CountVotes), 3) : 0;

                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                        }
                        break;

                    default:
                        break;
                }

                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                mb.statisticMetris(t1, 0, "gall_statistics_object");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> GetCountCityVoteStatics()
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var count = await _cityVoteRepos.GetAll()
                    .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                    .Where(x => x.OrganizationUnitId.HasValue).CountAsync();
                return DataResult.ResultSuccess(count, "Get success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        #endregion

        #region Common
        private async Task NotifierNewVoteUser(CityVote vote, string creatername = "Ban quản trị")
        {
            var detailUrlApp = $"yoolife://app/evote/detail?id={vote.Id}";
            var citizens = (from cz in _citizenRepos.GetAll()
                            where cz.State == STATE_CITIZEN.ACCEPTED
                            select new UserIdentifier(cz.TenantId, cz.AccountId ?? 0)).Distinct().ToList();

            var messageDeclined = new UserMessageNotificationDataBase(
                             AppNotificationAction.CityVoteNew,
                             AppNotificationIcon.CityVoteNewIcon,
                              TypeAction.Detail,
                                $"{creatername} đã tạo một khảo sát mới. Nhấn để xem chi tiết !",
                                detailUrlApp,
                                "",
                                "",
                                ""

                             );
            await _appNotifier.SendMessageNotificationInternalAsync(
                "Yoolife khảo sát số !",
                $"{creatername} đã tạo một khảo sát mới. Nhấn để xem chi tiết !",
                 detailUrlApp,
                 "",
                 citizens.ToArray(),
                 messageDeclined,
                 AppType.USER
                );
        }
        #endregion

        public async Task<object> ExportVotesToExcel(CityVoteExcelExportInput input)
        {
            try
            {
                var orIds = (from ou in _appOrganizationUnitRepository.GetAll()
                             join uou in _userOrganizationUnitRepository.GetAll() on ou.ParentId equals uou.OrganizationUnitId into tb_uou
                             from uou in tb_uou
                             select new
                             {
                                 OrganizationUnitId = ou.ParentId,
                                 UserId = uou.UserId,
                                 Type = ou.Type
                             })

                    .Where(x => x.UserId == AbpSession.UserId && x.Type == APP_ORGANIZATION_TYPE.VOTE)
                    .Select(x => x.OrganizationUnitId).ToList();

                var totalUsers = _userRepos.GetAll().Count();
                totalUsers = totalUsers > 0 ? totalUsers : 1;
                var query = (from cv in _cityVoteRepos.GetAll()
                             join ou in _appOrganizationUnitRepository.GetAll() on cv.OrganizationUnitId equals ou.Id into tb_ou
                             from ou in tb_ou.DefaultIfEmpty()
                                 //join ci in _citizenRepos.GetAll() on cv.CreatorUserId equals ci.AccountId into tb_ci
                                 //from ci in tb_ci.DefaultIfEmpty()
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
                                 // Properties = cv.Properties,
                                 TenantId = cv.TenantId,
                                 LastModificationTime = cv.LastModificationTime,
                                 LastModifierUserId = cv.LastModifierUserId,
                                 TotalVotes = (from sv in _userVoteRepos.GetAll()
                                               where sv.CityVoteId == cv.Id
                                               select sv).Count(),
                                 TotalUsers = totalUsers > 0 ? totalUsers : 1,
                                 UserIsVoted = (from sv in _userVoteRepos.GetAll()
                                                where sv.CityVoteId == cv.Id && sv.CreatorUserId == AbpSession.UserId
                                                select sv).First(),
                                 OrganizationName = ou.DisplayName,
                                 IsShowNumbersVote = cv.IsShowNumbersVote
                             }).AsQueryable();
                if (orIds != null)
                {
                    query = query.Where(x => orIds.Contains(x.OrganizationUnitId.Value));
                }

                var now = DateTime.Now;
                switch (input.State)
                {
                    //new
                    case STATE_GET_VOTE.NEW:
                        query = query.Where(x => x.FinishTime >= DateTime.Now);
                        break;
                    // voted
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

                    case STATE_GET_VOTE.ALL:
                        break;
                }
                var list = await query.ToListAsync();
                foreach (var item in list)
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
                            opt.CountVote = await _userVoteRepos.GetAll()
                                .Where(x => x.CityVoteId == item.Id && x.OptionVoteId == opt.Id).CountAsync();
                            if (opt.CountVote > 0)
                            {
                                var usersDataResult = await GetUserVoteById(item.Id);

                                if (usersDataResult.Success)
                                {

                                    item.UserVoted = (List<UserVoteDto>)usersDataResult.Data;
                                }
                            }

                            opt.Percent = (float)Math.Round((float)((float)opt.CountVote / (float)totalUsers), 3);

                        }
                    }

                    //if (!string.IsNullOrWhiteSpace(item.OrganizationName))
                    //{
                    //    item.Name = $"( {item.OrganizationName} ) " + item.Name;
                    //}
                }
                var result = _cityVoteExcelExporter.ExportToFile(list);
                return DataResult.ResultSuccess(result, "Export Success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }

    }
}
