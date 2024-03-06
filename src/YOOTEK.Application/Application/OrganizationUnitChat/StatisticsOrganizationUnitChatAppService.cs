using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Chat;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Services.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Yootek.Authorization;
using Yootek.QueriesExtension;
using Yootek.Chat.Dto;
using Yootek.Authorization.Users;
using Abp.Authorization.Users;
using Yootek.Services;

namespace Yootek.Abp.Application.Chat.OrganizationUnitChat
{
    public interface IStatisticsOrganizationUnitChatAppService : IApplicationService
    {
    }

    public class StatisticsOrganizationUnitChatAppService : YootekAppServiceBase, IStatisticsOrganizationUnitChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IRepository<CitizenReflectComment, long> _citizenReflectChatRepos;
        private readonly IRepository<CitizenReflect, long> _reflectChatRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationRepository;

        public StatisticsOrganizationUnitChatAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IRepository<CitizenReflectComment, long> citizenReflectChatRepos,
            IRepository<CitizenReflect, long> reflectChatRepos,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationRepository
            )
        {
            _chatMessageRepository = chatMessageRepository;
            _citizenReflectChatRepos = citizenReflectChatRepos;
            _organizationUnitRepository = organizationUnitRepository;
            _userOrganizationRepository= userOrganizationRepository;
            _reflectChatRepos = reflectChatRepos;
        }


        public IQueryable<ChatMessage> QueryGetAllChatOrganization()
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = (from mes in _chatMessageRepository.GetAll()
                         select new ChatMessageStatic
                         {
                             Id = mes.Id,
                             CreationTime = mes.CreationTime,
                             UserId = mes.UserId,
                             IsOrganizationUnit = mes.IsOrganizationUnit,
                             Side = mes.Side,
                             UrbanId = _userOrganizationRepository.GetAll().Where(x => x.UserId == mes.UserId).Select(x => x.OrganizationUnitId).FirstOrDefault(),
                             BuildingId = _userOrganizationRepository.GetAll().Where(x => x.UserId == mes.UserId).Select(x => x.OrganizationUnitId).FirstOrDefault(),
                         })
                         .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                         .Where(x => x.IsOrganizationUnit == true && x.Side == ChatSide.Sender).AsQueryable();
            return query;

        }

        public IQueryable<CitizenReflectCommentStatic> QueryGetAllChatReflects()
        {
            List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
            var query = (from mes in _citizenReflectChatRepos.GetAll()
                         join rc in _reflectChatRepos.GetAll() on mes.FeedbackId equals rc.Id into tb_rc
                         from rc in tb_rc.DefaultIfEmpty()
                         select new CitizenReflectCommentStatic
                         {
                             Id = mes.Id,
                             CreationTime = mes.CreationTime,
                             UrbanId = rc.UrbanId,
                             BuildingId = rc.BuildingId

                         })
                         .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                         .AsQueryable();

            return query;

        }
        public async Task<DataResult> GetStatisticsChatOrganization(StatisticsGetOrganizationChatInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                DateTime now = DateTime.Now;
                int monthCurrent = now.Month;
                int yearCurrent = now.Year;

                Dictionary<string, ResultStatisticsOrganizationChat> dataResult = new Dictionary<string, ResultStatisticsOrganizationChat>();

                switch (input.QueryCase)
                {
                    case QueryCaseOrganizationChat.ByMonth:
                        if (monthCurrent >= input.NumberRange)
                        {
                            for (int index = monthCurrent - input.NumberRange + 1; index <= monthCurrent; index++)
                            {
                                var result = new ResultStatisticsOrganizationChat();
                                var query = QueryGetAllChatOrganization();
                                result.CountChatOrganizations = query
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                    .Count();
                                result.CountChatReflects = QueryGetAllChatReflects()
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                    .Count();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                        }
                        else
                        {
                            for (var index = 13 - (input.NumberRange - monthCurrent); index <= 12; index++)
                            {
                                var result = new ResultStatisticsOrganizationChat();
                                var query = QueryGetAllChatOrganization();
                                result.CountChatOrganizations = query
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent - 1)
                                    .Count();
                                result.CountChatReflects = QueryGetAllChatReflects()
                                  .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent - 1)
                                  .Count();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), result);
                            }
                            for (var index = 1; index <= monthCurrent; index++)
                            {
                                var result = new ResultStatisticsOrganizationChat();
                                var query = QueryGetAllChatOrganization();
                                result.CountChatOrganizations = query
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                    .Count();
                                result.CountChatReflects = QueryGetAllChatReflects()
                                    .Where(x => x.CreationTime.Month == index && x.CreationTime.Year == yearCurrent)
                                    .Count();
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
        public async Task<object> GetStatisticChatOrganization(GetStatisticOrganizationUnitChatInput input)
        {
            try
            {
                IEnumerable<StatisticOrganizationUnitChatDto> query = null;
                List<long> departmentChat = new();
                AppOrganizationUnit? organizationUnit = null;
                switch (input.QueryCaseScope)
                {
                    case QueryCaseScope.TENANT:
                        break;
                    case QueryCaseScope.URBAN:
                        organizationUnit = _organizationUnitRepository.FirstOrDefault((long)input.UrbanId)
                                ?? throw new Exception("Urban not found");
                        break;
                    case QueryCaseScope.BUILDING:
                        organizationUnit = _organizationUnitRepository.FirstOrDefault((long)input.BuildingId)
                                ?? throw new Exception("Building not found");
                        break;
                    default:
                        break;
                }
                departmentChat = _organizationUnitRepository.GetAll()
                    .Where(x => x.TenantId == input.TenantId && x.Code.StartsWith(organizationUnit.Code) && x.Type == APP_ORGANIZATION_TYPE.CHAT)
                    .Select(x => (long)x.ParentId)
                    .ToList();

                query = _chatMessageRepository.GetAll()
                    .Where(x => x.TenantId == input.TenantId
                        && x.CreationTime >= input.DateStart && x.CreationTime < input.DateEnd
                        && x.IsOrganizationUnit == true && x.Side == ChatSide.Sender
                        && departmentChat.Contains(x.TargetUserId))
                    .Select(x => new StatisticOrganizationUnitChatDto
                    {
                        Id = x.Id,
                        Year = x.CreationTime.Year,
                        Month = x.CreationTime.Month,
                        Day = x.CreationTime.Day,
                        CreationTime = x.CreationTime,
                        TargetUserId = x.TargetUserId,
                        UserId = x.UserId,
                    }).AsEnumerable();

                List<DataStatisticOrganizationUnitChat> result = new();
                IEnumerable<GroupOrganizationUnitChatStatistic> groupedQuery = null;

                switch (input.QueryCaseDateTime)
                {
                    case QueryCaseDateTime.YEAR:
                        groupedQuery = query.GroupBy(x => new
                        {
                            x.Year
                        }).Select(y => new GroupOrganizationUnitChatStatistic()
                        {
                            Year = y.Key.Year,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.MONTH:
                        groupedQuery = query.GroupBy(x => new
                        {
                            x.Year,
                            x.Month
                        }).Select(y => new GroupOrganizationUnitChatStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Items = y.ToList(),
                        });
                        break;
                    case QueryCaseDateTime.DAY:
                        groupedQuery = query.GroupBy(x => new { x.Year, x.Month, x.Day }).Select(y => new GroupOrganizationUnitChatStatistic()
                        {
                            Year = y.Key.Year,
                            Month = y.Key.Month,
                            Day = y.Key.Day,
                            Items = y.ToList(),
                        });
                        break;
                    default:
                        break;
                }

                if (groupedQuery != null)
                {
                    foreach (GroupOrganizationUnitChatStatistic group in groupedQuery)
                    {
                        result.Add(new DataStatisticOrganizationUnitChat
                        {
                            Year = group.Year,
                            Month = group.Month,
                            Day = group.Day,
                            TotalCount = group.Items.Count,
                        });
                    }
                }
                return result.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
    }
}
