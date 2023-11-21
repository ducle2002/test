

using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using IMAX.Application;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using IMAX.IMAX.Services.IMAX.SmartCommunity.VehicleCitizen;
using IMAX.Organizations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IMAX.Services
{

    public interface ICitizenParkingManagementAppService : IApplicationService
    {

    }

    [AbpAuthorize]
    public class CitizenParkingManagementAppService : IMAXAppServiceBase, ICitizenParkingManagementAppService
    {

        private readonly IRepository<CitizenTemp, long> _citizenTempRepos;
        private readonly IRepository<CitizenParking, long> _citizenParkingRepos;
        private readonly ICitizenVehicleExcelExporter _exporter;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepos;
        public CitizenParkingManagementAppService(
            IRepository<CitizenTemp, long> citizenTempRepos,
            IRepository<CitizenParking, long> citizenParkingRepos,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepos,
            ICitizenVehicleExcelExporter exporter
            )
        {
            _citizenTempRepos = citizenTempRepos;
            _citizenParkingRepos = citizenParkingRepos;
            _appOrganizationUnitRepos = appOrganizationUnitRepos;
            _exporter = exporter;
        }


        public async Task<object> GetAllAsync(GetAllCitizenParkingInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = _citizenParkingRepos.GetAll();
                    var paginatedData = await query
                            .ApplySearchFilter(input.Keyword, x => x.ParkingName, x => x.ParkingCode)
                            .ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByCitizenParking.PARKING_CODE)
                            .PageBy(input).ToListAsync();
                    var result = paginatedData.MapTo<List<CitizenParkingDto>>();

                    return DataResult.ResultSuccess(result, "Get success!", query.Count());
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
                var data = await _citizenParkingRepos.GetAsync(id);
                return DataResult.ResultSuccess(data, "Success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }

        }

        public async Task<object> CreateOrUpdate(CitizenParkingDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateData = await _citizenParkingRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        await _citizenParkingRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_vehicle");

                    var data = DataResult.ResultSuccess(updateData, "Update success!");
                    return data;
                }
                else
                {
                    var insertInput = input.MapTo<CitizenParking>();

                    long id = await _citizenParkingRepos.InsertAndGetIdAsync(insertInput);
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

        public async Task<object> Delete(long id)
        {
            try
            {
                var deleteData = await _citizenParkingRepos.GetAsync(id);
                if (deleteData != null)
                {
                    await _citizenParkingRepos.DeleteAsync(deleteData);
                }
                return DataResult.ResultSuccess(deleteData, "Delete success!");
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

        [Obsolete]
        public async Task<object> ExportToExcel(ExportToExcelInput input)
        {
            try
            {
                var query = (from p in _citizenParkingRepos.GetAll()
                             join ub in _appOrganizationUnitRepos.GetAll() on p.UrbanId equals ub.Id into tbl_urban
                             from ub in tbl_urban
                             join bu in _appOrganizationUnitRepos.GetAll() on p.BuildingId equals bu.Id into tbl_building
                             from bu in tbl_building
                             select new ExportToExcelOutputDto()
                             {
                                 Id = p.Id,
                                 BuildingCode = bu.ProjectCode,
                                 UrbanCode = ub.ProjectCode,
                                 Description = p.Description,
                                 ParkingCode = p.ParkingCode,
                                 ParkingName = p.ParkingName,
                                 BuildingId = p.BuildingId,
                                 UrbanId = p.UrbanId,
                             }).AsQueryable();
                var data = await query.WhereIf(input.Ids.Count > 0, x => input.Ids.Contains(x.Id)).ToListAsync();

                var result = _exporter.ExportParkingToFile(data);
                return DataResult.ResultSuccess(result, "Export success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<object> ImportExcel([FromForm] ImportExcelInput input)
        {
            const int URBAN_CODE_INDEX = 1;
            const int BUILDING_CODE_INDEX = 2;
            const int PARKING_NAME_INDEX = 3;
            const int PARKING_CODE_INDEX = 4;
            const int PARKING_DESCRIPTION_INDEX = 5;

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
                    var parking = new CitizenParking();

                    parking.TenantId = AbpSession.TenantId;

                    if (worksheet.Cells[row, PARKING_CODE_INDEX].Value != null)
                        parking.ParkingCode = worksheet.Cells[row, PARKING_CODE_INDEX].Value.ToString().Trim();

                    if (worksheet.Cells[row, PARKING_NAME_INDEX].Value != null)
                        parking.ParkingName = worksheet.Cells[row, PARKING_NAME_INDEX].Value.ToString().Trim();

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

                    if (worksheet.Cells[row, PARKING_DESCRIPTION_INDEX].Value != null)
                        parking.Description = worksheet.Cells[row, PARKING_DESCRIPTION_INDEX].Value.ToString().Trim();

                    var id = _citizenParkingRepos.InsertAndGetIdAsync(parking);

                }

                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);

                return DataResult.ResultSuccess("Success");
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
