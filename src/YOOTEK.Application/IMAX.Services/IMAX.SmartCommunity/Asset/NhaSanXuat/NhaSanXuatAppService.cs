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
    public interface INhaSanXuatAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllNhaSanXuatInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(NhaSanXuatDto input);
        Task<DataResult> Update(NhaSanXuatDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class NhaSanXuatAppService : IMAXAppServiceBase, INhaSanXuatAppService
    {
        private readonly IRepository<NhaSanXuat, long> _repository;
        public NhaSanXuatAppService(IRepository<NhaSanXuat, long> repository )
        {
            _repository = repository;
        }
        public async Task<DataResult> GetAllAsync(GetAllNhaSanXuatInputDto input)
        {
            try
            {
                IQueryable<NhaSanXuatDto> query = (from o in _repository.GetAll()
                                                   select new NhaSanXuatDto
                                                   {
                                                       	Id = o.Id,
	Code = o.Code,
	Title = o.Title,
	Description = o.Description,
	Address = o.Address,
	Phone = o.Phone,
	TenantId = o.TenantId,
                                                   })
                	.WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()) || x.Description.ToLower().Contains(input.Keyword.ToLower()) || x.Address.ToLower().Contains(input.Keyword.ToLower()) || x.Phone.ToLower().Contains(input.Keyword.ToLower()));
                List<NhaSanXuatDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<NhaSanXuatDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Create(NhaSanXuatDto dto)
        {
            try
            {
                NhaSanXuat item = dto.MapTo<NhaSanXuat>();
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
        public async Task<DataResult> Update(NhaSanXuatDto dto)
        {
            try
            {
                NhaSanXuat item = await _repository.GetAsync(dto.Id);
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
