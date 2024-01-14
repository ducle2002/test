using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Yootek.App.ServiceHttpClient;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Meter.dto;
using Yootek.Organizations;
using Newtonsoft.Json;

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
    }

    public class AdminMeterAppService : YootekAppServiceBase, IAdminMeterAppService
    {
        private readonly IRepository<Meter, long> _meterRepository;
        private readonly IRepository<MeterType, long> _meterTypeRepository;
        private readonly IHttpQRCodeService _httpQRCodeService;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        public AdminMeterAppService(
            IRepository<Meter, long> meterRepository,
            IRepository<MeterType, long> meterTypeRepository,
            IHttpQRCodeService httpQrCodeService, 
            IRepository<AppOrganizationUnit, long> organizationUnitRepository)
        {
            _meterRepository = meterRepository;
            _meterTypeRepository = meterTypeRepository;
            _httpQRCodeService = httpQrCodeService;
            _organizationUnitRepository = organizationUnitRepository;
        }


        public async Task<DataResult> GetAllMeterAsync(GetAllMeterDto input)
        {
            try
            {
                var tenantId = AbpSession.TenantId;
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
                            BuildingName = _organizationUnitRepository.GetAll().Where(x => x.Id == sm.BuildingId ).Select(x => x.DisplayName).FirstOrDefault(),
                            UrbanName = _organizationUnitRepository.GetAll().Where(x => x.Id == sm.UrbanId ).Select(x => x.DisplayName).FirstOrDefault(),
                        })
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
                    throw new UserFriendlyException(409, "Căn hộ đã được thêm đồng hồ");
                }
                
                var meter = ObjectMapper.Map<Meter>(input);

                meter.TenantId = AbpSession.TenantId;

                var data = await _meterRepository.InsertAsync(meter);
                await CurrentUnitOfWork.SaveChangesAsync();
                data.QrCode = QRCodeGenerator(data.Id, QRCodeActionType.Meter);

                var createQrCodeResult = await _httpQRCodeService.CreateQRObject(new CreateQRObjectDto()
                {
                    Name = $"QR/Meter/{AbpSession.TenantId}/{data.Id}",
                    ActionType = QRCodeActionType.Meter,
                    Code = data.QrCode,
                    Data = JsonConvert.SerializeObject(new
                    {
                        meterId = data.Id,
                    }),
                    Status = QRCodeStatus.Active,
                    Type = QRCodeType.Text
                });

                if (!createQrCodeResult.Success)
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
    }
}