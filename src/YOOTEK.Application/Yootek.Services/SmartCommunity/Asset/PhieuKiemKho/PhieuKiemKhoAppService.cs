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
    public interface IPhieuKiemKhoAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuKiemKhoInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuKiemKhoDto input);
        Task<DataResult> Update(PhieuKiemKhoDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuKiemKhoAppService : YootekAppServiceBase, IPhieuKiemKhoAppService
    {
        private readonly IRepository<PhieuKiemKho, long> _repository;
        private readonly IRepository<KhoTaiSan, long> _khoTaiSanRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<PhieuKiemKhoToTaiSan, long> _phieuKiemKhoToTaiSanRepository;
        public PhieuKiemKhoAppService(IRepository<PhieuKiemKho, long> repository,
IRepository<KhoTaiSan, long> khoTaiSanRepository,
IRepository<User, long> userRepository, IRepository<PhieuKiemKhoToTaiSan, long> phieuKiemKhoToTaiSanRepository)
        {
            _repository = repository;
            _khoTaiSanRepository = khoTaiSanRepository;
            _userRepository = userRepository;
            _phieuKiemKhoToTaiSanRepository = phieuKiemKhoToTaiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuKiemKhoInputDto input)
        {
            try
            {
                IQueryable<PhieuKiemKhoDto> query = (from o in _repository.GetAll()
                                                     select new PhieuKiemKhoDto
                                                     {
                                                         Id = o.Id,
                                                         Code = o.Code,
                                                         NgayKiemKho = o.NgayKiemKho,
                                                         KhoTaiSanId = o.KhoTaiSanId,
                                                         NguoiLapPhieuId = o.NguoiLapPhieuId,
                                                         FileDinhKem = o.FileDinhKem,
                                                         NguoiXacNhanId = o.NguoiXacNhanId,
                                                         GhiChu = o.GhiChu,
                                                         TenantId = o.TenantId,
                                                         KhoTaiSanText = !o.KhoTaiSanId.HasValue ? "" : _khoTaiSanRepository.GetAll().Where(x => x.Id == o.KhoTaiSanId.Value).Select(x => x.Title).FirstOrDefault(),
                                                         NguoiLapPhieuText = !o.NguoiLapPhieuId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.NguoiLapPhieuId.Value).Select(x => x.FullName).FirstOrDefault(),
                                                         NguoiXacNhanText = !o.NguoiXacNhanId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.NguoiXacNhanId.Value).Select(x => x.FullName).FirstOrDefault(),
                                                         TaiSans = _phieuKiemKhoToTaiSanRepository.GetAll().Where(x => x.PhieuKiemKhoId == o.Id).ToList()
                                                     })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()))
    .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.ToLower().Equals(input.Code.ToLower()))

    .WhereIf(input.KhoTaiSanId > 0, x => x.KhoTaiSanId.Value == input.KhoTaiSanId)
;
                List<PhieuKiemKhoDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuKiemKhoDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(PhieuKiemKhoDto dto)
        {
            try
            {
                PhieuKiemKho item = dto.MapTo<PhieuKiemKho>();
                item.TenantId = AbpSession.TenantId;
                item.Code = getCode();
                if (dto.TaiSans.Count <= 0)
                {
                    return DataResult.ResultError(Common.Resource.QuanLyChung.InsertFail, "Không có tài sản nào");
                }
                var id = await _repository.InsertAndGetIdAsync(item);
                if (id > 0)
                {
                    foreach (PhieuKiemKhoToTaiSan taiSan in dto.TaiSans)
                    {
                        taiSan.Id = 0;
                        taiSan.PhieuKiemKhoId = id;
                        taiSan.TenantId = AbpSession.TenantId;
                        await _phieuKiemKhoToTaiSanRepository.InsertAsync(taiSan);
                    }
                }
                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Update(PhieuKiemKhoDto dto)
        {
            try
            {
                PhieuKiemKho item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    if (dto.TaiSans.Count <= 0)
                    {
                        return DataResult.ResultError(Common.Resource.QuanLyChung.InsertFail, "Không có tài sản nào");
                    }
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    var lstId = dto.TaiSans.Where(x => x.PhieuKiemKhoId > 0).Select(x => x.Id).ToList();
                    var TaiSanIds = _phieuKiemKhoToTaiSanRepository.GetAll().Where(x => x.PhieuKiemKhoId == dto.Id && !lstId.Contains(x.Id)).Select(x => x.Id).ToList();
                    await _repository.UpdateAsync(item);
                    //Xử lý tài sản
                    foreach (PhieuKiemKhoToTaiSan taiSan in dto.TaiSans)
                    {
                        taiSan.TenantId = AbpSession.TenantId;
                        if (taiSan.PhieuKiemKhoId > 0)
                        {
                            await _phieuKiemKhoToTaiSanRepository.UpdateAsync(taiSan);
                        }
                        else
                        {
                            taiSan.Id = 0;
                            taiSan.PhieuKiemKhoId = dto.Id;
                            await _phieuKiemKhoToTaiSanRepository.InsertAsync(taiSan);
                        }
                    }
                    //Xóa tài sản cũ
                    if (TaiSanIds.Count > 0)
                        await _phieuKiemKhoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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
                //Xóa tài sản cũ
                var TaiSanIds = _phieuKiemKhoToTaiSanRepository.GetAll().Where(x => x.PhieuKiemKhoId == id).Select(x => x.Id).ToList();
                if (TaiSanIds.Count > 0)
                    await _phieuKiemKhoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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

        private string getCode()
        {
            string code = "";
            var item = _repository.GetAll().Where(x => x.TenantId == AbpSession.TenantId).OrderByDescending(x => x.Id).Skip(0).Take(1).FirstOrDefault();
            if (item != null)
            {
                string numberPart = item.Code.Substring(3);
                int number = int.Parse(numberPart);
                number++;
                string incrementedNumber = "";
                if (number <= 999999999)
                    incrementedNumber = number.ToString().PadLeft(10, '0');
                else
                    incrementedNumber = number.ToString();
                // Kết hợp lại với chuỗi "PNK" và trả về kết quả
                return "PKK" + incrementedNumber;
            }
            else
            {
                code = "PKK0000000001";
            }
            return code;
        }
    }
}
