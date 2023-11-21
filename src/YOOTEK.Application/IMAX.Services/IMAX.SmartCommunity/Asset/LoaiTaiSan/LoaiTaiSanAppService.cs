using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace IMAX.Services
{
    public interface ILoaiTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllLoaiTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(LoaiTaiSanDto input);
        Task<DataResult> Update(LoaiTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class LoaiTaiSanAppService : IMAXAppServiceBase, ILoaiTaiSanAppService
    {
        private readonly IRepository<LoaiTaiSan, long> _repository;
        public LoaiTaiSanAppService(IRepository<LoaiTaiSan, long> repository )
        {
            _repository = repository;
        }
        public async Task<DataResult> GetAllAsync(GetAllLoaiTaiSanInputDto input)
        {
            try
            {
                IQueryable<LoaiTaiSanDto> query = (from o in _repository.GetAll()
                                                   select new LoaiTaiSanDto
                                                   {
                                                       	Id = o.Id,
	Code = o.Code,
	Title = o.Title,
	Description = o.Description,
	TenantId = o.TenantId,
                                                   })
                	.WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()) || x.Description.ToLower().Contains(input.Keyword.ToLower()));
                List<LoaiTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }       
        public async Task<DataResult> GetById(long id)
        {
            try
            {
                var item = await _repository.GetAsync(id);
                var data = item.MapTo<LoaiTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Create(LoaiTaiSanDto dto)
        {
            try
            {
                LoaiTaiSan item = dto.MapTo<LoaiTaiSan>();
                item.TenantId = AbpSession.TenantId;
                await _repository.InsertAsync(item);
                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {                
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Update(LoaiTaiSanDto dto)
        {
            try
            {
                LoaiTaiSan item = await _repository.GetAsync(dto.Id);
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
                throw new UserFriendlyException(e.Message);
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
                throw new UserFriendlyException(e.Message);
            }
        }
    }
}
