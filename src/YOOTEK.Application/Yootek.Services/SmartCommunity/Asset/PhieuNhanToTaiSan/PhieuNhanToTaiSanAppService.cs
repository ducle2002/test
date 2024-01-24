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
    public interface IPhieuNhanToTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuNhanToTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuNhanToTaiSanDto input);
        Task<DataResult> Update(PhieuNhanToTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuNhanToTaiSanAppService : YootekAppServiceBase, IPhieuNhanToTaiSanAppService
    {
        private readonly IRepository<PhieuNhanToTaiSan, long> _repository;
        private readonly IRepository<PhieuNhanTaiSan, long> _phieuNhanTaiSanRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;
        public PhieuNhanToTaiSanAppService(IRepository<PhieuNhanToTaiSan, long> repository,
IRepository<PhieuNhanTaiSan, long> phieuNhanTaiSanRepository,
IRepository<TaiSan, long> taiSanRepository)
        {
            _repository = repository;
            _phieuNhanTaiSanRepository = phieuNhanTaiSanRepository;
            _taiSanRepository = taiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuNhanToTaiSanInputDto input)
        {
            try
            {
                IQueryable<PhieuNhanToTaiSanDto> query = (from o in _repository.GetAll()
                                                          select new PhieuNhanToTaiSanDto
                                                          {
                                                              Id = o.Id,
                                                              PhieuNhanTaiSanId = o.PhieuNhanTaiSanId,
                                                              TaiSanId = o.TaiSanId,
                                                              SoLuong = o.SoLuong,
                                                              DonGia = o.DonGia,
                                                              ThanhTien = o.ThanhTien,
                                                              GhiChu = o.GhiChu,
                                                              TenantId = o.TenantId,
                                                              PhieuNhanTaiSanText = _phieuNhanTaiSanRepository.GetAll().Where(x => x.Id == o.PhieuNhanTaiSanId).Select(x => x.Code).FirstOrDefault(),
                                                              TaiSanText = _taiSanRepository.GetAll().Where(x => x.Id == o.TaiSanId).Select(x => x.Title).FirstOrDefault(),
                                                          })
    .WhereIf(input.PhieuNhanTaiSanId > 0, x => x.PhieuNhanTaiSanId == input.PhieuNhanTaiSanId)

    .WhereIf(input.TaiSanId > 0, x => x.TaiSanId == input.TaiSanId)
;
                List<PhieuNhanToTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuNhanToTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(PhieuNhanToTaiSanDto dto)
        {
            try
            {
                PhieuNhanToTaiSan item = dto.MapTo<PhieuNhanToTaiSan>();
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
        public async Task<DataResult> Update(PhieuNhanToTaiSanDto dto)
        {
            try
            {
                PhieuNhanToTaiSan item = await _repository.GetAsync(dto.Id);
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
