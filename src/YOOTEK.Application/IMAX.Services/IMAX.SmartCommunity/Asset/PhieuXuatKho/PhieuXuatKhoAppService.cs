using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using IMAX.App.ServiceHttpClient;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity;
using IMAX.Application;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using IMAX.IMAX.EntityDb.SmartCommunity.Apartment;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface IPhieuXuatKhoAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllPhieuXuatKhoInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(PhieuXuatKhoDto input);
        Task<DataResult> Update(PhieuXuatKhoDto input);
        Task<DataResult> UpdateStatus(long id);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class PhieuXuatKhoAppService : IMAXAppServiceBase, IPhieuXuatKhoAppService
    {
        private readonly IRepository<PhieuXuatKho, long> _repository;
        private readonly IRepository<KhoTaiSan, long> _khoTaiSanRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<PhieuXuatKhoToTaiSan, long> _phieuXuatKhoToTaiSanRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;
        private readonly IRepository<NhomTaiSan, long> _nhomTaiSanRepository;
        private readonly IRepository<TaiSanChiTiet, long> _taiSanChiTietRepository;
        private readonly IHttpQRCodeService _httpQRCodeService;
        private readonly IRepository<ApartmentHistory, long> _apartmentHistoryRepository;
        public PhieuXuatKhoAppService(IRepository<PhieuXuatKho, long> repository,
IRepository<KhoTaiSan, long> khoTaiSanRepository,
IRepository<User, long> userRepository, IRepository<PhieuXuatKhoToTaiSan, long> phieuXuatKhoToTaiSanRepository, IRepository<TaiSan, long> taiSanRepository, IRepository<NhomTaiSan, long> nhomTaiSanRepository, IRepository<TaiSanChiTiet, long> taiSanChiTietRepository, IHttpQRCodeService httpQRCodeService, IRepository<ApartmentHistory, long> apartmentHistoryRepository)
        {
            _repository = repository;
            _khoTaiSanRepository = khoTaiSanRepository;
            _userRepository = userRepository;
            _phieuXuatKhoToTaiSanRepository = phieuXuatKhoToTaiSanRepository;
            _taiSanRepository = taiSanRepository;
            _nhomTaiSanRepository = nhomTaiSanRepository;
            _taiSanChiTietRepository = taiSanChiTietRepository;
            _nhomTaiSanRepository = nhomTaiSanRepository;
            _taiSanChiTietRepository = taiSanChiTietRepository;
            _apartmentHistoryRepository = apartmentHistoryRepository;
            _httpQRCodeService = httpQRCodeService;
        }
        public async Task<DataResult> GetAllAsync(GetAllPhieuXuatKhoInputDto input)
        {
            try
            {
                if (!input.OrderBy.HasValue)
                {
                    input.OrderBy = FieldSortPhieuXuatKho.ID;
                }
                IQueryable<PhieuXuatKhoDto> query = (from o in _repository.GetAll()
                                                     select new PhieuXuatKhoDto
                                                     {
                                                         Id = o.Id,
                                                         Code = o.Code,
                                                         NgayXuat = o.NgayXuat,
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
                                                     })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()))
    .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.ToLower().Equals(input.Code.ToLower()))

    .WhereIf(input.KhoTaiSanId > 0, x => x.KhoTaiSanId.Value == input.KhoTaiSanId)

    .WhereIf(input.ThuKhoId > 0, x => x.ThuKhoId.Value == input.ThuKhoId)

    .WhereIf(input.TrangThai > 0, x => x.TrangThai.Value == input.TrangThai)
;
                List<PhieuXuatKhoDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<PhieuXuatKhoDto>();
                data.TaiSans = _phieuXuatKhoToTaiSanRepository.GetAll().Where(x => x.PhieuXuatKhoId == id).ToList();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Create(PhieuXuatKhoDto dto)
        {
            try
            {
                PhieuXuatKho item = dto.MapTo<PhieuXuatKho>();
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
                var idts = dto.TaiSans.Select(x => x.TaiSanId).ToList();
                var taiSanKhos = _taiSanRepository.GetAll().Where(x => idts.Contains(x.Id)).ToList();
                foreach (var taiSan in dto.TaiSans)
                {
                    var taiSanKhoItem = taiSanKhos.Find(x => x.Id == taiSan.TaiSanId);
                    if (taiSanKhoItem == null)
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Tài sản: " + taiSan.TaiSanId + " không tồn tại");
                    }
                    if (taiSan.SoLuong > taiSanKhoItem.SoLuongTrongKho)
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Số lượng trong kho của tài sản " + taiSanKhoItem.Title + " không đủ");
                    }
                }
                var id = await _repository.InsertAndGetIdAsync(item);
                if (id == 0)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.InsertFail, "Thêm mới thất bại");
                }
                foreach (var taiSan in dto.TaiSans)
                {
                    var taiSanKhoItem = taiSanKhos.Find(x => x.Id == taiSan.TaiSanId);
                    taiSan.Id = 0;
                    taiSan.PhieuXuatKhoId = id;
                    taiSan.ThanhTien = taiSan.DonGia * taiSan.SoLuong;
                    taiSan.TenantId = AbpSession.TenantId;
                    taiSan.BaoHanhTu = DateTime.Now;
                    taiSan.BaoHanhDen = DateTime.Now.AddDays(taiSanKhoItem.ThoiGianBaoHanh);
                    taiSan.ThoiGianBaoHanh = taiSanKhoItem.ThoiGianBaoHanh;
                    taiSan.TrangThai = (int)AssetStatus.ChoDuyet;
                    await _phieuXuatKhoToTaiSanRepository.InsertAsync(taiSan);
                }

                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Update(PhieuXuatKhoDto dto)
        {
            try
            {
                PhieuXuatKho item = await _repository.GetAsync(dto.Id);
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
                    var lstId = dto.TaiSans.Where(x => x.PhieuXuatKhoId > 0).Select(x => x.Id).ToList();
                    var TaiSanIds = _phieuXuatKhoToTaiSanRepository.GetAll().Where(x => x.PhieuXuatKhoId == dto.Id && !lstId.Contains(x.Id)).Select(x => x.Id).ToList();

                    var taiSanKhos = _taiSanRepository.GetAll().Where(x => dto.TaiSans.Select(x => x.TaiSanId).ToList().Contains(x.Id)).ToList();
                    foreach (var taiSan in dto.TaiSans)
                    {
                        var taiSanKhoItem = taiSanKhos.Find(x => x.Id == taiSan.TaiSanId);
                        if (taiSanKhoItem == null)
                        {
                            throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Tài sản: " + taiSan.TaiSanId + " không tồn tại");
                        }
                        if (taiSan.SoLuong > taiSanKhoItem.SoLuongTrongKho)
                        {
                            throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Số lượng trong kho của tài sản " + taiSanKhoItem.Title + " không đủ");
                        }
                    }

                    await _repository.UpdateAsync(item);
                    //Xử lý tài sản
                    foreach (var taiSan in dto.TaiSans)
                    {
                        var taiSanKhoItem = taiSanKhos.Find(x => x.Id == taiSan.TaiSanId);
                        taiSan.ThanhTien = taiSan.DonGia * taiSan.SoLuong;
                        taiSan.TenantId = AbpSession.TenantId;
                        taiSan.BaoHanhTu = DateTime.Now;
                        taiSan.BaoHanhDen = DateTime.Now.AddDays(taiSanKhoItem.ThoiGianBaoHanh);
                        taiSan.ThoiGianBaoHanh = taiSanKhoItem.ThoiGianBaoHanh;
                        taiSan.TrangThai = (int)AssetStatus.ChoDuyet;
                        if (taiSan.PhieuXuatKhoId > 0)
                        {
                            await _phieuXuatKhoToTaiSanRepository.UpdateAsync(taiSan);
                        }
                        else
                        {
                            taiSan.Id = 0;
                            taiSan.PhieuXuatKhoId = dto.Id;
                            await _phieuXuatKhoToTaiSanRepository.InsertAsync(taiSan);
                        }
                    }
                    //Xóa tài sản cũ
                    if (TaiSanIds.Count > 0)
                        await _phieuXuatKhoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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
                PhieuXuatKho item = await _repository.GetAsync(id);
                if (item == null)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Bản ghi không tồn tại");
                }
                if (item.TrangThai == (int)AssetStatus.DaDuyet)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Bản ghi đã được phê duyệt");
                }
                item.TrangThai = (int)AssetStatus.DaDuyet;
                var TaiSans = _phieuXuatKhoToTaiSanRepository.GetAll().Where(x => x.PhieuXuatKhoId == id).ToList();
                if (TaiSans.Count <= 0)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Không thể phê duyệt vì không có tài sản");
                }
                var taiSanKhos = _taiSanRepository.GetAll().Where(x => TaiSans.Select(y => y.TaiSanId).ToList().Contains(x.Id)).ToList();
                foreach (PhieuXuatKhoToTaiSan taiSan in TaiSans)
                {
                    var taiSanKhoItem = taiSanKhos.Find(x => x.Id == taiSan.TaiSanId);
                    if (taiSanKhoItem == null)
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Tài sản: " + taiSan.TaiSanId + " không tồn tại");
                    }
                    if (taiSan.SoLuong > taiSanKhoItem.SoLuongTrongKho)
                    {
                        throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail, "Số lượng trong kho của tài sản " + taiSanKhoItem.Title + " không đủ");
                    }
                }
                await _repository.UpdateAsync(item);
                foreach (var ts in TaiSans)
                {
                    var taiSanKhoItem = taiSanKhos.Find(x => x.Id == ts.TaiSanId);
                    taiSanKhoItem.SoLuongTrongKho = taiSanKhoItem.SoLuongTrongKho - ts.SoLuong;
                    taiSanKhoItem.TongSoLuong = taiSanKhoItem.TongSoLuong - ts.SoLuong;
                    ts.TrangThai = (int)AssetStatus.DaDuyet;
                    ts.TongSoLuong = taiSanKhoItem.SoLuongTrongKho;
                    await _taiSanRepository.UpdateAsync(taiSanKhoItem); //Cập nhật lại số lượng trong kho
                    await _phieuXuatKhoToTaiSanRepository.UpdateAsync(ts);
                    //Sinh mã tài sản chi tiết Thêm mới tài sản chi tiết
                    var nhomTaiSan = _nhomTaiSanRepository.GetAll().Where(x => x.Id == taiSanKhoItem.NhomTaiSanId).FirstOrDefault();
                    if (nhomTaiSan != null)
                    {
                        string code = taiSanKhoItem.Code + "-" + ts.Id;
                        TaiSanChiTiet _tsChiTiet = new TaiSanChiTiet()
                        {
                            Code = code,
                            Title = taiSanKhoItem.Title,
                            HinhThuc = (int)HinhThucTaiSanChiTietEnum.CDVK,
                            MaHeThongId = nhomTaiSan.MaHeThongId,
                            NhomTaiSanId = nhomTaiSan.Id,
                            TrangThai = (int)TrangThaiTaiSanChiTietEnum.SD,
                            ApartmentId = item.CanHoId,
                            MaSoBaoHanh = "",
                            GiaTriTaiSan = (long)ts.DonGia,
                            NgayBatDau = ts.BaoHanhTu,
                            NgayKetThuc = ts.BaoHanhDen,
                            GhiChu = ts.GhiChu,
                            BuildingId = item.ToaNhaId,
                            SoLuong = ts.SoLuong.ToString(),
                            TenantId = AbpSession.TenantId
                        };
                        var dataTSCT = await _taiSanChiTietRepository.InsertAsync(_tsChiTiet);
                        dataTSCT.QrCode = QRCodeGenerator(dataTSCT.Id, QRCodeActionType.Asset);
                        var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                        {
                            Name = $"QR/Asset/{AbpSession.TenantId}/{dataTSCT.Code}",
                            ActionType = QRCodeActionType.Asset,
                            Code = dataTSCT.QrCode,
                            Data = JsonConvert.SerializeObject(new
                            {
                                assetId = dataTSCT.Id,
                            }),
                            Status = QRCodeStatus.Active,
                            Type = QRCodeType.Text
                        });

                        if (!createQRCodeResult.Success)
                        {
                            throw new UserFriendlyException("Qrcode create fail !");
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
                //Code lưu log căn hộ
                if (item.CanHoId.HasValue)
                {
                    User user = await GetCurrentUserAsync();
                    ApartmentHistory itemHis = new ApartmentHistory()
                    {
                        TenantId = AbpSession.TenantId,
                        ApartmentId = item.CanHoId.Value,
                        Title = "Giao tài sản",
                        Description = "Nhập bàn giao tài sản mới mã hóa đơn xuất: " + item.Code,
                        Type = EApartmentHistoryType.Other,
                        DateStart = item.NgayXuat,
                        ExecutorName = user.FullName
                    };
                    await _apartmentHistoryRepository.InsertAsync(itemHis);
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
                PhieuXuatKho item = await _repository.GetAsync(id);
                if (item == null)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.DeleteFail, "Bản ghi không tồn tại");
                }
                if (item.TrangThai == (int)AssetStatus.DaDuyet)
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.DeleteFail, "Không thể xóa bản ghi này");
                }
                //Xóa tài sản cũ
                var TaiSanIds = _phieuXuatKhoToTaiSanRepository.GetAll().Where(x => x.PhieuXuatKhoId == id).Select(x => x.Id).ToList();
                if (TaiSanIds.Count > 0)
                    await _phieuXuatKhoToTaiSanRepository.DeleteAsync(x => TaiSanIds.Contains(x.Id));
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
                return "PXK" + incrementedNumber;
            }
            else
            {
                code = "PXK0000000001";
            }
            return code;
        }
    }
}
