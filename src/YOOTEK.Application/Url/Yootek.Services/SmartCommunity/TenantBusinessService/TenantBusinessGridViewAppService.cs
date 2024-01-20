using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto.GridViewBusiness;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface ITenantBusinessGridViewAppService : IApplicationService
    {
        #region Object
        Task<object> GetObjectDataAsync(GetObjectInputDto input);
        #endregion
        #region Item
        Task<object> GetItemDataAsync(GetItemInputDto input);
        #endregion
        #region Rate
        Task<object> GetRateDataAsync(GetRateInputDto input);
        #endregion
    }

    public class TenantBusinessGridViewAppService : YootekAppServiceBase, ITenantBusinessGridViewAppService
    {

        private readonly IRepository<ObjectPartner, long> _objectPartnerRepos;
        private readonly IRepository<ObjectType, long> _objectTypeRepos;
        private readonly IRepository<Items, long> _itemsRepos;
        private readonly IRepository<ItemType, long> _itemTypeRepos;
        private readonly IRepository<Rate, long> _rateRepos;
        private readonly IRepository<Order, long> _orderRepos;
        private readonly IRepository<Booking, long> _bookingRepos;
        private readonly IRepository<LocalService, long> _localServiceRepos;

        public TenantBusinessGridViewAppService(
            IRepository<ObjectPartner, long> objectPartnerRepos,
            IRepository<ObjectType, long> objectTypeRepos,
            IRepository<Items, long> itemsRepos,
            IRepository<ItemType, long> itemTypeRepos,
            IRepository<Rate, long> rateRepos,
            IRepository<Order, long> orderRepos,
            IRepository<Booking, long> bookingRepos,
            IRepository<LocalService, long> localServiceRepos)
        {
            _objectPartnerRepos = objectPartnerRepos;
            _objectTypeRepos = objectTypeRepos;
            _itemsRepos = itemsRepos;
            _itemTypeRepos = itemTypeRepos;
            _rateRepos = rateRepos;
            _orderRepos = orderRepos;
            _bookingRepos = bookingRepos;
            _localServiceRepos = localServiceRepos;
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
                             join t in _localServiceRepos.GetAll() on obj.Type.Value equals t.Type into tbl_obj
                             from o in tbl_obj.DefaultIfEmpty()
                                 //where obj.Type == input.Type
                                 //     let dt = (input.Latitude.HasValue && input.Longitude.HasValue && obj.Longitude.HasValue && obj.Latitude.HasValue) ? (new GeoCoordinate { Latitude = obj.Latitude.Value, Longitude = obj.Longitude.Value }.GetDistanceTo(coord)) / 1000.0 : 10000
                             select new ObjectDto()
                             {
                                 Id = obj.Id,
                                 Address = obj.Address,
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
                                 IsAdminCreate = obj.IsAdminCreate,
                                 Rates = new List<Rate>(),
                                 CountRate = 0,
                                 Rate = 0,

                                 CountNewOrder = (from booking in _bookingRepos.GetAll()
                                                  where booking.StoreId == obj.Id && booking.State == StateBooking.Requesting
                                                  select booking).AsQueryable().Count(),
                                 UrbanId = o.UrbanId,
                                 BuildingId = o.BuildingId

                             })
                             .WhereIf(input.Type.HasValue, u => u.Type == input.Type)
                             .WhereIf(input.Id.HasValue, u => u.Id == input.Id)
                             .WhereIf(input.IsAdminCreate.HasValue, u => u.IsAdminCreate == input.IsAdminCreate)
                             .WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) || (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay))
                             .WhereIf(input.UrbanId.HasValue, u => u.UrbanId == input.UrbanId)
                             .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                             .AsQueryable();

                #region Data Common
                #endregion
                #region Truy van tung Form
                if (input.FormId != (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_LOCATIONMAP && input.Combo != null && input.Combo.IsActive)
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
            // if (input.Latitude != null && input.Longitude != null)
            // {
            //     var coord = (input.Latitude.HasValue && input.Longitude.HasValue) ? new GeoCoordinate(input.Latitude.Value, input.Longitude.Value) : null;
            //     var point = new Location(input.Latitude.Value, input.Longitude.Value);
            //     var location1 = CalculateLocationByDistance(point, 20, -Math.PI / 4.0);
            //     var location2 = CalculateLocationByDistance(point, 20, 3 * Math.PI / 4.0);
            //     query = query
            //         .Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE)
            //         .Where(x => x.Latitude < location1.Latitude && x.Longitude > location1.Longitude && x.Latitude > location2.Latitude && x.Longitude < location2.Longitude);
            //     query = (from obj in query
            //              let dt = (input.Latitude.HasValue && input.Longitude.HasValue && obj.Longitude.HasValue && obj.Latitude.HasValue) ? (new GeoCoordinate { Latitude = obj.Latitude.Value, Longitude = obj.Longitude.Value }.GetDistanceTo(coord)) / 1000.0 : 10000
            //              select new ObjectDto()
            //              {
            //                  Id = obj.Id,
            //                  CreationTime = obj.CreationTime,
            //                  CreatorUserId = obj.CreatorUserId,
            //                  DeleterUserId = obj.DeleterUserId,
            //                  DeletionTime = obj.DeletionTime,
            //                  IsDeleted = obj.IsDeleted,
            //                  LastModificationTime = obj.LastModificationTime,
            //                  LastModifierUserId = obj.LastModifierUserId,
            //                  Like = obj.Like,
            //                  Name = obj.Name,
            //                  Operator = obj.Operator,
            //                  Owner = obj.Owner,
            //                  Properties = obj.Properties,
            //                  QueryKey = obj.QueryKey,
            //                  PropertyHistories = obj.PropertyHistories,
            //                  StateProperties = obj.StateProperties,
            //                  Type = obj.Type,
            //                  GroupType = obj.GroupType,
            //                  State = obj.State,
            //                  Latitude = obj.Latitude,
            //                  Longitude = obj.Longitude,
            //                  IsDataStatic = obj.IsDataStatic,
            //                  SocialTenantId = obj.SocialTenantId,
            //                  Rates = obj.Rates,
            //                  CountRate = obj.CountRate,
            //                  Rate = obj.Rate,
            //                  Distance = dt,
            //                  TenantId = obj.TenantId,
            //                  UrbanId = obj.UrbanId,
            //                  BuildingId = obj.BuildingId
            //              });
            // }

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
                    query = query.Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE).OrderByDescending(x => x.Rate);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_LOW_RATE:
                    query = query.Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE).OrderByDescending(x => x.Rate.HasValue).ThenBy(x => x.Rate);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_DETAIL:
                    //query = (from obj in query
                    //         join pd in _itemsRepos.GetAll() on obj.Id equals pd.ObjectPartnerId
                    //         select obj 
                    //         )
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_LOCATIONMAP:
                    query = query.Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE).OrderBy(x => x.CreationTime);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_SEARCHING_OBJECT:
                    var listKey = input.Keyword != null ? input.Keyword.Split('+') : null;
                    if (listKey != null)
                    {
                        foreach (var key in listKey)
                        {
                            query = query.Where(u => u.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE && (u.QueryKey.ToLower().Contains(key.ToLower()) || u.Name.ToLower().Contains(key.ToLower()) || u.Properties.ToLower().Contains(key.ToLower())))
                          .OrderBy(x => x.Name);
                        }
                    }
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_CREATE:
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_NEW:
                    query = query.Where(u => u.TenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.NEW);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_ACTIVE:
                    query = query.Where(u => u.TenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_OBJECT_DISABLE:
                    query = query.Where(u => u.TenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.DISABLE);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_OBJECT_REFUSE:
                    query = query.Where(u => u.TenantId == (int)AbpSession.TenantId && u.State == (int)CommonENumObject.STATE_OBJECT.REFUSE);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_TENANT_GET_OBJECT_GETALL:

                    break;
            }

            return query;
        }
        public async Task<object> GetObjectDataAsync(GetObjectInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var obj = new object();
                var query = QueryGetAllData(input).ApplySearchFilter(input.Keyword, x => x.Name, x => x.Address);
                var count = 0;
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
                            var list = query.ToList();

                            obj = list.OrderBy(x => x.Distance).Skip(input.SkipCount).Take(input.MaxResultCount);
                        }
                        else obj = null;
                    }
                    else
                    {
                        var list = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
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
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetObjectDataByIdAsync(long id)
        {
            try
            {
                var data = await _objectPartnerRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        #region Item
        protected IQueryable<ItemsDto> QueryGetAllData(GetItemInputDto input)
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

                var query = (from item in _itemsRepos.GetAll()
                             join obj in _objectPartnerRepos.GetAll() on item.ObjectPartnerId equals obj.Id into tb_obj
                             from obj in tb_obj.DefaultIfEmpty()
                             where item.Type == input.Type
                             select new ItemsDto()
                             {
                                 Id = item.Id,
                                 CreationTime = item.CreationTime,
                                 CreatorUserId = item.CreatorUserId,
                                 DeleterUserId = item.DeleterUserId,
                                 DeletionTime = item.DeletionTime,
                                 IsDeleted = item.IsDeleted,
                                 LastModificationTime = item.LastModificationTime,
                                 LastModifierUserId = item.LastModifierUserId,
                                 Like = item.Like,
                                 Name = item.Name,
                                 ShopName = obj.Name,
                                 Properties = item.Properties,
                                 QueryKey = item.QueryKey,
                                 PropertyHistories = item.PropertyHistories,
                                 StateProperties = item.StateProperties,
                                 Type = item.Type,
                                 TypeGoods = item.TypeGoods,
                                 ObjectPartnerId = item.ObjectPartnerId,
                                 State = item.State,
                                 Rates = (from rate in _rateRepos.GetAll()
                                          where rate.ItemId == item.Id
                                          select rate).ToList(),
                                 CountRate = (from rate in _rateRepos.GetAll()
                                              where rate.ItemId == item.Id
                                              select rate).AsQueryable().Count(),
                                 Rate = (float)System.Math.Round((float)Queryable.Average(from rate in _rateRepos.GetAll()
                                                                                          where rate.ItemId == item.Id
                                                                                          select rate.RatePoint), 1),
                                 TenantId = obj.TenantId
                                 //Items = (input.FormCase == (int)CommonENumObject.FORMCASE_GET_DATA.OBJECT_DETAIL) ? (
                                 // (from it in _itemsRepos.GetAll()
                                 //  where it.ObjectPartnerId == obj.Id
                                 //  select it).ToList()
                                 //) : null

                             })
                             .WhereIf(!string.IsNullOrEmpty(input.Keyword), u => u.QueryKey.Contains(input.Keyword) || u.Name.Contains(input.Keyword) || u.Properties.Contains(input.Keyword))
                             .WhereIf(input.TypeGoods > 0, u => u.TypeGoods == input.TypeGoods)
                             .WhereIf(input.Id > 0, u => u.Id == input.Id)
                             .WhereIf(input.FromDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) || (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay))
                             .AsQueryable();

                #region Data Common
                #endregion
                #region Truy van tung Form

                switch (input.FormId)
                {
                    //san pham moi dang ky
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_ADMIN_GET_ITEM_NEW:
                        query = query.Where(x => x.State == null || x.State == (int)CommonENumItem.STATE_ITEM.NEW);
                        break;
                    //san pham da duoc duyet
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_ADMIN_GET_ITEM_ACTIVE:
                        query = query.Where(x => x.State != null && x.State == (int)CommonENumItem.STATE_ITEM.ACTIVE);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_ADMIN_GET_ITEM_GETALL:
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_ADMIN_ITEM_REFUSE:
                        query = query.Where(x => x.State != null && x.State == (int)CommonENumItem.STATE_ITEM.REFUSE);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_ADMIN_ITEM_DISABLE:
                        query = query.Where(x => x.State != null && x.State == (int)CommonENumItem.STATE_ITEM.DISABLE);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_PARTNER_ITEM_GETALL:
                        query = query.Where(x => x.ObjectPartnerId == input.ObjectPartnerId);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_PARTNER_ITEM_ACTIVE:
                        query = query.Where(x => (x.State == (int)CommonENumItem.STATE_ITEM.ACTIVE) && x.ObjectPartnerId == input.ObjectPartnerId);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_PARTNER_ITEM_NEW:
                        query = query.Where(x => (x.State == null || x.State == (int)CommonENumItem.STATE_ITEM.NEW) && x.ObjectPartnerId == input.ObjectPartnerId);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_PARTNER_ITEM_DETAIL:
                        query = query.Where(x => x.ObjectPartnerId == input.ObjectPartnerId);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_PARTNER_ITEM_DISABLE:
                        query = query.Where(x => (x.State == (int)CommonENumItem.STATE_ITEM.DISABLE) && x.ObjectPartnerId == input.ObjectPartnerId);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_PARTNER_ITEM_REFUSE:
                        query = query.Where(x => (x.State == (int)CommonENumItem.STATE_ITEM.DISABLE) && x.ObjectPartnerId == input.ObjectPartnerId);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_USER_ITEM_GETALL:
                        query = query.Where(x => x.State == (int)CommonENumItem.STATE_ITEM.ACTIVE).OrderByDescending(x => x.CreationTime);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_USER_ITEM_HIGHT_RATE:
                        query = query.Where(x => x.State == (int)CommonENumItem.STATE_ITEM.ACTIVE).OrderByDescending(x => x.Rate);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_USER_ITEM_LOW_RATE:
                        query = query.Where(x => x.State == (int)CommonENumItem.STATE_ITEM.ACTIVE).OrderByDescending(x => x.Rate.HasValue).ThenBy(x => x.Rate);
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_USER_ITEM_DETAIL:
                        //query = (from obj in query
                        //         join pd in _itemsRepos.GetAll() on obj.Id equals pd.ObjectPartnerId
                        //         select obj 
                        //         )
                        break;
                    case (int)CommonENumItem.FORM_ID_ITEM.FORM_USER_ITEM_GETALL_BY_OBJECT:
                        query = query.Where(x => x.ObjectPartnerId == input.ObjectPartnerId);
                        break;


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

        public async Task<object> GetItemDataAsync(GetItemInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = QueryGetAllData(input);
                int numberData = 0;
                var obj = new object();
                if (input.FormCase == null || input.FormCase == (int)CommonENumItem.FORMCASE_GET_DATA.GETALL_ITEM)
                {
                    numberData = query.Count();
                    obj = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                }
                else if (input.FormCase == (int)CommonENumItem.FORMCASE_GET_DATA.GET_DETAIL_ITEM)
                {
                    obj = await query.FirstOrDefaultAsync();
                }

                var data = DataResult.ResultSuccess(obj, "Get success", numberData);
                mb.statisticMetris(t1, 0, "gall_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        #endregion

        #region Rate
        protected IQueryable<RateDto> QueryGetAllData(GetRateInputDto input)
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
                var query = (from rate in _rateRepos.GetAll()
                                 //join obj in _objectPartnerRepos.GetAll() on rate.ObjectId equals obj.Id into tb_obj
                                 //from obj in tb_obj.DefaultIfEmpty()
                             join item in _itemsRepos.GetAll() on rate.ItemId equals item.Id into tb_item
                             from item in tb_item.DefaultIfEmpty()
                             join aswrt in _rateRepos.GetAll() on rate.Id equals aswrt.AnswerRateId into tb_aswrt
                             from aswrt in tb_aswrt.DefaultIfEmpty()
                             select new RateDto()
                             {
                                 ItemId = rate.ItemId,
                                 Comment = rate.Comment,
                                 CreationTime = rate.CreationTime,
                                 CreatorUserId = rate.CreatorUserId,
                                 DeleterUserId = rate.DeleterUserId,
                                 DeletionTime = rate.DeletionTime,
                                 Email = rate.Email,
                                 Id = rate.Id,
                                 Answerd = aswrt,
                                 HasAnswered = aswrt != null ? true : false,
                                 LastModificationTime = rate.LastModificationTime,
                                 LastModifierUserId = rate.LastModifierUserId,
                                 ObjectId = rate.ObjectId,
                                 RatePoint = rate.RatePoint,
                                 UserName = rate.UserName,
                                 IsItemReview = item != null ? true : false,
                                 Item = new ItemsDto()
                                 {
                                     Name = item.Name,
                                     Properties = item.Properties
                                 }
                             })
                             .WhereIf(input.ItemId != null, x => x.ItemId == input.ItemId)
                             .AsQueryable();

                #region Data Common
                #endregion
                #region Truy van tung Form
                switch (input.FormId)
                {
                    //san pham moi dang ky
                    case (int)CommonENumRate.FORM_ID_RATE.FORM_PARTNER_GETALL_REVIEW:
                        query = query.Where(x => x.ObjectId == input.ObjectPartnerId);
                        break;
                    case (int)CommonENumRate.FORM_ID_RATE.FORM_USER_GETALL_REVIEW_BY_PRODUCT:
                        query = query.Where(x => x.ItemId == input.ItemId);
                        break;
                    case (int)CommonENumRate.FORM_ID_RATE.FORM_USER_GETALL_REVIEW_BY_SHOP:
                        query = query.Where(x => x.ObjectId == input.ObjectPartnerId && x.ItemId == null);
                        break;
                    default:
                        break;
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

        public async Task<object> GetRateDataAsync(GetRateInputDto input)
        {
            try
            {

                long t1 = TimeUtils.GetNanoseconds();
                var obj = new object();
                var query = QueryGetAllData(input);
                var count = query.Count();
                var list = await query.ToListAsync();

                var data = DataResult.ResultSuccess(list, "Get success");
                mb.statisticMetris(t1, 0, "gall_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        #endregion

    }
}
