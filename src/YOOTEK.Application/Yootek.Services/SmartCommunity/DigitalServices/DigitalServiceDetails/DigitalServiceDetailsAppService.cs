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
    public interface IDigitalServiceDetailsAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllDigitalServiceDetailsInputDto input);
        Task<DataResult> GetAllDetailsAsync(GetAllGridDigitalServiceDetailsInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(DigitalServiceDetailsDto input);
        Task<DataResult> Update(DigitalServiceDetailsDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class DigitalServiceDetailsAppService : YootekAppServiceBase, IDigitalServiceDetailsAppService
    {
        private readonly IRepository<DigitalServiceDetails, long> _repository;
        private readonly IRepository<DigitalServices, long> _digitalServicesRepository;
        public DigitalServiceDetailsAppService(IRepository<DigitalServiceDetails, long> repository,
IRepository<DigitalServices, long> digitalServicesRepository)
        {
            _repository = repository;
            _digitalServicesRepository = digitalServicesRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllDigitalServiceDetailsInputDto input)
        {
            try
            {
                IQueryable<DigitalServiceDetailsDto> query = (from o in _repository.GetAll()
                                                              select new DigitalServiceDetailsDto
                                                              {
                                                                  Id = o.Id,
                                                                  Code = o.Code,
                                                                  Title = o.Title,
                                                                  ServicesId = o.ServicesId,
                                                                  Price = o.Price,
                                                                  Description = o.Description,
                                                                  Image = o.Image,
                                                                  TenantId = o.TenantId,
                                                                  ServicesText = !o.ServicesId.HasValue ? "" : _digitalServicesRepository.GetAll().Where(x => x.Id == o.ServicesId.Value).Select(x => x.Title).FirstOrDefault(),
                                                              })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()))
    .WhereIf(input.ServicesId > 0, x => x.ServicesId.Value == input.ServicesId)
;
                List<DigitalServiceDetailsDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<DigitalServiceDetailsDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(DigitalServiceDetailsDto dto)
        {
            try
            {
                DigitalServiceDetails item = dto.MapTo<DigitalServiceDetails>();
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
        public async Task<DataResult> Update(DigitalServiceDetailsDto dto)
        {
            try
            {
                DigitalServiceDetails item = await _repository.GetAsync(dto.Id);
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

        public async Task<DataResult> GetAllDetailsAsync(GetAllGridDigitalServiceDetailsInputDto input)
        {
            try
            {
                IQueryable<DigitalServiceDetailsGridDto> query = (from o in _repository.GetAll()
                                                              select new DigitalServiceDetailsGridDto
                                                              {
                                                                  Id = o.Id,
                                                                  Code = o.Code,
                                                                  Title = o.Title,
                                                                  ServicesId = o.ServicesId,
                                                                  Price = o.Price,
                                                                  Description = o.Description,
                                                                  Image = o.Image
                                                              }).WhereIf(input.ServicesId > 0, x => x.ServicesId.Value == input.ServicesId)
;
                List<DigitalServiceDetailsGridDto> result = await query.ApplySort(input.OrderBy, SortBy.ASC).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
