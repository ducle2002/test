using Abp;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using GeoCoordinatePortable;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Services.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.DichVu.Business
{
    public interface IBusinessGridViewObjectAppService : IApplicationService
    {
        Task<object> GetObjectDataAsync(GetObjectInputDto input);

    }

    public class BusinessGridViewObjectAppService : YootekAppServiceBase, IBusinessGridViewObjectAppService
    {

        private readonly IRepository<ObjectPartner, long> _objectPartnerRepos;
        private readonly IRepository<ObjectType, long> _objectTypeRepos;
        private readonly IRepository<Items, long> _itemsRepos;
        private readonly IRepository<ItemType, long> _itemTypeRepos;
        private readonly IRepository<Rate, long> _rateRepos;

        public BusinessGridViewObjectAppService(
            IRepository<ObjectPartner, long> objectPartnerRepos,
            IRepository<ObjectType, long> objectTypeRepos,
            IRepository<Items, long> itemsRepos,
            IRepository<ItemType, long> itemTypeRepos,
            IRepository<Rate, long> rateRepos
            )
        {
            _objectPartnerRepos = objectPartnerRepos;
            _objectTypeRepos = objectTypeRepos;
            _itemsRepos = itemsRepos;
            _itemTypeRepos = itemTypeRepos;
            _rateRepos = rateRepos;
        }

        protected Location CalculateLocationByDistance(Location point1, double distance, double θ)
        {
            var lat = point1.Latitude * Math.PI / 180.0;
            var log = point1.Longitude * Math.PI / 180.0;
            var R = 6378.0;

            var lat2 = Math.Asin(Math.Sin(lat) * Math.Cos(distance / R) + Math.Cos(lat) * Math.Sin(distance / R) * Math.Cos(θ));
            var log2 = log + Math.Atan2(Math.Sin(θ) * Math.Sin(distance / R) * Math.Cos(lat), Math.Cos(distance / R) - Math.Sin(lat) * Math.Sin(lat2));

            return new Location(lat2 * 180.0 / Math.PI, log2 * 180.0 / Math.PI);
        }

        protected IQueryable<ObjectDto> QueryGetAllData(GetObjectInputDto input)
        {
            try
            {
                DateTime fromDay = new DateTime(), toDay = new DateTime();
                if (input.FromDay.HasValue)
                {
                    fromDay = new DateTime(input.FromDay.Value.Year, input.FromDay.Value.Month, input.FromDay.Value.Day, 0, 0, 0);

                }
                if (input.ToDay.HasValue)
                {
                    toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);

                }

                var query = (from obj in _objectPartnerRepos.GetAll()
                             where obj.TenantId == null
                             //where obj.Type == input.Type
                             //     let dt = (input.Latitude.HasValue && input.Longitude.HasValue && obj.Longitude.HasValue && obj.Latitude.HasValue) ? (new GeoCoordinate { Latitude = obj.Latitude.Value, Longitude = obj.Longitude.Value }.GetDistanceTo(coord)) / 1000.0 : 10000
                             select new ObjectDto()
                             {
                                 Id = obj.Id,
                                 CreationTime = obj.CreationTime,
                                 CreatorUserId = obj.CreatorUserId,
                                 DeleterUserId = obj.DeleterUserId,
                                 DeletionTime = obj.DeletionTime,
                                 IsDeleted = obj.IsDeleted,
                                 LastModificationTime = obj.LastModificationTime,
                                 LastModifierUserId = obj.LastModifierUserId,
                                 Like = obj.Like,
                                 Name = obj.Name,
                                 Operator = obj.Operator,
                                 Owner = obj.Owner,
                                 Properties = obj.Properties,
                                 QueryKey = obj.QueryKey,
                                 PropertyHistories = obj.PropertyHistories,
                                 StateProperties = obj.StateProperties,
                                 Type = obj.Type,
                                 GroupType = obj.GroupType,
                                 State = obj.State,
                                 Latitude = obj.Latitude,
                                 Longitude = obj.Longitude,
                                 DistrictId = obj.DistrictId,
                                 WardId = obj.WardId,
                                 ProvinceId = obj.ProvinceId,
                                 IsDataStatic = obj.IsDataStatic,
                                 SocialTenantId = obj.SocialTenantId,
                                 Rates = (from rate in _rateRepos.GetAll()
                                          where rate.ObjectId == obj.Id
                                          select rate)
                                          .ToList(),
                                 CountRate = obj.CountRate,
                                 Rate = obj.RatePoint,
                                 Items = (input.FormCase == (int)CommonENumObject.FORM_ID_OBJECT.FORM_PARTNER_OBJECT_DETAIL) ? (
                                  (from it in _itemsRepos.GetAll()
                                   where it.ObjectPartnerId == obj.Id
                                   select it).Take(10).ToList()
                                 ) : null

                             })
                             .WhereIf(input.Type >= 2 && input.Type < 100,
                                u => (u.GroupType.Value == input.Type))
                             .WhereIf(input.Type < 2 || input.Type > 200, u => u.Type == input.Type)
                             .WhereIf(input.Id.HasValue, u => u.Id == input.Id)
                             .WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) || (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay))
                             .AsQueryable();

                #region Data Common
                #endregion
                #region Truy van tung Form
                if (input.Combo != null && input.Combo.IsActive)
                {

                    if (input.Combo.FormIds != null && input.Combo.IsUnionFormId)
                    {
                        var conquery = query;
                        foreach (var formId in input.Combo.FormIds)
                        {
                            var newquery = QueryFormId(conquery, input, formId).AsEnumerable();
                            query = query.Union(newquery);
                        }
                    }
                    else if (input.Combo.FormIds != null)
                    {
                        foreach (var formId in input.Combo.FormIds)
                        {
                            query = QueryFormId(query, input, formId);
                        }
                    }
                }
                else
                {
                    query = QueryFormId(query, input, input.FormId.Value);
                }
                #endregion

                return query;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                return null;
            }
        }

        protected IQueryable<ObjectDto> QueryFormId(IQueryable<ObjectDto> query, GetObjectInputDto input, int formId)
        {
            var formid = input.FormId > 30 ? input.FormId / 10 : input.FormId;
            if ((input.FormId == 3 || input.FormId == 34) && input.Latitude != null && input.Longitude != null)
            {
                var coord = (input.Latitude.HasValue && input.Longitude.HasValue) ? new GeoCoordinate(input.Latitude.Value, input.Longitude.Value) : null;
                var point = new Location(input.Latitude.Value, input.Longitude.Value);
                var location1 = CalculateLocationByDistance(point, 20, -Math.PI / 4.0);
                var location2 = CalculateLocationByDistance(point, 20, 3 * Math.PI / 4.0);
                query = query
                    .Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE)
                    .Where(x => x.Latitude < location1.Latitude && x.Longitude > location1.Longitude && x.Latitude > location2.Latitude && x.Longitude < location2.Longitude);
                query = (from obj in query
                         let dt = (input.Latitude.HasValue && input.Longitude.HasValue && obj.Longitude.HasValue && obj.Latitude.HasValue) ? (new GeoCoordinate { Latitude = obj.Latitude.Value, Longitude = obj.Longitude.Value }.GetDistanceTo(coord)) / 1000.0 : 10000
                         select new ObjectDto()
                         {
                             Id = obj.Id,
                             CreationTime = obj.CreationTime,
                             CreatorUserId = obj.CreatorUserId,
                             DeleterUserId = obj.DeleterUserId,
                             DeletionTime = obj.DeletionTime,
                             IsDeleted = obj.IsDeleted,
                             LastModificationTime = obj.LastModificationTime,
                             LastModifierUserId = obj.LastModifierUserId,
                             Like = obj.Like,
                             Name = obj.Name,
                             Operator = obj.Operator,
                             Owner = obj.Owner,
                             Properties = obj.Properties,
                             QueryKey = obj.QueryKey,
                             PropertyHistories = obj.PropertyHistories,
                             StateProperties = obj.StateProperties,
                             Type = obj.Type,
                             GroupType = obj.GroupType,
                             State = obj.State,
                             Latitude = obj.Latitude,
                             Longitude = obj.Longitude,
                             IsDataStatic = obj.IsDataStatic,
                             SocialTenantId = obj.SocialTenantId,
                             Rates = obj.Rates,
                             CountRate = obj.CountRate,
                             Rate = obj.Rate,
                             Distance = dt

                         });
            }

            switch (formId)
            {
                //cua hang moi dang ky
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_GET_OBJECT_NEW:
                    query = query.Where(x => x.State == null || x.State == (int)CommonENumObject.STATE_OBJECT.NEW).OrderBy(x => x.CreationTime);
                    break;
                //cua hang da duoc duyet
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_GET_OBJECT_ACTIVE:
                    query = query.Where(x => x.State != null && x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE).OrderBy(x => x.CreationTime);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_GET_OBJECT_GETALL:
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_OBJECT_REFUSE:
                    query = query.Where(x => x.State != null && x.State == (int)CommonENumObject.STATE_OBJECT.REFUSE).OrderBy(x => x.CreationTime);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_OBJECT_DISABLE:
                    query = query.Where(x => x.State != null && x.State == (int)CommonENumObject.STATE_OBJECT.DISABLE).OrderBy(x => x.CreationTime);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_PARTNER_OBJECT_GETALL:
                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId && x.SocialTenantId == AbpSession.TenantId);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_PARTNER_OBJECT_NEW:
                    query = query.Where(x => (x.State == null || x.State == (int)CommonENumObject.STATE_OBJECT.NEW) && x.CreatorUserId == AbpSession.UserId);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_PARTNER_OBJECT_DETAIL:
                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_GETALL:
                    query = query.Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE).OrderBy(x => x.CreationTime);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_HIGHT_RATE:
                    query = query.Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE).Where(x => x.Rate > 3.5).OrderByDescending(x => x.Rate);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_LOW_RATE:
                    query = query.Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE).Where(x => x.Rate < 4).OrderByDescending(x => x.Rate.HasValue).ThenBy(x => x.Rate);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_DETAIL:
                    //query = (from obj in query
                    //         join pd in _itemsRepos.GetAll() on obj.Id equals pd.ObjectPartnerId
                    //         select obj 
                    //         )
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_LOCATIONMAP:

                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_SEARCHING_OBJECT:
                    char[] arr = { '+', ' ' };
                    var listKey = input.Keyword != null ? input.Keyword.Trim().Split(arr) : null;
                    if (listKey != null)
                    {
                        foreach (var key in listKey)
                        {
                            query = query.Where(u => u.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE && u.Name.ToLower().Contains(key.ToLower()))
                          .OrderBy(x => x.Name);
                        }
                    }
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_CREATE:
                    query = query.Where(u => u.SocialTenantId == (int)AbpSession.TenantId && u.CreatorUserId == (int)AbpSession.UserId);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_NEW:
                    query = query.Where(u => u.SocialTenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.NEW);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_ACTIVE:
                    query = query.Where(u => u.SocialTenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_OBJECT_DISABLE:
                    query = query.Where(u => u.SocialTenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.DISABLE);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_OBJECT_REFUSE:
                    query = query.Where(u => u.SocialTenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.REFUSE);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_GETALL:
                    query = query.Where(u => u.SocialTenantId == (int)AbpSession.TenantId);
                    break;
            }

            return query;
        }
        public async Task<object> GetObjectDataAsync(GetObjectInputDto input)
        {
            try
            {
                var random = new Random();
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var obj = new object();
                    var query = QueryGetAllData(input);
                    var count = query.Count();
                    if (input.FormCase == null || input.FormCase == (int)CommonENumObject.FORMCASE_GET_DATA.OBJECT_GETALL)
                    {
                        if (input.FormId != 4)
                        {
                            count = query.Count();
                        }
                        if (input.FormId == (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_LOCATIONMAP)
                        {
                            if (input.Latitude != null && input.Longitude != null)
                            {
                                var list = await query.ToListAsync();

                                obj = list.OrderBy(x => x.Distance).Skip(input.SkipCount).Take(input.MaxResultCount);
                            }
                            else obj = null;
                        }
                        //if (input.FormId == (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_GETALL)
                        //{
                        //    query = query.Skip(random.Next(1,query.Count()/2+10)).Take(100).OrderBy(x => Guid.NewGuid());
                        //    var list = await query.ToListAsync();
                        //    obj = list.OrderBy(x => x.Distance).Skip(input.SkipCount).Take(input.MaxResultCount);
                        //}
                        else
                        {
                            var list = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                            obj = list;
                        }


                    }
                    else if (input.FormCase == (int)CommonENumObject.FORMCASE_GET_DATA.OBJECT_DETAIL)
                    {
                        obj = await query.FirstOrDefaultAsync();
                    }
                    else if (input.FormCase == (int)CommonENumObject.FORMCASE_GET_DATA.OBJECT_COUNT)
                    {
                        count = query.Count();
                    }

                    var data = DataResult.ResultSuccess(obj, "Get success", count);
                    mb.statisticMetris(t1, 0, "gall_obj");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw new AbpException(ex.Message);
            }
        }

        public async Task<object> GetNewBusinessCount()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    var count = await _objectPartnerRepos.GetAll()
                        .Where(x => x.SocialTenantId.Value == AbpSession.TenantId
                            && x.State == (int)CommonENumObject.STATE_OBJECT.NEW)
                        .CountAsync();
                    return DataResult.ResultSuccess(count, "Get success!");
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw new AbpException(ex.Message);
            }
        }


    }
}
