

//using Abp.Application.Services;
//using Abp.Authorization;
//using Abp.AutoMapper;
//using Abp.Domain.Repositories;
//using Abp.UI;
//using Yootek.Common.DataResult;
//using System;
//using System.Threading.Tasks;
//using System.Linq;
//using Abp.Linq.Extensions;
//using Microsoft.EntityFrameworkCore;
//using System.Linq.Dynamic.Core;
//using System.Collections.Generic;
//using DocumentFormat.OpenXml.Bibliography;
//using Yootek.EntityDb;

//namespace Yootek.Services
//{
//    public interface IMaterialInventoryAppService : IApplicationService
//    {
//        Task<object> GetAllAsync(GetAllMaterialInputDto input);
//        Task<object> Create(MaterialDto input);
//        Task<object> Update(MaterialDto input);
//        Task<object> Delete(long id);

//    }

//    [AbpAuthorize]
//    public class MaterialInventoryAppService : YootekAppServiceBase, IMaterialInventoryAppService
//    {

//        private readonly IRepository<Material, long> _materialRepos;
//        private readonly IRepository<MaterialCategory, long> _materialCategoryRepos;

//        public MaterialInventoryAppService(
//            IRepository<Material, long> materialRepos,
//            IRepository<MaterialCategory, long> materialCategoryRepos
//            )
//        {
//            _materialRepos = materialRepos;
//            _materialCategoryRepos = materialCategoryRepos;
//        }

//        public async Task<object> GetAllAsync(GetAllMaterialInputDto input)
//        {
//            try
//            {
//                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
//                {
//                    var queryInput = new QueryMaterialDto()
//                    {
//                        BuildingId = input.BuildingId,
//                        CategoryType = input.CategoryType,
//                        FormQuery = 1,
//                        Keyword = input.Keyword,
//                        UrbanId = input.UrbanId
//                    };
//                    var query = await QueryMaterialAsync(queryInput);

//                    if (input.LocationId > 0)
//                    {
//                        query = query.Where(x => x.LocationId == input.LocationId);
//                    }
//                    else if (input.LocationId == 0)
//                    {
//                        query = query.Where(x => x.LocationId == null);
//                    }

//                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

//                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
//                    return data;
//                }
//            }
//            catch (Exception e)
//            {
//                Logger.Fatal(e.Message);
//                throw;
//            }
//        }

//        public async Task<object> Create(MaterialDto input)
//        {
//            try
//            {
//                long t1 = TimeUtils.GetNanoseconds();

//                input.TenantId = AbpSession.TenantId;

//                var insertInput = input.MapTo<Material>();
//                long id = await _materialRepos.InsertAndGetIdAsync(insertInput);
//                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
//                mb.statisticMetris(t1, 0, "is_material");

//                return data;

//            }
//            catch (Exception e)
//            {
//                Logger.Fatal(e.Message);
//                throw;
//            }
//        }


//        public async Task<object> Update(MaterialDto input)
//        {
//            try
//            {

//                if (input.Id > 0)
//                {
//                    long t1 = TimeUtils.GetNanoseconds();
//                    //update
//                    var updateData = await _materialRepos.GetAsync(input.Id);
//                    if (updateData != null)
//                    {
//                        input.MapTo(updateData);
//                        await _materialRepos.UpdateAsync(updateData);
//                    }
//                    mb.statisticMetris(t1, 0, "Ud_administrative");

//                    var data = DataResult.ResultSuccess(updateData, "Update success !");
//                    return data;
//                }

//                return null;
//            }
//            catch (Exception e)
//            {
//                Logger.Fatal(e.Message);
//                throw;
//            }
//        }

//        public async Task<object> Delete(long id)
//        {
//            try
//            {
//                await _materialRepos.DeleteAsync(id);
//                var data = DataResult.ResultSuccess("Delete success!");
//                return data;
//            }
//            catch (Exception e)
//            {
//                Logger.Fatal(e.Message);
//                throw;
//            }
//        }

//    }
//}
