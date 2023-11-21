

using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using System.Threading.Tasks;
using System;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using IMAX.IMAX.Services.IMAX.SmartCommunity.VehicleCitizen;
using System.IO;
using OfficeOpenXml;
using IMAX.Organizations;
using IMAX.Application;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace IMAX.Services
{

    public interface IVehicleCitizenManagementAppService : IApplicationService
    {

    }

    [AbpAuthorize]
    public class VehicleCitizenManagementAppService : IMAXAppServiceBase, IVehicleCitizenManagementAppService
    {

        private readonly IRepository<CitizenVehicle, long> _citizenVehicleRepos;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly ICitizenVehicleExcelExporter _excelExporter;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepos;
        private readonly IRepository<CitizenParking, long> _parkingRepos;

        public VehicleCitizenManagementAppService(
            IRepository<CitizenVehicle, long> citizenVehicleRepos,
            IRepository<CitizenTemp, long> citizenTempRepos,
            ICitizenVehicleExcelExporter excelExporter,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepos,
            IRepository<CitizenParking, long> parkingRepos
            )
        {
            _citizenVehicleRepos = citizenVehicleRepos;
            _citizenTempRepos = citizenTempRepos;
            _excelExporter = excelExporter;
            _appOrganizationUnitRepos = appOrganizationUnitRepos;
            _parkingRepos = parkingRepos;
        }

        public async Task<object> GetAllVehicleByApartmentAsync(GetAllVehicleByApartmentInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from vh in _citizenVehicleRepos.GetAll()
                                 select new CitizenVehicleDto()
                                 {
                                     Id = vh.Id,
                                     ApartmentCode = vh.ApartmentCode,
                                     BuildingId = vh.BuildingId,
                                     CitizenTempId = vh.CitizenTempId,
                                     CreationTime = vh.CreationTime,
                                     CreatorUserId = vh.CreatorUserId,
                                     Description = vh.Description,
                                     UrbanId = vh.UrbanId,
                                     VehicleCode = vh.VehicleCode,
                                     VehicleName = vh.VehicleName,
                                     VehicleType = vh.VehicleType,
                                     TenantId = vh.TenantId,
                                     ParkingId = vh.ParkingId,
                                     OwnerName = vh.OwnerName,
                                     CardNumber = vh.CardNumber,
                                     State = vh.State,
                                     RegistrationDate = vh.RegistrationDate,
                                     ExpirationDate = vh.ExpirationDate
                                 })
                                 .Where(x => x.State == CitizenVehicleState.ACCEPTED)
                                 .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                 .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                 .ApplySearchFilter(input.Keyword, x => x.ApartmentCode, x => x.VehicleCode, x => x.VehicleName)
                                 .AsQueryable();
                    var dirs = query.ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByVehicleByApartment.VEHICLE_CODE).AsEnumerable().GroupBy(x => x.ApartmentCode).Select(x => new
                    {
                        ApartmentCode = x.Key,
                        Vehicles = x.ToList(),
                    }).ToDictionary(x => x.ApartmentCode, y => y.Vehicles);


                    var paginatedData = dirs.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

                    return DataResult.ResultSuccess(paginatedData, "Get success!", dirs.Count());
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetAllVehicleByAparmentCodeAsync(GetAllCitizenVehicleInput input)
        {
            try
            {
                var result = _citizenVehicleRepos.GetAllList(x => x.ApartmentCode == input.ApartmentCode && x.State == CitizenVehicleState.ACCEPTED);
                return DataResult.ResultSuccess(result.MapTo<List<CitizenVehicleByApartmentDto>>(), "Get success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }


        public async Task<object> GetAllVehicleAsync(GetAllCitizenVehicleInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from vh in _citizenVehicleRepos.GetAll()
                                 join cz in _citizenTempRepos.GetAll() on vh.CitizenTempId equals cz.Id into tb_cz
                                 from cz in tb_cz.DefaultIfEmpty()
                                 select new CitizenVehicleDto()
                                 {
                                     Id = vh.Id,
                                     ApartmentCode = vh.ApartmentCode,
                                     BuildingId = vh.BuildingId,
                                     CitizenName = cz.FullName,
                                     CitizenTempId = vh.CitizenTempId,
                                     CreationTime = vh.CreationTime,
                                     CreatorUserId = vh.CreatorUserId,
                                     Description = vh.Description,
                                     UrbanId = vh.UrbanId,
                                     VehicleCode = vh.VehicleCode,
                                     VehicleName = vh.VehicleName,
                                     VehicleType = vh.VehicleType,
                                     TenantId = vh.TenantId,
                                     State = vh.State,
                                     ParkingId = vh.ParkingId,
                                     RegistrationDate = vh.RegistrationDate,
                                     ExpirationDate = vh.ExpirationDate

                                 })
                                 .Where(x => x.State == CitizenVehicleState.WAITING || x.State == CitizenVehicleState.REJECTED || x.State == null || x.State == CitizenVehicleState.OVERDUE)
                                 .AsQueryable();
                    var paginatedData = await query.PageBy(input).ToListAsync();

                    return DataResult.ResultSuccess(paginatedData, "Get success!", query.Count());
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetById(long id)
        {
            try
            {
                var data = await _citizenVehicleRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> CreateOrUpdateVehicleByApartment(CreateOrUpdateVehicleByApartmentDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                if (input.DeleteList != null && input.DeleteList.Count > 0)
                {
                    await _citizenVehicleRepos.DeleteAsync(x => input.DeleteList.Contains(x.Id));
                }
                if (input.Value != null && input.Value.Count > 0)
                {
                    foreach (CitizenVehicleDto detail in input.Value)
                    {
                        var item = detail.MapTo<CitizenVehicle>();
                        item.ApartmentCode = input.ApartmentCode;
                        item.BuildingId = input.BuildingId;
                        item.UrbanId = input.UrbanId;
                        item.Description = input.Description;
                        item.OwnerName = input.OwnerName;
                        item.TenantId = AbpSession.TenantId;
                        item.RegistrationDate = detail.RegistrationDate;
                        item.ExpirationDate = detail.ExpirationDate;

                        // Kiểm tra xem ExpirationDate có vượt quá ngày hiện tại không
                        if (item.ExpirationDate != null && item.ExpirationDate <= DateTime.Now)
                        {
                            // Nếu State là REJECTED thì không cần cập nhật
                            if (item.State != CitizenVehicleState.REJECTED)
                            {
                                item.State = CitizenVehicleState.OVERDUE;
                            }
                        }
                        else
                        {
                            item.State = CitizenVehicleState.ACCEPTED;
                        }

                        if (item.Id > 0)
                        {
                            await _citizenVehicleRepos.UpdateAsync(item);
                        }
                        else
                        {
                            var id = await _citizenVehicleRepos.InsertAndGetIdAsync(item);
                        }

                    }
                }


                //if(input.Value != null && input.Value.Count > 0)
                //{
                //    foreach (CitizenVehicleDto detail in input.Value)
                //    {
                //        var item = detail.MapTo<CitizenVehicle>();
                //        item.ApartmentCode = input.ApartmentCode;
                //        item.BuildingId = input.BuildingId;
                //        item.UrbanId = input.UrbanId;
                //        item.Description = input.Description;
                //        item.OwnerName = input.OwnerName;
                //        item.TenantId = AbpSession.TenantId;
                //        item.RegistrationDate = input.RegistrationDate;
                //        item.ExpirationDate = input.ExpirationDate;

                //        if (item.Id > 0)
                //        {
                //            await _citizenVehicleRepos.UpdateAsync(item);
                //        }
                //        else
                //        {
                //            item.State = CitizenVehicleState.ACCEPTED;
                //            var id = await _citizenVehicleRepos.InsertAndGetIdAsync(item);
                //        }
                //    }
                //}

                var message = "";
                await CurrentUnitOfWork.SaveChangesAsync();

                return DataResult.ResultSuccess("Update success");

            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());

                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }


        public async Task<object> CreateOrUpdateVehicle(CitizenVehicleDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateData = await _citizenVehicleRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        await _citizenVehicleRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_vehicle");

                    var data = DataResult.ResultSuccess(updateData, "Update success!");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CitizenVehicle>();

                    long id = await _citizenVehicleRepos.InsertAndGetIdAsync(insertInput);
                    insertInput.Id = id;

                    mb.statisticMetris(t1, 0, "is_vehicle");

                    var data = DataResult.ResultSuccess(insertInput, "Insert success!");
                    return data;
                }

            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());

                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteByApartment(string apartmentCode)
        {
            try
            {
                await _citizenVehicleRepos.DeleteAsync(x => x.ApartmentCode == apartmentCode);
                return DataResult.ResultSuccess("Delete success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteMultipleByApartment([FromBody] List<string> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                await _citizenVehicleRepos.DeleteAsync(x => ids.Contains(x.ApartmentCode));
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> Delete(long id)
        {
            try
            {
                var deleteData = await _citizenVehicleRepos.GetAsync(id);
                if (deleteData != null)
                {
                    await _citizenVehicleRepos.DeleteAsync(deleteData);
                }
                return DataResult.ResultSuccess("Deleted!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteMultiple([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = Delete(id);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> ApproveCitizenVehicleInsert(ApproveCitizenVehicleInsertDto input)
        {
            try
            {
                var vehicle = await _citizenVehicleRepos.GetAsync(input.Id);
                vehicle.State = input.State;
                await _citizenVehicleRepos.UpdateAsync(vehicle);
                return DataResult.ResultSuccess(vehicle, "Update success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception!");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }


        //[Obsolete]
        public async Task<object> CreateListAsync(RegisterCitizenVehicleInput input)
        {
            try
            {
                //var insertList = new List<CitizenVehicle>();
                foreach (var vh in input.ListVehicle)
                {
                    var insertData = vh.MapTo<CitizenVehicle>();
                    //insertData.State = CitizenVehicleState.WAITING;
                    insertData.CitizenTempId = input.CitizenTempId;
                    insertData.ApartmentCode = input.ApartmentCode;
                    insertData.BuildingId = input.BuildingId;
                    insertData.UrbanId = input.UrbanId;
                    insertData.TenantId = AbpSession.TenantId;
                    insertData.Description = input.Description;
                    long id = await _citizenVehicleRepos.InsertAndGetIdAsync(insertData);
                    insertData.Id = id;
                    insertData.RegistrationDate = vh.RegistrationDate;
                    insertData.ExpirationDate = vh.ExpirationDate;

                    // Kiểm tra xem ExpirationDate có vượt quá ngày hiện tại không
                    if (insertData.ExpirationDate != null && insertData.ExpirationDate <= DateTime.Now)
                    {
                        // Nếu State là REJECTED thì không cần cập nhật
                        if (insertData.State != CitizenVehicleState.REJECTED)
                        {
                            insertData.State = CitizenVehicleState.OVERDUE;
                        }
                    }
                    else
                    {
                        insertData.State = CitizenVehicleState.ACCEPTED;
                    }
                }
                var data = DataResult.ResultSuccess(input.ListVehicle, "Success!", input.ListVehicle.Count);
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> ExportToExcel(ExportVehicleToExcelInput input)
        {
            try
            {
                var query = (from vh in _citizenVehicleRepos.GetAll()
                             join pk in _parkingRepos.GetAll() on vh.ParkingId equals pk.Id into tbl_prk
                             from pk in tbl_prk
                             join ub in _appOrganizationUnitRepos.GetAll() on vh.UrbanId equals ub.Id into tbl_urban
                             from ub in tbl_urban
                             join bu in _appOrganizationUnitRepos.GetAll() on vh.BuildingId equals bu.Id into tbl_building
                             from bu in tbl_building
                             select new CitizenVehicleExcelOutputDto()
                             {
                                 Id = vh.Id,
                                 ApartmentCode = vh.ApartmentCode,
                                 BuildingId = vh.BuildingId,
                                 CitizenTempId = vh.CitizenTempId,
                                 CreationTime = vh.CreationTime,
                                 CreatorUserId = vh.CreatorUserId,
                                 Description = vh.Description,
                                 UrbanId = vh.UrbanId,
                                 VehicleCode = vh.VehicleCode,
                                 VehicleName = vh.VehicleName,
                                 VehicleType = vh.VehicleType,
                                 TenantId = vh.TenantId,
                                 ParkingId = vh.ParkingId,
                                 State = vh.State,
                                 UrbanCode = ub.ProjectCode,
                                 BuildingCode = bu.ProjectCode,
                                 ParkingName = pk.ParkingName,
                                 OwnerName = vh.OwnerName,
                                 CardNumber = vh.CardNumber,
                             })
                             .Where(x => x.State == CitizenVehicleState.ACCEPTED)
                             .WhereIf(input.ApartmentCodes != null, x => input.ApartmentCodes.Contains(x.ApartmentCode))
                             .OrderBy(x => x.ApartmentCode).AsQueryable();
                var data = await query.ToListAsync();
                var result = _excelExporter.ExportCitizenVehicleToFile(data);
                return DataResult.ResultSuccess(result, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        protected VehicleType GetVehicleTypeNumber(string type)
        {
            if (type.ToLower().Contains("car")
                || type.ToLower().Contains("ô tô")
                || type.ToLower().Contains("자동차".ToLower())) return VehicleType.Car;
            if (type.ToLower().Contains("motorbike")
                || type.ToLower().Contains("motorcycle")
                || type.ToLower().Contains("xe máy")
                || type.ToLower().Contains("오토바이".ToLower())) return VehicleType.Motorbike;
            if (type.ToLower().Contains("Bicycle")
                || type.ToLower().Contains("bike")
                || type.ToLower().Contains("xe đạp")
                || type.ToLower().Contains("자전거".ToLower())) return VehicleType.Bicycle;
            return VehicleType.Other;
        }

        public async Task<object> ImportVehicleFromExcel([FromForm] ImportVehicleInput input)
        {
            const int COL_URBAN_CODE = 1;
            const int COL_BUILDING_CODE = 2;
            const int COL_APARTMENT_CODE = 3;
            const int COL_CUSTOMER_NAME = 4;
            const int COL_CARD_NUMBER = 5;
            const int COL_VEHICLE_TYPE = 6;
            const int COL_VEHICLE_CODE = 7;
            const int COL_VEHICLE_NAME = 8;
            const int COL_PARKING_LOT_CODE = 9;
            const int COL_DESCRIPTION = 10;
            const int COL_REGISTER = 11;
            const int COL_EXPIRES = 12;

            var file = input.Form;

            var fileName = file.FileName;
            var fileExt = Path.GetExtension(fileName);
            if (fileExt != ".xlsx" && fileExt != ".xls")
            {
                return DataResult.ResultError("File not support", "Error");
            }

            var filePath = Path.GetRandomFileName() + fileExt;
            var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            try
            {
                var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();

                var rowCount = worksheet.Dimension.End.Row;
                var colCount = worksheet.Dimension.End.Column;

                for (var row = 2; row <= rowCount; row++)
                {
                    var citizenVehicle = new CitizenVehicle();

                    if (worksheet.Cells[row, COL_VEHICLE_CODE].Value != null)
                        citizenVehicle.VehicleCode = worksheet.Cells[row, COL_VEHICLE_CODE].Value.ToString().Trim();

                    if ((await _citizenVehicleRepos.FirstOrDefaultAsync(x => x.VehicleCode == citizenVehicle.VehicleCode)) != null) continue;

                    if (worksheet.Cells[row, COL_APARTMENT_CODE].Value != null)
                    {
                        citizenVehicle.ApartmentCode = worksheet.Cells[row, COL_APARTMENT_CODE].Value.ToString().Trim();
                    }
                    else continue;

                    if (worksheet.Cells[row, COL_URBAN_CODE].Value != null)
                    {
                        var ubIDstr = worksheet.Cells[row, COL_URBAN_CODE].Value.ToString().Trim();
                        var ubObj = await _appOrganizationUnitRepos.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == ubIDstr.ToLower());
                        if (ubObj != null) citizenVehicle.UrbanId = ubObj.Id;
                    }

                    if (worksheet.Cells[row, COL_BUILDING_CODE].Value != null)
                    {
                        var buildIDStr = worksheet.Cells[row, COL_BUILDING_CODE].Value.ToString().Trim();
                        var buildObj = await _appOrganizationUnitRepos.FirstOrDefaultAsync(x => x.ProjectCode.ToLower() == buildIDStr.ToLower());
                        if (buildObj != null) citizenVehicle.BuildingId = buildObj.Id;
                    }

                    if (worksheet.Cells[row, COL_VEHICLE_NAME].Value != null)
                        citizenVehicle.VehicleName = worksheet.Cells[row, COL_VEHICLE_NAME].Value.ToString().Trim();

                    if (worksheet.Cells[row, COL_VEHICLE_TYPE].Value != null)
                        citizenVehicle.VehicleType = GetVehicleTypeNumber(worksheet.Cells[row, COL_VEHICLE_TYPE].Value.ToString().Trim());
                    else citizenVehicle.VehicleType = VehicleType.Other;

                    if (worksheet.Cells[row, COL_PARKING_LOT_CODE].Value != null)
                    {
                        var parkingIDstr = worksheet.Cells[row, COL_PARKING_LOT_CODE].Value.ToString().Trim();
                        citizenVehicle.ParkingId = (await _parkingRepos.FirstOrDefaultAsync(x => x.ParkingCode == parkingIDstr)).Id;
                    }

                    if (worksheet.Cells[row, COL_CARD_NUMBER].Value != null)
                        citizenVehicle.CardNumber = worksheet.Cells[row, COL_CARD_NUMBER].Value.ToString().Trim();

                    if (worksheet.Cells[row, COL_CUSTOMER_NAME].Value != null)
                        citizenVehicle.OwnerName = worksheet.Cells[row, COL_CUSTOMER_NAME].Value.ToString().Trim();

                    if (worksheet.Cells[row, COL_DESCRIPTION].Value != null)
                        citizenVehicle.Description = worksheet.Cells[row, COL_DESCRIPTION].Value.ToString().Trim();

                    citizenVehicle.State = CitizenVehicleState.ACCEPTED;
                    citizenVehicle.TenantId = AbpSession.TenantId;

                    /*var citizenVehicle = new CitizenVehicle()
                    {
                        ApartmentCode = worksheet.Cells[row, COL_APARTMENT_CODE].Value.ToString(),
                        
                        TenantId = AbpSession.TenantId,
                        State = CitizenVehicleState.ACCEPTED,
                        UrbanId = ubID,
                        BuildingId = buildID,
                        ParkingId = parkingID,
                        CardNumber = worksheet.Cells[row, COL_CARD_NUMBER].Value.ToString(),
                        OwnerName = worksheet.Cells[row, COL_CUSTOMER_NAME].Value.ToString()
                    };*/

                    await _citizenVehicleRepos.InsertAsync(citizenVehicle);
                }

                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);

                return DataResult.ResultSuccess("Success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        private async Task CreateListVehicleAsync(List<CitizenVehicleDto> input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    if (input == null || !input.Any())
                    {
                        return;
                    }
                    foreach (var vh in input)
                    {
                        long id = await _citizenVehicleRepos.InsertAndGetIdAsync(vh);
                        vh.Id = id;

                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception !");
                Logger.Fatal(ex.Message);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> ImportVehicleExcel([FromForm] ImportVehicleExcelInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    IFormFile file = input.File;
                    string fileName = file.FileName;
                    string fileExt = Path.GetExtension(fileName);
                    if (fileExt != ".xlsx" && fileExt != ".xls")
                    {
                        return DataResult.ResultError("File not supported", "Error");
                    }

                    string filePath = Path.GetRandomFileName() + fileExt;

                    using (FileStream stream = File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                        stream.Close();
                    }

                    var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.End.Row;

                    const int URBAN_CODE_INDEX = 1;
                    const int BUILDING_CODE_INDEX = 2;
                    const int APARTMENT_CODE_INDEX = 3;
                    const int CUSTOMER_INDEX = 4;
                    const int CARD_NUMBER_INDEX = 5;
                    const int VEHICLE_TYPE_INDEX = 6;
                    const int VEHICLE_CODE_INDEX = 7;
                    const int VEHICLE_NAME_INDEX = 8;
                    const int PARKING_CODE_INDEX = 9;
                    const int DESCRIPTION_INDEX = 10;
                    const int REGISTRE_INDEX = 11;
                    const int EXPIRES_INDEX = 12;


                    var listNew = new List<CitizenVehicleDto>();

                    for (var row = 2; row <= rowCount; row++)
                    {
                        string apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Text.Trim();
                        string customer = worksheet.Cells[row, CUSTOMER_INDEX].Text.Trim();
                        string cardNumber = worksheet.Cells[row, CARD_NUMBER_INDEX].Text.Trim();
                        string vehicleType = worksheet.Cells[row, VEHICLE_TYPE_INDEX].Text.Trim();
                        string vehicleCode = worksheet.Cells[row, VEHICLE_CODE_INDEX].Text.Trim();
                        string vehicleName = worksheet.Cells[row, VEHICLE_NAME_INDEX].Text.Trim();
                        string parkingCode = worksheet.Cells[row, PARKING_CODE_INDEX].Text.Trim();
                        string description = worksheet.Cells[row, DESCRIPTION_INDEX].Text?.Trim();
                        var registreString = worksheet.Cells[row, REGISTRE_INDEX].Text?.Trim();
                        var registre = !string.IsNullOrEmpty(registreString)
                            ? DateTime.ParseExact(registreString, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : (DateTime?)null;
                        var expiresString = worksheet.Cells[row, EXPIRES_INDEX].Text?.Trim();
                        var expires = !string.IsNullOrEmpty(expiresString)
                            ? DateTime.ParseExact(expiresString, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : (DateTime?)null;


                        string buildingCode = worksheet.Cells[row, BUILDING_CODE_INDEX].Text?.Trim();
                        string urbanCode = worksheet.Cells[row, URBAN_CODE_INDEX].Text.Trim();
                        var parking = await _parkingRepos.FirstOrDefaultAsync(x => x.ParkingCode == parkingCode);
                        var parkingId = parking != null ? parking.Id : default;

                        var vehicleCitizen = new CitizenVehicleDto()
                        {
                            TenantId = AbpSession.TenantId,
                            CreatorUserId = AbpSession.UserId,
                            ApartmentCode = apartmentCode,
                            OwnerName = customer,
                            RegistrationDate = registre,
                            ExpirationDate = expires,
                            Description = description,
                            VehicleCode = vehicleCode,
                            VehicleName = vehicleName,
                            VehicleType = GetVehicleTypeNumber(vehicleType) != null ? GetVehicleTypeNumber(vehicleType) : VehicleType.Other,
                            ParkingId = parkingId,
                            CardNumber = cardNumber,

                        };
                        if (vehicleCitizen.ExpirationDate != null && vehicleCitizen.ExpirationDate <= DateTime.Now)
                        {
                            // Nếu State là REJECTED thì không cần cập nhật
                            if (vehicleCitizen.State != CitizenVehicleState.REJECTED)
                            {
                                vehicleCitizen.State = CitizenVehicleState.OVERDUE;
                            }
                        }
                        else
                        {
                            vehicleCitizen.State = CitizenVehicleState.ACCEPTED;
                        }


                        var listBuilding = _appOrganizationUnitRepos.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                        var listUrban = _appOrganizationUnitRepos.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);

                        if (!string.IsNullOrEmpty(buildingCode))
                        {
                            var building = listBuilding.FirstOrDefault(x => x.ProjectCode == buildingCode);
                            if (building != null) vehicleCitizen.BuildingId = building.ParentId;
                        }

                        if (!string.IsNullOrEmpty(urbanCode))
                        {
                            var urban = listUrban.FirstOrDefault(x => x.ProjectCode == urbanCode);
                            if (urban != null) vehicleCitizen.UrbanId = urban.ParentId;
                        }


                        listNew.Add(vehicleCitizen);
                    }

                    await CreateListVehicleAsync(listNew);


                    // Xóa tệp đã tạo
                    File.Delete(filePath);

                    return DataResult.ResultSuccess(listNew, "Upload success");
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

    }
}
