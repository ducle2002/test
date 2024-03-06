using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yootek.Authorization;
using Yootek.QueriesExtension;

namespace Yootek.Services
{
    public interface ITypeAdministrativeAppService : IApplicationService
    {
        Task<object> GetAdministratvieGridViewAsync(GetAdministratvieGridViewInputDto input);
        Task<object> GetAllTypeAdministrativeByUserAsync();
        Task<object> GetAllTypeAdministrativeByAdminAsync(GetAllTypeAdministrativeInput input);
        //Task<object> GetConfigAdministrativeGridViewAsync(GetConfigAdministrativeGridViewInputDto input);
        Task<object> GetAdministrativePropertyGridViewAsync(GetAdministrativePropertyInputDto input);
        Task<object> GetInitViewTypeAdministrativeAsync();

    }

    [AbpAuthorize]
    public class CommonAdministrativeAppService : YootekAppServiceBase, ITypeAdministrativeAppService
    {
        private readonly IRepository<TypeAdministrative, long> _typeAdministrativeRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<Administrative, long> _administrativeRepos;
        private readonly IRepository<AdministrativeValue, long> _valueAdministrativeRepos;
        private readonly IRepository<AdministrativeProperty, long> _propertyRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;


        public CommonAdministrativeAppService(
            IRepository<TypeAdministrative, long> typeAdministrativeRepos,
            IRepository<User, long> userRepo,
            IRepository<Administrative, long> administrativeRepos,
            IRepository<AdministrativeValue, long> valueAdministrativeRepos,
            IRepository<AdministrativeProperty, long> propertyRepos,
            IRepository<AppOrganizationUnit, long> organizationRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository
            )
        {
            _typeAdministrativeRepos = typeAdministrativeRepos;
            _administrativeRepos = administrativeRepos;
            _valueAdministrativeRepos = valueAdministrativeRepos;
            _propertyRepos = propertyRepos;
            _organizationRepos = organizationRepos;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _userRepos = userRepo;
        }

        public async Task<object> GetAdministratvieGridViewAsync(GetAdministratvieGridViewInputDto input)
        {
            try
            {
                // var ouUI = await _userOrganizationUnitRepository.GetAll()
                //     .Where(x => x.UserId == AbpSession.UserId)
                //     .Select(x => x.OrganizationUnitId)
                //     .ToListAsync();
                //var queryOu = await _organizationRepos.GetAll()
                //    .Where(o => o.Id == input.OrganizationUnitId)
                //    .Select(o => o.ParentId)
                //    .FirstOrDefaultAsync();
                var query = (from ad in _administrativeRepos.GetAll()
                             join us in _userRepos.GetAll() on ad.CreatorUserId equals us.Id into ad_us
                             from us in ad_us.DefaultIfEmpty()
                             select new AdministrativeDto()
                             {
                                 Id = ad.Id,
                                 OrganizationUnitId = ad.OrganizationUnitId,
                                 Properties = ad.Properties,
                                 TenantId = ad.TenantId,
                                 ADTypeId = ad.ADTypeId,
                                 CreationTime = ad.CreationTime,
                                 CreatorUserId = ad.CreatorUserId,
                                 CreatorUserName = us.FullName,
                                 CreatorUserAvatar = us.ImageUrl,
                                 LastModificationTime = ad.LastModificationTime,
                                 LastModifierUserId = ad.LastModifierUserId,
                                 State = ad.State,
                                 Name = (from t in _typeAdministrativeRepos.GetAll()
                                         where t.Id == ad.ADTypeId
                                         select t.Name).FirstOrDefault(),
                             })
                              //   .Where(x => ouUI.Contains(x.OrganizationUnitId.Value))
                              .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                              .WhereIf(input.ADTypeId > 0, x => x.ADTypeId == input.ADTypeId)
                              .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId)
                              .OrderByDescending(x => x.CreationTime)
                              .AsQueryable();
                switch (input.FormId)
                {
                    //user
                    case FormGetAD.UserGetAll:
                        query = query.Where(x => x.CreatorUserId == AbpSession.UserId);
                        break;
                    case FormGetAD.UserGetRequesting:
                        query = query.Where(x => x.CreatorUserId == AbpSession.UserId && (x.State == AdministrativeState.Requesting || x.State == 0));
                        break;
                    case FormGetAD.UserGetAccepted:
                        query = query.Where(x => x.CreatorUserId == AbpSession.UserId && x.State == AdministrativeState.Accepted);
                        break;
                    case FormGetAD.UserGetDenied:
                        query = query.Where(x => x.CreatorUserId == AbpSession.UserId && x.State == AdministrativeState.Denied);
                        break;

                    //admin
                    case FormGetAD.AdminGetAll:
                        query = query.Where(x => x.ADTypeId == input.ADTypeId);
                        break;
                    case FormGetAD.AdminGetRequesting:
                        query = query.Where(x => (x.State == AdministrativeState.Requesting || x.State == 0));
                        break;
                    case FormGetAD.AdminGetAccepted:
                        query = query.Where(x => x.State == AdministrativeState.Accepted);
                        break;
                    case FormGetAD.AdminGetDenied:
                        query = query.Where(x => x.State == AdministrativeState.Denied);
                        break;
                    default:
                        query = query.Take(0);
                        break;
                }

                var day = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                if (input.TimeSelect.HasValue)
                {
                    switch (input.TimeSelect.Value)
                    {
                        case FormTimeSelect.ALL:
                            break;
                        case FormTimeSelect.TODAY:

                            var headDay = new DateTime(day.Year, day.Month, day.Day);
                            var endDay = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);

                            query = query.Where(x => x.CreationTime >= headDay && x.CreationTime < endDay);
                            break;
                        case FormTimeSelect.TOMORROW:

                            var headTDay = new DateTime(day.Year, day.Month, day.Day).AddDays(1);
                            var endTDay = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59).AddDays(1);

                            query = query.Where(x => x.CreationTime >= headTDay && x.CreationTime < endTDay);
                            break;
                        case FormTimeSelect.THISWEEK:
                            var dayOfWeek = day.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)day.DayOfWeek;
                            //var maxDay = DateTime.DaysInMonth(day.Year, day.Month);
                            var headWeek = day.AddDays(1 - dayOfWeek);

                            var endWeek = day.AddDays(7 - dayOfWeek);
                            query = query.Where(x => x.CreationTime >= headWeek && x.CreationTime < endWeek);
                            break;
                        case FormTimeSelect.THISMONTH:

                            var headMonth = new DateTime(day.Year, day.Month, 1);
                            var endMonth = new DateTime(day.Year, day.Month, DateTime.DaysInMonth(day.Year,
                                                            day.Month), 23, 59, 59);
                            query = query.Where(x => x.CreationTime >= headMonth && x.CreationTime < endMonth);
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(input.KeySearch))
                {
                    var administrativeIds = FindAdministrativeValue(input.ADTypeId, input.KeySearch);
                    if (administrativeIds.Count > 0)
                    {
                        query = query.Where(x => administrativeIds.Contains(x.Id));
                    }
                }

                var result = await query.PageBy(input).ToListAsync();

                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        if (item.State == 0) item.State = AdministrativeState.Requesting;
                    }
                }
                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAdministrativeByIdAsync(long id)
        {
            try
            {
                var data = await _administrativeRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllTypeAdministrativeByUserAsync()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var result = await _typeAdministrativeRepos.GetAllListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success");
                    mb.statisticMetris(t1, 0, "gall_obj");
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

        public async Task<object> GetTypeAdministrativeByIdAsync(long id)
        {
            try
            {
                var data = await _typeAdministrativeRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllTypeAdministrativeByAdminAsync(GetAllTypeAdministrativeInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    // var ouUI = await _userOrganizationUnitRepository.GetAll()
                    // .Where(x => x.UserId == AbpSession.UserId)
                    // .Select(x => x.OrganizationUnitId)
                    // .ToListAsync();
                    var query = (from ci in _typeAdministrativeRepos.GetAll()
                                 select new TypeAdministrativeDto()
                                 {
                                     Id = ci.Id,
                                     TenantId = ci.TenantId,
                                     Name = ci.Name,
                                     OrganizationUnitId = ci.OrganizationUnitId,
                                     UrbanId = ci.UrbanId,
                                     BuildingId = ci.BuildingId,
                                     Detail = ci.Detail,
                                     ImageUrl = ci.ImageUrl,
                                     FileUrl = ci.FileUrl,
                                     Surcharge = ci.Surcharge,
                                     Price = ci.Price,
                                     PriceDetail = ci.PriceDetail,
                                     OrganizationUnitName = (from or in _organizationRepos.GetAll()
                                                             where or.Id == ci.OrganizationUnitId
                                                             select or.DisplayName).FirstOrDefault()
                                 })
                                 .ApplySearchFilter(input.Keyword, x => x.Name);



                    if (input.Keyword != null)
                    {
                        var listKey = input.Keyword.Split('+');
                        if (listKey != null)
                        {
                            foreach (var key in listKey)
                            {
                                query = query.Where(u => (u.Name.Contains(key) || u.Detail.Contains(key)));
                            }
                        }
                    }

                    var result = await query
                        .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                            .ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByTypeAdministrative.NAME)
                            .PageBy(input).ToListAsync();
                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
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

        public async Task<object> GetAdministrativePropertyGridViewAsync(GetAdministrativePropertyInputDto input)
        {
            try
            {
                var result = QueryAdministrativeProperty(input);
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

        public async Task<object> GetInitViewTypeAdministrativeAsync()
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from ci in _typeAdministrativeRepos.GetAll()
                                 select new TypeAdministrativeDto()
                                 {
                                     Id = ci.Id,
                                     Name = ci.Name,
                                     OrganizationUnitId = ci.OrganizationUnitId,
                                     UrbanId = ci.UrbanId,
                                     BuildingId = ci.BuildingId,
                                     FileUrl = ci.FileUrl
                                 })
                                 .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds);
                    var result = await query.ToListAsync();
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            item.Properties = QueryAdministrativeProperty(new GetAdministrativePropertyInputDto() { TypeId = item.Id });
                            
                        }
                    }
                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
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


        public List<long> FindAdministrativeValue(long ADTypeId, string keySearch)
        {
            try
            {
                var ids = (from id in _valueAdministrativeRepos.GetAll()
                           where id.Value.Contains(keySearch.ToLower())
                           select id.AdministrativeId.Value).ToList();
                return ids;

            }
            catch
            {
                return null;
            }
        }


        #region common

        /// <summary>
        /// It's a function that queries the database for a list of properties, and if the property is a
        /// table, it will also query the database for the table columns
        /// </summary>
        /// <param name="GetAdministrativePropertyInputDto"></param>
        /// <returns>
        /// A list of ADPropetyDto objects.
        /// </returns>
        private List<ADPropetyDto> QueryAdministrativeProperty(GetAdministrativePropertyInputDto input)
        {
            var query = (from pr in _propertyRepos.GetAll()
                         where pr.ParentId == null
                         select new ADPropetyDto()
                         {
                             TypeId = pr.TypeId,
                             ConfigId = pr.ConfigId,
                             Id = pr.Id,
                             Key = pr.Key,
                             Type = pr.Type,
                             Value = pr.Value,
                             DisplayName = pr.DisplayName,
                             ParentId = pr.ParentId,
                             IsTableColumn = pr.IsTableColumn,
                         })
                             .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                             .WhereIf(input.ConfigId.HasValue, x => x.ConfigId == input.ConfigId)
                             .WhereIf(input.TypeId.HasValue, x => x.TypeId == input.TypeId)
                             .AsQueryable();
            var result = query.ToList();
            if (result != null)
            {
                foreach (var item in result)
                {
                    if (item.Type == ADPropertyType.TABLE)
                    {
                        item.TableColumn = _propertyRepos.GetAllList(x => x.ParentId == item.Id);
                    }

                    else if (item.Type == ADPropertyType.OPTIONCHECKBOX)
                    {
                        item.OptionValues = _propertyRepos.GetAllList(x => x.ParentId == item.Id);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// It removes all diacritics from a string
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>
        /// a string.
        /// </returns>
        private string ConvertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        private bool CheckContains(string source, string key)
        {
            string sourceConvert = ConvertToUnSign3(source.ToUpper());
            string keyConvert = ConvertToUnSign3(key.ToUpper());
            if (sourceConvert.Contains(keyConvert)) return true;
            else return false;
        }

        private bool CheckAdministrativeExist(List<Administrative> list, Administrative ad)
        {
            if (list.Count() == 0) return true;
            foreach (var adSource in list) if (adSource.Id == ad.Id) return false;
            return true;
        }
        #endregion
    }
}
