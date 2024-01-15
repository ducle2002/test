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
    public interface IPhieuNhapKhoToTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuNhapKhoToTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuNhapKhoToTaiSanDto input);
        Task<DataResult> Update(PhieuNhapKhoToTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuNhapKhoToTaiSanAppService : YootekAppServiceBase, IPhieuNhapKhoToTaiSanAppService
    {
        private readonly IRepository<PhieuNhapKhoToTaiSan, long> _repository;
        private readonly IRepository<PhieuNhapKho, long> _phieuNhapKhoRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;
        public PhieuNhapKhoToTaiSanAppService(IRepository<PhieuNhapKhoToTaiSan, long> repository,
IRepository<PhieuNhapKho, long> phieuNhapKhoRepository,
IRepository<TaiSan, long> taiSanRepository)
        {
            _repository = repository;
            _phieuNhapKhoRepository = phieuNhapKhoRepository;
            _taiSanRepository = taiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuNhapKhoToTaiSanInputDto input)
        {
            try
            {
                IQueryable<PhieuNhapKhoToTaiSanDto> query = (from o in _repository.GetAll()
                                                             select new PhieuNhapKhoToTaiSanDto
                                                             {
                                                                 Id = o.Id,
                                                                 PhieuNhapKhoId = o.PhieuNhapKhoId,
                                                                 TaiSanId = o.TaiSanId,
                                                                 SoLuong = o.SoLuong,
                                                                 DonGia = o.DonGia,
                                                                 ThanhTien = o.ThanhTien,
                                                                 GhiChu = o.GhiChu,
                                                                 TenantId = o.TenantId,
                                                                 PhieuNhapKhoText = _phieuNhapKhoRepository.GetAll().Where(x => x.Id == o.PhieuNhapKhoId).Select(x => x.Code).FirstOrDefault(),
                                                                 TaiSanText = _taiSanRepository.GetAll().Where(x => x.Id == o.TaiSanId).Select(x => x.Title).FirstOrDefault(),
                                                             })
    .WhereIf(input.PhieuNhapKhoId > 0, x => x.PhieuNhapKhoId == input.PhieuNhapKhoId)

    .WhereIf(input.TaiSanId > 0, x => x.TaiSanId == input.TaiSanId)
;
                List<PhieuNhapKhoToTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuNhapKhoToTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(PhieuNhapKhoToTaiSanDto dto)
        {
            try
            {
                PhieuNhapKhoToTaiSan item = dto.MapTo<PhieuNhapKhoToTaiSan>();
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
        public async Task<DataResult> Update(PhieuNhapKhoToTaiSanDto dto)
        {
            try
            {
                PhieuNhapKhoToTaiSan item = await _repository.GetAsync(dto.Id);
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
