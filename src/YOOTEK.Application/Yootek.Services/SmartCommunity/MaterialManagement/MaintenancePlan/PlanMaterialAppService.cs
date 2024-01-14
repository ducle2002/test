using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IPlanMaterialAppService : IApplicationService
    {
        Task<object> GetAllPlanMaterialAsync(GetAllPlanMaterialInput input);
        Task<object> GetPlanMaterialByIdAsync(long id);
        Task<object> CreateOrUpdatePlanMaterial(PlanMaterialDto input);
        Task<object> DeletePlanMaterial(long id);
        Task<object> UpdateStatePlanMaterial(PlanMaterialDto input);

    }


    [AbpAuthorize]
    public class PlanMaterialAppService : YootekAppServiceBase, IPlanMaterialAppService
    {
        private readonly IRepository<MaintenancePlan, long> _planMaterialRepos;
        /*private readonly IRepository<Material, long> _userRepos;*/
        /*private readonly IRepository<Role, int> _roleRepos;*/
        public PlanMaterialAppService(
            IRepository<MaintenancePlan, long> planMaterialRepos
            /*IRepository<Material, long> userRepos*/
            )
        {
            _planMaterialRepos = planMaterialRepos;
        }

      
        public async Task<object> CreateOrUpdatePlanMaterial(PlanMaterialDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _planMaterialRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _planMaterialRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_planMaterial");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<MaintenancePlan>();
                    long id = await _planMaterialRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;
                    mb.statisticMetris(t1, 0, "is_planMaterial");
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
            /*throw new NotImplementedException();*/
        }

        public async Task<object> DeletePlanMaterial(long id)
        {
            try
            {

                await _planMaterialRepos.DeleteAsync(id);
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

        public async Task<object> GetAllPlanMaterialAsync(GetAllPlanMaterialInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from pm in _planMaterialRepos.GetAll()
                                     /*join us in _userRepos.GetAll() on ma.TenantId equals us.Id into tb_us
                                     from us in tb_us.DefaultIfEmpty()*/
                                 select new PlanMaterialDto()
                                 {
                                     Id = pm.Id,
                                     ImplementDate = pm.ImplementDate,
                                     Asset = pm.Asset,
                                     /*Icon = ci.Icon != null ? ci.Icon : us.Icon,
                                     Email = ci.Email != null ? ci.Email : us.EmailAddress,*/
                                     Place = pm.Place,
                                     IsDeleted = pm.IsDeleted,
                                     DeletionTime = pm.DeletionTime,
                                     State = pm.State,
                                     TenantId = pm.TenantId,
                                     CreatorUserId = pm.CreatorUserId,
                                     LastModificationTime = pm.LastModificationTime,
                                     LastModifierUserId = pm.LastModifierUserId,
                                     DeleterUserId = pm.DeleterUserId,
                                     CreationTime = pm.CreationTime,
                                 });
                    if (input.Keyword != null)
                    {
                        var listKey = input.Keyword.Split('+');
                        if (listKey != null)
                        {
                            foreach (var key in listKey)
                            {
                                query = query.Where(u => (u.Asset.Contains(key)));
                            }
                        }
                    }
                    DateTime fromDay = new DateTime(), toDay = new DateTime();

                    if (input.DayCreat.HasValue)
                    {
                        fromDay = new DateTime(input.DayCreat.Value.Year, input.DayCreat.Value.Month, input.DayCreat.Value.Day, 0, 0, 0);
                        query = query.WhereIf(input.DayCreat.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime >= fromDay) || (!u.LastModificationTime.HasValue && u.CreationTime >= fromDay));
                    }
                    /*if (input.ToDay.HasValue)
                    {
                        toDay = new DateTime(input.ToDay.Value.Year, input.ToDay.Value.Month, input.ToDay.Value.Day, 23, 59, 59);
                        query = query.WhereIf(input.ToDay.HasValue, u => (u.LastModificationTime.HasValue && u.LastModificationTime <= toDay) || (!u.LastModificationTime.HasValue && u.CreationTime <= toDay));
                    }*/

                    if (input.State.HasValue)
                    {
                        query = query.WhereIf(input.State.HasValue, x => x.State == (int)input.State);
                    }


                    var result = await query.PageBy(input).ToListAsync();
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


        public async Task<object> GetPlanMaterialByIdAsync(long id)
        {
            {
                try
                {

                    var result = await _planMaterialRepos.GetAsync(id);
                    var data = DataResult.ResultSuccess(result, "Get success!");
                    return data;
                }
                catch (Exception e)
                {
                    var data = DataResult.ResultError(e.ToString(), "Exception !");
                    Logger.Fatal(e.Message);
                    return data;
                }
            }
        }

        public async Task<object> UpdateStatePlanMaterial(PlanMaterialDto input)
        {
            try
            {
                /*long t1 = TimeUtils.GetNanoseconds();*/

                /*                input.TenantId = AbpSession.TenantId;
                */
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _planMaterialRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _planMaterialRepos.UpdateAsync(updateData);
                    }
                    /*mb.statisticMetris(t1, 0, "Ud_material");*/

                    var data = DataResult.ResultSuccess(updateData, "Update state success !");
                    return data;
                }
                else return null;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
