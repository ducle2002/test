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
    public interface IPhieuGiaoTaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuGiaoTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuGiaoTaiSanDto input);
        Task<DataResult> Update(PhieuGiaoTaiSanDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuGiaoTaiSanAppService : YootekAppServiceBase, IPhieuGiaoTaiSanAppService
    {
        private readonly IRepository<PhieuGiaoTaiSan, long> _repository;
        private readonly IRepository<KhoTaiSan, long> _khoTaiSanRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<PhieuGiaoToTaiSan, long> _phieuGiaoToTaiSanRepository;
        public PhieuGiaoTaiSanAppService(IRepository<PhieuGiaoTaiSan, long> repository,
IRepository<KhoTaiSan, long> khoTaiSanRepository,
IRepository<User, long> userRepository, IRepository<PhieuGiaoToTaiSan, long> phieuGiaoToTaiSanRepository)
        {
            _repository = repository;
            _khoTaiSanRepository = khoTaiSanRepository;
            _userRepository = userRepository;
            _phieuGiaoToTaiSanRepository = phieuGiaoToTaiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuGiaoTaiSanInputDto input)
        {
            try
            {
                IQueryable<PhieuGiaoTaiSanDto> query = (from o in _repository.GetAll()
                                                        select new PhieuGiaoTaiSanDto
                                                        {
                                                            Id = o.Id,
                                                            Code = o.Code,
                                                            NgayGiao = o.NgayGiao,
                                                            KhoTaiSanId = o.KhoTaiSanId,
                                                            NguoiNhan = o.NguoiNhan,
                                                            NguoiLapPhieuId = o.NguoiLapPhieuId,
                                                            TongTien = o.TongTien,
                                                            TongTienBangChu = o.TongTienBangChu,
                                                            KeToanId = o.KeToanId,
                                                            FileDinhKem = o.FileDinhKem,
                                                            ThuKhoId = o.ThuKhoId,
                                                            TrangThai = o.TrangThai,
                                                            LyDo = o.LyDo,
                                                            TenantId = o.TenantId,
                                                            KhoTaiSanText = !o.KhoTaiSanId.HasValue ? "" : _khoTaiSanRepository.GetAll().Where(x => x.Id == o.KhoTaiSanId.Value).Select(x => x.Title).FirstOrDefault(),
                                                            NguoiLapPhieuText = !o.NguoiLapPhieuId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.NguoiLapPhieuId.Value).Select(x => x.FullName).FirstOrDefault(),
                                                            KeToanText = !o.KeToanId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.KeToanId.Value).Select(x => x.FullName).FirstOrDefault(),
                                                            ThuKhoText = !o.ThuKhoId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.ThuKhoId.Value).Select(x => x.FullName).FirstOrDefault(),
                                                            TaiSans = _phieuGiaoToTaiSanRepository.GetAll().Where(x => x.PhieuGiaoTaiSanId == o.Id).ToList()
                                                        })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.NguoiNhan.ToLower().Contains(input.Keyword.ToLower()))
    .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.ToLower().Equals(input.Code.ToLower()))

    .WhereIf(input.KhoTaiSanId > 0, x => x.KhoTaiSanId.Value == input.KhoTaiSanId)

    .WhereIf(input.ThuKhoId > 0, x => x.ThuKhoId.Value == input.ThuKhoId)

    .WhereIf(input.TrangThai > 0, x => x.TrangThai.Value == input.TrangThai)
;
                List<PhieuGiaoTaiSanDto> result = await query.OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuGiaoTaiSanDto>();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(PhieuGiaoTaiSanDto dto)
        {
            try
            {
                PhieuGiaoTaiSan item = dto.MapTo<PhieuGiaoTaiSan>();
                item.TenantId = AbpSession.TenantId;
                item.Code = getCode();
                if (dto.TaiSans.Count <= 0)
                {
                    return DataResult.ResultError(Common.Resource.QuanLyChung.InsertFail, "Không có tài sản nào");
                }
                var id = await _repository.InsertAndGetIdAsync(item);
                if (id > 0)
                {
                    foreach (PhieuGiaoToTaiSan taiSan in dto.TaiSans)
                    {
                        taiSan.Id = 0;
                        taiSan.PhieuGiaoTaiSanId = id;
                        taiSan.ThanhTien = taiSan.DonGia * taiSan.SoLuong;
                        taiSan.TenantId = AbpSession.TenantId;
                        await _phieuGiaoToTaiSanRepository.InsertAsync(taiSan);
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
        public async Task<DataResult> Update(PhieuGiaoTaiSanDto dto)
        {
            try
            {
                PhieuGiaoTaiSan item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    if (dto.TaiSans.Count <= 0)
                    {
                        return DataResult.ResultError(Common.Resource.QuanLyChung.InsertFail, "Không có tài sản nào");
                    }
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    var lstId = dto.TaiSans.Where(x => x.PhieuGiaoTaiSanId > 0).Select(x => x.Id).ToList();
                    var TaiSanIds = _phieuGiaoToTaiSanRepository.GetAll().Where(x => x.PhieuGiaoTaiSanId == dto.Id && !lstId.Contains(x.Id)).Select(x => x.Id).ToList();
                    await _repository.UpdateAsync(item);
                    //Xử lý tài sản
                    foreach (PhieuGiaoToTaiSan taiSan in dto.TaiSans)
                    {
                        taiSan.ThanhTien = taiSan.DonGia * taiSan.SoLuong;
                        taiSan.TenantId = AbpSession.TenantId;
                        if (taiSan.PhieuGiaoTaiSanId > 0)
                        {
                            await _phieuGiaoToTaiSanRepository.UpdateAsync(taiSan);
                        }
                        else
                        {
                            taiSan.Id = 0;
                            taiSan.PhieuGiaoTaiSanId = dto.Id;
                            await _phieuGiaoToTaiSanRepository.InsertAsync(taiSan);
                        }
                    }
                    //Xóa tài sản cũ
                    if (TaiSanIds.Count > 0)
                        await _phieuGiaoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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
                var TaiSanIds = _phieuGiaoToTaiSanRepository.GetAll().Where(x => x.PhieuGiaoTaiSanId == id).Select(x => x.Id).ToList();
                if (TaiSanIds.Count > 0)
                    await _phieuGiaoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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
                return "GTS" + incrementedNumber;
            }
            else
            {
                code = "GTS0000000001";
            }
            return code;
        }
    }
}
