using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.App.ServiceHttpClient;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace Yootek.Services
{
    public interface IParkingAppService
    {
        Task<object> CreateParkingAsync(CreateParkingDto input);
        Task<object> GetListParkingAsync(GetListParkingDto input);
        Task<object> GetParkingByIdAsync(long id);
        Task<object> UpdateParkingAsync(UpdateParkingDto input);
        Task<object> DeleteParkingAsync(long id);
    }

    public interface IParkingExcelExporter
    {
        FileDto ExportToExcel(List<Parking> input);
    }

    public class ParkingExcelExporter : NpoiExcelExporterBase, IParkingExcelExporter
    {
        public ParkingExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }
        public FileDto ExportToExcel(List<Parking> input)
        {
            return CreateExcelPackage("parkingPositions.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Feedback");
                    AddHeader(sheet,
                        L("NameParkingPosition"),
                        L("VehicleType"),
                        L("ParkingPosition"),
                        L("Description"),
                        L("CreationTime"),
                        L("QRCode"));
                    AddObjects(sheet, input,
                        _ => _.Name,
                        _ => VehicleType((int)_.VehicleType),
                        _ => _.Position,
                        _ => _.Description,
                        _ => _.CreationTime);
                    for (var i = 1; i <= input.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[4], "dd/mm/yyyy");
                        sheet.GetRow(i).CreateCell(5);
                        //sheet.GetRow(i).Height = 10;
                        //if (input[i - 1].QrCode != null || string.IsNullOrWhiteSpace(input[i - 1].QrCode))
                        //{
                        //    string qrData = "yoolife://app/add_parking/" + input[i - 1].QrCode + "/" + (int)QRCodeActionType.CarParking + "/{\"parkingId\":" + input[i - 1].Id + "}";
                        //    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        //    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                        //    QRCode qrCode = new QRCode(qrCodeData);
                        //    var qrCodeImage = qrCode.GetGraphic(20);

                        //    ImageConverter converter = new ImageConverter();
                        //    byte[] file = ;

                        //    var pictureindex = excelPackage.AddPicture(file, PictureType.PNG);
                        //    IDrawing drawing = sheet.CreateDrawingPatriarch();
                        //    IClientAnchor anchor = excelPackage.GetCreationHelper().CreateClientAnchor();
                        //    anchor.Col1 = 5;
                        //    anchor.Row1 = i;
                        //    anchor.AnchorType = AnchorType.MoveAndResize;
                        //    IPicture picture = drawing.CreatePicture(anchor, pictureindex);
                        //    picture.Resize(1, 1);
                        //}
                    }
                    for (var i = 0; i < 5; i++)
                    {
                        if (i.IsIn(3)) continue;
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
        protected string VehicleType(int input)
        {
            switch (input)
            {
                case 1: return L("Car");
                case 2: return L("Motorbike");
                case 3: return L("Bicycle");
                default: return L("Other");
            }
        }

        protected static byte[] ToByteArray(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }

    public class ParkingAppService : YootekAppServiceBase, IParkingAppService
    {
        private readonly IRepository<Parking, long> _parkingRepository;
        private readonly IRepository<UserParking, long> _userParkingRepository;
        private readonly IHttpQRCodeService _httpQRCodeService;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepository;
        private readonly IParkingExcelExporter _parkingExcelExporter;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepos;

        public ParkingAppService(IRepository<Parking, long> parkingRepository, IHttpQRCodeService httpQRCodeService,
            IRepository<UserParking, long> userParkingRepository,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepository,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepos,
            IParkingExcelExporter parkingExcelExporter)
        {
            _parkingRepository = parkingRepository;
            _httpQRCodeService = httpQRCodeService;
            _userParkingRepository = userParkingRepository;
            _userOrganizationUnitRepository = userOrganizationUnitRepository;
            _parkingExcelExporter = parkingExcelExporter;
            _appOrganizationUnitRepos = appOrganizationUnitRepos;
        }

        public async Task<object> CreateParkingAsync(CreateParkingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var parking = input.MapTo<Parking>();

                parking.TenantId = AbpSession.TenantId;

                var data = await _parkingRepository.InsertAsync(parking);
                await CurrentUnitOfWork.SaveChangesAsync();
                data.QrCode = QRCodeGen(data.Id, QRCodeActionType.CarParking);

                var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                {
                    Name = $"QR/Parking/{AbpSession.TenantId}/{data.Position}",
                    ActionType = QRCodeActionType.CarParking,
                    Code = data.QrCode,
                    Data = JsonConvert.SerializeObject(new
                    {
                        parkingId = data.Id,
                    }),
                    Status = QRCodeStatus.Active,
                    Type = QRCodeType.Text
                });

                if (!createQRCodeResult.Success)
                {
                    throw new UserFriendlyException("Qrcode create fail !");
                }


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

        public async Task<object> GetListParkingAsync(GetListParkingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var query = _parkingRepository.GetAll()
                    .WhereIf(input.VehicleType.HasValue, x => x.VehicleType == input.VehicleType)
                    .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                    .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                    .WhereIf(input.CitizenParkingId.HasValue, x => x.CitizenParkingId == input.CitizenParkingId)
                    .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Position)
                    .AsQueryable();
                var totalCount = await query.CountAsync();
                var data = await query.ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByListParking.POSITION).PageBy(input).ToListAsync();
                var result = data.MapTo<List<ParkingDto>>();
                if (result.Count > 0)
                {
                    var listCodes = result.Select(x => x.QrCode).ToList();
                    var listQrObjects = await _httpQRCodeService.GetListQRObjectByListCode(new GetListQRObjectByListCodeInput() { ActionType = QRCodeActionType.CarParking, ListCode = listCodes });
                    foreach (var item in result)
                    {
                        try
                        {
                            var QR = listQrObjects.Result[item.QrCode];
                            item.QRAction = $"yoolife://app/add_parking/{item.QrCode}/{(int)QR.ActionType}/{QR.Data}";
                        }
                        catch { }

                    }
                }

                mb.statisticMetris(t1, 0, "ParkingService.GetListParkingAsync");

                return DataResult.ResultSuccess(result, "Get list success", totalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetAllParkingAsync()
        {
            try
            {
                var query = from p in _parkingRepository.GetAll()
                            select new
                            {
                                Label = p.Name,
                                Value = p.Id
                            };
                var data = await query.ToListAsync();
                return DataResult.ResultSuccess(data, "Success", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetParkingByIdAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var data = _parkingRepository.FirstOrDefault(x => x.Id == id);

                if (data == null) return DataResult.ResultCode(null, "Parking not found", 404); ;

                var result = data.MapTo<ParkingDto>();
                var qrcode = await _httpQRCodeService.GetQRObjectByCode(new GetQRObjectByCodeDto() { Code = data.QrCode });
                if (qrcode.Success) result.QrCode = qrcode.Result.Code;
                mb.statisticMetris(t1, 0, "ParkingService.GetParkingByIdAsync");

                return DataResult.ResultSuccess(data, "Get success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateParkingAsync(UpdateParkingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var parking = _parkingRepository.GetAll().FirstOrDefault(x => x.Id == input.Id);
                if (parking == null)
                {
                    return DataResult.ResultCode(null, "Parking not found", 404);
                }

                var updateData = input.MapTo(parking);

                parking = await _parkingRepository.UpdateAsync(updateData);

                mb.statisticMetris(t1, 0, "ParkingService.UpdateParkingAsync");

                return DataResult.ResultSuccess(parking, "Update success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteParkingAsync(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var parking = _parkingRepository.FirstOrDefault(x => x.Id == id);
                if (parking == null)
                {
                    return DataResult.ResultCode(null, "Parking not found", 404);
                }

                await _parkingRepository.DeleteAsync(parking);

                await _httpQRCodeService.DeleteQRObject(new DeleteQRObjectDto() { Code = parking.QrCode });

                mb.statisticMetris(t1, 0, "ParkingService.DeleteParkingAsync");

                return DataResult.ResultSuccess(null, "Delete success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public Task<DataResult> DeleteMultipleParking([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return Task.FromResult(DataResult.ResultError("Err", "input empty"));
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = DeleteParkingAsync(id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

                var data = DataResult.ResultSuccess("Delete success!");
                return Task.FromResult(data);
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return Task.FromResult(data);
            }
        }

        [Obsolete]
        public async Task<object> CreateListParkingAsync(List<Parking> input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var duplicateList = new List<Parking>();
                var insertList = new List<Parking>();
                if (input != null)
                {
                    var index = 0;
                    foreach (var obj in input)
                    {
                        index++;
                        obj.TenantId = AbpSession.TenantId;
                        if (obj.Name != null)
                        {
                            var parking = await _parkingRepository
                                .FirstOrDefaultAsync(x => (x.TenantId == AbpSession.TenantId)
                                && (x.Name.Trim() == obj.Name.Trim()));
                            if (parking == null)
                            {
                                var insertInput = obj.MapTo<Parking>();
                                var dataInsert = await _parkingRepository.InsertAsync(insertInput);
                                await CurrentUnitOfWork.SaveChangesAsync();

                                dataInsert.QrCode = QRCodeGen(dataInsert.Id, QRCodeActionType.CarParking);

                                var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                                {
                                    Name = $"QR/Parking/{AbpSession.TenantId}/{dataInsert.Position}",
                                    ActionType = QRCodeActionType.CarParking,
                                    Code = dataInsert.QrCode,
                                    Data = JsonConvert.SerializeObject(new
                                    {
                                        parkingId = dataInsert.Id,
                                    }),
                                    Status = QRCodeStatus.Active,
                                    Type = QRCodeType.Text
                                });

                                if (!createQRCodeResult.Success)
                                {
                                    throw new UserFriendlyException("Qrcode create fail !");
                                }
                                await CurrentUnitOfWork.SaveChangesAsync();
                                insertList.Add(insertInput.MapTo<Parking>());
                            }
                            else
                            {
                                var parkingDupl = obj.MapTo<Parking>();
                                duplicateList.Add(parkingDupl);
                            }
                        }
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                mb.statisticMetris(t1, 0, "admin_islist_obj");
                var result = new { duplicateList, insertList };
                var data = DataResult.ResultSuccess(result, "Insert success !");
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [Obsolete]
        public async Task<object> ImportParkingFromExcel([FromForm] UploadParkingExcel input)
        {
            try
            {
                var file = input.File;

                var fileName = file.FileName;
                var fileExt = Path.GetExtension(fileName);
                if (fileExt != ".xlsx" && fileExt != ".xls")
                {
                    return DataResult.ResultError("File not support", "Error");
                }

                // file path
                var filePath = Path.GetRandomFileName() + fileExt;
                var stream = File.Create(filePath);
                await file.CopyToAsync(stream);

                var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();

                var rowCount = worksheet.Dimension.End.Row;
                var colCount = worksheet.Dimension.End.Column;

                const int URBAN_CODE_INDEX = 1;
                const int BUILDING_CODE_INDEX = 2;
                const int LOT_NAME_INDEX = 3;
                const int VEHICLE_TYPE_INDEX = 4;
                const int LOCATION_INDEX = 5;
                const int DESCRIPTION_INDEX = 6;

                var insertList = new List<Parking>();
                var duplicateList = new List<Parking>();
                for (var row = 2; row <= rowCount; row++)
                {
                    var parking = new Parking();
                    //parking.ImageUrls = worksheet.Cells[row, IMAGE_URL_INDEX].Value != null ? worksheet.Cells[row, IMAGE_URL_INDEX].Value.ToString() : "";
                    if (worksheet.Cells[row, LOT_NAME_INDEX].Value != null)
                        parking.Name = worksheet.Cells[row, LOT_NAME_INDEX].Value.ToString().Trim();

                    if (worksheet.Cells[row, VEHICLE_TYPE_INDEX].Value != null)
                        parking.VehicleType = getVehicleType(worksheet.Cells[row, VEHICLE_TYPE_INDEX].Value.ToString().Trim());

                    if (worksheet.Cells[row, LOCATION_INDEX].Value != null)
                        parking.Position = worksheet.Cells[row, LOCATION_INDEX].Value.ToString().Trim();

                    if (worksheet.Cells[row, DESCRIPTION_INDEX].Value != null)
                        parking.Description = worksheet.Cells[row, DESCRIPTION_INDEX].Value.ToString().Trim();
                    //parking.QrCode = worksheet.Cells[row, QR_CODE_INDEX].Value.ToString();
                    parking.TenantId = AbpSession.TenantId;

                    if (worksheet.Cells[row, URBAN_CODE_INDEX].Value != null)
                    {
                        var ubIDstr = worksheet.Cells[row, URBAN_CODE_INDEX].Value.ToString().Trim();
                        var ubObj = await _appOrganizationUnitRepos.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == ubIDstr.ToLower());
                        if (ubObj != null) parking.UrbanId = ubObj.Id;
                    }

                    if (worksheet.Cells[row, BUILDING_CODE_INDEX].Value != null)
                    {
                        var buildIDStr = worksheet.Cells[row, BUILDING_CODE_INDEX].Value.ToString().Trim();
                        var buildObj = await _appOrganizationUnitRepos.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == buildIDStr.ToLower());
                        if (buildObj != null) parking.BuildingId = buildObj.Id;
                    }

                    var parkingEntry = await _parkingRepository
                                .FirstOrDefaultAsync(x => (x.TenantId == AbpSession.TenantId)
                                && (x.Name.Trim() == parking.Name.Trim()));
                    if (parkingEntry != null) duplicateList.Add(parking);
                    else insertList.Add(parking);

                }
                await CreateParkingListAsync(insertList);

                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);

                var result = new
                {
                    insertList,
                    duplicateList
                };

                return DataResult.ResultSuccess(result, "Upload success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        private async Task CreateParkingListAsync(List<Parking> lots)
        {
            try
            {
                if (lots.Count == 0 || lots == null) return;
                foreach (var lot in lots)
                {
                    var id = await _parkingRepository.InsertAndGetIdAsync(lot);
                    if (lot.QrCode == null || string.IsNullOrWhiteSpace(lot.QrCode))
                    {
                        lot.QrCode = QRCodeGen(lot.Id, QRCodeActionType.CarParking);

                        var createQRCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                        {
                            Name = $"QR/Parking/{AbpSession.TenantId}/{lot.Position}",
                            ActionType = QRCodeActionType.CarParking,
                            Code = lot.QrCode,
                            Data = JsonConvert.SerializeObject(new
                            {
                                parkingId = lot.Id,
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
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private ParkingVehicleType getVehicleType(string input)
        {
            if (input.Contains(L("Car"))) return ParkingVehicleType.Car;
            if (input.Contains(L("Motorbike"))) return ParkingVehicleType.Motorbike;
            if (input.Contains(L("Bicycle"))) return ParkingVehicleType.Bicycle;
            else return ParkingVehicleType.Other;
        }
        public async Task<object> ExportParkingToExcel(ParkingExcelExportInput input)
        {
            try
            {
                var query = await _parkingRepository.GetAll()
                    .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id)).ToListAsync();
                var result = _parkingExcelExporter.ExportToExcel(query);
                return DataResult.ResultSuccess(result, "Export Success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}