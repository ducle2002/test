using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Clb.Votes;
using Yootek.Yootek.Services.Yootek.Clb.Dto;
using Yootek.Notifications;
using Yootek.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Yootek.Yootek.Services.Yootek.Clb.Vote
{
    public interface IClbCityVoteAppService : IApplicationService
    {
        Task<object> GetStatisticCityVote(GetStatisticClbCityVoteInput input);
        Task<object> GetAllCityVoteAsync(GetAllCityVoteInput input);
        Task<object> CreateOrUpdateCityVote(ClbCityVoteDto input);
        Task<object> CreateCityVote(CreateClbCityVoteInput input);
        Task<object> UpdateCityVote(UpdateClbCityVoteInput input);
        Task<object> GetVoteByIdAsync(GetVoteByIdInput id);

    }

    //[AbpAuthorize(PermissionNames.Pages_Digitals_Surveys, PermissionNames.Pages_Government, PermissionNames.Pages_Government_Citizens_Vote)]
    [AbpAuthorize]
    public class ClbCityVoteAppService : YootekAppServiceBase, IClbCityVoteAppService
    {
        private readonly IRepository<ClbCityVote, long> _cityVoteRepos;
        private readonly IRepository<ClbUserVote, long> _userVoteRepos;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly IClbCityVoteExcelExporter _cityVoteExcelExporter;

        public ClbCityVoteAppService(
            IRepository<ClbCityVote, long> cityVoteRepos,
            IRepository<ClbUserVote, long> userVoteRepos,
            IRepository<User, long> userRepos,
            IRepository<Citizen, long> citizenRepos,
            IAppNotifier appNotifier,
            IClbCityVoteExcelExporter cityVoteExcelExporter
            )
        {
            _cityVoteRepos = cityVoteRepos;
            _userVoteRepos = userVoteRepos;
            _userRepos = userRepos;
            _citizenRepos = citizenRepos;
            _appNotifier = appNotifier;
            _cityVoteExcelExporter = cityVoteExcelExporter;
        }


        #region Vote

        public async Task<object> GetAllCityVoteAsync(GetAllCityVoteInput input)
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
                                 Description = cv.Description,
                                 Id = cv.Id,
                                 StartTime = cv.StartTime,
                                 FinishTime = cv.FinishTime,
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
                                 IsShowNumbersVote = cv.IsShowNumbersVote,
                                 Status = cv.Status,
                             })
                             .ApplySearchFilter(input.Keyword, x => x.Name)
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


                /*if (input.OrganizationUnitId.HasValue && orIds.Contains(input.OrganizationUnitId))
                {
                    query = query.Where(x => x.OrganizationUnitId == input.OrganizationUnitId);
                }*/

                var dataGrids = await query.PageBy(input).ToListAsync();

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

                        if (item.VoteOptions == null) continue;
                        foreach (var opt in item.VoteOptions)
                        {
                            opt.CountVote = await _userVoteRepos.GetAll()
                                .Where(x => x.CityVoteId == item.Id && x.OptionVoteId == opt.Id).CountAsync();

                            opt.Percent = (float)Math.Round((float)((float)opt.CountVote / (float)totalUsers), 3);

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
                var cityVote = await _cityVoteRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("CityVote not found");
                long totalUsers = _userRepos.GetAll().Count();
                var cityVoteDto = cityVote.MapTo<ClbCityVoteDto>();
               // cityVoteDto.DetailLinkApp = $"yoolife://app/evote/detail?id={cityVote.Id}";
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
        public async Task<object> CreateOrUpdateCityVote(ClbCityVoteDto input)
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
                    var insertInput = input.MapTo<ClbCityVote>();
                    long id = await _cityVoteRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;

                    var createrName = "Ban quản trị";

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
        public async Task<object> CreateCityVote(CreateClbCityVoteInput input)
        {
            try
            {
                if (input.StartTime >= input.FinishTime || input.FinishTime <= DateTime.Now)
                    throw new UserFriendlyException(400, "DateTime is invalid");
                var cityVoteInsert = input.MapTo<ClbCityVote>();
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
                var id = await _cityVoteRepos.InsertAndGetIdAsync(cityVoteInsert);
                cityVoteInsert.Id = id;
                var createrName = "Ban quản trị";

                await NotifierNewVoteUser(cityVoteInsert, createrName);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> UpdateCityVote(UpdateClbCityVoteInput input)
        {
            try
            {
                var cityVoteUpdate = await _cityVoteRepos.FirstOrDefaultAsync(input.Id)
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

        #endregion

        #region Statistics
        private IQueryable<ClbCityVoteDto> QueryGetAllCityVote()
        {
            var query = (from vt in _cityVoteRepos.GetAll()
                         select new ClbCityVoteDto()
                         {
                             Id = vt.Id,
                             CreationTime = vt.CreationTime,
                         }).AsQueryable();

            return query;
        }

        private IQueryable<ClbUserVote> QueryGetAllUserVote()
        {
            var query = (from vt in _userVoteRepos.GetAll()
                         select new ClbUserVote()
                         {
                             Id = vt.Id,
                             CreationTime = vt.CreationTime
                         }).AsQueryable();
            return query;
        }
        public async Task<object> GetStatisticCityVote(GetStatisticClbCityVoteInput input)
        {
            try
            {
                var query = _cityVoteRepos.GetAll()
                    .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId)
                    .Where(x => x.StartTime >= input.DateStart)
                    .Where(x => x.StartTime < input.DateEnd)
                    .Select(x => new StatisticClbCityVoteDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Year = x.StartTime.Year,
                        Month = x.StartTime.Month,
                        Day = x.StartTime.Day,
                        StartTime = x.StartTime,
                        FinishTime = x.FinishTime,
                    }).AsEnumerable();
                List<DataStatisticClbCityVoteDto> result = new();
                IEnumerable<GroupClbCityVoteStatistic> groupedQuery = null;

                switch (input.QueryDateTime)
                {
                    case QueryCaseDateTime.YEAR:
                        groupedQuery = query.GroupBy(x => new { x.Year }).Select(y => new GroupClbCityVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.MONTH:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month }).Select(y => new GroupClbCityVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.DAY:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day }).Select(y => new GroupClbCityVoteStatistic()
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
                    foreach (GroupClbCityVoteStatistic group in groupedQuery)
                    {
                        result.Add(new DataStatisticClbCityVoteDto
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
        public async Task<object> GetStatisticUserVote(GetStatisticClbUserVoteInput input)
        {
            try
            {
                var query = _userVoteRepos.GetAll()
                    .Where(x => x.CityVoteId == input.VoteId)
                    .Select(x => new StatisticClbUserVoteDto
                    {
                        Id = x.Id,
                        Year = x.CreationTime.Year,
                        Month = x.CreationTime.Month,
                        Day = x.CreationTime.Day,
                        Hour = x.CreationTime.Hour,
                        OptionVoteId = x.OptionVoteId,
                        Comment = x.Comment,
                    }).AsEnumerable();
                List<DataStatisticClbUserVoteDto> result = new();
                IEnumerable<GroupClbUserVoteStatistic> groupedQuery = null;

                switch (input.QueryDateTime)
                {
                    case QueryCaseDateTime.YEAR:
                        groupedQuery = query.GroupBy(x => new { x.Year }).Select(y => new GroupClbUserVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.MONTH:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month }).Select(y => new GroupClbUserVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.DAY:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day }).Select(y => new GroupClbUserVoteStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Day = y.Key.Day,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.HOUR:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day, x.Hour }).Select(y => new GroupClbUserVoteStatistic()
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
                    foreach (var group in groupedQuery)
                    {
                        result.Add(new DataStatisticClbUserVoteDto
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
        public async Task<DataResult> GetStatisticsCityVote(StatisticsGetClbCityVoteInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var totalUsers = _userRepos.GetAll().AsQueryable().Count();
                totalUsers = totalUsers > 0 ? totalUsers : 1 > 0 ? totalUsers : 1;
                DateTime now = DateTime.Now;
                int monthCurrent = now.Month;
                int yearCurrent = now.Year;

                var dataResult = new Dictionary<string, ResultStatisticsClbCityVote>();

                switch (input.QueryCase)
                {
                    case QueryCaseCityVote.ByMonth:
                        if (monthCurrent >= input.NumberRange)
                        {
                            for (int index = monthCurrent - input.NumberRange + 1; index <= monthCurrent; index++)
                            {
                                var result = new ResultStatisticsClbCityVote();
                                var query = QueryGetAllCityVote();
                                result.CountVotes = query
                                    .Count(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent);

                                result.CountUserVotes = QueryGetAllUserVote()
                                    .Count(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent);
                                result.TotalUsers = totalUsers > 0 ? totalUsers : 1;
                                result.PercentUserVotes = result.CountVotes > 0 ? (float)Math.Round((float)((float)result.CountUserVotes / (float)totalUsers / result.CountVotes), 3) : 0;

                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                        }
                        else
                        {
                            for (var index = 13 - (input.NumberRange - monthCurrent); index <= 12; index++)
                            {
                                var result = new ResultStatisticsClbCityVote();
                                var query = QueryGetAllCityVote();
                                result.CountVotes = query
                                    .Count(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent - 1);

                                result.CountUserVotes = QueryGetAllUserVote()
                                    .Count(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent);
                                result.TotalUsers = totalUsers > 0 ? totalUsers : 1;
                                result.PercentUserVotes = result.CountVotes > 0 ? (float)Math.Round((float)((float)result.CountUserVotes / (float)totalUsers / result.CountVotes), 3) : 0;

                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                            for (var index = 1; index <= monthCurrent; index++)
                            {
                                var result = new ResultStatisticsClbCityVote();
                                var query = QueryGetAllCityVote();
                                result.CountVotes = query
                                    .Count(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent);

                                result.CountUserVotes = QueryGetAllUserVote()
                                    .Count(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent);
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

        #endregion

        #region Common
        private async Task NotifierNewVoteUser(ClbCityVote vote, string creatername = "Ban quản trị")
        {
            var detailUrlApp = $"yoolife://app/evote/detail?id={vote.Id}";
            var citizens = (from cz in _citizenRepos.GetAll()
                            where cz.State == STATE_CITIZEN.ACCEPTED
                            select new UserIdentifier(cz.TenantId, cz.AccountId.Value)).Distinct().ToList();

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
                                 IsShowNumbersVote = cv.IsShowNumbersVote
                             }).AsQueryable();
                
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
                        item.VoteOptions = JsonConvert.DeserializeObject<List<ClbVoteOption>>(item.Options);
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
