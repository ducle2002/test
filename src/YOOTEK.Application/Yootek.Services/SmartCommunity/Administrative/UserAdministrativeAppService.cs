using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.ApbCore.Data;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Yootek.Notifications;
using Abp;

namespace Yootek.Services
{
    public interface IUserAdministrativeAppService
    {
        Task<object> CreateAdministrative(AdministrativeDto input);
        Task<object> CancelAdministrativeByUser(CancelAdministrativeByUserInput input);
        Task<object> UpdateAdministrative(AdministrativeDto input);
        Task<object> DeleteAdministrative(long id);
        Task CreateOrUpdateValueWithAdministrative(string input, long adId, long typeId);
        Task<long> CreateOrUpdateValue(ValueAdministrativeDto input);
        Task<object> DeleteValueWithAdministrativeIdAsync(long adId);
    }

    [AbpAuthorize]
    public class UserAdministrativeAppService : YootekAppServiceBase, IUserAdministrativeAppService
    {
        private readonly IRepository<TypeAdministrative, long> _typeAdministrativeRepos;
        private readonly IRepository<Administrative, long> _administrativeRepos;
        private readonly IRepository<Citizen, long> _citizenRepos;
        private readonly IRepository<AdministrativeValue, long> _valueAdministrativeRepos;
        private readonly IAppNotifier _appNotifier;
        private readonly ISqlExecuter _sqlExecute;

        public UserAdministrativeAppService(
            IRepository<TypeAdministrative, long> typeAdministrativeRepos,
            IRepository<Administrative, long> administrativeRepos,
            IRepository<Citizen, long> citizenRepos,
            IRepository<AdministrativeValue, long> valueAdministrativeRepos,
            ISqlExecuter sqlExecute,
            IAppNotifier appNotifier)
        {
            _typeAdministrativeRepos = typeAdministrativeRepos;
            _administrativeRepos = administrativeRepos;
            _citizenRepos = citizenRepos;
            _valueAdministrativeRepos = valueAdministrativeRepos;
            _sqlExecute = sqlExecute;
            _appNotifier = appNotifier;
        }


        public async Task<object> CreateAdministrative(AdministrativeDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var adType = await _typeAdministrativeRepos.FirstOrDefaultAsync(input.ADTypeId);
                if(adType == null) return DataResult.ResultSuccess("Type administrative not found !");

                var insertInput = input.MapTo<Administrative>();
                insertInput.State = AdministrativeState.Requesting;
                long id = await _administrativeRepos.InsertAndGetIdAsync(insertInput);
                insertInput.Id = id;
                await CreateOrUpdateValueWithAdministrative(input.Properties, id, input.ADTypeId);

                var citizen = await _citizenRepos.FirstOrDefaultAsync(x => x.ApartmentCode == input.ApartmentCode && x.AccountId == AbpSession.UserId);
                var citizenName = citizen?.FullName ?? "Cư dân";
                var admins = await UserManager.GetUserOrganizationUnitByUrbanOrNull(adType.UrbanId);
               
                if(admins != null) await NotifierNewAdministrative(insertInput, admins.ToArray(), citizenName);
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                mb.statisticMetris(t1, 0, "is_administrative");

                return data;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CancelAdministrativeByUser(CancelAdministrativeByUserInput input)
        {
            try
            {
                Administrative? administrative = await _administrativeRepos.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("Administrative not found");
                if (administrative.State == AdministrativeState.Requesting)
                {
                    administrative.State = AdministrativeState.Cancel;
                    administrative.DeniedReason = input.DeniedReason;
                }
                await _administrativeRepos.UpdateAsync(administrative);
                return DataResult.ResultSuccess(true, "Cancel administrative success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> UpdateAdministrative(AdministrativeDto input)
        {
            try
            {

                if (input.Id > 0)
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    //update
                    var updateData = await _administrativeRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        updateData.Properties = input.Properties;
                        //call back
                        await _administrativeRepos.UpdateAsync(updateData);
                        await CreateOrUpdateValueWithAdministrative(input.Properties, input.Id, input.ADTypeId);
                    }
                    mb.statisticMetris(t1, 0, "Ud_administrative");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<object> DeleteAdministrative(long id)
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
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task CreateOrUpdateValueWithAdministrative(string input, long adId, long typeId)
        {

            try
            {
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
        public async Task<object> DeleteValueWithAdministrativeIdAsync(long adId)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}", adId);

                var sql = string.Format("DELETE from AdministrativeValue" +
                    " WHERE AdministrativeId IN ({0})",
                    sb.ToString());
                var par = new SqlParameter();
                var i = await _sqlExecute.ExecuteAsync(sql);
                CurrentUnitOfWork.SaveChanges();
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

        #region Common

        private async Task NotifierNewAdministrative(Administrative data, UserIdentifier[] admin, string creatorName)
        {
            var detailUrlApp = $"yooioc://app/adminstrative/detail?id={data.Id}";
            var detailUrlWA = $"/adminstrative?id={data.Id}";
            var messageDeclined = new UserMessageNotificationDataBase(
                            AppNotificationAction.ReflectCitizenNew,
                            AppNotificationIcon.ReflectCitizenNewIcon,
                            TypeAction.Detail,
                            $"{creatorName} đã gửi 1 form hành chính số mới. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWA
                            );

            await _appNotifier.SendMessageNotificationInternalAsync(
                "Yoolife hành chính số!",
                $"{creatorName} đã gửi 1 form hành chính số mới. Nhấn để xem chi tiết !",
                detailUrlApp,
                detailUrlWA,
                admin.ToArray(),
                messageDeclined,
                AppType.IOC
                );

        }

        #endregion

    }
}
