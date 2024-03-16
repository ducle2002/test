using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QRCoder;
using Yootek.App.ServiceHttpClient;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;
using Yootek.Application;
using Yootek.Authorization;
using Yootek.Common.DataResult;
using Yootek.Core.Dto;
using Yootek.EntityDb;
using Yootek.Net.MimeTypes;
using Yootek.Organizations;
using Yootek.QueriesExtension;
using Yootek.Storage;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Meter.dto;

namespace Yootek.Services
{
    public interface IAdminMeterAppService : IApplicationService
    {
        Task<DataResult> GetAllMeterAsync(GetAllMeterDto input);
        Task<DataResult> CreateMeter(CreateMeterInput input);
        Task<DataResult> UpdateMeter(UpdateMeterInput input);
        Task<DataResult> DeleteMeter(long id);
        Task<DataResult> DeleteManyMeter([FromBody] List<long> ids);
        Task<DataResult> GetMeterByIdAsync(long id);
        Task<DataResult> CreateMetersList(List<CreateMeterInput> inputs);
        Task<object> ImportCreateMeterExcel([FromForm] ImportCreateMeterInput input);
    }

    public class AdminMeterAppService : YootekAppServiceBase, IAdminMeterAppService
    {
        private readonly IRepository<Meter, long> _meterRepository;
        private readonly IRepository<MeterType, long> _meterTypeRepository;
        private readonly IHttpQRCodeService _httpQRCodeService;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        public AdminMeterAppService(
            IRepository<Meter, long> meterRepository,
            IRepository<MeterType, long> meterTypeRepository,
            IHttpQRCodeService httpQrCodeService,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository, ITempFileCacheManager tempFileCacheManager)
        {
            _meterRepository = meterRepository;
            _meterTypeRepository = meterTypeRepository;
            _httpQRCodeService = httpQrCodeService;
            _organizationUnitRepository = organizationUnitRepository;
            _tempFileCacheManager = tempFileCacheManager;
        }


        public async Task<DataResult> GetAllMeterAsync(GetAllMeterDto input)
        {
            try
            {
                var tenantId = AbpSession.TenantId;
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                IQueryable<MeterDto> query = (from sm in _meterRepository.GetAll()
                                              select new MeterDto
                                              {
                                                  Id = sm.Id,
                                                  TenantId = sm.TenantId,
                                                  Name = sm.Name,
                                                  ApartmentCode = sm.ApartmentCode,
                                                  MeterTypeId = sm.MeterTypeId,
                                                  Code = sm.Code,
                                                  QrCode = sm.QrCode,
                                                  UrbanId = sm.UrbanId,
                                                  BuildingId = sm.BuildingId,
                                                  CreationTime = sm.CreationTime,
                                                  CreatorUserId = sm.CreatorUserId ?? 0,
                                                  BuildingName = _organizationUnitRepository.GetAll().Where(x => x.Id == sm.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                                                  UrbanName = _organizationUnitRepository.GetAll().Where(x => x.Id == sm.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                                              })
                    .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                    .WhereIf(input.MeterTypeId != null, m => m.MeterTypeId == input.MeterTypeId)
                    .WhereIf(input.UrbanId != null, m => m.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId != null, m => m.BuildingId == input.BuildingId)
                    .WhereIf(input.ApartmentCode != null, m => m.ApartmentCode == input.ApartmentCode)
                    .ApplySearchFilter(input.Keyword, x => x.Name, x => x.ApartmentCode);

                List<MeterDto> result = await query
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                if (result.Count > 0)
                {
                    // var listCodes = result.Select(x => x.QrCode).ToList();
                    // var listQrObjects = await _httpQRCodeService.GetListQRObjectByListCode(new GetListQRObjectByListCodeInput() { ActionType = QRCodeActionType.Meter, ListCode = listCodes });
                    foreach (var item in result)
                    {
                        try
                        {
                            // var QR = listQrObjects.Result[item.QrCode];
                            item.QRAction = $"yooioc://app/meter?id={item.Id}&tenantId={AbpSession.TenantId}";
                            // item.QRObject = QR;
                        }
                        catch { }

                    }
                }
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }



        public async Task<DataResult> CreateMeter(CreateMeterInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                Meter? _meter = await _meterRepository.FirstOrDefaultAsync(m =>
                    m.ApartmentCode == input.ApartmentCode && m.MeterTypeId == input.MeterTypeId);

                if (_meter != null)
                {
                    throw new UserFriendlyException(409, "The apartment is equipped with a clock !");
                }

                var meter = ObjectMapper.Map<Meter>(input);

                meter.TenantId = AbpSession.TenantId;

                var data = await _meterRepository.InsertAsync(meter);
                await CurrentUnitOfWork.SaveChangesAsync();
                data.QrCode = QRCodeGenerator(data.Id, QRCodeActionType.Meter);

                await CurrentUnitOfWork.SaveChangesAsync();
                mb.statisticMetris(t1, 0, "ParkingService.CreateParkingAsync");

                return DataResult.ResultSuccess(data, "Insert success");
            }
            catch (UserFriendlyException e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Code, e.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateMetersList(List<CreateMeterInput> inputs)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                foreach (var input in inputs)
                {
                    Meter? _meter = await _meterRepository.FirstOrDefaultAsync(m =>
                        m.ApartmentCode == input.ApartmentCode && m.MeterTypeId == input.MeterTypeId);

                    if (_meter != null)
                    {
                        throw new UserFriendlyException(409, "The apartment is equipped with a clock !");
                    }

                    var meter = ObjectMapper.Map<Meter>(input);

                    meter.TenantId = AbpSession.TenantId;

                    var data = await _meterRepository.InsertAsync(meter);


                    data.QrCode = QRCodeGenerator(data.Id, QRCodeActionType.Meter);

                }

                mb.statisticMetris(t1, 0, "ParkingService.CreateParkingAsync");

                return DataResult.ResultSuccess(null, "Insert success");
            }
            catch (UserFriendlyException e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Code, e.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> ImportCreateMeterExcel([FromForm] ImportCreateMeterInput input)
        {
            try
            {

                IFormFile file = input.File;
                string fileName = file.FileName;
                string fileExt = Path.GetExtension(fileName);
                if (fileExt != ".xlsx" && fileExt != ".xls")
                {
                    return DataResult.ResultError("File not supported", "Error");
                }

                // Generate a unique file path
                string filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + fileExt);

                using (FileStream stream = File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    stream.Close();
                }

                var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets.First();
                int rowCount = worksheet.Dimension.End.Row;

                var listNew = new List<CreateMeterInput>();
                for (var row = 2; row <= rowCount; row++)
                {
                    var meter = new CreateMeterInput();
                    if (!string.IsNullOrEmpty(worksheet.Cells[row, 1].Text.Trim()))
                    {
                        var ubIDstr = worksheet.Cells[row, 1].Text.Trim();
                        var ubObj = await _organizationUnitRepository.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == ubIDstr.ToLower());
                        if (ubObj != null) { meter.UrbanId = ubObj.Id; }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(worksheet.Cells[row, 2].Text.Trim()))
                    {
                        var buildIDStr = worksheet.Cells[row, 2].Text.Trim();
                        var buildObj = await _organizationUnitRepository.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == buildIDStr.ToLower());
                        if (buildObj != null) { meter.BuildingId = buildObj.Id; }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    meter.ApartmentCode = worksheet.Cells[row, 3].Text.Trim();
                    meter.Name = worksheet.Cells[row, 4].Text.Trim();
                    meter.Code = worksheet.Cells[row, 5].Text.Trim();
                    meter.MeterTypeId = input.MeterTypeId;
                    listNew.Add(meter);
                }
                await CreateMetersList(listNew);

                File.Delete(filePath);

                return DataResult.ResultSuccess(listNew, "Upload success");

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<DataResult> UpdateMeter(UpdateMeterInput input)
        {
            try
            {
                Meter? updateData = await _meterRepository.FirstOrDefaultAsync(input.Id)
                                    ?? throw new Exception("Meter not found!");
                Meter meter = ObjectMapper.Map(input, updateData);
                await _meterRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteMeter(long id)
        {
            try
            {
                await _meterRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteManyMeter([FromBody] List<long> ids)
        {
            try
            {
                await _meterRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetMeterByIdAsync(long id)
        {
            try
            {
                IQueryable<MeterDto> query = (from sm in _meterRepository.GetAll()
                                              where sm.Id == id
                                              select new MeterDto
                                              {
                                                  Name = sm.Name,
                                                  Code = sm.Code,
                                                  Id = sm.Id,
                                                  ApartmentCode = sm.ApartmentCode,
                                                  BuildingId = sm.BuildingId,
                                                  UrbanId = sm.UrbanId,
                                                  TenantId = sm.TenantId,
                                                  QRAction = $"yooioc://app/meter?id={sm.Id}&tenantId={AbpSession.TenantId}",
                                                  MeterTypeId = sm.MeterTypeId,
                                                  MeterTypeName = _meterTypeRepository.GetAll().Where(type => type.Id == sm.MeterTypeId).Select(type => type.Name).FirstOrDefault(),
                                                  CreationTime = sm.CreationTime,
                                                  CreatorUserId = sm.CreatorUserId ?? 0,
                                                  BuildingName = _organizationUnitRepository.GetAll().Where(o => o.Id == sm.BuildingId)
                                                      .Select(b => b.DisplayName).FirstOrDefault(),
                                                  UrbanName = _organizationUnitRepository.GetAll().Where(o => o.Id == sm.UrbanId)
                                                      .Select(b => b.DisplayName).FirstOrDefault(),
                                              }
                    ).AsQueryable();

                var result = query.FirstOrDefault();

                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> ExportQRcode([FromBody] GetAllMeterDto input)
        {
            try
            {
                var tenantId = AbpSession.TenantId;
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                IQueryable<MeterDto> query = (from sm in _meterRepository.GetAll()
                                              select new MeterDto
                                              {
                                                  Id = sm.Id,
                                                  TenantId = sm.TenantId,
                                                  Name = sm.Name,
                                                  ApartmentCode = sm.ApartmentCode,
                                                  MeterTypeId = sm.MeterTypeId,
                                                  Code = sm.Code,
                                                  QrCode = sm.QrCode,
                                                  UrbanId = sm.UrbanId,
                                                  BuildingId = sm.BuildingId,
                                                  CreationTime = sm.CreationTime,
                                                  CreatorUserId = sm.CreatorUserId ?? 0,

                                              })
                    .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                    .WhereIf(input.MeterTypeId.HasValue, m => m.MeterTypeId == input.MeterTypeId)
                    .WhereIf((input.UrbanId.HasValue && input.UrbanId.Value>0), m => m.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId != null, m => m.BuildingId == input.BuildingId)
                    .WhereIf(input.ApartmentCode != null, m => m.ApartmentCode == input.ApartmentCode)
                    .ApplySearchFilter(input.Keyword, x => x.Name, x => x.ApartmentCode);

                List<MeterDto> result = await query.ToListAsync();
                string zipFileName = "QRCodeImages.zip";
                string outputDirectory = "QRCodeImages";
                if (File.Exists(zipFileName))
                {
                    // Nếu đã tồn tại, xóa tệp đó
                    File.Delete(zipFileName);
                }
                if (Directory.Exists(outputDirectory))
                {
                    // Nếu đã tồn tại, xóa thư mục đó
                    Directory.Delete(outputDirectory, true);
                }
                string logoPath = "logo.png";
                string fullPath = "";
                if (File.Exists(logoPath))
                    fullPath = Path.GetFullPath(logoPath);

                Directory.CreateDirectory(outputDirectory);
                if (result.Count > 0)
                {

                    foreach (var item in result)
                    {
                        try
                        {
                            item.QRAction = $"yooioc://app/meter?id={item.Id}&tenantId={AbpSession.TenantId}";
                            QRCodeGenerator qr = new QRCodeGenerator();
                            QRCodeData data = qr.CreateQrCode(item.QRAction, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            QRCode code = new QRCode(data);
                            using (Bitmap qrImage = code.GetGraphic(20)) // Điều chỉnh kích thước 20 nếu cần thiết
                            {
                                Bitmap logo = new Bitmap(fullPath); // Đường dẫn đến ảnh bạn muốn chèn
                                int logoSize = 30; // Kích thước của logo (điều chỉnh tùy ý)
                                using (Graphics g = Graphics.FromImage(qrImage))
                                {
                                    g.DrawImage(logo, new Rectangle((qrImage.Width - logoSize) / 2, (qrImage.Height - logoSize) / 2, logoSize, logoSize));
                                    string text = item.Code??item.QrCode; // Chuỗi bạn muốn chèn
                                    Font font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular); // Điều chỉnh font và kích thước tùy ý
                                    SizeF textSize = g.MeasureString(text, font);
                                    int x = (qrImage.Width - (int)textSize.Width) / 2;
                                    int y = qrImage.Height - (int)textSize.Height - 10; // Điều chỉnh vị trí dựa trên kích thước của chữ và vị trí tùy ý
                                    g.DrawString(text, font, Brushes.Black, x, y);
                                }

                                
                                string qrCodeFilePath = Path.Combine(outputDirectory, $"{item.Code??item.QrCode}.png");
                                qrImage.Save(qrCodeFilePath, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                        catch (Exception ex) { }

                    }
                }

                ZipFile.CreateFromDirectory(outputDirectory, zipFileName);
                byte[] fileBytes = System.IO.File.ReadAllBytes(zipFileName);
                var file = new FileDto(zipFileName, MimeTypeNames.ApplicationZip);
                _tempFileCacheManager.SetFile(file.FileToken, fileBytes);
                return DataResult.ResultSuccess(file, "Export excel success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

    }
}