using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface ICategoryMaterialManagementAppService : IApplicationService
    {

    }

    public class CategoryMaterialManagementAppService:  YootekAppServiceBase, ICategoryMaterialManagementAppService
    {
        private readonly IRepository<MaterialCategory, long> _materialCategoryRepos;

        public CategoryMaterialManagementAppService(
            IRepository<MaterialCategory, long> materialCategoryRepos
            )
        {
            _materialCategoryRepos = materialCategoryRepos;
        }

        public async Task<object> GetAllAsync(GetAllCategoryMaterialInputDto input)
        {
            try
            {
                var query = _materialCategoryRepos.GetAll().Where(x => x.Type == input.Type)
                    .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.ToLower().Contains(input.Keyword.ToLower())).AsQueryable();
                var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Create(CategoryMaterialDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                var insertInput = input.MapTo<MaterialCategory>();
                long id = await _materialCategoryRepos.InsertAndGetIdAsync(insertInput);
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                mb.statisticMetris(t1, 0, "is_material");

                return data;

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> Update(MaterialDto input)
        {
            try
            {

                if (input.Id > 0)
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    //update
                    var updateData = await _materialCategoryRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _materialCategoryRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_administrative");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Delete(long id)
        {
            try
            {
                await _materialCategoryRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteMultiple([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = Delete(id);
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
                return data;
            }
        }
    }

    
}
