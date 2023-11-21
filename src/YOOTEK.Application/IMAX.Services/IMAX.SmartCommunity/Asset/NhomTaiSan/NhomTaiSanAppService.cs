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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface INhomTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllNhomTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(NhomTaiSanDto input);
        Task<DataResult> Update(NhomTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class NhomTaiSanAppService : IMAXAppServiceBase, INhomTaiSanAppService
    {
        private readonly IRepository<NhomTaiSan, long> _repository;
        private readonly IRepository<MaHeThong, long> _repositoryMaHeThong;
        public NhomTaiSanAppService(IRepository<NhomTaiSan, long> repository, IRepository<MaHeThong, long> repositoryMaHeThong)
        {
            _repository = repository;
            _repositoryMaHeThong = repositoryMaHeThong;
        }

        public async Task<DataResult> GetAllAsync(GetAllNhomTaiSanInputDto input)
        {
            try
            {
                IQueryable<NhomTaiSanDto> query = (from o in _repository.GetAll()
                                                   select new NhomTaiSanDto
                                                   {
                                                       Id = o.Id,
                                                       Code = o.Code,
                                                       Title = o.Title,
                                                       Description = o.Description,
                                                       TenantId = o.TenantId,
                                                       ParentId = o.ParentId,
                                                       MaHeThongId = o.MaHeThongId,
                                                       MaHeThongText = _repositoryMaHeThong.GetAll().Where(x => x.Id == o.MaHeThongId).Select(x => x.Title).FirstOrDefault(),
                                                       ParentText = !o.ParentId.HasValue ? "" : _repository.GetAll().Where(x => x.Id == o.ParentId.Value).Select(x => x.Title).FirstOrDefault(),
                                                   })
                .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
                                x.Code.ToLower().Contains(input.Keyword.ToLower().Trim()) ||
                                x.Title.ToLower().Contains(input.Keyword.ToLower().Trim()) ||
                                x.Description.ToLower().Contains(input.Keyword.ToLower().Trim()))
                .WhereIf(input.ParentId > 0, x => x.ParentId.Value == input.ParentId)
                 .WhereIf(input.MaHeThongId > 0, x => x.MaHeThongId == input.MaHeThongId);

                List<NhomTaiSanDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<NhomTaiSanDto>();
                data.MaHeThongText = _repositoryMaHeThong.GetAll().Where(x => x.Id == data.MaHeThongId).Select(x => x.Title).FirstOrDefault();
                data.ParentText = !data.ParentId.HasValue ? "" : _repository.GetAll().Where(x => x.Id == data.ParentId.Value).Select(x => x.Title).FirstOrDefault();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Create(NhomTaiSanDto dto)
        {
            try
            {
                var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                if (result != null)
                {
                    throw new UserFriendlyException("Mã nhóm tài sản: " + dto.Code + " đã tồn tại");
                }
                NhomTaiSan item = dto.MapTo<NhomTaiSan>();
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
        public async Task<DataResult> Update(NhomTaiSanDto dto)
        {
            try
            {
                NhomTaiSan item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    if (item.Code != dto.Code)
                    {
                        var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                        if (result != null)
                        {
                            throw new UserFriendlyException("Mã nhóm tài sản: " + dto.Code + " đã tồn tại");
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
