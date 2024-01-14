using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.UI;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Organizations;
using Yootek.Services.Dto;
using Yootek.Users.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface ISmartHomeAppService : IApplicationService
    {
        //Task<object> CreateOrUpdateSmartHomeAsync(SmartHomeDto input);
        Task<object> CreateSmartHome(CreateSmartHomeInput input);
        Task<object> UpdateSmartHome(UpdateSmartHomeInput input);

        Task<object> GetListUserTenant();
        Task<object> GetAllSmartHome();
        Task<object> GetByIdSmartHomeAsync(long id);
        Task<object> GetSettingSmartHomeAsync(string code);
        Task<object> GetBackupSettingSmartHomeAsync(string code);
        Task<object> SearchSmartHomeAsync(DataSearch text);
        Task<object> DeleteSmartHome(long id);

    }


    //[AbpAuthorize(PermissionNames.Pages_Users)]
    [AbpAuthorize]
    public class SmartHomeAppService : YootekAppServiceBase, ISmartHomeAppService
    {
        private readonly IRepository<HomeDevice, long> _homeDeviceRepos;
        private readonly IRepository<Device, long> _deviceRepos;
        private readonly IRepository<SampleHouse, long> _sampleHouseRepos;
        private readonly IRepository<SmartHome, long> _smartHomeRepos;
        private readonly IRepository<HomeMember, long> _homeMemberRepos;
        private readonly UserManager _userManager;
        private readonly UserStore _store;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationRepos;
        private readonly IRepository<AppOrganizationUnit, long> _appOrganizationUnitRepository;
        private readonly IRepository<Apartment, long> _apartmentRepository;


        public SmartHomeAppService(
            UserStore store,
            IRepository<HomeDevice, long> homeDeviceRepos,
            IRepository<Device, long> deviceRepos,
            IRepository<SmartHome, long> smartHomeRepos,
            IRepository<SampleHouse, long> sampleHouseRepos,
            IRepository<HomeMember, long> homeMemberRepos,
            UserManager userManager,
            IRepository<UserOrganizationUnit, long> userOrganizationRepos,
            IRepository<AppOrganizationUnit, long> appOrganizationUnitRepository,
            IRepository<Apartment, long> apartmentRepository
        )
        {
            _homeDeviceRepos = homeDeviceRepos;
            _deviceRepos = deviceRepos;
            _smartHomeRepos = smartHomeRepos;
            _sampleHouseRepos = sampleHouseRepos;
            _homeMemberRepos = homeMemberRepos;
            _userManager = userManager;
            _store = store;
            _userOrganizationRepos = userOrganizationRepos;
            _appOrganizationUnitRepository = appOrganizationUnitRepository;
            _apartmentRepository = apartmentRepository;
        }

        #region SmartHome

        public async Task<object> CreateOrUpdateSmartHomeAsync(SmartHomeDto input)
        {

            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _smartHomeRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        if (updateData != null)
                        {
                            var oldProp = updateData.Properties;
                            input.MapTo(updateData);
                            updateData.PropertiesHistory = oldProp;

                            //call back
                            await _smartHomeRepos.UpdateAsync(updateData);
                        }
                    }
                    mb.statisticMetris(t1, 0, "Ud_smh");
                    var data = DataResult.ResultSuccess(updateData, "Insert success !");
                    return data;
                }
                else
                {
                    //check tồn tại nhà
                    var sm = await _smartHomeRepos.FirstOrDefaultAsync(x => x.Name.Trim() == input.Name.Trim());
                    if (sm != null)
                    {
                        var data1 = DataResult.ResultError(null, "Đã tồn tại smarthome");
                        return data1;
                    }

                    //Insert
                    var insertInput = input.MapTo<SmartHome>();
                    long id = await _smartHomeRepos.InsertAndGetIdAsync(insertInput);
                    if (id > 0)
                    {
                        insertInput.SmartHomeCode = GetUniqueKey();
                        insertInput.Id = id;
                    }
                    mb.statisticMetris(t1, 0, "is_smh");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreateSmartHome(CreateSmartHomeInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var insertInput = new SmartHomeDto();
                insertInput.TenantId = AbpSession.TenantId;
                insertInput.Properties = input.Properties;
                insertInput.SmartHomeCode = GetUniqueKey();
                insertInput.CreatorUserId = input.UserId;
                insertInput.ApartmentCode = input.ApartmentCode;
                insertInput.HouseDetail = input.HouseDetail;
                if (!input.Properties.IsNullOrEmpty())
                {
                    dynamic home = JObject.Parse(input.Properties);
                    try
                    {
                        insertInput.ImageUrl = home.home_infor.img;
                        home.home_infor.home_code = insertInput.SmartHomeCode;
                    }
                    catch (RuntimeBinderException er)
                    {
                        var dt = DataResult.ResultCode(er.Message, "Format properties error !", 415);
                        return dt;
                    }
                    insertInput.Properties = Convert.ToString(home);
                }

                long id = await _smartHomeRepos.InsertAndGetIdAsync(insertInput);
                if (input.UserId > 0 && id > 0)
                {
                    insertInput.Id = id;
                    var member = new HomeMember()
                    {
                        SmartHomeCode = insertInput.SmartHomeCode,
                        IsAdmin = true,
                        UserId = input.UserId,
                        TenantId = insertInput.TenantId
                    };

                    await _homeMemberRepos.InsertAsync(member);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                if (id > 0)
                {
                    insertInput.Id = id;
                }

                mb.statisticMetris(t1, 0, "is_smh");
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                return data;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateSmartHome(UpdateSmartHomeInput input)
        {
            try
            {
                //update
                var updateData = await _smartHomeRepos.FirstOrDefaultAsync(x => (x.TenantId == AbpSession.TenantId) && (x.SmartHomeCode == input.SmartHomeCode));

                if (updateData != null)
                {
                    var oldProp = updateData.Properties;
                    updateData.Properties = input.Properties;
                    updateData.PropertiesHistory = oldProp;
                    updateData.HouseDetail = input.HouseDetail;
                    updateData.ApartmentCode = input.ApartmentCode;
                    updateData.Name = input.Name;
                    await _smartHomeRepos.UpdateAsync(updateData);
                }
                return 0;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> GetAllSmartHome()
        {
            try
            {
                var result = await _smartHomeRepos.GetAllListAsync();

                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                return data;
            }
        }


        //[Obsolete]
        //public IEnumerable<TypeDeviceInput> GetAllTypeDevice(RoomUpdate room, string type) 
        //{

        //    var lightDevices = (from device in _homeDeviceRepos.GetAll()
        //                        where device.RoomId == room.Id && device.GroupDevice == type || (AbpSession.TenantId != null && device.TenantId == AbpSession.TenantId)
        //                        select new TypeDeviceInput()
        //                        {
        //                            Id = device.Id,
        //                            DeviceType = type,
        //                            DeviceHomeId = device.HomeDeviceId,
        //                            TokenIds = device.DeviceSettingId,
        //                            DeviceNumber = device.DeviceNumber.Value,
        //                            Depscription = device.HomeDeviceAddress,
        //                            DeviceGateway = (from gate in _homeGatewayRepos.GetAll()
        //                                             where gate.Id == device.HomeServerId
        //                                             select gate).FirstOrDefault()
        //                        }).AsQueryable();

        //    return lightDevices.ToList();
        //}


        public async Task<object> GetListUserTenant()
        {
            try
            {
                var users = await _store.GetAllUserTenantAsync();
                var result = ObjectMapper.Map<List<UserDto>>(users);


                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                return data;
            }
        }

        public async Task<object> GetByIdSmartHomeAsync(long id)
        {
            try
            {
                var smarthome = await _smartHomeRepos.FirstOrDefaultAsync(x => x.Id == id);
                //var rooms = _roomSmarHomeRepos.GetAllList(room => room.SmartHomeId == smarthome.Id);

                //setting.Rooms = rooms.MapTo<List<RoomUpdate>>();

                //if (setting.Rooms != null)
                //{
                //    setting.Rooms.ForEach(room => {
                //        room.LightingIds = GetAllTypeDevice(room, AppConsts.LightingDevice).MapTo<List<TypeLightingDevice>>();
                //        room.CurtainIds = GetAllTypeDevice(room, AppConsts.CurtainDevice).MapTo<List<TypeCurtainDevice>>();
                //        room.AirIds = GetAllTypeDevice(room, AppConsts.AirDevice).MapTo<List<TypeAirDevice>>();
                //        room.ConditionerIds = GetAllTypeDevice(room, AppConsts.ConditionerDevice).MapTo<List<TypeConditionerDevice>>();
                //        room.ConnectionIds = GetAllTypeDevice(room, AppConsts.ConnectionDevice).MapTo<List<TypeConnectionDevice>>();
                //        room.DoorEntryIds = GetAllTypeDevice(room, AppConsts.DoorEntryDevice).MapTo<List<TypeDoorEntryDevice>>();
                //        room.FireAlarmIds = GetAllTypeDevice(room, AppConsts.FireAlarmDevice).MapTo<List<TypeFireAlarmDevice>>();
                //        room.WatterIds = GetAllTypeDevice(room, AppConsts.WaterDevice).MapTo<List<TypeWaterDevice>>();
                //        room.SecurityIds = GetAllTypeDevice(room, AppConsts.SecurityDevice).MapTo<List<TypeSecurityDevice>>();
                //    });
                //};
                var data = DataResult.ResultSuccess(smarthome, "Get success");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                return data;

            }
        }

        public async Task<object> GetSettingSmartHomeAsync(string code)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                var setting = new SmartHomeSettingOutput();
                var smarthome = await _smartHomeRepos.FirstOrDefaultAsync(x => (x.TenantId == AbpSession.TenantId) && (x.SmartHomeCode == code));
                setting.SmartHomecode = smarthome.SmartHomeCode;
                setting.Properties = smarthome.Properties;
                //var homeserver = _homeGatewayRepos.FirstOrDefault(x => ( x.TenantId == AbpSession.TenantId) && (x.SmartHomeId == smarthome.Id));

                //var rooms = _roomSmarHomeRepos.GetAllList(room =>  room.TenantId == AbpSession.TenantId && room.SmartHomeId == smarthome.Id);
                //setting.Rooms = rooms.MapTo<List<RoomUpdate>>();

                //if (rooms != null)
                //{
                //    setting.Rooms.ForEach(room =>
                //    {
                //        var homedevices = _homeDeviceRepos.GetAllList(d => d.RoomId == room.Id);
                //        room.Devices = homedevices.MapTo<List<HomeDeviceDto>>();

                //        room.Devices.ForEach(device =>
                //        {
                //            var apis = _smarthomeApiRepos.GetAllList(a => a.DeviceSmarthomeId == device.Id);
                //            device.ListApis = apis;
                //        });
                //    });
                //}
                stopwatch.Stop();
                var timecount = stopwatch.ElapsedMilliseconds;
                Logger.Info(timecount.ToString());
                var data = DataResult.ResultSuccess(setting, "Get success");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;

            }
        }

        public async Task<object> GetBackupSettingSmartHomeAsync(string code)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                var setting = new SmartHomeSettingOutput();
                var smarthome = await _smartHomeRepos.FirstOrDefaultAsync(x => (x.TenantId == AbpSession.TenantId) && (x.SmartHomeCode == code));
                setting.SmartHomecode = smarthome.SmartHomeCode;
                setting.Properties = smarthome.PropertiesHistory;
                stopwatch.Stop();
                var timecount = stopwatch.ElapsedMilliseconds;
                Logger.Info(timecount.ToString());
                var data = DataResult.ResultSuccess(setting, "Get success");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;

            }
        }


        public async Task<object> SearchSmartHomeAsync(DataSearch text)
        {
            try
            {
                var result = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == text.data);

                //var deviceApi = await _smarthomeApiRepos.GetAllListAsync(x => x.SmarthomeId == result.Id);

                var data = new
                {
                    smarthome = result
                    //api = deviceApi
                };
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;

            }
        }

        /*public async Task<object> DeleteSmartHome(long id)
        {
            try
            {
                var smarthome = await _smartHomeRepos.GetAsync(id);
                if (smarthome != null)
                {
                    await _smartHomeRepos.DeleteAsync(smarthome);
                    var data = DataResult.ResultSuccess("Xóa thành công !");
                    return data;
                }
                else
                {
                    var data = DataResult.ResultFail("Smarthome không tồn tại !");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }*/
        public async Task<object> DeleteSmartHome(long id)
        {
            try
            {

                await _smartHomeRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public object DeleteMultipleSmartHomes([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = DeleteSmartHome(id);
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
                throw;
            }
        }


        public async Task<object> CreateListAparment(List<SmartHomeDto> listApartments)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (listApartments != null)
                {
                    foreach (var obj in listApartments)
                    {
                        if (!obj.ApartmentCode.IsNullOrWhiteSpace())
                        {
                            var homeInput = new SmartHomeDto();
                            homeInput.TenantId = AbpSession.TenantId;
                            homeInput.CreatorUserId = AbpSession.UserId;
                            homeInput.ApartmentCode = obj.ApartmentCode;
                            homeInput.Name = !string.IsNullOrWhiteSpace(obj.Name) ? obj.Name : obj.ApartmentCode;
                            homeInput.UrbanCode = obj.UrbanCode;
                            homeInput.BuildingCode = obj.BuildingCode;
                            homeInput.ApartmentAreas = obj.ApartmentAreas;
                            homeInput.OrganizationUnitId = obj.OrganizationUnitId;
                            homeInput.SmartHomeCode = GetUniqueKey();

                            await _smartHomeRepos.InsertAndGetIdAsync(homeInput);
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }


                mb.statisticMetris(t1, 0, "admin_islist_obj");
                var data = DataResult.ResultSuccess("Insert success !");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> ImportSmartHomesExcel([FromForm] ImportExcelSmartHome input)
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
                    }

                    var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.End.Row;

                    const int APARTMENT_CODE_INDEX = 1;
                    const int APARTMENT_AREAS_INDEX = 2;
                    const int URBAN_CODE_INDEX = 3;
                    const int BUILDING_CODE_INDEX = 4;

                    var listApts = new List<ApartmentImportExcelDto>();
                    var listDupl = new List<ApartmentImportExcelDto>();

                    for (var row = 2; row <= rowCount; row++)
                    {
                        string apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Text.Trim();
                        decimal? apartmentArea = worksheet.Cells[row, APARTMENT_AREAS_INDEX].GetValue<decimal?>();
                        string buildingCode = worksheet.Cells[row, BUILDING_CODE_INDEX].Text?.Trim();
                        string urbanCode = worksheet.Cells[row, URBAN_CODE_INDEX].Text.Trim();

                        if (string.IsNullOrWhiteSpace(apartmentCode) || string.IsNullOrWhiteSpace(urbanCode))
                        {
                            throw new Exception($"Error at row {row}: ApartmentCode and UrbanCode are required.");
                        }

                        var apartment = new ApartmentImportExcelDto()
                        {
                            TenantId = AbpSession.TenantId,
                            CreatorUserId = AbpSession.UserId,
                            ApartmentCode = apartmentCode,
                            Area = apartmentArea
                        };

                        var listBuilding = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
                        var listUrban = _appOrganizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);

                        if (!string.IsNullOrEmpty(buildingCode))
                        {
                            var building = listBuilding.FirstOrDefault(x => x.ProjectCode == buildingCode);
                            if (building != null) apartment.BuildingId = building.ParentId;
                        }

                        if (!string.IsNullOrEmpty(urbanCode))
                        {
                            var urban = listUrban.FirstOrDefault(x => x.ProjectCode == urbanCode);
                            if (urban != null) apartment.UrbanId = urban.ParentId;
                        }

                        var existingApartment = await _apartmentRepository.FirstOrDefaultAsync(x =>
                            x.ApartmentCode == apartmentCode && x.BuildingId == apartment.BuildingId && x.UrbanId == apartment.UrbanId);

                        if (existingApartment != null)
                        {
                            apartment.Id = existingApartment.Id;
                            listDupl.Add(apartment);
                        }
                        else
                        {
                            listApts.Add(apartment);
                        }
                    }

                    await CreateListApartment(listApts);

                    // Xóa tệp đã tạo
                    File.Delete(filePath);

                    var res = new
                    {
                        newEntry = listApts,
                        duplicates = listDupl
                    };

                    return DataResult.ResultSuccess(res, "Upload success");
                }
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        public async Task<object> ImportSmartHomesToExcel([FromForm] IFormFile input)
        {
            try
            {
                var file = input;

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

                //
                const int APARTMENT_CODE_INDEX = 1;
                const int URBAN_CODE_INDEX = 2;
                const int BUILDING_CODE_INDEX = 3;
                const int APARTMENT_SIZE_INDEX = 4;

                var listApts = new List<ApartmentImportExcelDto>();
                var listDupl = new List<ApartmentImportExcelDto>();
                for (var row = 1; row <= rowCount; row++)
                {
                    var apartment = new ApartmentImportExcelDto()
                    {
                        Area = (int)worksheet.Cells[row, APARTMENT_SIZE_INDEX].Value,
                        ApartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Value.ToString(),
                        TenantId = AbpSession.TenantId,
                    };
                    if (await _smartHomeRepos.FirstOrDefaultAsync(x => x.ApartmentCode == apartment.ApartmentCode) == null) listDupl.Add(apartment);
                    else listApts.Add(apartment);
                }

                await CreateListApartment(listApts);

                await stream.DisposeAsync();
                stream.Close();
                File.Delete(filePath);

                var res = new
                {
                    newEntry = listApts,
                    dulicates = listDupl
                };

                return DataResult.ResultSuccess(res, "Upload success");
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.Message, "Error");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }
        private async Task CreateListApartment(List<ApartmentImportExcelDto> input)
        {
            try
            {
                if (input.Count == 0 || input == null) return;
                foreach (var apt in input)
                {
                    await _apartmentRepository.InsertAsync(apt);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion



        #region Voice Controll



        #endregion

    }
}
