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
    public interface IPhieuXuatKhoToTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuXuatKhoToTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuXuatKhoToTaiSanDto input);
        Task<DataResult> Update(PhieuXuatKhoToTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuXuatKhoToTaiSanAppService : YootekAppServiceBase, IPhieuXuatKhoToTaiSanAppService
    {
        private readonly IRepository<PhieuXuatKhoToTaiSan, long> _repository;
        private readonly IRepository<PhieuXuatKho, long> _phieuXuatKhoRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;
        public PhieuXuatKhoToTaiSanAppService(IRepository<PhieuXuatKhoToTaiSan, long> repository,
IRepository<PhieuXuatKho, long> phieuXuatKhoRepository,
IRepository<TaiSan, long> taiSanRepository)
        {
            _repository = repository;
            _phieuXuatKhoRepository = phieuXuatKhoRepository;
            _taiSanRepository = taiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuXuatKhoToTaiSanInputDto input)
        {
            try
            {
                IQueryable<PhieuXuatKhoToTaiSanDto> query = (from o in _repository.GetAll()
                                                             select new PhieuXuatKhoToTaiSanDto
                                                             {
                                                                 Id = o.Id,
                                                                 PhieuXuatKhoId = o.PhieuXuatKhoId,
                                                                 TaiSanId = o.TaiSanId,
                                                                 SoLuong = o.SoLuong,
                                                                 DonGia = o.DonGia,
                                                                 ThanhTien = o.ThanhTien,
                                                                 GhiChu = o.GhiChu,
                                                                 TenantId = o.TenantId,
                                                                 PhieuXuatKhoText = _phieuXuatKhoRepository.GetAll().Where(x => x.Id == o.PhieuXuatKhoId).Select(x => x.Code).FirstOrDefault(),
                                                                 TaiSanText = _taiSanRepository.GetAll().Where(x => x.Id == o.TaiSanId).Select(x => x.Title).FirstOrDefault(),
                                                             })
    .WhereIf(input.PhieuXuatKhoId > 0, x => x.PhieuXuatKhoId == input.PhieuXuatKhoId)

    .WhereIf(input.TaiSanId > 0, x => x.TaiSanId == input.TaiSanId)
;
                List<PhieuXuatKhoToTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuXuatKhoToTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(PhieuXuatKhoToTaiSanDto dto)
        {
            try
            {
                PhieuXuatKhoToTaiSan item = dto.MapTo<PhieuXuatKhoToTaiSan>();
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
        public async Task<DataResult> Update(PhieuXuatKhoToTaiSanDto dto)
        {
            try
            {
                PhieuXuatKhoToTaiSan item = await _repository.GetAsync(dto.Id);
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
