using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.ApbCore.Data;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using Yootek.Notifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface ITenantAdminBusinessAppService : IApplicationService
    {
        #region Object
        Task<object> GetAllObjectAsync(ObjectInputDto input);
        Task<object> CreateListObjectAsync(List<ObjectDto> input);
        Task<object> GetObjectByTypeAsync(int type);
        Task<object> CreateOrUpdateObject(CreateOrUpdateObjectInput input);
        Task<object> DeleteObject(long id);

        Task<object> UpdateStateObject(List<long> ids, int state);
        Task<object> DeleteListObject(List<long> ids);
        Task<object> GetStatisticsObject(StatisticsObjectInput input);
        #endregion
        #region Items
        Task<object> GetAllItemsAsync(ItemsInputDto input);
        Task<object> GetAllSetItemsAsync();
        Task<object> GetAllItemTypeAsync(ItemTypeInputDto input);
        Task<object> CreateOrUpdateItemsAsync(ItemsDto input);
        Task<object> CreateOrUpdateSetItemsAsync(SetItemsDto input);
        Task<object> CreateOrUpdateTypeItemAsync(ItemTypeDto input);

        Task<object> UpdateStateListItemsAsync(List<long> ids, int state);
        Task<object> GetAllItemViewSettingAsync(ItemViewSettingInputDto input);
        Task<object> GetItemViewSettingsAsync(ItemViewSettingInputDto input);
        Task<object> CreateOrUpdateItemViewSettingAsync(ItemViewSettingDto input);
        Task<object> DeleteItemViewSettingAsync(long id);
        Task<object> DeleteItemsAsync(long id);
        Task<object> DeleteSetItemsAsync(long id);
        Task<object> DeleteItemTypeAsync(long id);
        #endregion
        #region Order
        Task<object> GetAllOrderAsync();
        Task<object> GetAllOrderByObjectAsync(GetListOrderInput input);
        Task<object> CreateOrUpdateOrderAsync(OrderDto input);
        Task<object> UpdateStateOrder(OrderDto input);
        Task<object> DeleteOrderAsync(long id);
        object GetStatisticsOrder(GetAllOrderByMonthInput input);
        #endregion
        #region Voucher
        Task<object> GetAllVoucherAsync();
        Task<object> CreateOrUpdateVoucherAsync(VoucherDto input);
        Task<object> DeleteVoucherAsync(long id);
        #endregion
    }

    public class TenantAdminBusinessAppService : YootekAppServiceBase, ITenantAdminBusinessAppService
    {

        private readonly IRepository<ObjectPartner, long> _objectadminRepos;
        private readonly IRepository<Items, long> _itemsRepos;
        private readonly IRepository<ItemType, long> _itemTypeRepos;
        private readonly IRepository<BusinessNotify, long> _businessNotiRepos;
        private readonly IRepository<Voucher, long> _voucherRepos;
        private readonly IRepository<Order, long> _orderRepos;
        private readonly IRepository<SetItems, long> _setItemsRepos;
        private readonly IRepository<ItemViewSetting, long> _itemViewSettingRepos;
        private readonly IRepository<Rate, long> _rateRepos;
        private readonly ISqlExecuter _sqlExecute;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<ReportStore, long> _reportRepos;



        public TenantAdminBusinessAppService(
            IRepository<ObjectPartner, long> objectadminRepos,
            IRepository<Items, long> itemsRepos,
            IRepository<ItemType, long> itemTypeRepos,
            IRepository<BusinessNotify, long> businessNotiRepos,
            IRepository<Voucher, long> voucherRepos,
            IRepository<SetItems, long> setItemsRepos,
            IRepository<Order, long> orderRepos,
            IRepository<ItemViewSetting, long> itemViewSettingRepos,
            IRepository<Rate, long> rateRepos,
            ISqlExecuter sqlExecute,
            IAppNotifier appNotifier,
            IRepository<User, long> userRepos,
            IRepository<ReportStore, long> reportStore
            )
        {
            _objectadminRepos = objectadminRepos;
            _itemsRepos = itemsRepos;
            _itemTypeRepos = itemTypeRepos;
            _businessNotiRepos = businessNotiRepos;
            _voucherRepos = voucherRepos;
            _orderRepos = orderRepos;
            _setItemsRepos = setItemsRepos;
            _itemViewSettingRepos = itemViewSettingRepos;
            _rateRepos = rateRepos;
            _sqlExecute = sqlExecute;
            _appNotifier = appNotifier;
            _userRepos = userRepos;
            _reportRepos = reportStore;
        }


        #region Object
        public async Task<object> GetAllObjectAsync(ObjectInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _objectadminRepos.GetAllListAsync(x => x.TenantId == AbpSession.TenantId);
                var data = DataResult.ResultSuccess(result, "Get success");
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

        public async Task<object> GetObjectByTypeAsync(int type)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var result = await _objectadminRepos.FirstOrDefaultAsync(x => x.Type == type && x.CreatorUserId == AbpSession.UserId);
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "admin_gallbytype_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

      
        public async Task<object> CreateListObjectAsync(List<ObjectDto> input)
        {
            try
            {
                ConcurrentDictionary<int, Task<long>> concurrentTasks = new ConcurrentDictionary<int, Task<long>>();
                ConcurrentDictionary<int, Task<long>> taskRates = new ConcurrentDictionary<int, Task<long>>();
                long t1 = TimeUtils.GetNanoseconds();

                if (input != null)
                {
                    var index = 0;
                    foreach (var obj in input)
                    {
                        index++;
                        if (obj.Name != null)
                        {
                            var insertInput = obj.MapTo<ObjectPartner>();
                            insertInput.TenantId = AbpSession.TenantId;
                            await _objectadminRepos.InsertAsync(insertInput);
                            //if (!concurrentTasks.TryGetValue(index, out Task<long> value))
                            //{
                            //    concurrentTasks.TryAdd(index, taskInsert);
                            //}
                        }
                    }

                }
                await CurrentUnitOfWork.SaveChangesAsync();

                mb.statisticMetris(t1, 0, "admin_islist_obj");
                var data = DataResult.ResultSuccess("Insert success !");
                return data;

            }
            catch (Exception e)
            {
                throw;
            }
        }


      
        public async Task<object> CreateOrUpdateObject(CreateOrUpdateObjectInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _objectadminRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _objectadminRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_obj");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    if (input.Type > 200 && input.GroupType == null)
                    {
                        input.GroupType = input.Type / 100;
                    }
                    else if (input.GroupType == null)
                    {
                        input.GroupType = input.Type;
                    }
                    //Insert
                    input.State = (int)CommonENumObject.STATE_OBJECT.ACTIVE;
                    var insertInput = input.MapTo<ObjectPartner>();
                    long id = await _objectadminRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_obj");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> DeleteObject(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _objectadminRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> UpdateStateObject(List<long> ids, int state)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                StringBuilder sb = new StringBuilder();

                foreach (long id in ids)
                {
                    sb.AppendFormat("{0},", id);
                }

                var sql = string.Format("UPDATE ObjectPartners" +
                    "SET State = {1}, LastModificationTime = CURRENT_TIMESTAMP, DeleterUserId = {2}" +
                    " WHERE Id IN ({0})",
                    sb.ToString().TrimEnd(','), state, AbpSession.UserId);
                var par = new SqlParameter();
                //await _objectadminRepos.DeleteManyAsync(x => x.)
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
                var data = DataResult.ResultSuccess("Update state Success");
                mb.statisticMetris(t1, 0, "admin_udstateobj_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteListObject(List<long> ids)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                StringBuilder sb = new StringBuilder();

                foreach (long id in ids)
                {
                    sb.AppendFormat("{0},", id);
                }

                var sql = string.Format("UPDATE ObjectPartners" +
                    " SET IsDelete = 1,  DeleterUserId =  {1}, DeletionTime = CURRENT_TIMESTAMP " +
                    " WHERE Id IN ({0})",
                    sb.ToString().TrimEnd(','),
                    AbpSession.UserId
                    );
                var par = new SqlParameter();
                //await _objectadminRepos.DeleteManyAsync(x => x.)
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<int> QueryGetAllObjectByMonth(int month, int year, int? type)
        {
            try
            {
                if (type.HasValue)
                {
                    if (type >= 5 && type < 100)
                    {
                        var query = await _objectadminRepos.GetAllListAsync(x => x.CreationTime.Month == month && x.CreationTime.Year == year && x.TenantId == AbpSession.TenantId && x.Type > type * 100 && x.Type < (type * 100 + 20));
                        return query.Count();
                    }
                    else if (type < 5 || type > 200)
                    {
                        var query = await _objectadminRepos.GetAllListAsync(x => x.CreationTime.Month == month && x.CreationTime.Year == year && x.TenantId == AbpSession.TenantId && x.Type == type);
                        return query.Count();
                    }
                    else return 0;
                }
                else
                {
                    var query = await _objectadminRepos.GetAllListAsync(x => x.CreationTime.Month == month && x.CreationTime.Year == year && x.TenantId == AbpSession.TenantId);
                    return query.Count();
                }

                #region Data Common
                #endregion

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                return 0;
            }
        }
        public async Task<object> GetStatisticsObject(StatisticsObjectInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                DateTime now = DateTime.Now;
                int monthCurrent = now.Month;
                int yearCurrent = now.Year;

                Dictionary<string, int> dataResult = new Dictionary<string, int>();

                if (monthCurrent >= input.NumberMonth)
                {
                    for (int index = monthCurrent - input.NumberMonth + 1; index <= monthCurrent; index++)
                    {
                        int count = await QueryGetAllObjectByMonth(index, yearCurrent, input.Type);
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), count);
                    }
                }
                else
                {
                    for (var index = 13 - (input.NumberMonth - monthCurrent); index <= 12; index++)
                    {
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), await QueryGetAllObjectByMonth(index, yearCurrent - 1, input.Type));
                    }
                    for (var index = 1; index <= monthCurrent; index++)
                    {
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), await QueryGetAllObjectByMonth(index, yearCurrent, input.Type));
                    }
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

        #region Items
        public async Task<object> GetAllItemsAsync(ItemsInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemsRepos.GetAllListAsync(x => (input.Id != null && x.Id == input.Id) || (input.Type != null && x.Type == input.Type) || (input.Keyword != null && x.QueryKey.Contains(input.Keyword)));
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_item");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }


        public async Task<object> GetAllSetItemsAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _setItemsRepos.GetAllListAsync();
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_item");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }


        public async Task<object> GetAllItemTypeAsync(ItemTypeInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemTypeRepos.GetAllListAsync(x => (input.Id != null && x.Id == input.Id) || (input.Type != null && x.Type == input.Type));
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "admin_gall_itemtype");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }


        public async Task<object> GetAllItemViewSettingAsync(ItemViewSettingInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemViewSettingRepos.FirstOrDefaultAsync(x => (input.Id != null && x.Id == input.Id) || (input.Type != null && x.Type == input.Type) || (input.Keyword != null && x.QueryKey.Contains(input.Keyword)));
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "admin_gall_itemtype");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

        public async Task<object> GetItemViewSettingsAsync(ItemViewSettingInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemViewSettingRepos.GetAllListAsync(x => (input.Id != null && x.Id == input.Id) || (input.Type != null && x.Type == input.Type) || (input.Keyword != null && x.QueryKey.Contains(input.Keyword)));
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "admin_gall_itemtype");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

      
        public async Task<object> CreateOrUpdateItemsAsync(ItemsDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();


                if (input.Id > 0)
                {
                    //update
                    var updateData = await _itemsRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _itemsRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_item");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Items>();
                    long id = await _itemsRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_item");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
                throw;
            }
        }

      
        public async Task<object> CreateOrUpdateTypeItemAsync(ItemTypeDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _itemTypeRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _itemTypeRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_itemtype");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<ItemTypeDto>();
                    long id = await _itemTypeRepos.InsertAndGetIdAsync(insertInput);
                    if (id > 0)
                    {
                        insertInput.Id = id;
                    }
                    mb.statisticMetris(t1, 0, "admin_is_itemtype");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
                throw;
            }
        }


      
        public async Task<object> CreateOrUpdateSetItemsAsync(SetItemsDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _setItemsRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _setItemsRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_setitem");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<SetItems>();
                    long id = await _setItemsRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_setitem");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
                throw;
            }
        }


      
        public async Task<object> CreateOrUpdateItemViewSettingAsync(ItemViewSettingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _itemViewSettingRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _itemViewSettingRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_itemtype");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<ItemViewSetting>();
                    long id = await _itemViewSettingRepos.InsertAndGetIdAsync(insertInput);
                    if (id > 0)
                    {
                        insertInput.Id = id;
                    }
                    mb.statisticMetris(t1, 0, "admin_is_itemtview");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> UpdateStateListItemsAsync(List<long> ids, int state)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                StringBuilder sb = new StringBuilder();

                foreach (long id in ids)
                {
                    sb.AppendFormat("{0},", id);
                }

                var sql = string.Format("UPDATE Items " +
                    "SET State = {1}, LastModificationTime = CURRENT_TIMESTAMP, DeleterUserId = {2}" +
                    " WHERE Id IN ({0})",
                    sb.ToString().TrimEnd(','), state, AbpSession.UserId);
                //var par = new SqlParameter();
                //await _objectadminRepos.DeleteManyAsync(x => x.)
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
                var data = DataResult.ResultSuccess("Update state item Success");
                mb.statisticMetris(t1, 0, "admin_udstateitem_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> DeleteItemViewSettingAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _itemViewSettingRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_itemview");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteItemsAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _itemsRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_item");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteSetItemsAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _setItemsRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_setitem");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> DeleteItemTypeAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _itemTypeRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_setitem");
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

        #region Voucher
        public async Task<object> GetAllVoucherAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _voucherRepos.GetAllListAsync();
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "admin_gall_voucher");
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
        #region Order

        public async Task<object> GetAllOrderAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _orderRepos.GetAllListAsync();
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_voucher");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }


        }

        protected IQueryable<OrderDto> QueryGetAllOrderByObject(GetListOrderInput input)
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
                var query = (from order in _orderRepos.GetAll()
                             join obj in _objectadminRepos.GetAll() on order.ObjectPartnerId equals obj.Id into tb_obj
                             from obj in tb_obj.DefaultIfEmpty()
                             where order.TenantId != null && order.ObjectPartnerId == input.ObjectId
                             select new OrderDto()
                             {
                                 Id = order.Id,
                                 CreationTime = order.CreationTime,
                                 OrdererId = order.OrdererId,
                                 Orderer = order.Orderer,
                                 OrderCode = order.OrderCode,
                                 Properties = order.Properties,
                                 State = order.State,
                                 Type = order.Type,
                                 ObjectPartnerId = order.ObjectPartnerId,
                                 TenantId = order.TenantId,
                                 CreatorUserId = order.CreatorUserId,
                                 DeleterUserId = order.DeleterUserId,
                                 DeletionTime = order.DeletionTime,
                                 IsDeleted = order.IsDeleted,
                                 LastModificationTime = order.LastModificationTime,
                                 LastModifierUserId = order.LastModifierUserId,
                             })
                             .WhereIf(input.Type != null, x => x.Type == input.Type)
                             .AsQueryable();

                #region Data Common
                #endregion
                return query;

            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                return null;
            }
        }

        public async Task<object> GetAllOrderByObjectAsync(GetListOrderInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var query = QueryGetAllOrderByObject(input);
                    var list = await query.ToListAsync();
                    var data = DataResult.ResultSuccess(list, "Get success");
                    mb.statisticMetris(t1, 0, "gall_order_by_object");
                    return data;
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }

      
        public async Task<object> CreateOrUpdateOrderAsync(OrderDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _orderRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _orderRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_order");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Order>();
                    long id = await _orderRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_order");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
                throw;
            }
        }

      
        public async Task<object> UpdateStateOrder(OrderDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var updateData = await _orderRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    input.MapTo(updateData);
                    //call back
                    await _orderRepos.UpdateAsync(updateData);
                    var user = new UserIdentifier(updateData.TenantId, updateData.CreatorUserId.Value);

                    List<UserIdentifier> users = new List<UserIdentifier>();
                    users.Add(user);

                    var userUpdate = await _userRepos.FirstOrDefaultAsync(x => x.Id == AbpSession.UserId);
                    var state = "";
                    if (updateData.State == 2)
                        state = "xác nhận bởi " + userUpdate.FullName;
                    else if (updateData.State == 3)
                        state = "hoàn thành";
                    else if (updateData.State == 4)
                        state = "hủy bởi " + userUpdate.FullName;
                    var message = "Đơn hàng MĐH" + updateData.Id + " đã " + state;

                    await _appNotifier.MultiSendMessageAsync("App.AdminUpdateStateOrderMessage", users.ToArray(), message, true, false, false);
                }
                mb.statisticMetris(t1, 0, "Ud_feedback");

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

        public async Task<object> DeleteOrderAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _orderRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_order");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        protected IQueryable<OrderDto> QueryGetAllOrderByMonth(int month, int year)
        {
            try
            {
                var query = (from order in _orderRepos.GetAll()
                             where order.CreationTime.Month == month && order.CreationTime.Year == year
                             select new OrderDto()
                             {
                                 Id = order.Id,
                                 CreationTime = order.CreationTime,
                                 OrdererId = order.OrdererId,
                                 Orderer = order.Orderer,
                                 SetItemId = order.SetItemId,
                                 Properties = order.Properties,
                                 State = order.State,
                                 Type = order.Type,
                                 ObjectPartnerId = order.ObjectPartnerId,
                                 TenantId = order.TenantId,
                                 CreatorUserId = order.CreatorUserId,
                                 DeleterUserId = order.DeleterUserId,
                                 DeletionTime = order.DeletionTime,
                                 IsDeleted = order.IsDeleted,
                                 LastModificationTime = order.LastModificationTime,
                                 LastModifierUserId = order.LastModifierUserId,
                             })
                             .AsQueryable();

                #region Data Common
                #endregion
                return query;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                return null;
            }
        }

        public object GetStatisticsOrder(GetAllOrderByMonthInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                DateTime now = DateTime.Now;
                int monthCurrent = now.Month;
                int yearCurrent = now.Year;

                Dictionary<string, int> dataResult = new Dictionary<string, int>();

                if (monthCurrent >= input.NumberMonth)
                {
                    for (int index = monthCurrent - input.NumberMonth + 1; index <= monthCurrent; index++)
                    {
                        var query = QueryGetAllOrderByMonth(index, yearCurrent);
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), query.Count());
                    }
                }
                else
                {
                    for (var index = 13 - (input.NumberMonth - monthCurrent); index <= 12; index++)
                    {
                        var query = QueryGetAllOrderByMonth(index, yearCurrent - 1);
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), query.Count());
                    }
                    for (var index = 1; index <= monthCurrent; index++)
                    {
                        var query = QueryGetAllOrderByMonth(index, yearCurrent);
                        dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), query.Count());
                    }
                }

                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                mb.statisticMetris(t1, 0, "gall_statistics_order");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

      
        public async Task<object> CreateOrUpdateVoucherAsync(VoucherDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _orderRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _orderRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_order");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Order>();
                    long id = await _orderRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_order");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> DeleteVoucherAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _orderRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "admin_del_order");
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

        #region Report store
        public async Task<object> GetAllReportStoreAsync(ReportInputDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var test = AbpSession.TenantId;
                var query = (from report in _reportRepos.GetAll()
                             join obj in _objectadminRepos.GetAll() on report.ObjectPartnerId equals obj.Id into tb_obj
                             from obj in tb_obj.DefaultIfEmpty()

                             select new ReportOutputDto()
                             {
                                 ObjectPartnerId = report.ObjectPartnerId,
                                 NameObject = obj.Name,
                                 TypeReason = report.TypeReason,
                                 TypeService = obj.Type,
                                 Detail = report.Detail,
                                 UserCreator = (from user in _userRepos.GetAll()
                                                where user.Id == report.CreatorUserId
                                                select user).FirstOrDefault().FullName,
                                 TenantId = obj.TenantId
                             })
                             .WhereIf(input.ObjectPartnerId.HasValue, x => x.ObjectPartnerId == input.ObjectPartnerId)
                             .AsQueryable();

                var result = await query.ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
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
