using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Core.Dto;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Services.ExportData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Yootek.Application.QueryMiddleware;
using Newtonsoft.Json;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;
using Yootek.App.ServiceHttpClient;

namespace Yootek.Services
{
    public interface ITaiSanChiTietAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllTaiSanChiTietInputDto input);
        Task<DataResult> UpdateQRCodeAllAsync();
        Task<DataResult> UpdateQRCodeAsync(long id);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(TaiSanChiTietDto input);
        Task<DataResult> Update(TaiSanChiTietDto input);
        Task<DataResult> Delete(long id);
        Task<DataResult> GetEnums(string type);
        Task<DataResult> ImportExcel([FromForm] ImportTaiSanChiTietInput input, CancellationToken cancellationToken);
        DataResult ExportExcel([FromBody] GetAllTaiSanChiTietInputExcelDto input);
    }
    [AbpAuthorize]
    public class TaiSanChiTietAppService : YootekAppServiceBase, ITaiSanChiTietAppService
    {
        private readonly IRepository<TaiSanChiTiet, long> _repository;
        private readonly IRepository<NhomTaiSan, long> _nhomTaiSanRepository;
        private readonly IRepository<BlockTower, long> _blockTowersRepository;
        private readonly IRepository<Apartment, long> _apartmentsRepository;
        private readonly IRepository<AppOrganizationUnit, long> _abpOrganizationUnitsRepository;
        private readonly IRepository<EntityDb.Floor, long> _floorsRepository;
        private readonly IRepository<MaHeThong, long> _maHeThongRepository;
        private readonly IRepository<TaiSan, long> _taiSanRepository;
        private readonly IRepository<PhieuXuatKho, long> _phieuXuatKhoRepository;
        private readonly IRepository<PhieuXuatKhoToTaiSan, long> _phieuXuatKhoToTaiSanRepository;
        private readonly ITaiSanChiTietExcelExporter _repositoryExcelExporter;
        private readonly IHttpQRCodeService _httpQRCodeService;


        public TaiSanChiTietAppService(IRepository<TaiSanChiTiet, long> repository,
            IRepository<NhomTaiSan, long> nhomTaiSanRepository,
            IRepository<BlockTower, long> blockTowersRepository,
            IRepository<Apartment, long> apartmentsRepository,
            IRepository<AppOrganizationUnit, long> abpOrganizationUnitsRepository,
            IRepository<EntityDb.Floor, long> floorsRepository,
            ITaiSanChiTietExcelExporter repositoryExcelExporter,
            IRepository<MaHeThong, long> maHeThongRepository,
            IRepository<TaiSan, long> taiSanRepository,
            IRepository<PhieuXuatKho, long> phieuXuatKhoRepository,
            IRepository<PhieuXuatKhoToTaiSan, long> phieuXuatKhoToTaiSanRepository,
        IHttpQRCodeService httpQRCodeService)
        {
            _repository = repository;
            _nhomTaiSanRepository = nhomTaiSanRepository;
            _blockTowersRepository = blockTowersRepository;
            _apartmentsRepository = apartmentsRepository;
            _abpOrganizationUnitsRepository = abpOrganizationUnitsRepository;
            _floorsRepository = floorsRepository;
            _maHeThongRepository = maHeThongRepository;
            _repositoryExcelExporter = repositoryExcelExporter;
            _httpQRCodeService = httpQRCodeService;
            _taiSanRepository = taiSanRepository;
            _phieuXuatKhoRepository = phieuXuatKhoRepository;
            _phieuXuatKhoToTaiSanRepository = phieuXuatKhoToTaiSanRepository;
        }
        public async Task<DataResult> GetAllAsync(GetAllTaiSanChiTietInputDto input)
        {
            try
            {
                var query = _repository.GetAll()
                    .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()))
                    .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.ToLower().Equals(input.Code.ToLower()))
                    .WhereIf(input.HinhThuc > 0, x => x.HinhThuc == input.HinhThuc)
                    .WhereIf(input.NhomTaiSanId > 0, x => x.NhomTaiSanId == input.NhomTaiSanId)
                    .WhereIf(input.TrangThai > 0, x => x.TrangThai == input.TrangThai)
                    .WhereIf(input.ApartmentId > 0, x => x.ApartmentId == input.ApartmentId)
                    .WhereIf(input.MaHeThongId > 0, x => x.MaHeThongId == input.MaHeThongId)
                    .AsQueryable();
                var totalCount = await query.CountAsync();
                var data = await query.ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(FieldSortTaiSanChiTiet.CODE).PageBy(input).ToListAsync();
                var result = data.MapTo<List<TaiSanChiTietDto>>();
                if (result.Count > 0)
                {
                    var listCodes = result.Select(x => x.QrCode).ToList();
                    var listQrObjects = await _httpQRCodeService.GetListQRObjectByListCode(new GetListQRObjectByListCodeInput() { ActionType = QRCodeActionType.Asset, ListCode = listCodes });
                    foreach (var item in result)
                    {
                        try
                        {
                            var QR = listQrObjects.Result[item.QrCode];
                            //item.QRAction = $"yoolife://app/add_asset/{item.QrCode}/{(int)QR.ActionType}/{QR.Data}";
                            item.QRAction = $"yooioc://asset/detail?id={item.Id}&tenantId={AbpSession.TenantId}";
                        }
                        catch { }
                        item.NhomTaiSanText = _nhomTaiSanRepository.GetAll().Where(x => x.Id == item.NhomTaiSanId).Select(x => x.Title).FirstOrDefault();
                        item.MaHeThongText = _maHeThongRepository.GetAll().Where(x => x.Id == item.MaHeThongId).Select(x => x.Code).FirstOrDefault();
                        item.NhomTaiSanText = _nhomTaiSanRepository.GetAll().Where(x => x.Id == item.NhomTaiSanId).Select(x => x.Title).FirstOrDefault();
                        item.BlockText = !item.BlockId.HasValue ? "" : _blockTowersRepository.GetAll().Where(x => x.Id == item.BlockId.Value).Select(x => x.DisplayName).FirstOrDefault();
                        item.ApartmentText = !item.ApartmentId.HasValue ? "" : _apartmentsRepository.GetAll().Where(x => x.Id == item.ApartmentId.Value).Select(x => x.ApartmentCode).FirstOrDefault();
                        item.BuildingText = !item.BuildingId.HasValue ? "" : _abpOrganizationUnitsRepository.GetAll().Where(x => x.Id == item.BuildingId.Value).Select(x => x.DisplayName).FirstOrDefault();
                        item.FloorText = !item.FloorId.HasValue ? "" : _floorsRepository.GetAll().Where(x => x.Id == item.FloorId.Value).Select(x => x.DisplayName).FirstOrDefault();
                    }
                }
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> UpdateQRCodeAllAsync()
        {
            try
            {
                var query = _repository.GetAll().AsQueryable();
                var totalCount = await query.CountAsync();
                var data = await query.ToListAsync();
                if (data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        data[i].QrCode = QRCodeGen(data[i].Id, QRCodeActionType.Asset);
                        var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                        {
                            Name = $"QR/Asset/{AbpSession.TenantId}/{data[i].Code}",
                            ActionType = QRCodeActionType.Asset,
                            Code = data[i].QrCode,
                            Data = JsonConvert.SerializeObject(new
                            {
                                assetId = data[i].Id,
                            }),
                            Status = QRCodeStatus.Active,
                            Type = QRCodeType.Text
                        });

                        if (!createQRCodeResult.Success)
                        {
                            throw new UserFriendlyException("Qrcode create fail !");
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateQRCodeAsync(long id)
        {
            try
            {
                var item = await _repository.GetAsync(id);
                if (item is null)
                {
                    throw new UserFriendlyException("Qrcode create fail !");
                }
                item.QrCode = QRCodeGen(item.Id, QRCodeActionType.Asset);
                var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                {
                    Name = $"QR/Asset/{AbpSession.TenantId}/{item.Code}",
                    ActionType = QRCodeActionType.Asset,
                    Code = item.QrCode,
                    Data = JsonConvert.SerializeObject(new
                    {
                        assetId = item.Id,
                    }),
                    Status = QRCodeStatus.Active,
                    Type = QRCodeType.Text
                });

                if (!createQRCodeResult.Success)
                {
                    throw new UserFriendlyException("Qrcode create fail !");
                }
                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
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
                var data = item.MapTo<TaiSanChiTietDto>();
                data.MaHeThongText = _maHeThongRepository.GetAll().Where(x => x.Id == data.MaHeThongId).Select(x => x.Code).FirstOrDefault();
                data.NhomTaiSanText = _nhomTaiSanRepository.GetAll().Where(x => x.Id == data.NhomTaiSanId).Select(x => x.Title).FirstOrDefault();
                data.BlockText = !data.BlockId.HasValue ? "" : _blockTowersRepository.GetAll().Where(x => x.Id == data.BlockId.Value).Select(x => x.DisplayName).FirstOrDefault();
                data.ApartmentText = !data.ApartmentId.HasValue ? "" : _apartmentsRepository.GetAll().Where(x => x.Id == data.ApartmentId.Value).Select(x => x.ApartmentCode).FirstOrDefault();
                data.BuildingText = !data.BuildingId.HasValue ? "" : _abpOrganizationUnitsRepository.GetAll().Where(x => x.Id == data.BuildingId.Value).Select(x => x.DisplayName).FirstOrDefault();
                data.FloorText = !data.FloorId.HasValue ? "" : _floorsRepository.GetAll().Where(x => x.Id == data.FloorId.Value).Select(x => x.DisplayName).FirstOrDefault();
                if (!string.IsNullOrEmpty(data.QrCode))
                {
                    data.QRAction = $"yooioc://asset/detail?id={data.Id}&tenantId={AbpSession.TenantId}";
                    var qrcode = await _httpQRCodeService.GetQRObjectByCode(new GetQRObjectByCodeDto() { Code = data.QrCode });
                    if (qrcode.Success) data.QrCode = qrcode.Result.Code;

                }

                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Create(TaiSanChiTietDto dto)
        {
            try
            {
                var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                if (result != null)
                {
                    throw new UserFriendlyException("Mã tài sản: " + dto.Code + " đã tồn tại");
                }
                TaiSanChiTiet item = dto.MapTo<TaiSanChiTiet>();
                item.TenantId = AbpSession.TenantId;
                var data = await _repository.InsertAsync(item);
                data.QrCode = QRCodeGen(data.Id, QRCodeActionType.Asset);
                var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                {
                    Name = $"QR/Asset/{AbpSession.TenantId}/{data.Code}",
                    ActionType = QRCodeActionType.Asset,
                    Code = data.QrCode,
                    Data = JsonConvert.SerializeObject(new
                    {
                        assetId = data.Id,
                    }),
                    Status = QRCodeStatus.Active,
                    Type = QRCodeType.Text
                });

                if (!createQRCodeResult.Success)
                {
                    throw new UserFriendlyException("Qrcode create fail !");
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                var taiSanItem = _taiSanRepository.GetAll().Where(x => x.Code == item.Code).FirstOrDefault();
                if (taiSanItem == null)
                {
                    TaiSan oTaiSan = new TaiSan();
                    oTaiSan.Code = item.Code;
                    oTaiSan.Title = item.Title;
                    oTaiSan.NhomTaiSanId = item.NhomTaiSanId;
                    oTaiSan.MaHeThongId = item.MaHeThongId;
                    oTaiSan.GiaXuat = item.GiaTriTaiSan;
                    oTaiSan.GiaNhap = item.GiaTriTaiSan;
                    oTaiSan.SoLuongHong = 0;
                    if (item.NgayBatDau.HasValue && item.NgayKetThuc.HasValue)
                        oTaiSan.ThoiGianBaoHanh = item.NgayKetThuc.Value.Day - item.NgayBatDau.Value.Day;
                    else
                        oTaiSan.ThoiGianBaoHanh = 0;
                    oTaiSan.SoLuongDangSuDung = 0;
                    oTaiSan.SoLuongTrongKho = 0;
                    oTaiSan.TongSoLuong = 0;
                    oTaiSan.TenantId = AbpSession.TenantId;
                    oTaiSan.TrangThai = 0;
                    await _taiSanRepository.InsertAsync(oTaiSan);

                }
                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Update(TaiSanChiTietDto dto)
        {
            try
            {
                TaiSanChiTiet item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    if (item.Code != dto.Code)
                    {
                        var result = _repository.GetAll().Where(x => x.Code == dto.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                        if (result != null)
                        {
                            throw new UserFriendlyException("Mã nhóm tài sản: " + dto.Code + " đã tồn tại");
                        }
                    }
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    await _repository.UpdateAsync(item);
                    var taiSanItem = _taiSanRepository.GetAll().Where(x => x.Code == item.Code).FirstOrDefault();
                    if (taiSanItem == null)
                    {
                        TaiSan oTaiSan = new TaiSan();
                        oTaiSan.Code = item.Code;
                        oTaiSan.Title = item.Title;
                        oTaiSan.NhomTaiSanId = item.NhomTaiSanId;
                        oTaiSan.MaHeThongId = item.MaHeThongId;
                        oTaiSan.GiaXuat = item.GiaTriTaiSan;
                        oTaiSan.GiaNhap = item.GiaTriTaiSan;
                        oTaiSan.SoLuongHong = 0;
                        if (item.NgayBatDau.HasValue && item.NgayKetThuc.HasValue)
                            oTaiSan.ThoiGianBaoHanh = item.NgayKetThuc.Value.Day - item.NgayBatDau.Value.Day;
                        else
                            oTaiSan.ThoiGianBaoHanh = 0;
                        oTaiSan.SoLuongDangSuDung = 0;
                        oTaiSan.SoLuongTrongKho = 0;
                        oTaiSan.TongSoLuong = 0;
                        oTaiSan.TenantId = AbpSession.TenantId;
                        oTaiSan.TrangThai = 0;
                        await _taiSanRepository.InsertAsync(oTaiSan);

                    }
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
        public DataResult ExportExcel([FromBody] GetAllTaiSanChiTietInputExcelDto input)
        {
            try
            {

                List<TaiSanChiTietExportDto> items = (from o in _repository.GetAll()
                                                      select new TaiSanChiTietExportDto
                                                      {
                                                          Id = o.Id,
                                                          Code = o.Code,
                                                          Title = o.Title,
                                                          HinhThuc = o.HinhThuc,
                                                          TrangThai = o.TrangThai,
                                                          NhomTaiSanId = o.NhomTaiSanId,

                                                          MaHeThongText = _maHeThongRepository.GetAll().Where(x => x.Id == o.MaHeThongId).Select(x => x.Code).FirstOrDefault(),
                                                          BlockText = _blockTowersRepository.GetAll().Where(x => x.Id == o.BlockId).Select(x => x.DisplayName).FirstOrDefault(),
                                                          ApartmentCode = _apartmentsRepository.GetAll().Where(x => x.Id == o.ApartmentId).Select(x => x.ApartmentCode).FirstOrDefault(),
                                                          BuildingText = _abpOrganizationUnitsRepository.GetAll().Where(x => x.Id == o.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                                                          FloorText = _floorsRepository.GetAll().Where(x => x.Id == o.FloorId).Select(x => x.DisplayName).FirstOrDefault(),
                                                          NgayKetThuc = o.NgayKetThuc,
                                                          NgayBatDau = o.NgayBatDau,
                                                          TenantId = o.TenantId,
                                                          MaSoBaoHanh = o.MaSoBaoHanh,
                                                          GiaTriTaiSan = o.GiaTriTaiSan,
                                                          GhiChu = o.GhiChu,
                                                          SoLuong = o.SoLuong,
                                                          NhomTaiSanText = _nhomTaiSanRepository.GetAll().Where(x => x.Id == o.NhomTaiSanId).Select(x => x.Title).FirstOrDefault()
                                                      })
                .WhereIf(!string.IsNullOrEmpty(input.Keyword), x =>
x.Code.ToLower().Contains(input.Keyword.ToLower()) || x.Title.ToLower().Contains(input.Keyword.ToLower()))
.WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code.ToLower().Equals(input.Code.ToLower()))

.WhereIf(input.HinhThuc > 0, x => x.HinhThuc == input.HinhThuc)

.WhereIf(input.NhomTaiSanId > 0, x => x.NhomTaiSanId == input.NhomTaiSanId)

.WhereIf(input.TrangThai > 0, x => x.TrangThai == input.TrangThai).ToList(); ;


                FileDto file = _repositoryExcelExporter.ExportToFile(items);
                return DataResult.ResultSuccess(file, "Export excel success!");

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<DataResult> ImportExcel([FromForm] ImportTaiSanChiTietInput input, CancellationToken cancellationToken)
        {
            try
            {

                IFormFile file = input.File;
                bool replace = input.Replace;
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
                List<TaiSanChiTiet> listImports = new();
                int totalSuccess = 0;
                int totalReplace = 0;
                List<string> Errors = new();
                try
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.End.Row;
                    int colCount = worksheet.Dimension.End.Column;
                    string[] formatDate = { "dd/MM/yyyy" }; // Định dạng "dd/MM/yyyy"
                    for (int row = 2; row <= rowCount; row++)
                    {

                        int colum = 1;
                        bool checkPass = true;
                        #region Check dữ liệu
                        // Kiểm tra mã tài sản
                        var rsCode = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum, true)); colum++;
                        if (rsCode != null && !string.IsNullOrEmpty(rsCode.Value))
                        {
                            // Kiểm tra tên tài sản
                            var rsTitle = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum, true)); colum++;
                            // Kiểm tra hình thức
                            int HinhThuc = 0;
                            var rsHinhThuc = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum, true));
                            if (!rsHinhThuc.Error)
                            {
                                if (!Enum.IsDefined(typeof(HinhThucTaiSanChiTietEnum), rsHinhThuc.Value))
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is invalid.");
                                }
                                else
                                {
                                    if (Enum.TryParse(typeof(HinhThucTaiSanChiTietEnum), rsHinhThuc.Value, out object _result) && Enum.IsDefined(typeof(HinhThucTaiSanChiTietEnum), _result))
                                    {
                                        HinhThucTaiSanChiTietEnum enumValue = (HinhThucTaiSanChiTietEnum)_result;
                                        HinhThuc = (int)enumValue;
                                    }
                                    else
                                    {
                                        checkPass = false;
                                        Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is invalid.");
                                    }
                                }

                            }
                            colum++;


                            // Mã hệ thống
                            long MaHeThongId = 0;
                            var rsMaHeThong = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum, true));
                            if (!rsMaHeThong.Error)
                            {
                                var oCategory = _maHeThongRepository.GetAll().Where(x => x.Code == rsMaHeThong.Value.Trim() && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                                if (oCategory == null)
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is not exist.");
                                }
                                else
                                    MaHeThongId = oCategory.Id;
                            }
                            colum++;

                            // Nhóm tài sản
                            long NhomTaiSanId = 0;
                            var rsNhomTaiSan = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum, true));
                            if (!rsNhomTaiSan.Error)
                            {
                                var oCategory = _nhomTaiSanRepository.GetAll().Where(x => x.Code == rsNhomTaiSan.Value.Trim() && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                                if (oCategory == null)
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is not exist.");
                                }
                                else
                                    NhomTaiSanId = oCategory.Id;
                            }
                            colum++;

                            // Kiểm tra trạng thái
                            int TrangThai = 0;
                            var rsTrangThai = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum, true));
                            if (!rsTrangThai.Error)
                            {
                                if (!Enum.IsDefined(typeof(TrangThaiTaiSanChiTietEnum), rsTrangThai.Value))
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is invalid.");
                                }
                                else
                                {
                                    if (Enum.TryParse(typeof(TrangThaiTaiSanChiTietEnum), rsTrangThai.Value, out object _result) && Enum.IsDefined(typeof(TrangThaiTaiSanChiTietEnum), _result))
                                    {
                                        TrangThaiTaiSanChiTietEnum enumValue = (TrangThaiTaiSanChiTietEnum)_result;
                                        TrangThai = (int)enumValue;
                                    }
                                    else
                                    {
                                        checkPass = false;
                                        Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is invalid.");
                                    }
                                }

                            }
                            colum++;


                            // block
                            long? BlockId = null;
                            var rsBlock = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            if (!rsBlock.Error && !string.IsNullOrEmpty(rsBlock.Value))
                            {
                                var oCategory = _blockTowersRepository.GetAll().Where(x => x.Code == rsBlock.Value.Trim() && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                                if (oCategory == null)
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is not exist.");
                                }
                                else
                                    BlockId = oCategory.Id;
                            }
                            colum++;

                            // Căn hộ
                            long? CanHoId = null;
                            var rsCanHo = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            if (!rsCanHo.Error && !string.IsNullOrEmpty(rsCanHo.Value))
                            {
                                var oCategory = _apartmentsRepository.GetAll().Where(x => x.ApartmentCode == rsCanHo.Value.Trim() && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                                if (oCategory == null)
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is not exist.");
                                }
                                else
                                    CanHoId = oCategory.Id;
                            }
                            colum++;

                            // Mã Bảo hành
                            var rsMaBaoHanh = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            colum++;
                            // Giá trị tài sản
                            var rsGiaTriTaiSan = CheckAndAddError<long>(ref checkPass, Errors, GetCellValueExcelCheck<long>(worksheet, row, colum));
                            colum++;
                            // Ngày bắt đầu
                            DateTime? ngayBatDau = null;
                            var rsNgayBatDau = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum, true));
                            if (!rsNgayBatDau.Error)
                            {
                                if (DateTime.TryParseExact(rsNgayBatDau.Value, formatDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _date))
                                {
                                    ngayBatDau = _date;
                                }
                                else
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is invalid.");
                                }
                            }
                            colum++;

                            // Ngày kết thúc
                            DateTime? ngayKetThuc = null;
                            var rsNgayKetThuc = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            if (!rsNgayKetThuc.Error && !string.IsNullOrEmpty(rsNgayKetThuc.Value))
                            {
                                if (DateTime.TryParseExact(rsNgayKetThuc.Value, formatDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _date))
                                {
                                    ngayKetThuc = _date;
                                }
                                else
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is invalid.");
                                }
                            }
                            if (ngayBatDau.HasValue && ngayKetThuc.HasValue && ngayKetThuc.Value < ngayBatDau.Value)
                            {
                                checkPass = false;
                                Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is less {worksheet.Cells[1, colum - 1].Value?.ToString()?.Trim()}.");
                            }
                            colum++;

                            // Ghi chú
                            var rsGhiChu = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            colum++;
                            // Tòa nhà
                            long? ToaNhaId = null;
                            var rsToaNha = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            if (!rsToaNha.Error && !string.IsNullOrEmpty(rsToaNha.Value))
                            {
                                var oCategory = _abpOrganizationUnitsRepository.GetAll().Where(x => x.ProjectCode == rsToaNha.Value.Trim() && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                                if (oCategory == null)
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is not exist.");
                                }
                                else
                                    ToaNhaId = oCategory.Id;
                            }
                            colum++;
                            // Tầng
                            long? TangId = null;
                            var rsTang = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            if (!rsTang.Error && !string.IsNullOrEmpty(rsTang.Value))
                            {
                                var oCategory = _floorsRepository.GetAll().Where(x => x.DisplayName == rsTang.Value.Trim() && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                                if (oCategory == null)
                                {
                                    checkPass = false;
                                    Errors.Add($"Error at row {row}: {worksheet.Cells[1, colum].Value?.ToString()?.Trim()} is not exist.");
                                }
                                else
                                    TangId = oCategory.Id;
                            }
                            colum++;
                            // Số lượng
                            var rsSoLuong = CheckAndAddError<string>(ref checkPass, Errors, GetCellValueExcelCheck<string>(worksheet, row, colum));
                            colum++;


                            // Lấy dữ liệu trường không có
                            var additionalData = new Dictionary<string, string>();

                            for (int col = colum; col <= colCount; col++)
                            {
                                string columnHeader = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(columnHeader))
                                {
                                    string cellValue = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                                    additionalData[columnHeader] = cellValue;
                                }
                            }

                            // Chuyển danh sách dữ liệu thành chuỗi JSON.                            
                            string fieldDynamic = JsonConvert.SerializeObject(additionalData);
                            #endregion
                            if (checkPass)
                            {
                                TaiSanChiTiet itemExist = _repository.GetAll().Where(x => x.Code == rsCode.Value && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                                TaiSanChiTiet item = new TaiSanChiTiet();
                                if (itemExist != null)
                                {
                                    item.Id = itemExist.Id;
                                }
                                item.Code = rsCode.Value;
                                item.Title = rsTitle.Value;
                                item.HinhThuc = HinhThuc;
                                item.Title = rsTitle.Value;
                                item.MaHeThongId = MaHeThongId;
                                item.NhomTaiSanId = NhomTaiSanId;
                                item.TrangThai = TrangThai;
                                item.BlockId = BlockId;
                                item.ApartmentId = CanHoId;
                                item.MaSoBaoHanh = rsMaBaoHanh.Value;
                                item.GiaTriTaiSan = rsGiaTriTaiSan.Value;
                                item.NgayBatDau = ngayBatDau;
                                item.NgayKetThuc = ngayKetThuc;
                                item.GhiChu = rsGhiChu.Value;
                                item.BuildingId = ToaNhaId;
                                item.FloorId = TangId;
                                item.SoLuong = rsSoLuong.Value;
                                item.TenantId = AbpSession.TenantId;
                                item.FieldDynamic = fieldDynamic;
                                listImports.Add(item);
                            }
                        }

                    }
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
                    throw;
                }
                foreach (var oItem in listImports)
                {
                    var taiSanItem = _taiSanRepository.GetAll().Where(x => x.Code == oItem.Code).FirstOrDefault();
                    if (taiSanItem == null)
                    {
                        TaiSan oTaiSan = new TaiSan();
                        oTaiSan.Code = oItem.Code;
                        oTaiSan.Title = oItem.Title;
                        oTaiSan.NhomTaiSanId = oItem.NhomTaiSanId;
                        oTaiSan.MaHeThongId = oItem.MaHeThongId;
                        oTaiSan.GiaXuat = oItem.GiaTriTaiSan;
                        oTaiSan.GiaNhap = oItem.GiaTriTaiSan;
                        oTaiSan.SoLuongHong = 0;
                        if (oItem.NgayBatDau.HasValue && oItem.NgayKetThuc.HasValue)
                            oTaiSan.ThoiGianBaoHanh = oItem.NgayKetThuc.Value.Day - oItem.NgayBatDau.Value.Day;
                        else
                            oTaiSan.ThoiGianBaoHanh = 0;
                        oTaiSan.SoLuongDangSuDung = 0;
                        oTaiSan.SoLuongTrongKho = 0;
                        oTaiSan.TongSoLuong = 0;
                        oTaiSan.TenantId = AbpSession.TenantId;
                        oTaiSan.TrangThai = 0;
                        await _taiSanRepository.InsertAsync(oTaiSan);

                    }

                    if (replace)
                    {
                        if (oItem.Id > 0)
                        {
                            _repository.Update(oItem);
                            totalReplace += 1;
                        }
                        else
                        {
                            var data = _repository.Insert(oItem);
                            data.QrCode = QRCodeGen(data.Id, QRCodeActionType.Asset);
                            var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                            {
                                Name = $"QR/Asset/{AbpSession.TenantId}/{data.Code}",
                                ActionType = QRCodeActionType.Asset,
                                Code = data.QrCode,
                                Data = JsonConvert.SerializeObject(new
                                {
                                    assetId = data.Id,
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

                        totalSuccess += 1;
                    }
                    else
                    {
                        if (oItem.Id > 0)
                        {
                            totalReplace += 1;
                        }
                        else
                        {
                            var data = _repository.Insert(oItem);
                            data.QrCode = QRCodeGen(data.Id, QRCodeActionType.Asset);
                            var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                            {
                                Name = $"QR/Asset/{AbpSession.TenantId}/{data.Code}",
                                ActionType = QRCodeActionType.Asset,
                                Code = data.QrCode,
                                Data = JsonConvert.SerializeObject(new
                                {
                                    assetId = data.Id,
                                }),
                                Status = QRCodeStatus.Active,
                                Type = QRCodeType.Text
                            });

                            if (!createQRCodeResult.Success)
                            {
                                throw new UserFriendlyException("Qrcode create fail !");
                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                            totalSuccess += 1;
                        }
                    }



                }

                ResultImportExcel result = new ResultImportExcel() { Errors = Errors, TotalReplace = totalReplace, TotalSuccess = totalSuccess };
                return DataResult.ResultSuccess(result, "Import success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetEnums(string type)
        {
            try
            {
                List<EnumListItem> listEnum = new List<EnumListItem>();

                switch (type)
                {
                    case "HinhThucTaiSanChiTietEnum":
                        listEnum = GetEnumList<HinhThucTaiSanChiTietEnum>();
                        break;
                    case "TrangThaiTaiSanChiTietEnum":
                        listEnum = GetEnumList<TrangThaiTaiSanChiTietEnum>();
                        break;

                    default:
                        break;
                }
                return DataResult.ResultSuccess(listEnum, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        // Hàm kiểm tra và thêm lỗi
        protected static Result<T> CheckAndAddError<T>(ref bool checkPass, List<string> errors, Result<T> result)
        {
            if (result.Error)
            {
                checkPass = false;
                errors.Add(result.Message);
                return default;
            }
            return result;
        }
    }




}
