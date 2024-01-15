using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.DichVu.Business
{
    public interface ITenantPartnerBusinessAppService : IApplicationService
    {
        #region Object
        Task<object> CreateOrUpdateObject(ObjectDto input);
        Task<object> DeleteObject(long id);
        Task<object> GetTypeObjectRegistedAsync();
        #endregion
        #region TypeObject
        Task<object> GetAllTypeObjectAsync();
        #endregion
        #region Items
        Task<object> GetAllItemsAsync();
        Task<object> GetAllSetItemsAsync();
        Task<object> GetAllItemTypeAsync();
        Task<object> CreateOrUpdateItemsAsync(ItemsDto input);
        Task<object> CreateOrUpdateSetItemsAsync(SetItemsDto input);
        Task<object> DeleteItemsAsync(long id);
        Task<object> DeleteSetItemsAsync(long id);
        Task<object> GetAllItemViewSettingAsync(int? type);
        //Task<object> CreateOrUpdateTypeItemAsync(ItemTypeDto input);
        Task<object> CreateOrUpdateItemViewSettingAsync(ItemViewSettingDto input);
        Task<object> DeleteItemViewSettingAsync(long id);
        //Task<object> DeleteItemTypeAsync(long id);
        #endregion
        #region Order
        Task<object> GetAllOrderAsync(OrderFilterDto input);
        Task<object> GetAllOrderRepairAsync(OrderFilterDto input);

        #endregion
        #region Voucher
        Task<object> GetAllVoucherAsync();
        Task<object> CreateOrUpdateVoucherAsync(VoucherDto input);
        Task<object> DeleteVoucherAsync(long id);
        #endregion

        #region Rating
        Task<object> CreateOrUpdateRateAsync(RateDto input);
        Task<object> DeleteRateAsync(long id);

        #endregion

    }

    public class TenantPartnerBusinessAppService : YootekAppServiceBase, ITenantPartnerBusinessAppService
    {

        private readonly IRepository<ObjectPartner, long> _objectPartnerRepos;
        private readonly IRepository<ObjectType, long> _objectTypeRepos;
        private readonly IRepository<Items, long> _itemsRepos;
        private readonly IRepository<ItemType, long> _itemTypeRepos;
        private readonly IRepository<BusinessNotify, long> _businessNotiRepos;
        private readonly IRepository<Voucher, long> _voucherRepos;
        private readonly IRepository<Order, long> _orderRepos;
        private readonly IRepository<SetItems, long> _setItemsRepos;
        private readonly IRepository<ItemViewSetting, long> _itemViewSettingRepos;
        private readonly IRepository<Rate, long> _rateRepos;
        public TenantPartnerBusinessAppService(
            IRepository<ObjectPartner, long> objectPartnerRepos,
            IRepository<ObjectType, long> objectTypeRepos,
            IRepository<Items, long> itemsRepos,
            IRepository<ItemType, long> itemTypeRepos,
            IRepository<BusinessNotify, long> businessNotiRepos,
            IRepository<Voucher, long> voucherRepos,
            IRepository<SetItems, long> setItemsRepos,
            IRepository<Order, long> orderRepos,
            IRepository<ItemViewSetting, long> itemViewSettingRepos,
            IRepository<Rate, long> rateRepos
            )
        {
            _objectPartnerRepos = objectPartnerRepos;
            _objectTypeRepos = objectTypeRepos;
            _itemsRepos = itemsRepos;
            _itemTypeRepos = itemTypeRepos;
            _businessNotiRepos = businessNotiRepos;
            _voucherRepos = voucherRepos;
            _orderRepos = orderRepos;
            _setItemsRepos = setItemsRepos;
            _itemViewSettingRepos = itemViewSettingRepos;
            _rateRepos = rateRepos;
        }


        #region Object


      
        public async Task<object> CreateOrUpdateObject(ObjectDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input.TenantId == null)
                {
                    input.TenantId = AbpSession.TenantId;
                }
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _objectPartnerRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _objectPartnerRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "partner_ud_obj");
                    var data = DataResult.ResultSuccess(updateData, "Insert success !");
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
                    var insertInput = input.MapTo<ObjectPartner>();
                    long id = await _objectPartnerRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "partner_is_obj");
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
                await _objectPartnerRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "partner_del_obj");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetTypeObjectRegistedAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await (from obj in _objectPartnerRepos.GetAll()
                                    where obj.CreatorUserId == AbpSession.UserId
                                    select new
                                    {
                                        Name = obj.Name,
                                        Type = obj.Type,
                                        Id = obj.Id
                                    }).ToListAsync();

                mb.statisticMetris(t1, 0, "GetTypeObjectRegistedAsync");
                var data = DataResult.ResultSuccess(result, "Get success !");
                return data;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        #region Type Object

        public async Task<object> GetAllTypeObjectAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _objectTypeRepos.GetAllListAsync();
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "gall_objType");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }


        public async Task<object> DeleteTypeObject(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _objectTypeRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "partner_del_obj");
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
        #endregion

        #region Items
        public async Task<object> GetAllItemsAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemsRepos.GetAllListAsync();
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


        public async Task<object> GetAllItemTypeAsync()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemTypeRepos.GetAllListAsync();
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
                    mb.statisticMetris(t1, 0, "partner_ud_item");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Items>();
                    long id = await _itemsRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "partner_is_item");
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
                    mb.statisticMetris(t1, 0, "partner_ud_setitem");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<SetItems>();
                    long id = await _setItemsRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "partner_is_objtype");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
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
                mb.statisticMetris(t1, 0, "partner_del_item");
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
                mb.statisticMetris(t1, 0, "partner_del_setitem");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllItemViewSettingAsync(int? type)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var result = await _itemViewSettingRepos.FirstOrDefaultAsync(x => x.Type == (type ?? 1));
                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "partner_gall_itemtype");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }


      
        public async Task<object> CreateOrUpdateItemViewSettingAsync(ItemViewSettingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();


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
                    mb.statisticMetris(t1, 0, "partner_ud_itemtype");

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
                    mb.statisticMetris(t1, 0, "partner_is_itemtview");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
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
                mb.statisticMetris(t1, 0, "partner_del_itemview");
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
                mb.statisticMetris(t1, 0, "partner_gall_voucher");
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

        public async Task<object> GetAllOrderAsync(OrderFilterDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = (from ord in _orderRepos.GetAll()
                             join obj in _objectPartnerRepos.GetAll() on ord.ObjectPartnerId equals obj.Id into tb_obj
                             from obj in tb_obj.DefaultIfEmpty()
                             join setItem in _setItemsRepos.GetAll() on ord.SetItemId equals setItem.Id into tb_sitem
                             from setItems in tb_sitem.DefaultIfEmpty()
                             where obj.CreatorUserId == AbpSession.UserId
                             && ord.Type == input.Type
                             && setItems.IsDeleted == false
                             && ord.IsDeleted == false
                             select new OrderDto()
                             {
                                 Id = ord.Id,
                                 CreationTime = ord.CreationTime,
                                 OrdererId = ord.OrdererId,
                                 Orderer = ord.Orderer,
                                 OrderCode = ord.OrderCode,
                                 Properties = ord.Properties,
                                 State = ord.State,
                                 Type = ord.Type,
                                 ObjectPartnerId = obj.Id,
                                 SetItemId = ord.SetItemId,
                                 Items = ord.Items,
                                 SetItems = setItems
                             })
                             .WhereIf(input.State.HasValue, x => x.State == input.State)
                             .AsQueryable();

                var result = query.OrderBy(x => x.OrderCode).Take(20).ToList();
                //var result = await query.OrderBy(x => x.OrderCode).Take(20).ToListAsync();
                //var result = query.Take(20).ToList();
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

        public async Task<object> GetAllOrderRepairAsync(OrderFilterDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var query = (from ord in _orderRepos.GetAll()
                                 join obj in _objectPartnerRepos.GetAll() on ord.ObjectPartnerId equals obj.Id into tb_obj
                                 from obj in tb_obj.DefaultIfEmpty()
                                 where obj.CreatorUserId == AbpSession.UserId
                                 && ord.Type == input.Type
                                 && ord.IsDeleted == false
                                 select new OrderDto()
                                 {
                                     Id = ord.Id,
                                     CreationTime = ord.CreationTime,
                                     OrdererId = ord.OrdererId,
                                     Orderer = ord.Orderer,
                                     OrderCode = ord.OrderCode,
                                     Properties = ord.Properties,
                                     State = ord.State,
                                     Type = ord.Type,
                                     ObjectPartnerId = obj.Id,
                                 })
                                 .WhereIf(input.State.HasValue, x => x.State == input.State)
                                 .AsQueryable();

                    var result = query.Take(20).ToList();
                    var data = DataResult.ResultSuccess(result, "Get success");
                    mb.statisticMetris(t1, 0, "gall_voucher");
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
                    mb.statisticMetris(t1, 0, "partner_ud_order");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Order>();
                    long id = await _orderRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "partner_is_order");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
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
                mb.statisticMetris(t1, 0, "partner_del_order");
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
                    mb.statisticMetris(t1, 0, "partner_ud_order");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Order>();
                    long id = await _orderRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "partner_is_order");
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
                mb.statisticMetris(t1, 0, "partner_del_order");
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

        #region Rate
      
        public async Task<object> CreateOrUpdateRateAsync(RateDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _rateRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _rateRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "admin_ud_rate");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Rate>();
                    long id = await _rateRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "admin_is_rate");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }


            }
            catch (Exception e)
            {
                throw;
            }
        }



        public async Task<object> DeleteRateAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _orderRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "user_del_order");
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

    }
}
