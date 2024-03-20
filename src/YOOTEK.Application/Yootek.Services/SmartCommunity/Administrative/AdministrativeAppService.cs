using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Yootek.ApbCore.Data;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;

namespace Yootek.Services
{
    public interface IAdministrativeAppService : IApplicationService
    {
        #region administrative
        Task<object> CreateOrUpdateAdministrative(AdministrativeDto input);
        Task<DataResult> DeleteAdministrative(long id);
        Task<object> UpdateStateAdministrative(long id, int state);
        Task<object> UpdateConfirmationOwner(long id);
        #endregion

        #region Config
        Task<object> CreateOrUpdateType(TypeAdministrativeDto input);
        Task<object> DeleteType(long id);
        Task<long> CreateOrUpdateValue(ValueAdministrativeDto input);
        Task<object> DeleteValue(long id);
        Task<object> CreateOrUpdateProperty(ADPropetyInput input);
        Task<DataResult> CreateOrUpdateListProperty(List<ADPropetyInput> input);
        Task<DataResult> DeleteProperty(long id);


        #endregion
    }

    //[AbpAuthorize(PermissionNames.Pages_AdministrationService)]
    [AbpAuthorize]
    public class AdministrativeAppService : YootekAppServiceBase, IAdministrativeAppService
    {
        private readonly IRepository<TypeAdministrative, long> _typeAdministrativeRepos;
        private readonly IRepository<AdministrativeProperty, long> _administrativePropertiesRepos;
        private readonly IRepository<Administrative, long> _administrativeRepos;
        private readonly IRepository<AdministrativeValue, long> _valueAdministrativeRepos;
        private readonly IRepository<HomeMember, long> _homeMemberRepos;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<UserBill, long> _userBillRepos;
        private readonly IRepository<FcmTokens, long> _fcmTokenRepos;
        private readonly CloudMessagingManager _cloudMessagingManager;
        private readonly ISqlExecuter _sqlExecute;
        private readonly IAppNotifier _appNotifier;


        public AdministrativeAppService(
            IRepository<TypeAdministrative, long> typeAdministrativeRepos,
            IRepository<Administrative, long> administrativeRepos,
            IRepository<Citizen, long> citizenRepos,
            IRepository<AdministrativeValue, long> valueAdministrativeRepos,
            IRepository<AdministrativeProperty, long> administrativePropertiesRepos,
            IRepository<HomeMember, long> homeMemberRepos,
            IRepository<UserBill, long> userBillRepos,
            IRepository<FcmTokens, long> fcmTokenRepos,
            CloudMessagingManager cloudMessagingManager,
            IAppNotifier appNotifier,
            ISqlExecuter sqlExecute)
        {
            _typeAdministrativeRepos = typeAdministrativeRepos;
            _administrativeRepos = administrativeRepos;
            _administrativePropertiesRepos = administrativePropertiesRepos;
            _citizenRepos = citizenRepos;
            _valueAdministrativeRepos = valueAdministrativeRepos;
            _sqlExecute = sqlExecute;
            _userBillRepos = userBillRepos;
            _fcmTokenRepos = fcmTokenRepos;
            _homeMemberRepos = homeMemberRepos;
            _cloudMessagingManager = cloudMessagingManager;
            _appNotifier = appNotifier;
        }


        public async Task<object> CreateOrUpdateAdministrative(AdministrativeDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                if (input.Id > 0)
                {
                    //update
                    var updateData = await _administrativeRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        var oldProperties = updateData.Properties;
                        input.MapTo(updateData);
                        //call back
                        await _administrativeRepos.UpdateAsync(updateData);
                        await CreateOrUpdateValueWithAdministrative(input.Properties, input.Id, input.ADTypeId, oldProperties);
                    }
                    mb.statisticMetris(t1, 0, "Ud_administrative");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {

                    var insertInput = input.MapTo<Administrative>();
                    // Only admin can send administrative request
                    //var creatorUser = _homeMemberRepos.FirstOrDefault(x => x.Id == insertInput.CreatorUserId);
                    //if (creatorUser == null) { return null; }
                    //if (!creatorUser.IsAdmin) { return null; }
                    //
                    long id = await _administrativeRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    await CreateOrUpdateValueWithAdministrative(input.Properties, id, input.ADTypeId);
                    mb.statisticMetris(t1, 0, "is_administrative");
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


        public async Task<DataResult> HandleStateUserAdministrative(HandleStateAdministrativeInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var item = _administrativeRepos.FirstOrDefault(x => x.Id == input.Id);

                if (item != null && item.State != input.State)
                {
                    var typeAd = _typeAdministrativeRepos.FirstOrDefault(item.ADTypeId);
                    switch (input.State)
                    {
                        case AdministrativeState.Accepted:
                            item.State = input.State;
                            if (typeAd.Surcharge.HasValue && typeAd.Surcharge.HasValue)
                            {
                                var userBillId = await CreateUserBillAdministrative(item, typeAd);
                                item.UserBillId = userBillId;
                            }
                            await FireUserStateAdministrative(item);
                            break;
                        case AdministrativeState.Denied:
                            item.State = input.State;
                            item.DeniedReason = input.DeniedReason;
                            if (typeAd.Surcharge.HasValue && typeAd.Surcharge.HasValue && item.UserBillId.HasValue)
                            {
                                await DeleteUserBillAdministrative(item, typeAd);

                            }
                            await FireUserStateAdministrative(item);
                            break;
                        default:
                            break;
                    }
                    _administrativeRepos.Update(item);

                }
                mb.statisticMetris(t1, 0, "is_handle_administrative");
                var data = DataResult.ResultSuccess("Insert success !");
                return data;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }


        public async Task<DataResult> DeleteAdministrative(long id)
        {
            try
            {
                await _administrativeRepos.DeleteAsync(id);
                await DeleteValueWithAdministrativeIdAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteMultipleAdministrative([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteAdministrative(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

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



        public async Task<object> UpdateStateAdministrative(long id, int state)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}", id);

                var sql = string.Format("UPDATE Administrative" +
                    " SET State = {1}, LastModificationTime = CURRENT_TIMESTAMP, LastModifierUserId = {2}" +
                    " WHERE Id IN ({0})",
                    sb.ToString(), state, AbpSession.UserId);
                var par = new SqlParameter();
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
                var data = DataResult.ResultSuccess("Update state Success");
                mb.statisticMetris(t1, 0, "udstate_administrative");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> UpdateConfirmationOwner(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}", id);

                var sql = string.Format("UPDATE Administrative" +
                    " SET IsConfirmationOwner = {1}, LastModificationTime = CURRENT_TIMESTAMP, LastModifierUserId = {2}" +
                    " WHERE Id IN ({0})",
                    sb.ToString(), 1, AbpSession.UserId);
                var par = new SqlParameter();
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
                var data = DataResult.ResultSuccess("Update confirmation owner success");
                mb.statisticMetris(t1, 0, "ud_confir_owner");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        #region Config


        public async Task<object> CreateOrUpdateType(TypeAdministrativeDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _typeAdministrativeRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _typeAdministrativeRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_type_admin");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<TypeAdministrative>();
                    long id = await _typeAdministrativeRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_type_admin");
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

        public async Task<object> DeleteType(long id)
        {
            try
            {
                await _typeAdministrativeRepos.DeleteAsync(id);
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

        public Task<DataResult> DeleteMultipleTypes([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteType(id);
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


        public async Task<long> CreateOrUpdateValue(ValueAdministrativeDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                var data = await _valueAdministrativeRepos.FirstOrDefaultAsync(x => x.Key == input.Key && x.AdministrativeId == input.AdministrativeId);

                if (data != null)
                {
                    input.Id = data.Id;
                    input.MapTo(data);
                    //call back
                    await _valueAdministrativeRepos.UpdateAsync(data);
                    mb.statisticMetris(t1, 0, "Ud_value_administrative");

                    //var data = DataResult.ResultSuccess(updateData, "Update success !");
                    //return data;
                    return data.Id;
                }
                else
                {
                    var insertInput = input.MapTo<AdministrativeValue>();
                    long id = await _valueAdministrativeRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_value_administrative");
                    //var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    //return data;
                    return insertInput.Id;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return 0;
            }
        }

        private async Task CreateOrUpdateValueWithAdministrative(string input, long adId, long typeId, string oldProperties = null)
        {

            try
            {
                Dictionary<string, dynamic> oldValues = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(oldProperties);
                Dictionary<string, dynamic> dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(input);
                foreach (KeyValuePair<string, dynamic> ad in dict)
                {
                    ValueAdministrativeDto value = new ValueAdministrativeDto();
                    value.Key = ad.Key;
                    value.Value = ad.Key == "table" ? null : Convert.ToString(ad.Value);
                    value.AdministrativeId = adId;
                    value.TypeId = typeId;

                    var id = await CreateOrUpdateValue(value);
                    if (ad.Key == "table" && id > 0)
                    {
                        foreach (var obj in ad.Value)
                        {
                            Dictionary<string, dynamic> dictTable = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(Convert.ToString(obj));
                            foreach (KeyValuePair<string, dynamic> valueTable in dictTable)
                            {
                                ValueAdministrativeDto value1 = new ValueAdministrativeDto();
                                value1.Key = valueTable.Key;
                                value1.Value = Convert.ToString(valueTable.Value);
                                value1.AdministrativeId = adId;
                                value1.ParentId = id;
                                value1.TypeId = typeId;
                                await CreateOrUpdateValue(value1);
                            }
                        }
                    }
                }
                //var data = DataResult.ResultSuccess(null, "Insert success !");
                //return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                //return data;
            }

        }
        public async Task<object> DeleteValue(long id)
        {
            try
            {
                await _valueAdministrativeRepos.DeleteAsync(id);
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


        public async Task<DataResult> DeleteValueWithAdministrativeIdAsync(long adId)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _valueAdministrativeRepos.DeleteAsync(x => x.AdministrativeId == adId);   
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "delete_value_administrative");
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

        #region Property

        public async Task<object> CreateOrUpdateProperty(ADPropetyInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _administrativePropertiesRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _administrativePropertiesRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_config_administrative");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<AdministrativeProperty>();
                    long id = await _administrativePropertiesRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_config_administrative");
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


        public Task<DataResult> CreateProperty(ADPropetyInput input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                long t1 = TimeUtils.GetNanoseconds();
                var insertInput = input.MapTo<AdministrativeProperty>();
                long id = _administrativePropertiesRepos.InsertAndGetId(insertInput);
                insertInput.Id = id;
                if (input.Type == ADPropertyType.TABLE)
                {
                    foreach (var at in input.TableColumn)
                    {
                        at.ParentId = id;
                        at.TenantId = AbpSession.TenantId;
                        var atConvert = at.MapTo<AdministrativeProperty>();
                        _administrativePropertiesRepos.InsertAsync(atConvert);
                    }
                }
                else if (input.Type == ADPropertyType.OPTIONCHECKBOX)
                {
                    foreach (var at in input.OptionValues)
                    {
                        at.ParentId = id;
                        at.TenantId = AbpSession.TenantId;
                        var atConvert = at.MapTo<AdministrativeProperty>();
                        _administrativePropertiesRepos.InsertAsync(atConvert);
                    }
                }

                mb.statisticMetris(t1, 0, "is_config_administrative");
                var data = DataResult.ResultSuccess("Insert success !");
                return Task.FromResult(data);
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }


        public Task<DataResult> UpdateProperty(ADPropetyInput input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = _administrativePropertiesRepos.Get(input.Id);
                if (updateData != null)
                {
                    input.MapTo(updateData);
                    //call back
                    _administrativePropertiesRepos.Update(updateData);
                }
                if (input.Type == ADPropertyType.TABLE || input.Type == ADPropertyType.OPTIONCHECKBOX)
                {
                    var tasks = new List<Task>();
                    foreach (var at in input.Type == ADPropertyType.TABLE ? input.TableColumn : input.OptionValues)
                    {
                        if (at.StateRowFe == "add")
                        {
                            at.ParentId = updateData.Id;
                            at.TenantId = AbpSession.TenantId;
                            var atConvert = at.MapTo<AdministrativeProperty>();
                            _administrativePropertiesRepos.InsertAsync(atConvert);
                        }
                        else if (at.StateRowFe == "delete")
                        {
                            _administrativePropertiesRepos.DeleteAsync(at.Id);
                        }
                        else
                        {
                            var update = _administrativePropertiesRepos.Get(at.Id);
                            if (update != null)
                            {
                                at.MapTo(update);
                                //call back
                                _administrativePropertiesRepos.Update(update);
                            }
                        }

                    }
                }

                mb.statisticMetris(t1, 0, "update_administrative_attribute");
                var data = DataResult.ResultSuccess("Update success !");
                return Task.FromResult(data);
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }


        public Task<DataResult> CreateOrUpdateListProperty(List<ADPropetyInput> input)
        {
            try
            {
                if (input.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                List<Task> tasks = new List<Task>();
                foreach (var at in input)
                {
                    if (at.StateRowFe == "add")
                    {
                        var tk = CreateProperty(at);
                        tasks.Add(tk);
                    }

                    else if (at.StateRowFe == "delete")
                    {
                        var tk = DeleteProperty(at.Id);
                        tasks.Add(tk);
                    }
                    else
                    {
                        var tk = UpdateProperty(at);
                        tasks.Add(tk);
                    }

                }
                Task.WaitAll(tasks.ToArray());
                var data = DataResult.ResultSuccess("Success!");
                return Task.FromResult(data);
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }

        public Task<DataResult> DeleteProperty(long id)
        {
            try
            {
                _administrativePropertiesRepos.DeleteAsync(id);
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
        #endregion


        #region Common
        private Task<long> CreateUserBillAdministrative(Administrative administrative, TypeAdministrative type)
        {
            var checkBill = administrative.UserBillId != null ? _userBillRepos.FirstOrDefault(administrative.UserBillId.Value) : null;
            if (checkBill != null) return Task.FromResult(administrative.UserBillId.Value);

            var apartment = _homeMemberRepos.FirstOrDefault(x => x.UserId == administrative.CreatorUserId);
            var citizen = _citizenRepos.FirstOrDefault(x => x.AccountId == administrative.CreatorUserId && x.State == STATE_CITIZEN.ACCEPTED);
            var properties = new
            {
                customerName = citizen == null ? citizen.FullName : ""
            };

            var bill = new UserBill()
            {
                BillType = BillType.Other,
                Amount = 0,
                LastCost = type.Price != null ? type.Price.Value : 0,
                Title = "Hóa đơn hành chính số: " + type.Name,
                Status = UserBillStatus.Pending,
                ApartmentCode = apartment != null ? apartment.ApartmentCode : null,
                Code = GetUniqueKey(8),
                TenantId = AbpSession.TenantId,
                DueDate = DateTime.Now.AddDays(10), // Ngày hết hạn sau 10 ngày
                Period = DateTime.Now,
                Properties = JsonConvert.SerializeObject(properties)

            };
            var billId = _userBillRepos.InsertAndGetId(bill);
            return Task.FromResult(billId);
        }

        private Task DeleteUserBillAdministrative(Administrative administrative, TypeAdministrative typ)
        {
            var checkBill = administrative.UserBillId != null ? _userBillRepos.FirstOrDefault(administrative.UserBillId.Value) : null;
            if (checkBill != null)
                _userBillRepos.DeleteAsync(checkBill.Id);

            return Task.CompletedTask;

        }

        private async Task FireUserStateAdministrative(Administrative administrative)
        {
            if (administrative.State == AdministrativeState.Accepted) await HandAdministrativeAccept(administrative);
            else if (administrative.State == AdministrativeState.Denied) await HandAdministrativeDenied(administrative);
        }


        private async Task HandAdministrativeAccept(Administrative administrative)
        {
            string message = "Đăng ký dịch vụ hành chính số của bạn đã được chấp nhận.";
            string detailUrlApp = $"yoolife://app/adminstrative/detail?id={administrative.Id}";
            string detailUrlWA = $"/adminstrative?id={administrative.Id}";
            await SendMessageNotify(message, new UserIdentifier(administrative.TenantId, administrative.CreatorUserId ?? 0), detailUrlApp, detailUrlWA);
        }

        private async Task SendMessageNotify(string message, UserIdentifier user, string detailUrlApp, string detailUrlWA)
        {

            var messageDeclined = new UserMessageNotificationDataBase(
                             AppNotificationAction.StateAdministrative,
                             AppNotificationIcon.StateAdministrativeIcon,
                              TypeAction.Detail,
                               message,
                               detailUrlApp,
                               detailUrlWA,
                                "",
                                ""

                             );
            await _appNotifier.SendMessageNotificationInternalAsync(
                "Yoolife hành chính số!",
                 message,
                 detailUrlApp,
                 detailUrlWA,
                 new[] { user },
                 messageDeclined,
                 AppType.USER
                );
        }

        private async Task HandAdministrativeDenied(Administrative administrative, string refuseReason = "")
        {
            string message = $"Thật tiếc đăng ký dịch vụ hành chính số của bạn đã bị từ chối. Nhấn để xem chi tiết ";
            string detailUrlApp = $"yoolife://app/adminstrative/detail?id={administrative.Id}";
            string detailUrlWA = $"/adminstrative?id={administrative.Id}";
            await SendMessageNotify(message, new UserIdentifier(administrative.TenantId, administrative.CreatorUserId ?? 0), detailUrlApp, detailUrlWA);
        }

        #endregion
    }
}
