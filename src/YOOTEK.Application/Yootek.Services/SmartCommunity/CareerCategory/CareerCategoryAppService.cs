using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;
namespace Yootek.Services
{
    public interface ICareerCategoryAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllCareerCategoryInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(CareerCategoryDto input);
        Task<DataResult> Update(CareerCategoryDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class CareerCategoryAppService : YootekAppServiceBase, ICareerCategoryAppService
    {
        private readonly IRepository<CareerCategory, long> _repository;
        public CareerCategoryAppService(IRepository<CareerCategory, long> repository)
        {
            _repository = repository;
        }
        public async Task<DataResult> GetAllAsync(GetAllCareerCategoryInputDto input)
        {
            try
            {
                IQueryable<CareerCategoryDto> query = (from o in _repository.GetAll()
                                                       select new CareerCategoryDto
                                                       {
                                                           Id = o.Id,
                                                           Title = o.Title,
                                                           Code = o.Code,
                                                           Description = o.Description,
                                                           TenantId = o.TenantId,
                                                       })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Title.ToLower().Contains(input.Keyword.ToLower()) || x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Description.ToLower().Contains(input.Keyword.ToLower()));
                List<CareerCategoryDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetById(long id)
        {
            try
            {
                var item = await _repository.GetAsync(id);
                var data = item.MapTo<CareerCategoryDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(CareerCategoryDto dto)
        {
            try
            {
                CareerCategory item = dto.MapTo<CareerCategory>();
                item.TenantId = AbpSession.TenantId;
                await _repository.InsertAsync(item);
                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Update(CareerCategoryDto dto)
        {
            try
            {
                CareerCategory item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    await _repository.UpdateAsync(item);
                    return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Delete(long id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                var data = DataResult.ResultSuccess(Common.Resource.QuanLyChung.DeleteSuccess);
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
