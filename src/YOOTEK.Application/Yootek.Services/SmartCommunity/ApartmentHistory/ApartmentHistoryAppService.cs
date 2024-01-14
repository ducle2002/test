#nullable enable
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using Yootek.EntityDb;
using Yootek.Services.Exporter;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Path = System.IO.Path;

namespace Yootek.Services
{
    public interface IApartmentHistoryAppService : IApplicationService
    {
        Task<DataResult> GetAllApartmentHistoryAsync(GetAllApartmentHistoryInput input);
        Task<DataResult> CreateApartmentHistoryAsync(CreateApartmentHistoryDto input);
        Task<DataResult> UpdateApartmentHistoryAsync(UpdateApartmentHistoryDto input);
        Task<DataResult> DeleteApartmentHistoryAsync(long id);
        Task<DataResult> ImportApartmentHistoryExcelAsync(ImportApartmentHistoryExcelDto input, CancellationToken cancellationToken);
        Task<DataResult> ExportApartmentHistoryExcelAsync(ExportApartmentHistoryExcelDto input);
    }

    public class ApartmentHistoryAppService : YootekAppServiceBase, IApartmentHistoryAppService
    {
        private readonly IRepository<ApartmentHistory, long> _apartmentHistoryRepository;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IApartmentHistoryExcelExporter _apartmentHistoryExcelExporter;

        public ApartmentHistoryAppService(
            IRepository<ApartmentHistory, long> apartmentHistoryRepository,
            IRepository<Apartment, long> apartmentRepository, IApartmentHistoryExcelExporter apartmentHistoryExcelExporter)
        {
            _apartmentHistoryRepository = apartmentHistoryRepository;
            _apartmentRepository = apartmentRepository;
            _apartmentHistoryExcelExporter = apartmentHistoryExcelExporter;
        }

        public async Task<DataResult> GetAllApartmentHistoryAsync(GetAllApartmentHistoryInput input)
        {
            try
            {
                IQueryable<GetAllApartmentHistoryDto> query =
                    (from apartmentHistory in _apartmentHistoryRepository.GetAll()
                        where apartmentHistory.ApartmentId == input.ApartmentId
                        select new GetAllApartmentHistoryDto
                        {
                            Id = apartmentHistory.Id,
                            ApartmentId = apartmentHistory.ApartmentId,
                            Title = apartmentHistory.Title,
                            Description = apartmentHistory.Description,
                            Type = apartmentHistory.Type,
                            ImageUrls = apartmentHistory.ImageUrls,
                            FileUrls = apartmentHistory.FileUrls,
                            ExecutorName = apartmentHistory.ExecutorName,
                            ExecutorPhone = apartmentHistory.ExecutorPhone,
                            ExecutorEmail = apartmentHistory.ExecutorEmail,
                            SupervisorName = apartmentHistory.SupervisorName,
                            SupervisorPhone = apartmentHistory.SupervisorPhone,
                            SupervisorEmail = apartmentHistory.SupervisorEmail,
                            ReceiverName = apartmentHistory.ReceiverName,
                            ReceiverPhone = apartmentHistory.ReceiverPhone,
                            ReceiverEmail = apartmentHistory.ReceiverEmail,
                            Cost = apartmentHistory.Cost,
                            DateStart = apartmentHistory.DateStart,
                            DateEnd = apartmentHistory.DateEnd
                        }).WhereIf(
                        input.Type.HasValue,
                        x => x.Type == input.Type
                    ).Where(
                        x => x.ApartmentId == input.ApartmentId
                    );

                var result = await query.ApplySort(input.OrderBy, input.SortBy)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Success", result.Count);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> CreateApartmentHistoryAsync(CreateApartmentHistoryDto input)
        {
            try
            {
                var apartmentHistory = input.MapTo<ApartmentHistory>();
                apartmentHistory.TenantId = AbpSession.TenantId;
                await _apartmentHistoryRepository.InsertAsync(apartmentHistory);
                return DataResult.ResultSuccess(apartmentHistory, "Create success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> UpdateApartmentHistoryAsync(UpdateApartmentHistoryDto input)
        {
            try
            {
                var apartmentHistory = await _apartmentHistoryRepository.GetAsync(input.Id);
                if (apartmentHistory == null)
                {
                    throw new UserFriendlyException("Not found!");
                }

                ObjectMapper.Map(input, apartmentHistory);
                if (input.ImageUrls != null && input.ImageUrls.Count > 0)
                    apartmentHistory.ImageUrls = input.ImageUrls;
                if (input.FileUrls != null && input.FileUrls.Count > 0)
                    apartmentHistory.FileUrls = input.FileUrls;

                // apartmentHistory = MapUpdate(apartmentHistory, input);

                await _apartmentHistoryRepository.UpdateAsync(apartmentHistory);
                return DataResult.ResultSuccess(apartmentHistory, "Update success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteApartmentHistoryAsync(long id)
        {
            try
            {
                var apartmentHistory = await _apartmentHistoryRepository.GetAsync(id);
                if (apartmentHistory == null)
                {
                    throw new UserFriendlyException("Not found!");
                }

                await _apartmentHistoryRepository.DeleteAsync(apartmentHistory);
                return DataResult.ResultSuccess(apartmentHistory, "Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> ImportApartmentHistoryExcelAsync([FromForm] ImportApartmentHistoryExcelDto input, CancellationToken cancellationToken)
                {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    IFormFile file = input.Form;
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
                    List<ApartmentHistory> listApartmentImports = new();
                    try
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                        int rowCount = worksheet.Dimension.End.Row;
                        int colCount = worksheet.Dimension.End.Column;

                        // Format columns
                        const int apartmentCodeIndex = 1;
                        const int titleIndex = 2;
                        const int descriptionIndex = 3;
                        const int typeIndex = 4;
                        const int executorNameIndex = 5;
                        const int executorPhoneIndex = 6;
                        const int executorEmailIndex = 7;
                        const int supervisorNameIndex = 8;
                        const int supervisorPhoneIndex = 9;
                        const int supervisorEmailIndex = 10;
                        const int receiverNameIndex = 11;
                        const int receiverPhoneIndex = 12;
                        const int receiverEmailIndex = 13;
                        const int costIndex = 14;
                        const int dateStartIndex = 15;
                        const int dateEndIndex = 16;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var apartmentCode = GetCellValueNotDefault<string>(worksheet, row, apartmentCodeIndex);
                            var title = GetCellValueNotDefault<string>(worksheet, row, titleIndex);
                            var description = GetCellValueNotDefault<string>(worksheet, row, descriptionIndex);
                            var type = GetCellValueNotDefault<int>(worksheet, row, typeIndex);
                            var executorName = GetCellValue<string>(worksheet, row, executorNameIndex);
                            var executorPhone = GetCellValue<string?>(worksheet, row, executorPhoneIndex) ?? "";
                            var executorEmail = GetCellValue<string>(worksheet, row, executorEmailIndex) ?? "";
                            Console.WriteLine(title);
                            var supervisorName =
                                GetCellValue<string>(worksheet, row, supervisorNameIndex) ?? "";
                            var supervisorPhone = GetCellValue<string>(worksheet, row, supervisorPhoneIndex) ?? "";
                            var supervisorEmail = GetCellValue<string>(worksheet, row, supervisorEmailIndex) ?? "";
                            var receiverName = GetCellValue<string>(worksheet, row, receiverNameIndex) ?? "";
                            var receiverPhone = GetCellValue<string>(worksheet, row, receiverPhoneIndex) ?? "";
                            var receiverEmail = GetCellValue<string>(worksheet, row, receiverEmailIndex) ?? "";
                            Console.WriteLine(receiverEmail);
                            var cost = GetCellValue<long>(worksheet, row, costIndex);
                            Console.WriteLine(cost);
                            var dateStart = GetCellValue<string>(worksheet, row, dateStartIndex) ?? "";
                            var dateEnd = GetCellValue<string>(worksheet, row, dateEndIndex) ?? "";
                            Console.WriteLine(dateStart);

                            var apartment = await _apartmentRepository.FirstOrDefaultAsync(x => x.ApartmentCode == apartmentCode.Trim());
                            if (apartment == null)
                            {
                                await stream.DisposeAsync();
                                stream.Close();
                                File.Delete(filePath);
                                return DataResult.ResultError($"Apartment {apartmentCode} not found", "Error");
                            }
                            
                            listApartmentImports.Add(new ApartmentHistory()
                            {
                                TenantId = AbpSession.TenantId,
                                ApartmentId = apartment.Id,
                                Title = title,
                                Description = description,
                                Type = GetApartmentHistoryType(type),
                                ExecutorName = executorName,
                                ExecutorPhone = executorPhone,
                                ExecutorEmail = executorEmail,
                                SupervisorName = supervisorName,
                                SupervisorPhone = supervisorPhone,
                                SupervisorEmail = supervisorEmail,
                                ReceiverName = receiverName,
                                ReceiverPhone = receiverPhone,
                                ReceiverEmail = receiverEmail,
                                Cost = cost,
                                DateStart = DateTime.Parse(dateStart),
                                DateEnd = dateEnd == "" ? null : DateTime.Parse(dateEnd),
                            });
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

                    listApartmentImports.ForEach(apartment =>
                    {
                        _apartmentHistoryRepository.Insert(apartment);
                    });
                    return DataResult.ResultSuccess(true, "Upload apartment history success");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> ExportApartmentHistoryExcelAsync(ExportApartmentHistoryExcelDto input)
        {
            try
            {
                var query = (from apartmentHistories in _apartmentHistoryRepository.GetAll()
                    where input.ApartmentId.Contains(apartmentHistories.ApartmentId)
                    select new ApartmentHistoryExcelOutput
                    {
                        ApartmentId = apartmentHistories.ApartmentId,
                        ApartmentCode = (from apartment in _apartmentRepository.GetAll()
                            where apartment.Id == apartmentHistories.ApartmentId
                            select apartment.ApartmentCode).FirstOrDefault(),
                        Title = apartmentHistories.Title,
                        Description = apartmentHistories.Description,
                        Type = apartmentHistories.Type,
                        ImageUrls = String.Join("\n", apartmentHistories.ImageUrls ?? new List<string>()),
                        FileUrls = String.Join("\n", apartmentHistories.FileUrls ?? new List<string>()),
                        ExecutorName = apartmentHistories.ExecutorName,
                        ExecutorPhone = apartmentHistories.ExecutorPhone ?? "",
                        ExecutorEmail = apartmentHistories.ExecutorEmail ?? "",
                        SupervisorName = apartmentHistories.SupervisorName ?? "",
                        SupervisorPhone = apartmentHistories.SupervisorPhone ?? "",
                        SupervisorEmail = apartmentHistories.SupervisorEmail ?? "",
                        ReceiverName = apartmentHistories.ReceiverName ?? "",
                        ReceiverPhone = apartmentHistories.ReceiverPhone ?? "",
                        ReceiverEmail = apartmentHistories.ReceiverEmail ?? "",
                        Cost = apartmentHistories.Cost ?? 0,
                        DateStart = apartmentHistories.DateStart,
                        DateEnd = apartmentHistories.DateEnd ?? null
                    }).WhereIf(input.Type.HasValue,x => x.Type == input.Type)
                    .WhereIf(input.DateStart.HasValue, x => x.DateStart >= input.DateStart)
                    .WhereIf(input.DateEnd.HasValue, x => x.DateEnd <= input.DateEnd)
                    .ToList();
                
                var result = _apartmentHistoryExcelExporter.ExportApartmentHistoryToFile(query);
                return DataResult.ResultSuccess(result, "Success");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
            
        }

        private EApartmentHistoryType GetApartmentHistoryType(int type)
        {
            switch (type)
            {
                case (int)EApartmentHistoryType.Repair:
                    return EApartmentHistoryType.Repair;
                case (int)EApartmentHistoryType.ForRent:
                    return EApartmentHistoryType.ForRent;
                case (int)EApartmentHistoryType.ForSale:
                    return EApartmentHistoryType.ForSale;
                case (int)EApartmentHistoryType.Violation:
                    return EApartmentHistoryType.Violation;
                case (int)EApartmentHistoryType.LostAsset:
                    return EApartmentHistoryType.LostAsset;
                case (int)EApartmentHistoryType.HandoverProperty:
                    return EApartmentHistoryType.HandoverProperty;
                case (int)EApartmentHistoryType.Other:
                    return EApartmentHistoryType.Other;
                default:
                    return EApartmentHistoryType.Other;
            }
        }
        
    }
}