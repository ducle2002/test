using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Services
{
    public interface ILocalServiceAppService : IApplicationService
    {
        Task<object> CreateOrUpdateLocalService(LocalServiceDto input);
        Task<object> DeleteLocalService(long id);
        Task<object> GetAllLocalServiceByOrganization(GetAllLocalServiceInput input);
        Task<object> GetAllLocalServicesByUser(GetAllLocalServicesByUserInput input);
        Task<object> GetLocalServiceType(GetAllLocalServiceTypeInput input);
    }

    [AbpAuthorize]
    public class LocalServiceAppService : YootekAppServiceBase, ILocalServiceAppService
    {
        private readonly IRepository<LocalService, long> _localServiceRepos;
        private readonly IRepository<ObjectPartner, long> _objectPartnerRepos;
        private readonly IRepository<Rate, long> _rateRepos;

        public LocalServiceAppService(
            IRepository<LocalService, long> localServiceRepos,
            IRepository<ObjectPartner, long> objectPartner,
            IRepository<Rate, long> rateRepos)
        {
            _localServiceRepos = localServiceRepos;
            _objectPartnerRepos = objectPartner;
            _rateRepos = rateRepos;
        }
        public async Task<object> CreateOrUpdateLocalService(LocalServiceDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _localServiceRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _localServiceRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_local_service");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<LocalService>();
                    long id = await _localServiceRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_local_service");
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

        public async Task<object> DeleteLocalService(long id)
        {
            try
            {
                await _localServiceRepos.DeleteAsync(id);
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

        public async Task<object> DeleteMultipleLocalServices([FromBody] List<long> ids)
        {
            try
            {

                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteLocalService(id);
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

        public async Task<object> GetAllLocalServiceByOrganization(GetAllLocalServiceInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var a = AbpSession.UserId;
                var query = (from localService in _localServiceRepos.GetAll()
                             select new LocalService()
                             {
                                 Id = localService.Id,
                                 Type = localService.Type ?? (int)localService.Id,
                                 TypeBooking = localService.TypeBooking,
                                 GroupType = localService.GroupType,
                                 FormViewType = localService.FormViewType,
                                 Name = localService.Name,
                                 Icon = localService.Icon,
                                 AllowPayment = localService.AllowPayment,
                                 BuildingId = localService.BuildingId,
                                 TenantId = localService.TenantId,
                                 UrbanId = localService.UrbanId,
                                 OrganizationUnitId = localService.OrganizationUnitId
                             }
                    ).WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId || x.UrbanId == input.OrganizationUnitId || x.BuildingId == input.OrganizationUnitId)
                   .WhereIf(input.GroupType.HasValue, x => x.GroupType == input.GroupType)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
                   .AsQueryable();

                var result = await query.PageBy(input).ToListAsync();

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_list_localservice");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetLocalServiceType(GetAllLocalServiceTypeInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var a = AbpSession.UserId;
                var query = (from type in _localServiceRepos.GetAll()
                             select new LocalService()
                             {
                                 Id = type.Id,
                                 Type = type.Type ?? (int)type.Id,
                                 TypeBooking = type.TypeBooking,
                                 GroupType = type.GroupType,
                                 FormViewType = type.FormViewType,
                                 Name = type.Name,
                                 Icon = type.Icon,
                                 AllowPayment = type.AllowPayment,
                                 BuildingId = type.BuildingId,
                                 TenantId = type.TenantId,
                                 UrbanId = type.UrbanId,
                                 OrganizationUnitId = type.OrganizationUnitId
                             })

                    .WhereIf(input.UrbanId > 0, x => x.UrbanId == input.UrbanId || x.OrganizationUnitId == input.UrbanId)
                    .WhereIf(input.BuildingId > 0, x => x.BuildingId == input.BuildingId || x.BuildingId == null)
                    .WhereIf(input.GroupType.HasValue, x => x.GroupType == input.GroupType)
                    .ApplySearchFilter(input.Keyword, x => x.Name)
                    .AsQueryable();

                var result = await query.PageBy(input).ToListAsync();

                var data = DataResult.ResultSuccess(result, "Get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_list_localservice");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllLocalServicesByUser(GetAllLocalServicesByUserInput input)
        {
            try
            {
                var query = (from partner in _objectPartnerRepos.GetAll()
                    join localType in _localServiceRepos.GetAll() on partner.Type equals localType.Type into tbType
                    from type in tbType
                    select new ObjectDto()
                        {
                            Id = partner.Id,
                            Like = partner.Like,
                            Name = partner.Name,
                            Operator = partner.Operator,
                            Owner = partner.Owner,
                            Properties = partner.Properties,
                            QueryKey = partner.QueryKey,
                            PropertyHistories = partner.PropertyHistories,
                            StateProperties = partner.StateProperties,
                            Type = partner.Type,
                            GroupType = (int)type.GroupType,
                            State = partner.State,
                            Latitude = partner.Latitude,
                            Longitude = partner.Longitude,
                            IsDataStatic = partner.IsDataStatic,
                            SocialTenantId = partner.SocialTenantId,
                            IsAdminCreate = partner.IsAdminCreate, 
                            CountRate = _rateRepos.GetAll().Count(x => x.ObjectId == partner.Id),
                            Rate = _rateRepos.GetAll().Count(x => x.ObjectId == partner.Id) != 0 ?(float)Math.Round((float)(from rate in _rateRepos.GetAll()
                                    .Where(x => x.ObjectId == partner.Id)
                                    .DefaultIfEmpty()
                                select rate.RatePoint).Average(), 1) : 0,
                        }
                    )
                    .Where(x => x.State == (int)CommonENumObject.STATE_OBJECT.ACTIVE)
                    .WhereIf(input.GroupType != null, x => x.GroupType == input.GroupType)
                    .WhereIf(input.Type != null, x => x.Type == input.Type)
                    .ApplySearchFilter(input.Keyword, x => x.Name)
                    .AsQueryable();

                var result = await query
                    .PageBy(input)
                    .ToListAsync();

                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public async Task<object> GetPartnerServiceById(long id)
        {
            try
            {
                var query = (from partner in _objectPartnerRepos.GetAll()
                             join localType in _localServiceRepos.GetAll() on partner.Type equals localType.Type into tbType
                             from type in tbType
                             select new LocalServicePartnerDto
                             {

                                 Id = partner.Id,
                                 Like = partner.Like,
                                 Name = partner.Name,
                                 Operator = partner.Operator,
                                 Owner = partner.Owner,
                                 Properties = partner.Properties,
                                 QueryKey = partner.QueryKey,
                                 PropertyHistories = partner.PropertyHistories,
                                 StateProperties = partner.StateProperties,
                                 Type = partner.Type,
                                 GroupType = (int)type.GroupType,
                                 State = partner.State,
                                 Latitude = partner.Latitude,
                                 Longitude = partner.Longitude,
                                 IsDataStatic = partner.IsDataStatic,
                                 SocialTenantId = partner.SocialTenantId,
                                 IsAdminCreate = partner.IsAdminCreate,
                                 CountRate = _rateRepos.GetAll().Count(x => x.ObjectId == id),
                                 Rate = _rateRepos.GetAll().Count(x => x.ObjectId == id) != 0 ? (float)Math.Round((float)(from rate in _rateRepos.GetAll()
                                         .Where(x => x.ObjectId == id)
                                         .DefaultIfEmpty()
                                                                                                                          select rate.RatePoint).Average(), 1) : 0,
                                 TypeBooking = (int)type.TypeBooking,
                                 PersonalRate = _rateRepos.GetAll().FirstOrDefault(r => r.CreatorUserId == AbpSession.UserId)
                             }
                    )
                    .Where(x => x.Id == id)
                    .AsQueryable();
                return DataResult.ResultSuccess(query.FirstOrDefault(), "success");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }


        public async Task<object> GetLocalServiceByIdAsync(long id)
        {
            try
            {
                var data = _localServiceRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetHighestLocalServiceTypeNumber()
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var a = AbpSession.UserId;
                var query = (from ls in _localServiceRepos.GetAll()
                             select new
                             {
                                 ls.Type
                             })
                   .AsQueryable();

                var result = await query.OrderByDescending(x => x.Type).FirstOrDefaultAsync();

                var data = DataResult.ResultSuccess(result?.Type, "Get success", query.Count());
                mb.statisticMetris(t1, 0, "gall_list_localservice");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> CreateListLocalService(List<LocalServiceDto> input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input != null)
                {
                    foreach (var obj in input)
                    {
                        obj.TenantId = AbpSession.TenantId;
                        var insertInput = obj.MapTo<LocalServiceDto>();
                        await _localServiceRepos.InsertAndGetIdAsync(insertInput);
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
    }
}
