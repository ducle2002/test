#nullable enable
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.Core.Dto;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Yootek.Services.ExportData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yootek.Common.Enum;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using DocumentFormat.OpenXml.Vml;
using System.IO;
using DocumentFormat.OpenXml.Drawing;
using Yootek.Authorization;
using Yootek.QueriesExtension;

namespace Yootek.Services
{
    public interface IAdminApartmentAppService : IApplicationService
    {
        Task<DataResult> GetAllApartmentAsync(GetAllApartmentInput input);
        Task<DataResult> GetApartmentByIdAsync(long id);
        Task<DataResult> CreateApartment(CreateApartmentInput input);
        Task<DataResult> UpdateApartment(UpdateApartmentInput input);
        Task<DataResult> DeleteApartment(long id);
        Task<DataResult> DeleteManyApartments([FromBody] List<long> ids);
        DataResult ExportApartmentsToExcel([FromBody] ExportApartmentInput input);
        Task<DataResult> ImportApartments([FromForm] ImportApartmentInput input, CancellationToken cancellationToken);
    }
    public class AdminApartmentAppService : YootekAppServiceBase, IAdminApartmentAppService
    {
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IApartmentExcelExporter _apartmentExcelExporter;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnit;
        private readonly IRepository<ApartmentStatus, long> _statusRepository;
        private readonly IRepository<BlockTower, long> _blockRepository;
        private readonly IRepository<ApartmentType, long> _typeRepository;
        private readonly IRepository<Floor, long> _floorRepository;
        private readonly IRepository<Citizen, long> _citizenRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepository;
        private readonly IRepository<BillConfig, long> _billConfigRepo;


        public AdminApartmentAppService(
            IRepository<Apartment, long> apartmentRepository,
            IApartmentExcelExporter apartmentExcelExporter,
            IRepository<ApartmentStatus, long> statusRepository,
            IRepository<Floor, long> floorRepository,
            IRepository<ApartmentType, long> typeRepository,
            IRepository<BlockTower, long> blockRepository,
            IRepository<AppOrganizationUnit, long> organizationUnit,
            IRepository<Citizen, long> citizenRepository,
            IRepository<CitizenTemp, long> citizenTempRepository,
            IRepository<BillConfig, long> billConfigRepo
            )
        {
            _apartmentRepository = apartmentRepository;
            _apartmentExcelExporter = apartmentExcelExporter;
            _floorRepository = floorRepository;
            _organizationUnit = organizationUnit;
            _statusRepository = statusRepository;
            _blockRepository = blockRepository;
            _typeRepository = typeRepository;
            _citizenTempRepository = citizenTempRepository;
            _citizenRepository = citizenRepository;
            _billConfigRepo = billConfigRepo;
        }
        public async Task<DataResult> GetAllApartmentAsync(GetAllApartmentInput input)
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                IQueryable<GetAllApartmentDto> query = (from apartment in _apartmentRepository.GetAll()
                                                        select new GetAllApartmentDto
                                                        {
                                                            Id = apartment.Id,
                                                            ApartmentCode = apartment.ApartmentCode,
                                                            Name = apartment.Name,
                                                            Area = apartment.Area,
                                                            ImageUrl = apartment.ImageUrl,
                                                            BuildingId = apartment.BuildingId,
                                                            UrbanId = apartment.UrbanId,
                                                            BuildingName = _organizationUnit.GetAll().Where(x => x.Id == apartment.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                                                            UrbanName = _organizationUnit.GetAll().Where(x => x.Id == apartment.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                                                            StatusId = apartment.StatusId,
                                                            TypeId = apartment.TypeId,
                                                            CreationTime = apartment.CreationTime,
                                                            TypeName = _typeRepository.GetAll().Where(x => x.Id == apartment.TypeId).Select(x => x.Name).FirstOrDefault(),
                                                            StatusName = _statusRepository.GetAll().Where(x => x.Id == apartment.StatusId).Select(x => x.Name).FirstOrDefault(),
                                                            NumberOfMember = _citizenTempRepository.GetAll().Where(x => x.IsStayed == true && x.ApartmentCode == apartment.ApartmentCode).Count(),
                                                            BlockId = apartment.BlockId,
                                                            BlockName = _blockRepository.GetAll().Where(x => x.Id == apartment.BlockId).Select(x => x.DisplayName).FirstOrDefault(),
                                                            FloorId = apartment.FloorId,
                                                            FloorName = _floorRepository.GetAll().Where(x => x.Id == apartment.FloorId).Select(x => x.DisplayName).FirstOrDefault(),
                                                            BillConfig = apartment.BillConfig,
                                                            
                                                        })
                         .WhereByBuildingOrUrbanIf(!IsGranted(PermissionNames.Data_Admin), buIds)
                         .WhereIf(input.StatusId.HasValue, x => x.StatusId == input.StatusId)
                         .WhereIf(input.TypeId.HasValue, x => x.TypeId == input.TypeId)
                         .WhereIf(input.FloorId.HasValue, x => x.FloorId == input.FloorId)
                         .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                         .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                         .ApplySearchFilter(input.Keyword, x => x.Name, x => x.ApartmentCode);

                List<GetAllApartmentDto> result = await query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByApartment.APARTMENT_CODE) // sort default
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                foreach (GetAllApartmentDto apartment in result)
                {
                    apartment.OwnerName = GetOwner(apartment.ApartmentCode).Select(x => x.FullName).FirstOrDefault();
                    apartment.OwnerPhoneNumber = GetOwner(apartment.ApartmentCode).Select(x => x.PhoneNumber).FirstOrDefault();
                    //apartment.BillConfigDetail = _billConfigRepo.GetAll()
                    //    .Where(x => apartment.BillConfigId.Contains(x.Id))
                    //    .Select(x => new GetAllBillConfigDto
                    //    {
                    //        Id = x.Id,
                    //        Title = x.Title,
                    //        TenantId = x.TenantId,
                    //        BillType = x.BillType

                    //    }).ToList();

                }

                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetApartmentByIdAsync(long id)
        {
            try
            {
                Apartment? apartment = await _apartmentRepository.FirstOrDefaultAsync(id);
                if (apartment == null)
                {
                    return DataResult.ResultSuccess(null, "Get apartment detail success!");
                }
                GetApartmentDetailDto apartmentDto = apartment.MapTo<GetApartmentDetailDto>();

                // Get FloorName, StatusName, and TypeName
                apartmentDto.BuildingName = GetOrganizationName(apartmentDto.BuildingId);
                apartmentDto.UrbanName = GetOrganizationName(apartmentDto.UrbanId);
                apartmentDto.FloorName = GetFloorName(apartmentDto.FloorId);
                apartmentDto.StatusName = GetStatusName(apartmentDto.StatusId);
                apartmentDto.TypeName = GetTypeName(apartmentDto.TypeId);
                apartmentDto.BlockName = GetBlockName(apartmentDto.BlockId);

                // Get list of members
                apartmentDto.Members = GetMembers(apartmentDto.ApartmentCode);

                // Get owner's name and phone number
                CitizenTemp owner = GetOwner(apartmentDto.ApartmentCode).FirstOrDefault();
                apartmentDto.OwnerName = owner?.FullName;
                apartmentDto.OwnerPhoneNumber = owner?.PhoneNumber;

                if (apartment.BillConfig != null)
                {
                    var billConfigList = JsonConvert.DeserializeObject<List<BillConfigProperties>>(apartment.BillConfig);

                    List<GetAllBillConfigDto> listBillConfig = billConfigList
                        .Select(billConfig =>
                            new GetAllBillConfigDto
                            {
                                BillType = billConfig.BillType,
                                Properties = billConfig.Properties,
                            })
                        .ToList();

                    apartmentDto.ListBillConfig = listBillConfig;
                }


                return DataResult.ResultSuccess(apartmentDto, "Get apartment detail success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<DataResult> CreateApartment(CreateApartmentInput input)
        {
            try
            {
                Apartment? apartmentOrg = await _apartmentRepository.FirstOrDefaultAsync(x => x.ApartmentCode == input.ApartmentCode);
                if (apartmentOrg != null) throw new UserFriendlyException(409, "Apartment is exist");
                string? billConfigJson = input.ListBillConfig != null ? JsonConvert.SerializeObject(input.ListBillConfig) : null;
                Apartment apartment = input.MapTo<Apartment>();
                apartment.TenantId = AbpSession.TenantId;
                apartment.BillConfig = billConfigJson;

                
                await _apartmentRepository.InsertAsync(apartment);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateApartment(UpdateApartmentInput input)
        {
            try
            {
                Apartment? apartmentOrg = await _apartmentRepository.FirstOrDefaultAsync(input.Id)
                    ?? throw new UserFriendlyException("Apartment not found!");
                Apartment? apartmentTemp = await _apartmentRepository.FirstOrDefaultAsync(x =>
                    x.Id != input.Id && x.ApartmentCode == input.ApartmentCode);
                if (apartmentTemp != null)
                {
                    throw new UserFriendlyException("Code is existed!");
                };
                string billConfigJson = JsonConvert.SerializeObject(input.ListBillConfig);
                apartmentOrg.BillConfig = billConfigJson;


                
                ObjectMapper.Map(input, apartmentOrg);
                //  apartmentOrg.BillConfigId = input.BillConfigDetail?.Select(x => x.BillConfigId ?? 0).ToArray();

                await _apartmentRepository.UpdateAsync(apartmentOrg);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteApartment(long id)
        {
            try
            {
                await _apartmentRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteManyApartments([FromBody] List<long> ids)
        {
            try
            {
                await _apartmentRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list apartment success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public DataResult ExportApartmentsToExcel([FromBody] ExportApartmentInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    List<ApartmentExportDto> apartments =
                        (from apartment in _apartmentRepository.GetAll()
                         select (new ApartmentExportDto()
                         {
                             Id = apartment.Id,
                             TenantId = apartment.TenantId,
                             Name = apartment.Name,
                             ApartmentCode = apartment.ApartmentCode,
                             Description = apartment.Description,
                             CustomerName = _citizenTempRepository.GetAll().Where(x => x.ApartmentCode == apartment.ApartmentCode)
                                .Select(x => x.FullName).FirstOrDefault(),
                             Area = apartment.Area,
                             BuildingId = apartment.BuildingId,
                             BuildingCode = _organizationUnit.GetAll().Where(x => x.Id == apartment.BuildingId).Select(x => x.ProjectCode).FirstOrDefault(),
                             UrbanId = apartment.UrbanId,
                             UrbanCode = _organizationUnit.GetAll().Where(x => x.Id == apartment.UrbanId).Select(x => x.ProjectCode).FirstOrDefault(),
                             StatusId = apartment.StatusId,
                             StatusName = _statusRepository.GetAll().Where(x => x.Id == apartment.StatusId).Select(x => x.Name).FirstOrDefault(),
                             TypeId = apartment.TypeId,
                             TypeName = _typeRepository.GetAll().Where(x => x.Id == apartment.TypeId).Select(x => x.Name).FirstOrDefault(),
                             BlockId = apartment.BlockId,
                             BlockName = _blockRepository.GetAll().Where(x => x.Id == apartment.BlockId).Select(x => x.DisplayName).FirstOrDefault(),
                             FloorId = apartment.FloorId,
                             FloorName = _floorRepository.GetAll().Where(x => x.Id == apartment.FloorId).Select(x => x.DisplayName).FirstOrDefault(),
                             ProvinceCode = apartment.ProvinceCode,
                             DistrictCode = apartment.DistrictCode,
                             WardCode = apartment.WardCode,
                             Address = apartment.Address,
                         }))
                         .WhereIf(input.Ids != null && input.Ids.Any(), x => input.Ids!.Contains(x.Id))
                         .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                         .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                         .WhereIf(input.StatusId.HasValue, x => x.StatusId == input.StatusId)
                         .WhereIf(input.TypeId.HasValue, x => x.TypeId == input.TypeId)
                         .ToList();

                    FileDto file = _apartmentExcelExporter.ExportToFile(apartments);
                    return DataResult.ResultSuccess(file, "Export excel success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> ImportApartments([FromForm] ImportApartmentInput input, CancellationToken cancellationToken)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    IFormFile file = input.File;
                    string fileName = file.FileName;
                    string fileExt = System.IO.Path.GetExtension(fileName);
                    if (!IsFileExtensionSupported(fileExt))
                    {
                        return DataResult.ResultError("File not support", "Error");
                    }
                    string filePath = System.IO.Path.GetRandomFileName() + fileExt;
                    FileStream stream = File.Create(filePath);
                    await file.CopyToAsync(stream, cancellationToken);
                    ExcelPackage package = new(stream);
                    List<Apartment> listApartmentImports = new();
                    try
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                        int rowCount = worksheet.Dimension.End.Row;
                        int colCount = worksheet.Dimension.End.Column;

                        // Format columns
                        const int NAME_INDEX = 1;
                        const int APARTMENT_CODE_INDEX = 2;
                        const int DESCRIPTION_INDEX = 3;
                        const int BUILDING_INDEX = 4;
                        const int URBAN_INDEX = 5;
                        const int BLOCK_INDEX = 6;
                        const int FLOOR_INDEX = 7;
                        const int APARTMENT_AREA_INDEX = 8;
                        const int STATUS_INDEX = 9;
                        const int CLASSIFY_APARTMENT_INDEX = 10;
                        const int ADDRESS_INDEX = 11;
                        const int BILL_CONFIG_CODE_INDEX = 12;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            string name = GetCellValueNotDefault<string>(worksheet, row, NAME_INDEX);
                            string apartmentCode = GetCellValueNotDefault<string>(worksheet, row, APARTMENT_CODE_INDEX);
                            string description = GetCellValue<string>(worksheet, row, DESCRIPTION_INDEX);
                            string buildingCode = GetCellValue<string>(worksheet, row, BUILDING_INDEX);
                            string urbanCode = GetCellValue<string>(worksheet, row, URBAN_INDEX);
                            decimal apartmentArea = GetCellValue<decimal>(worksheet, row, APARTMENT_AREA_INDEX);
                            string statusCode = GetCellValue<string>(worksheet, row, STATUS_INDEX);
                            string classifyApartmentCode = GetCellValue<string>(worksheet, row, CLASSIFY_APARTMENT_INDEX);
                            string blockCode = GetCellValue<string>(worksheet, row, BLOCK_INDEX);
                            string floorCode = GetCellValue<string>(worksheet, row, FLOOR_INDEX);
                            string address = GetCellValue<string>(worksheet, row, ADDRESS_INDEX);
                            string billConfigCode = GetCellValue<string>(worksheet, row, BILL_CONFIG_CODE_INDEX);

                            Apartment? apartment = await _apartmentRepository.FirstOrDefaultAsync(x => x.ApartmentCode == apartmentCode);
                            if (apartment != null) throw new UserFriendlyException($"Error at row {row}: ApartmentCode is existed.");

                            if (listApartmentImports.Select(x => x.ApartmentCode).Contains(apartmentCode))
                            {
                                throw new UserFriendlyException($"Error at row {row}: ApartmentCode cannot be duplicated");
                            }


                            List<GetAllBillConfigDto> billConfigsList = null;

                            if (!string.IsNullOrEmpty(billConfigCode))
                            {
                                billConfigsList = new List<GetAllBillConfigDto>();
                                var billConfigsElectric = _billConfigRepo.GetAll()
                                    .Where(x => (x.BillType == BillType.Electric) && billConfigCode.Contains(x.Code))
                                    .ToList();
                                if (billConfigsElectric.Any())
                                {
                                    var billConfig = new GetAllBillConfigDto
                                    {
                                        BillType = BillType.Electric,
                                        Properties = new BillProperites()
                                        {
                                            //customerName = customer,
                                            formulas = billConfigsElectric.Select(x => x.Id).ToArray(),
                                            formulaDetails = billConfigsElectric.ToArray()
                                        },
                                        // Properties = JsonConvert.DeserializeObject<BillProperites?>(config.Properties)
                                    };
                                    billConfigsList.Add(billConfig);
                                }
                                var billConfigsWater = _billConfigRepo.GetAll()
                                    .Where(x => (x.BillType == BillType.Water) && billConfigCode.Contains(x.Code))
                                    .ToList();
                                if (billConfigsWater.Any())
                                {
                                    var billConfig = new GetAllBillConfigDto
                                    {
                                        BillType = BillType.Water,
                                        Properties = new BillProperites()
                                        {
                                            formulas = billConfigsWater.Select(x => x.Id).ToArray(),
                                            formulaDetails = billConfigsWater.ToArray()
                                        },
                                    };
                                    billConfigsList.Add(billConfig);
                                }

                                
                            }

                            listApartmentImports.Add(new Apartment()
                            {
                                TenantId = AbpSession.TenantId,
                                Name = name,
                                ApartmentCode = apartmentCode,
                                Description = description,
                                BuildingId = _organizationUnit.FirstOrDefault(x => x.ProjectCode == buildingCode && x.ParentId != null && x.Type == 0)?.Id ?? 0,
                                UrbanId = _organizationUnit.FirstOrDefault(x => x.ProjectCode == urbanCode)?.Id ?? 0,
                                FloorId = _floorRepository.FirstOrDefault(x => x.Code == floorCode)?.Id ?? 0,
                                BlockId = _blockRepository.FirstOrDefault(x => x.Code == blockCode)?.Id ?? 0,
                                Area = apartmentArea,
                                StatusId = _statusRepository.FirstOrDefault(x => x.Code == statusCode)?.Id ?? 0,
                                TypeId = _typeRepository.FirstOrDefault(x => x.Code == classifyApartmentCode)?.Id ?? 0,
                                Address = address,
                                ProvinceCode = GetProvinceCode(urbanCode, buildingCode),
                                DistrictCode = GetDistrictCode(urbanCode, buildingCode),
                                WardCode = GetWardCode(urbanCode, buildingCode),
                                BillConfig = billConfigsList != null ? JsonConvert.SerializeObject(billConfigsList) : null
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
                        _apartmentRepository.Insert(apartment);
                    });
                    return DataResult.ResultSuccess(true, "Upload apartment success");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        #region method helper
        private string? GetFloorName(long? floorId)
        {
            return _floorRepository.FirstOrDefault(floorId ?? 0)?.DisplayName;
        }
        private string? GetStatusName(long? statusId)
        {
            return _statusRepository.FirstOrDefault(statusId ?? 0)?.Name;
        }
        private string? GetBlockName(long? blockId)
        {
            return _blockRepository.FirstOrDefault(blockId ?? 0)?.DisplayName;
        }
        private string? GetTypeName(long typeId)
        {
            return _typeRepository.FirstOrDefault(typeId)?.Name;
        }
        private List<MemberOfApartmentDto> GetMembers(string apartmentCode)
        {
            return _citizenTempRepository
                .GetAll()
                .Where(x => x.ApartmentCode == apartmentCode)
                .Select(x => new MemberOfApartmentDto
                {
                    Id = x.Id,
                    Generation = x.OwnerGeneration,
                    Name = x.FullName,
                    PhoneNumber = x.PhoneNumber,
                    Relationship = x.RelationShip,
                    IsStayed = x.IsStayed,
                })
                .ToList();
        }
        private IQueryable<CitizenTemp> GetOwner(string apartmentCode)
        {
            return _citizenTempRepository
                .GetAll()
                .Where(x => x.ApartmentCode == apartmentCode && x.IsStayed == true && x.RelationShip == RELATIONSHIP.Contractor)
                .OrderByDescending(x => x.OwnerGeneration);
        }
        private string GetOrganizationName(long? organizationId)
        {
            return _organizationUnit.GetAll().Where(x => x.Id == (organizationId ?? 0)).Select(x => x.DisplayName).FirstOrDefault();
        }
        private string GetProvinceCode(string? urbanCode, string? buildingCode)
        {
            return _organizationUnit.FirstOrDefault(x => x.ProjectCode == buildingCode)?.ProvinceCode
                ?? _organizationUnit.FirstOrDefault(x => x.ProjectCode == urbanCode)?.ProvinceCode
                ?? "";
        }
        private string GetDistrictCode(string? urbanCode, string? buildingCode)
        {
            return _organizationUnit.FirstOrDefault(x => x.ProjectCode == buildingCode)?.DistrictCode
                ?? _organizationUnit.FirstOrDefault(x => x.ProjectCode == urbanCode)?.DistrictCode
                ?? "";
        }
        private string GetWardCode(string? urbanCode, string? buildingCode)
        {
            return _organizationUnit.FirstOrDefault(x => x.ProjectCode == buildingCode)?.WardCode
                ?? _organizationUnit.FirstOrDefault(x => x.ProjectCode == urbanCode)?.WardCode
                ?? "";
        }

        public async Task<object> GetAllApartmentTenantPaging(GetAllApartmentInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    // var ouUI = await _userOrganizationUnitRepository.GetAll()
                    // .Where(x => x.UserId == AbpSession.UserId)
                    // .Select(x => x.OrganizationUnitId)
                    // .ToListAsync();
                    var query = (from sm in _apartmentRepository.GetAll()
                                 where (sm.ApartmentCode != null)
                                 select new ApartmentDto
                                 {
                                     Id = sm.Id,
                                     ApartmentCode = sm.ApartmentCode,
                                     Name = sm.Name,
                                     Citizens = (from citizen in _citizenTempRepository.GetAll()
                                                 where citizen.ApartmentCode.ToUpper() == sm.ApartmentCode.ToUpper()
                                                 select new CitizenRole
                                                 {
                                                     Name = citizen.FullName,
                                                     Type = citizen.Type,
                                                     RelationShip = citizen.RelationShip,
                                                     Nationality = citizen.Nationality,
                                                     Contact = citizen.PhoneNumber,
                                                     OwnerGeneration = citizen.OwnerGeneration > 0 ? citizen.OwnerGeneration : 0,
                                                     OwnerId = citizen.OwnerId
                                                 })
                                              .ToList(),
                                     BuildingId = sm.BuildingId,
                                     Area = sm.Area,
                                     UrbanId = sm.UrbanId,
                                 })
                             .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                             .WhereIf(input.UrbanId.HasValue, u => u.UrbanId == input.UrbanId)
                             .AsQueryable();

                    if (input.Keyword != null)
                    {
                        var citizen = (from cz in _citizenTempRepository.GetAll()
                                       where cz.ApartmentCode != null && cz.FullName != null
                                       select cz).AsQueryable();
                        var listKey = input.Keyword.Split('+');
                        if (listKey != null)
                        {
                            foreach (var key in listKey)
                            {


                                var citizenTemp = citizen.Where(x => x.FullName.Contains(key)).Select(x => x.ApartmentCode).Take(10).ToList();
                                query = query.Where(u =>
                                    (u.ApartmentCode.ToLower().Contains(key.ToLower()) || citizenTemp.Contains(u.ApartmentCode)));
                            }
                        }
                    }

                    query = query.OrderBy(x => x.ApartmentCode);

                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                    foreach (var apt in result)
                    {
                        var highestGen = apt.Citizens.Select(x => x.OwnerGeneration).Max() ?? 0;
                        apt.CurrentCitizenCount = apt.Citizens.Where(x => x.OwnerGeneration == highestGen).Count();
                    }

                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllApartmentTenant()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query = (from sm in _apartmentRepository.GetAll()
                                 where (sm.ApartmentCode != null)
                                 select new
                                 {
                                     Id = sm.Id,
                                     ApartmentCode = sm.ApartmentCode,
                                     Name = sm.Name,
                                     BuildingId = sm.BuildingId,
                                     Area = sm.Area,
                                     UrbanId = sm.UrbanId
                                 })
                             .AsQueryable();
                    query = query.OrderBy(x => x.ApartmentCode);

                    var result = await query.ToListAsync();

                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        #endregion
    }
}