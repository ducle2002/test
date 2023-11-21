using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DocumentFormat.OpenXml.Office2010.Excel;
using IMAX.Application;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using Microsoft.EntityFrameworkCore;
using Nest;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface IPhieuNhapKhoAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuNhapKhoInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuNhapKhoDto input);
        Task<DataResult> Update(PhieuNhapKhoDto input);
        Task<DataResult> UpdateStatus(long id);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuNhapKhoAppService : IMAXAppServiceBase, IPhieuNhapKhoAppService
    {
        private readonly IRepository<PhieuNhapKho, long> _repository;
        private readonly IRepository<KhoTaiSan, long> _khoTaiSanRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<PhieuNhapKhoToTaiSan, long> _phieuNhapKhoToTaiSanRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;

        public PhieuNhapKhoAppService(IRepository<PhieuNhapKho, long> repository,
IRepository<KhoTaiSan, long> khoTaiSanRepository,
IRepository<User, long> userRepository, IRepository<PhieuNhapKhoToTaiSan, long> phieuNhapKhoToTaiSanRepository, IRepository<TaiSan, long> taiSanRepository)
        {
            _repository = repository;
            _khoTaiSanRepository = khoTaiSanRepository;
            _userRepository = userRepository;
            _phieuNhapKhoToTaiSanRepository = phieuNhapKhoToTaiSanRepository;
            _taiSanRepository = taiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuNhapKhoInputDto input)
        {
            try
            {
                if(!input.OrderBy.HasValue)
                {
                    input.OrderBy = FieldSortPhieuNhapKho.ID;
                }    
                IQueryable<PhieuNhapKhoDto> query = (from o in _repository.GetAll()
                                                     select new PhieuNhapKhoDto
                                                     {
                                                         Id = o.Id,
                                                         Code = o.Code,
                                                         NgayNhap = o.NgayNhap,
                                                         KhoTaiSanId = o.KhoTaiSanId,
                                                         NguoiGiaoHang = o.NguoiGiaoHang,
                                                         NguoiLapPhieuId = o.NguoiLapPhieuId,
                                                         TongTien = o.TongTien,
                                                         TongTienBangChu = o.TongTienBangChu,
                                                         KeToanId = o.KeToanId,
                                                         FileDinhKem = o.FileDinhKem,
                                                         ThuKhoId = o.ThuKhoId,
                                                         TrangThai = o.TrangThai,
                                                         TenantId = o.TenantId,
                                                         KhoTaiSanText = !o.KhoTaiSanId.HasValue ? "" : _khoTaiSanRepository.GetAll().Where(x => x.Id == o.KhoTaiSanId.Value).Select(x => x.Title).FirstOrDefault(),
                                                         NguoiLapPhieuText = !o.NguoiLapPhieuId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.NguoiLapPhieuId.Value).Select(x => x.FullName).FirstOrDefault(),
                                                         KeToanText = !o.KeToanId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.KeToanId.Value).Select(x => x.FullName).FirstOrDefault(),
                                                         ThuKhoText = !o.ThuKhoId.HasValue ? "" : _userRepository.GetAll().Where(x => x.Id == o.ThuKhoId.Value).Select(x => x.FullName).FirstOrDefault(),


                                                     })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.NguoiGiaoHang.ToLower().Contains(input.Keyword.ToLower()))
    .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.ToLower().Equals(input.Code.ToLower()))

    .WhereIf(input.KhoTaiSanId > 0, x => x.KhoTaiSanId.Value == input.KhoTaiSanId)

    .WhereIf(input.NguoiLapPhieuId > 0, x => x.NguoiLapPhieuId.Value == input.NguoiLapPhieuId)

    .WhereIf(input.KeToanId > 0, x => x.KeToanId.Value == input.KeToanId)

    .WhereIf(input.ThuKhoId > 0, x => x.ThuKhoId.Value == input.ThuKhoId)

    .WhereIf(input.TrangThai > 0, x => x.TrangThai.Value == input.TrangThai)
;
                List<PhieuNhapKhoDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuNhapKhoDto>();
                data.TaiSans = _phieuNhapKhoToTaiSanRepository.GetAll().Where(x => x.PhieuNhapKhoId == id).ToList();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Create(PhieuNhapKhoDto dto)
        {
            try
            {
                PhieuNhapKho item = dto.MapTo<PhieuNhapKho>();
                item.TenantId = AbpSession.TenantId;
                item.Code = getCode();
                item.TrangThai = (int)AssetStatus.ChoDuyet;
                if (dto.TaiSans.Count <= 0)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Không có tài sản nào");
                }
                //Check kiểm tra 2 tài sản trùng nhau
                var duplicateIds = dto.TaiSans
                .GroupBy(record => record.TaiSanId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
                if (duplicateIds.Any())
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Có tài sản trùng nhau");
                }
                var id = await _repository.InsertAndGetIdAsync(item);
                if (id == 0)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Thêm mới thất bại");
                }
                foreach (PhieuNhapKhoToTaiSan taiSan in dto.TaiSans)
                {
                    taiSan.Id = 0;
                    taiSan.PhieuNhapKhoId = id;
                    taiSan.ThanhTien = taiSan.DonGia * taiSan.SoLuong;
                    taiSan.TenantId = AbpSession.TenantId;
                    taiSan.TrangThai = (int)AssetStatus.ChoDuyet;
                    await _phieuNhapKhoToTaiSanRepository.InsertAsync(taiSan);
                }

                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Update(PhieuNhapKhoDto dto)
        {
            try
            {
                PhieuNhapKho item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    if (item.TrangThai == (int)AssetStatus.DaDuyet)
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Không thể chỉnh sửa bản ghi này");
                    }
                    if (dto.TaiSans.Count <= 0)
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Không có tài sản nào");
                    }
                    //Check kiểm tra 2 tài sản trùng nhau
                    var duplicateIds = dto.TaiSans
                    .GroupBy(record => record.TaiSanId)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key);
                    if (duplicateIds.Any())
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Có tài sản trùng nhau");
                    }
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    var lstId = dto.TaiSans.Where(x => x.PhieuNhapKhoId > 0).Select(x => x.Id).ToList();
                    var TaiSanIds = _phieuNhapKhoToTaiSanRepository.GetAll().Where(x => x.PhieuNhapKhoId == dto.Id && !lstId.Contains(x.Id)).Select(x => x.Id).ToList();
                    await _repository.UpdateAsync(item);
                    //Xử lý tài sản
                    foreach (var taiSan in dto.TaiSans)
                    {
                        taiSan.ThanhTien = taiSan.DonGia * taiSan.SoLuong;
                        taiSan.TenantId = AbpSession.TenantId;
                        taiSan.TrangThai = (int)AssetStatus.ChoDuyet;
                        if (taiSan.PhieuNhapKhoId > 0)
                        {
                            await _phieuNhapKhoToTaiSanRepository.UpdateAsync(taiSan);
                        }
                        else
                        {
                            taiSan.Id = 0;
                            taiSan.PhieuNhapKhoId = dto.Id;
                            await _phieuNhapKhoToTaiSanRepository.InsertAsync(taiSan);
                        }
                    }
                    //Xóa tài sản cũ
                    if (TaiSanIds.Count > 0)
                        await _phieuNhapKhoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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

        //Phê duyệt
        public async Task<DataResult> UpdateStatus(long id)
        {
            try
            {
                PhieuNhapKho item = await _repository.GetAsync(id);
                if (item == null)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Bản ghi không tồn tại");
                }
                if (item.TrangThai == (int)AssetStatus.DaDuyet)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Bản ghi đã được phê duyệt");
                }
                item.TrangThai = (int)AssetStatus.DaDuyet;
                var TaiSans = _phieuNhapKhoToTaiSanRepository.GetAll().Where(x => x.PhieuNhapKhoId == id).ToList();
                if (TaiSans.Count <= 0)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Không thể phê duyệt vì không có tài sản");
                }
                var taiSanKhos = _taiSanRepository.GetAll().Where(x => TaiSans.Select(y => y.TaiSanId).ToList().Contains(x.Id)).ToList();
                foreach (var taiSan in TaiSans)
                {
                    var taiSanKhoItem = taiSanKhos.Find(x => x.Id == taiSan.TaiSanId);
                    if (taiSanKhoItem == null)
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Tài sản: " + taiSan.TaiSanId + " không tồn tại");
                    }

                }
                await _repository.UpdateAsync(item);
                foreach (var ts in TaiSans)
                {
                    var taiSanKhoItem = taiSanKhos.Find(x => x.Id == ts.TaiSanId);
                    taiSanKhoItem.SoLuongTrongKho = taiSanKhoItem.SoLuongTrongKho + ts.SoLuong;
                    taiSanKhoItem.TongSoLuong = taiSanKhoItem.TongSoLuong + ts.SoLuong;
                    ts.TrangThai = (int)AssetStatus.DaDuyet;
                    ts.TongSoLuong = taiSanKhoItem.SoLuongTrongKho;
                    await _taiSanRepository.UpdateAsync(taiSanKhoItem);
                    await _phieuNhapKhoToTaiSanRepository.UpdateAsync(ts);
                }
                return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.ApproveSuccess);
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
                PhieuNhapKho item = await _repository.GetAsync(id);
                if (item == null)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Bản ghi không tồn tại");
                }
                if (item.TrangThai == (int)AssetStatus.DaDuyet)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Không thể xóa bản ghi này");
                }
                //Xóa tài sản cũ
                var TaiSanIds = _phieuNhapKhoToTaiSanRepository.GetAll().Where(x => x.PhieuNhapKhoId == id).Select(x => x.Id).ToList();
                if (TaiSanIds.Count > 0)
                    await _phieuNhapKhoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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
                return "PNK" + incrementedNumber;
            }
            else
            {
                code = "PNK0000000001";
            }
            return code;
        }
    }
}
