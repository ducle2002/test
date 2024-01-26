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
    public interface IPhieuKiemKhoToTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuKiemKhoToTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuKiemKhoToTaiSanDto input);
        Task<DataResult> Update(PhieuKiemKhoToTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuKiemKhoToTaiSanAppService : YootekAppServiceBase, IPhieuKiemKhoToTaiSanAppService
    {
        private readonly IRepository<PhieuKiemKhoToTaiSan, long> _repository;
        private readonly IRepository<PhieuKiemKho, long> _phieuKiemKhoRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;
        public PhieuKiemKhoToTaiSanAppService(IRepository<PhieuKiemKhoToTaiSan, long> repository,
IRepository<PhieuKiemKho, long> phieuKiemKhoRepository,
IRepository<TaiSan, long> taiSanRepository)
        {
            _repository = repository;
            _phieuKiemKhoRepository = phieuKiemKhoRepository;
            _taiSanRepository = taiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuKiemKhoToTaiSanInputDto input)
        {
            try
            {
                IQueryable<PhieuKiemKhoToTaiSanDto> query = (from o in _repository.GetAll()
                                                             select new PhieuKiemKhoToTaiSanDto
                                                             {
                                                                 Id = o.Id,
                                                                 PhieuKiemKhoId = o.PhieuKiemKhoId,
                                                                 TaiSanId = o.TaiSanId,
                                                                 TongSoLuong = o.TongSoLuong,
                                                                 SoLuongDat = o.SoLuongDat,
                                                                 SoLuongHong = o.SoLuongHong,
                                                                 GhiChu = o.GhiChu,
                                                                 TenantId = o.TenantId,
                                                                 PhieuKiemKhoText = _phieuKiemKhoRepository.GetAll().Where(x => x.Id == o.PhieuKiemKhoId).Select(x => x.Code).FirstOrDefault(),
                                                                 TaiSanText = _taiSanRepository.GetAll().Where(x => x.Id == o.TaiSanId).Select(x => x.Title).FirstOrDefault(),
                                                             })
    .WhereIf(input.PhieuKiemKhoId > 0, x => x.PhieuKiemKhoId == input.PhieuKiemKhoId)

    .WhereIf(input.TaiSanId > 0, x => x.TaiSanId == input.TaiSanId)
;
                List<PhieuKiemKhoToTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuKiemKhoToTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(PhieuKiemKhoToTaiSanDto dto)
        {
            try
            {
                PhieuKiemKhoToTaiSan item = dto.MapTo<PhieuKiemKhoToTaiSan>();
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
        public async Task<DataResult> Update(PhieuKiemKhoToTaiSanDto dto)
        {
            try
            {
                PhieuKiemKhoToTaiSan item = await _repository.GetAsync(dto.Id);
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
