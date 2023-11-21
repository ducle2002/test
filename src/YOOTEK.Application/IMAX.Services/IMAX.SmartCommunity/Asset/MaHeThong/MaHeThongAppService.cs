using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Application;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface IMaHeThongAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllMaHeThongInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(MaHeThongDto input);
        Task<DataResult> Update(MaHeThongDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class MaHeThongAppService : IMAXAppServiceBase, IMaHeThongAppService
    {
        private readonly IRepository<MaHeThong, long> _repository;
        public MaHeThongAppService(IRepository<MaHeThong, long> repository)
        {
            _repository = repository;
        }
        public async Task<DataResult> GetAllAsync(GetAllMaHeThongInputDto input)
        {
            try
            {
                IQueryable<MaHeThongDto> query = (from o in _repository.GetAll()
                                                  select new MaHeThongDto
                                                  {
                                                      Id = o.Id,
                                                      Code = o.Code,
                                                      Title = o.Title,
                                                      Description = o.Description,
                                                      TenantId = o.TenantId,
                                                  })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()) || x.Description.ToLower().Contains(input.Keyword.ToLower()));
                List<MaHeThongDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<MaHeThongDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Create(MaHeThongDto dto)
        {
            try
            {
                var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId== AbpSession.TenantId).FirstOrDefault();
                if (result != null)
                {
                    throw new UserFriendlyException("Mã hệ thống: " + dto.Code + " đã tồn tại");
                }
                MaHeThong item = dto.MapTo<MaHeThong>();
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
        public async Task<DataResult> Update(MaHeThongDto dto)
        {
            try
            {
                MaHeThong item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    if (item.Code != dto.Code)
                    {
                        var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                        if (result != null)
                        {
                            throw new UserFriendlyException("Mã hệ thống: " + dto.Code + " đã tồn tại");
                        }
                    }
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
                MaHeThong item = await _repository.GetAsync(id);
                if (item == null)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.DeleteFail, "Bản ghi không tồn tại");
                }
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
