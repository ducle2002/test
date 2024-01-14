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
    public interface IPhieuGiaoToTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuGiaoToTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuGiaoToTaiSanDto input);
        Task<DataResult> Update(PhieuGiaoToTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuGiaoToTaiSanAppService : YootekAppServiceBase, IPhieuGiaoToTaiSanAppService
    {
        private readonly IRepository<PhieuGiaoToTaiSan, long> _repository;
        private readonly IRepository<PhieuGiaoTaiSan, long> _phieuGiaoTaiSanRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;
        public PhieuGiaoToTaiSanAppService(IRepository<PhieuGiaoToTaiSan, long> repository,
IRepository<PhieuGiaoTaiSan, long> phieuGiaoTaiSanRepository,
IRepository<TaiSan, long> taiSanRepository)
        {
            _repository = repository;
            _phieuGiaoTaiSanRepository = phieuGiaoTaiSanRepository;
            _taiSanRepository = taiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuGiaoToTaiSanInputDto input)
        {
            try
            {
                IQueryable<PhieuGiaoToTaiSanDto> query = (from o in _repository.GetAll()
                                                          select new PhieuGiaoToTaiSanDto
                                                          {
                                                              Id = o.Id,
                                                              PhieuGiaoTaiSanId = o.PhieuGiaoTaiSanId,
                                                              TaiSanId = o.TaiSanId,
                                                              SoLuong = o.SoLuong,
                                                              DonGia = o.DonGia,
                                                              ThanhTien = o.ThanhTien,
                                                              GhiChu = o.GhiChu,
                                                              TenantId = o.TenantId,
                                                              BaoHanhTu = o.BaoHanhTu,
                                                              BaoHanhDen = o.BaoHanhDen,
                                                              PhieuGiaoTaiSanText = _phieuGiaoTaiSanRepository.GetAll().Where(x => x.Id == o.PhieuGiaoTaiSanId).Select(x => x.Code).FirstOrDefault(),
                                                              TaiSanText = _taiSanRepository.GetAll().Where(x => x.Id == o.TaiSanId).Select(x => x.Title).FirstOrDefault(),
                                                          })
    .WhereIf(input.PhieuGiaoTaiSanId > 0, x => x.PhieuGiaoTaiSanId == input.PhieuGiaoTaiSanId)

    .WhereIf(input.TaiSanId > 0, x => x.TaiSanId == input.TaiSanId)
;
                List<PhieuGiaoToTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuGiaoToTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(PhieuGiaoToTaiSanDto dto)
        {
            try
            {
                PhieuGiaoToTaiSan item = dto.MapTo<PhieuGiaoToTaiSan>();
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
        public async Task<DataResult> Update(PhieuGiaoToTaiSanDto dto)
        {
            try
            {
                PhieuGiaoToTaiSan item = await _repository.GetAsync(dto.Id);
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
