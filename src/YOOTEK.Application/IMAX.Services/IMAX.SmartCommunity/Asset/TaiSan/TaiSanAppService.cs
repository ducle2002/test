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
using IMAX.Services.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Services
{
    public interface ITaiSanAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllTaiSanInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> GetDSTaiSanNhapXuatById(long id);
        Task<DataResult> Create(TaiSanDto input);
        Task<DataResult> Update(TaiSanDto input);
        Task<DataResult> Delete(long id);
        Task<DataResult> ImportTaiSan([FromForm] ImportTaiSanInput input, CancellationToken cancellationToken);
    }
    [AbpAuthorize]
    public class TaiSanAppService : IMAXAppServiceBase, ITaiSanAppService
    {
        private readonly IRepository<TaiSan, long> _repository;
        private readonly IRepository<NhomTaiSan, long> _nhomTaiSanRepository;
        private readonly IRepository<LoaiTaiSan, long> _loaiTaiSanRepository;
        private readonly IRepository<NhaSanXuat, long> _nhaSanXuatRepository;
        private readonly IRepository<DonViTaiSan, long> _donViTaiSanRepository;
        private readonly IRepository<KhoTaiSan, long> _khoTaiSanRepository;
        private readonly IRepository<PhieuNhapKhoToTaiSan, long> _pnkTaiSanRepository;
        private readonly IRepository<PhieuXuatKhoToTaiSan, long> _pxkTaiSanRepository;
        public TaiSanAppService(IRepository<TaiSan, long> repository,
IRepository<NhomTaiSan, long> nhomTaiSanRepository,
IRepository<LoaiTaiSan, long> loaiTaiSanRepository,
IRepository<NhaSanXuat, long> nhaSanXuatRepository,
IRepository<DonViTaiSan, long> donViTaiSanRepository,
IRepository<KhoTaiSan, long> khoTaiSanRepository,
IRepository<PhieuNhapKhoToTaiSan, long> pnkTaiSanRepository,
IRepository<PhieuXuatKhoToTaiSan, long> pxkTaiSanRepository)
        {
            _repository = repository;
            _nhomTaiSanRepository = nhomTaiSanRepository;
            _loaiTaiSanRepository = loaiTaiSanRepository;
            _nhaSanXuatRepository = nhaSanXuatRepository;
            _donViTaiSanRepository = donViTaiSanRepository;
            _khoTaiSanRepository = khoTaiSanRepository;
            _pnkTaiSanRepository = pnkTaiSanRepository;
            _pxkTaiSanRepository = pxkTaiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllTaiSanInputDto input)
        {
            try
            {
                IQueryable<TaiSanDto> query = (from o in _repository.GetAll()
                                               select new TaiSanDto
                                               {
                                                   Id = o.Id,
                                                   Code = o.Code,
                                                   Title = o.Title,
                                                   Description = o.Description,
                                                   SerialNumber = o.SerialNumber,
                                                   NhomTaiSanId = o.NhomTaiSanId,
                                                   LoaiTaiSanId = o.LoaiTaiSanId,
                                                   NhaSanXuatId = o.NhaSanXuatId,
                                                   DonViTinhId = o.DonViTinhId,
                                                   KhoTaiSanId = o.KhoTaiSanId,
                                                   TongSoLuong = o.TongSoLuong,
                                                   SoLuongTrongKho = o.SoLuongTrongKho,
                                                   SoLuongHong = o.SoLuongHong,
                                                   SoLuongDangSuDung = o.SoLuongDangSuDung,
                                                   GiaNhap = o.GiaNhap,
                                                   GiaXuat = o.GiaXuat,
                                                   AnhDaiDien = o.AnhDaiDien,
                                                   HinhAnh = o.HinhAnh,
                                                   ThoiGianBaoHanh = o.ThoiGianBaoHanh,
                                                   SoLuongCanhBao = o.SoLuongCanhBao,
                                                   TyLeHaoMon = o.TyLeHaoMon,
                                                   TrangThai = o.TrangThai,
                                                   TenantId = o.TenantId,
                                                   NhomTaiSanText = !o.NhomTaiSanId.HasValue ? "" : _nhomTaiSanRepository.GetAll().Where(x => x.Id == o.NhomTaiSanId.Value).Select(x => x.Title).FirstOrDefault(),
                                                   LoaiTaiSanText = !o.LoaiTaiSanId.HasValue ? "" : _loaiTaiSanRepository.GetAll().Where(x => x.Id == o.LoaiTaiSanId.Value).Select(x => x.Title).FirstOrDefault(),
                                                   NhaSanXuatText = !o.NhaSanXuatId.HasValue ? "" : _nhaSanXuatRepository.GetAll().Where(x => x.Id == o.NhaSanXuatId.Value).Select(x => x.Title).FirstOrDefault(),
                                                   DonViTinhText = !o.DonViTinhId.HasValue ? "" : _donViTaiSanRepository.GetAll().Where(x => x.Id == o.DonViTinhId.Value).Select(x => x.Title).FirstOrDefault(),
                                                   KhoTaiSanText = !o.KhoTaiSanId.HasValue ? "" : _khoTaiSanRepository.GetAll().Where(x => x.Id == o.KhoTaiSanId.Value).Select(x => x.Title).FirstOrDefault(),
                                               })
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()) || x.Description.ToLower().Contains(input.Keyword.ToLower()))
    .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.ToLower().Equals(input.Code.ToLower()))

    .WhereIf(input.NhomTaiSanId > 0, x => x.NhomTaiSanId.Value == input.NhomTaiSanId)

    .WhereIf(input.LoaiTaiSanId > 0, x => x.LoaiTaiSanId.Value == input.LoaiTaiSanId)

    .WhereIf(input.NhaSanXuatId > 0, x => x.NhaSanXuatId.Value == input.NhaSanXuatId)

    .WhereIf(input.DonViTinhId > 0, x => x.DonViTinhId.Value == input.DonViTinhId)

    .WhereIf(input.KhoTaiSanId > 0, x => x.KhoTaiSanId.Value == input.KhoTaiSanId)

    .WhereIf(input.TrangThai > 0, x => x.TrangThai == input.TrangThai)
;
                List<TaiSanDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
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
                var data = item.MapTo<TaiSanDto>();
                data.NhomTaiSanText = !data.NhomTaiSanId.HasValue ? "" : _nhomTaiSanRepository.GetAll().Where(x => x.Id == data.NhomTaiSanId.Value).Select(x => x.Title).FirstOrDefault();
                data.NhaSanXuatText = !data.NhaSanXuatId.HasValue ? "" : _nhaSanXuatRepository.GetAll().Where(x => x.Id == data.NhaSanXuatId.Value).Select(x => x.Title).FirstOrDefault();
                data.DonViTinhText = !data.DonViTinhId.HasValue ? "" : _donViTaiSanRepository.GetAll().Where(x => x.Id == data.DonViTinhId.Value).Select(x => x.Title).FirstOrDefault();
                data.KhoTaiSanText = !data.KhoTaiSanId.HasValue ? "" : _khoTaiSanRepository.GetAll().Where(x => x.Id == data.KhoTaiSanId.Value).Select(x => x.Title).FirstOrDefault();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> GetDSTaiSanNhapXuatById(long id)
        {
            try
            {
                var dsTaiSanNhap = _pnkTaiSanRepository.GetAll().Where(x => (x.TaiSanId == id && x.TrangThai.HasValue && x.TrangThai.Value == (int)AssetStatus.DaDuyet)).ToList();
                var dsTaiSanXuat = _pxkTaiSanRepository.GetAll().Where(x => (x.TaiSanId == id && x.TrangThai.HasValue && x.TrangThai.Value == (int)AssetStatus.DaDuyet)).ToList();
                List<TaiSanNhapXuatDto> dsTaiSan = new List<TaiSanNhapXuatDto>();
                foreach (var ts in dsTaiSanNhap)
                {
                    dsTaiSan.Add(new TaiSanNhapXuatDto()
                    {
                        Id = ts.Id,
                        SoLuong = ts.SoLuong,
                        DonGia = ts.DonGia,
                        DonViTinh = _donViTaiSanRepository.GetAll().Where(x => x.Id == ts.DonViTinhId).Select(x => x.Title).FirstOrDefault(),
                        ThanhTien = ts.ThanhTien,
                        TongSoLuong = ts.TongSoLuong,
                        Type = 1,
                        Created = ts.CreationTime
                    });
                }
                foreach (var ts in dsTaiSanXuat)
                {
                    dsTaiSan.Add(new TaiSanNhapXuatDto()
                    {
                        Id = ts.Id,
                        SoLuong = ts.SoLuong,
                        DonGia = ts.DonGia,
                        DonViTinh = _donViTaiSanRepository.GetAll().Where(x => x.Id == ts.DonViTinhId).Select(x => x.Title).FirstOrDefault(),
                        ThanhTien = ts.ThanhTien,
                        TongSoLuong = ts.TongSoLuong,
                        Type = 2,
                        Created = ts.CreationTime
                    });
                }
                dsTaiSan = dsTaiSan.OrderBy(x => x.Created).ToList();
                return DataResult.ResultSuccess(dsTaiSan, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<DataResult> Create(TaiSanDto dto)
        {
            try
            {
                var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                if (result != null)
                {
                    throw new UserFriendlyException("Mã tài sản: " + dto.Code + " đã tồn tại");
                }

                TaiSan item = dto.MapTo<TaiSan>();
                item.TenantId = AbpSession.TenantId;
                var nhomTaiSan = _nhomTaiSanRepository.GetAll().Where(x => x.Id == dto.NhomTaiSanId).FirstOrDefault();
                if (nhomTaiSan != null)
                    item.MaHeThongId = nhomTaiSan.MaHeThongId;
                await _repository.InsertAsync(item);
                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
        public async Task<DataResult> Update(TaiSanDto dto)
        {
            try
            {
                TaiSan item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    if (item.Code != dto.Code)
                    {
                        var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                        if (result != null)
                        {
                            throw new UserFriendlyException("Mã tài sản: " + dto.Code + " đã tồn tại");
                        }
                    }
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    var nhomTaiSan = _nhomTaiSanRepository.GetAll().Where(x => x.Id == dto.NhomTaiSanId).FirstOrDefault();
                    if (nhomTaiSan != null)
                        item.MaHeThongId = nhomTaiSan.MaHeThongId;
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

        public async Task<DataResult> ImportTaiSan([FromForm] ImportTaiSanInput input, CancellationToken cancellationToken)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    IFormFile file = input.File;
                    string fileName = file.FileName;
                    string fileExt = Path.GetExtension(fileName);
                    if (!IsFileExtensionSupported(fileExt))
                    {
                        return DataResult.ResultError("File not support", "Error");
                    }
                    string filePath = Path.GetRandomFileName() + fileExt;
                    FileStream stream = File.Create(filePath);
                    await file.CopyToAsync(stream, cancellationToken);
                    ExcelPackage package = new(stream);
                    List<TaiSan> listImports = new();
                    try
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                        int rowCount = worksheet.Dimension.End.Row;
                        int colCount = worksheet.Dimension.End.Column;

                        // Format columns
                        const int NAME_INDEX = 1;
                        const int APARTMENT_CODE_INDEX = 2;
                        const int IMAGE_URL_INDEX = 3;
                        const int DESCRIPTION_INDEX = 4;
                        const int BUILDING_INDEX = 5;
                        const int URBAN_INDEX = 6;
                        const int BLOCK_INDEX = 7;
                        const int FLOOR_INDEX = 8;
                        const int APARTMENT_AREA_INDEX = 9;
                        const int STATUS_INDEX = 10;
                        const int CLASSIFY_APARTMENT_INDEX = 11;
                        const int PROVINCE_INDEX = 12;
                        const int DISTRICT_INDEX = 13;
                        const int WARD_INDEX = 14;
                        const int ADDRESS_INDEX = 15;

                        //for (int row = 2; row <= rowCount; row++)
                        //{
                        //    string name = GetCellValueNotDefault<string>(worksheet, row, NAME_INDEX);
                        //    string apartmentCode = GetCellValueNotDefault<string>(worksheet, row, APARTMENT_CODE_INDEX);
                        //    string imageUrl = GetCellValue<string>(worksheet, row, IMAGE_URL_INDEX);
                        //    string description = GetCellValue<string>(worksheet, row, DESCRIPTION_INDEX);
                        //    string buildingCode = GetCellValue<string>(worksheet, row, BUILDING_INDEX);
                        //    string urbanCode = GetCellValue<string>(worksheet, row, URBAN_INDEX);
                        //    decimal apartmentArea = GetCellValue<decimal>(worksheet, row, APARTMENT_AREA_INDEX);
                        //    string statusCode = GetCellValue<string>(worksheet, row, STATUS_INDEX);
                        //    string classifyApartmentCode = GetCellValue<string>(worksheet, row, CLASSIFY_APARTMENT_INDEX);
                        //    string blockCode = GetCellValue<string>(worksheet, row, BLOCK_INDEX);
                        //    string floorName = GetCellValue<string>(worksheet, row, FLOOR_INDEX);
                        //    string provinceCode = GetCellValue<string>(worksheet, row, PROVINCE_INDEX);
                        //    string districtCode = GetCellValue<string>(worksheet, row, DISTRICT_INDEX);
                        //    string wardCode = GetCellValue<string>(worksheet, row, WARD_INDEX);
                        //    string address = GetCellValue<string>(worksheet, row, ADDRESS_INDEX);

                        //    TaiSan? oItem = await _repository.FirstOrDefaultAsync(x => x.Code == apartmentCode);
                        //    if (oItem != null) throw new UserFriendlyException($"Error at row {row}: ApartmentCode is existed.");

                        //    listImports.Add(new Apartment()
                        //    {
                        //        TenantId = AbpSession.TenantId,
                        //        Name = name,
                        //        ApartmentCode = apartmentCode,
                        //        ImageUrl = imageUrl,
                        //        Description = description,
                        //        BuildingId = _organizationUnit.FirstOrDefault(x => x.Code == buildingCode)?.Id ?? 0,
                        //        UrbanId = _organizationUnit.FirstOrDefault(x => x.Code == urbanCode)?.Id ?? 0,
                        //        FloorId = _floorRepository.FirstOrDefault(x => x.DisplayName == floorName)?.Id ?? 0,
                        //        BlockId = _blockRepository.FirstOrDefault(x => x.Code == blockCode)?.Id ?? 0,
                        //        Area = apartmentArea,
                        //        StatusId = _statusRepository.FirstOrDefault(x => x.Code == statusCode)?.Id ?? 0,
                        //        TypeId = _typeRepository.FirstOrDefault(x => x.Code == classifyApartmentCode)?.Id ?? 0,
                        //        Address = address,
                        //        ProvinceCode = provinceCode,
                        //        DistrictCode = districtCode,
                        //        WardCode = wardCode,
                        //    });
                        //}

                        await stream.DisposeAsync();
                        stream.Close();
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        await stream.DisposeAsync();
                        stream.Close();
                        File.Delete(filePath);
                        Logger.Fatal(ex.Message);
                        throw new UserFriendlyException(ex.Message);
                    }

                    listImports.ForEach(oItem =>
                    {
                        _repository.Insert(oItem);
                    });
                    return DataResult.ResultSuccess(true, "Upload apartment success");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
    }
}
