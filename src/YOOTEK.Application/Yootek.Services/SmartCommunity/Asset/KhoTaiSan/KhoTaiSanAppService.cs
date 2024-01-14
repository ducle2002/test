using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Yootek.Services
{
    public interface IKhoTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllKhoTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(KhoTaiSanDto input);
        Task<DataResult> Update(KhoTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class KhoTaiSanAppService : YootekAppServiceBase, IKhoTaiSanAppService
    {
        private readonly IRepository<KhoTaiSan, long> _repository;
        public KhoTaiSanAppService(IRepository<KhoTaiSan, long> repository )
        {
            _repository = repository;
        }
        public async Task<DataResult> GetAllAsync(GetAllKhoTaiSanInputDto input)
        {
            try
            {
                IQueryable<KhoTaiSanDto> query = (from o in _repository.GetAll()
                                                   select new KhoTaiSanDto
                                                   {
                                                       	Id = o.Id,
	Code = o.Code,
	Title = o.Title,
	DiaDiem = o.DiaDiem,
	Description = o.Description,
	TenantId = o.TenantId,
                                                   })
                	.WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()) || x.DiaDiem.ToLower().Contains(input.Keyword.ToLower()) || x.Description.ToLower().Contains(input.Keyword.ToLower()));
                List<KhoTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<KhoTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(KhoTaiSanDto dto)
        {
            try
            {
                KhoTaiSan item = dto.MapTo<KhoTaiSan>();
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
        public async Task<DataResult> Update(KhoTaiSanDto dto)
        {
            try
            {
                KhoTaiSan item = await _repository.GetAsync(dto.Id);
                if(item!=null)
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
