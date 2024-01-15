using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.UI;
using Yootek.Authorization;
using Yootek.Authorization.Users;
using Yootek.BackgroundJob;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Smarthome.Device;
using Yootek.Notifications;
using Yootek.Services.Dto;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Yootek.Services
{
    public interface IUserSmartHomeAppService : IApplicationService
    {
        Task<object> GetSampleHouse();
        Task<object> CreateSmartHomeAsync(CreateSmartHomeInput input);
        Task<object> GetAllSmarthomeUser();
        Task<object> GetSmarthomeUserByCode(string code);
        Task<object> UpdateSmartHomeAsync(UpdateSmartHomeInput input);
        Task<object> GetSettingSmartHome(string code);
        Task<object> GetByIdSmartHomeAsync(long id);
        Task<object> CreateSmarthomeMember(CreateMemberInput input);
        Task<object> GetAllMembers(string code);
        Task<object> ChangeAdminSmarthome(MemberSmarthomeInput input);
        Task<object> DeleteSmarthome(string code);
        Task<object> DeleteMemberSmarthome(MemberSmarthomeInput input);

        #region Home gateway
        Task<object> GetAllHomeGatewayAsync(string code);
        Task<object> CreateHomeGatewayAsync(CreateHomeGatewayInput input);
        Task<object> UpdateHomeGatewayAsync(CreateHomeGatewayInput input);
        Task<object> DeleteHomeGatewayAsync(long id);
        #endregion

        #region Voice Control
        Task<object> GetHomeDevice(long id);
        Task<object> GetAllDeviceActions();
        #endregion 

    }

    //[AbpAuthorize(PermissionNames.Pages_User_Detail)]
    public class UserSmartHomeAppService : YootekAppServiceBase, IUserSmartHomeAppService
    {
        private readonly IRepository<HomeDevice, long> _homeDeviceRepos;
        private readonly IRepository<HomeGateway, long> _homeGatewayRepos;
        private readonly IRepository<SmartHome, long> _smartHomeRepos;
        private readonly IRepository<HomeMember, long> _homeMemberRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IRepository<SampleHouse, long> _sampleHouseRepos;
        private readonly UserManager _userManager;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly ISmarthomeCommunicator _smarthomeCommunicator;
        private readonly ITenantCache _tenantCache;

        public IBackgroundTaskQueue _queue { get; }
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UserSmartHomeAppService(
            IRepository<HomeDevice, long> homeDeviceRepos,
            IRepository<SmartHome, long> smartHomeRepos,
            IRepository<HomeGateway, long> homeGatewayRepos,
            IRepository<HomeMember, long> homeMemberRepos,
            IRepository<SampleHouse, long> sampleHouseRepos,
            IRepository<User, long> userRepos,
            UserManager userManager,
            IOnlineClientManager onlineClientManager,
            ISmarthomeCommunicator smarthomeCommunicator,
            ITenantCache tenantCache,
            IBackgroundTaskQueue queue,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            _sampleHouseRepos = sampleHouseRepos;
            _homeDeviceRepos = homeDeviceRepos;
            _smartHomeRepos = smartHomeRepos;
            _homeGatewayRepos = homeGatewayRepos;
            _userManager = userManager;
            _homeMemberRepos = homeMemberRepos;
            _onlineClientManager = onlineClientManager;
            _smarthomeCommunicator = smarthomeCommunicator;
            _tenantCache = tenantCache;
            _userRepos = userRepos;
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [AbpAllowAnonymous]
        public async Task<object> GetSampleHouse()
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    var result = await _sampleHouseRepos.FirstOrDefaultAsync(x => x.TenantId == null);
                    if (result != null)
                    {
                        var data = DataResult.ResultCode(result.Properties, "Get success", 200);
                        return data;
                    }
                    else
                    {
                        var data = DataResult.ResultCode(null, "Tenant have not sample house", 201);
                        return data;
                    }
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception");
                return data;

            }
        }

      
        public async Task<object> CreateSmartHomeAsync(CreateSmartHomeInput input)
        {

            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                var insertInput = new SmartHomeDto();
                insertInput.TenantId = AbpSession.TenantId;
                insertInput.Properties = input.Properties;
                insertInput.SmartHomeCode = GetUniqueKey();
                long id = await _smartHomeRepos.InsertAndGetIdAsync(insertInput);

                if (id > 0)
                {
                    insertInput.Id = id;
                    var member = new HomeMember()
                    {
                        SmartHomeCode = insertInput.SmartHomeCode,
                        IsAdmin = true,
                        UserId = AbpSession.UserId,
                        TenantId = insertInput.TenantId
                    };

                    await _homeMemberRepos.InsertAsync(member);
                }

                if (!input.Properties.IsNullOrEmpty())
                {
                    _queue.QueueBackgroundWorkItem(async token =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;

                            try
                            {
                                SmarthomeMobileDto home = JsonSerializer.Deserialize<SmarthomeMobileDto>(input.Properties);
                                if (home != null && home.physical_device_config != null && home.physical_device_config.Count > 0 && home.visual_device_config != null && home.visual_device_config.Count > 0)
                                {
                                    foreach (var vsdv in home.visual_device_config)
                                    {
                                        var physdevs = home.physical_device_config.FirstOrDefault(x => x.key == vsdv._id);
                                        if (physdevs != null)
                                        {
                                            var device = new HomeDevice()
                                            {
                                                Name = vsdv.dev_name,
                                                VisualDeviceId = vsdv._id,
                                                SmartHomeCode = insertInput.SmartHomeCode,
                                                EquipmentCompany = physdevs.type_device,
                                                TypeDevice = vsdv.dev_type,
                                                Properties = JsonSerializer.Serialize(physdevs),
                                                DevId = physdevs.property.dev_id,
                                                DevIP = physdevs.property.ipGateway
                                            };
                                            await _homeDeviceRepos.InsertAsync(device);
                                        }
                                    }

                                }
                            }
                            catch (Exception)
                            {

                            }
                            await Task.Delay(TimeSpan.FromSeconds(5), token);

                        }
                    });

                }

                var result = new UpdateSmartHomeInput()
                {
                    Properties = insertInput.Properties,
                    SmartHomeCode = insertInput.SmartHomeCode
                };
                mb.statisticMetris(t1, 0, "is_smh");
                var data = DataResult.ResultSuccess(result, "Insert success !");
                return data;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetSmarthomeUserByCode(string code)
        {
            try
            {
                var result = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == code);


                if (result != null)
                {
                    var dt = new
                    {
                        SmarthomeCode = result.SmartHomeCode,
                        Properties = result.Properties
                    };

                    var data = DataResult.ResultCode(dt, "Get success", 200);
                    return data;
                }
                else
                {
                    var data = DataResult.ResultCode(null, "Smarthome null", 201);
                    return data;
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception");
                return data;

            }
        }

        public async Task<object> GetAllSmarthomeUser()
        {
            try
            {
                var listSMH = await (from h in _homeMemberRepos.GetAll()
                                     join sm in _smartHomeRepos.GetAll() on h.SmartHomeCode equals sm.SmartHomeCode
                                     where h.UserId == AbpSession.UserId
                                     select h.SmartHomeCode)
                            .ToListAsync();

                var result = new List<object>();

                if (listSMH != null)
                {
                    foreach (var code in listSMH)
                    {
                        var smh = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == code);

                        if (smh != null)
                        {
                            var dt = new
                            {
                                SmarthomeCode = smh.SmartHomeCode,
                                Properties = smh.Properties
                            };
                            result.Add(dt);
                        }
                    }
                }

                var data = DataResult.ResultCode(result, "Get success", 200);
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception");
                return data;

            }
        }

        public async Task<object> GetAllSmarthomeCodeUser()
        {
            try
            {
                var listSMH = await (from h in _smartHomeRepos.GetAll()

                                     where h.CreatorUserId == AbpSession.UserId
                                     select h.SmartHomeCode)
                            .ToListAsync();


                //var result = new List<object>();

                //if(listSMH != null)
                //{
                //    foreach(var code in listSMH)
                //    {
                //        var smh = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == code);

                //        if(smh != null)
                //        {
                //            var dt = new
                //            {
                //                SmarthomeCode = smh.SmartHomeCode,
                //                Properties = smh.Properties
                //            };
                //            result.Add(dt);
                //        }
                //    }
                //}

                var data = DataResult.ResultCode(listSMH, "Get success", 200);
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception");
                return data;

            }
        }

      
        public async Task<object> UpdateSmartHomeAsync(UpdateSmartHomeInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var updateData = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == input.SmartHomeCode);

                if (updateData != null)
                {
                    var oldProp = updateData.Properties;
                    updateData.Properties = input.Properties;
                    updateData.PropertiesHistory = oldProp;
                    await _smartHomeRepos.UpdateAsync(updateData);

                }

                if (!input.Properties.IsNullOrEmpty())
                {
                    _queue.QueueBackgroundWorkItem(async token =>
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;

                            try
                            {
                                SmarthomeMobileDto home = JsonSerializer.Deserialize<SmarthomeMobileDto>(input.Properties);
                                if (home != null && home.physical_device_config != null && home.physical_device_config.Count > 0 && home.visual_device_config != null && home.visual_device_config.Count > 0)
                                {
                                    await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                                    {
                                        List<HomeDevice> currentDevices = await _homeDeviceRepos.GetAllListAsync(x => x.SmartHomeCode == input.SmartHomeCode);
                                        if (currentDevices != null)
                                        {
                                            foreach (var crdv in currentDevices)
                                            {
                                                crdv.IsDeleted = true;
                                            }
                                        }

                                        foreach (var vsdv in home.visual_device_config)
                                        {
                                            var physdevs = home.physical_device_config.FirstOrDefault(x => x.key == vsdv._id);
                                            if (physdevs != null)
                                            {
                                                var homeDv = currentDevices.FirstOrDefault(x => x.VisualDeviceId == vsdv._id);
                                                if (homeDv != null)
                                                {
                                                    homeDv.Name = vsdv.dev_name;
                                                    homeDv.VisualDeviceId = vsdv._id;
                                                    homeDv.SmartHomeCode = input.SmartHomeCode;
                                                    homeDv.EquipmentCompany = physdevs.type_device;
                                                    homeDv.TypeDevice = vsdv.dev_type;
                                                    homeDv.Properties = JsonSerializer.Serialize(physdevs);
                                                    homeDv.IsDeleted = false;
                                                }
                                                else
                                                {
                                                    var device = new HomeDevice()
                                                    {
                                                        Name = vsdv.dev_name,
                                                        VisualDeviceId = vsdv._id,
                                                        SmartHomeCode = input.SmartHomeCode,
                                                        DevId = physdevs.property.dev_id,
                                                        EquipmentCompany = physdevs.type_device,
                                                        TypeDevice = vsdv.dev_type,
                                                        Properties = JsonSerializer.Serialize(physdevs),
                                                        DevIP = physdevs.property.ipGateway
                                                    };
                                                    await _homeDeviceRepos.InsertAsync(device);
                                                }

                                            }
                                        }

                                        await UnitOfWorkManager.Current.SaveChangesAsync();
                                    });


                                }
                            }
                            catch (Exception)
                            {

                            }
                            await Task.Delay(TimeSpan.FromSeconds(5), token);

                        }
                    });

                }


                var members = await _homeMemberRepos.GetAllListAsync(x => (x.TenantId == AbpSession.TenantId) && (x.SmartHomeCode == input.SmartHomeCode) && (x.UserId != AbpSession.UserId));
                if (members.Count > 0)
                {
                    var clients = new List<IOnlineClient>();
                    foreach (var mb in members)
                    {
                        var user = new UserIdentifier(mb.TenantId, mb.UserId.Value);
                        var client = await _onlineClientManager.GetAllByUserIdAsync(user);
                        clients.AddRange(client);

                    }
                    _smarthomeCommunicator.NotifyUpdateSmarthomeToClient(clients, input.SmartHomeCode);

                }
                mb.statisticMetris(t1, 0, "mb_up_smh");
                var data = DataResult.ResultSuccess(input, "Update success !");
                return data;


            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "exception !");
                Logger.Fatal(e.Message, e);
                return data;
            }
        }




      
        public async Task<object> GetByIdSmartHomeAsync(long id)
        {
            try
            {

                var smarthome = await _smartHomeRepos.FirstOrDefaultAsync(x => x.Id == id);
                var result = new UpdateSmartHomeInput()
                {
                    Properties = smarthome.Properties,
                    SmartHomeCode = smarthome.SmartHomeCode
                };

                var data = DataResult.ResultSuccess(result, "Get success");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                return data;

            }
        }

      
        public async Task<object> GetSettingSmartHome(string code)
        {
            try
            {

                var smarthome = await _smartHomeRepos.FirstOrDefaultAsync(x => x.TenantId == AbpSession.TenantId && (code != null && x.SmartHomeCode == code));

                if (smarthome != null)
                {
                    var result = new UpdateSmartHomeInput()
                    {
                        Properties = smarthome.Properties,
                        SmartHomeCode = smarthome.SmartHomeCode
                    };
                    var data = DataResult.ResultCode(result, "Get success", 200);
                    return data;
                }
                else
                {
                    var data = DataResult.ResultCode(null, "Smarthome don't exist !", 415);
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception");
                return data;

            }
        }

        public async Task<object> CreateSmarthomeMember(CreateMemberInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                //var isAdmin = AbpSession.UserId;
                var user = await _userManager.FindByNameOrEmailAsync(input.UserNameOrEmail);
                if (user != null)
                {
                    var member = new HomeMember()
                    {
                        SmartHomeCode = input.SmarthomeCode,
                        UserId = user.Id
                    };
                    await _homeMemberRepos.InsertAsync(member);
                    mb.statisticMetris(t1, 0, "is_member");
                    var data = DataResult.ResultCode(user, "Add member success !", 200);
                    return data;
                }
                else
                {
                    mb.statisticMetris(t1, 0, "is_member");
                    var data = DataResult.ResultCode(null, "User or email don't exist !", 415);
                    return data;
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultCode(e.ToString(), "Exception !", 500);
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllMembers(string code)
        {
            try
            {

                var query = (from mb in _homeMemberRepos.GetAll()
                             join us in _userRepos.GetAll() on mb.UserId equals us.Id
                             where mb.SmartHomeCode == code
                             select new UserSmarthome()
                             {
                                 UserId = us.Id,
                                 FullName = us.FullName,
                                 ImageUrl = us.ImageUrl,
                                 IsAdmin = mb.IsAdmin,
                                 UserName = us.UserName
                             }).AsQueryable();

                var result = await query.ToListAsync();

                var data = DataResult.ResultSuccess(result, "Get success");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                return data;

            }
        }


        public async Task<object> ChangeAdminSmarthome(MemberSmarthomeInput input)
        {
            try
            {

                var admin = await _homeMemberRepos.GetAsync(AbpSession.UserId.Value);

                if (admin != null && admin.IsAdmin)
                {
                    var newAdmin = await _homeMemberRepos.GetAsync(input.UserId);
                    if (newAdmin != null)
                    {
                        newAdmin.IsAdmin = true;
                        admin.IsAdmin = false;
                        await _homeMemberRepos.UpdateAsync(newAdmin);
                        await _homeMemberRepos.UpdateAsync(admin);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                var data = DataResult.ResultSuccess("Update success !");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                return data;

            }
        }

        public async Task<object> DeleteSmarthome(string code)
        {
            try
            {
                var smarthome = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == code);
                var member = await _homeMemberRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == code && x.UserId == AbpSession.UserId);
                if (smarthome != null && member != null)
                {
                    if (member.IsAdmin)
                    {
                        await _smartHomeRepos.DeleteAsync(smarthome);
                        await _homeMemberRepos.DeleteAsync(member);
                    }
                    else
                    {
                        await _homeMemberRepos.DeleteAsync(member);
                    }

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
        }
        public async Task<object> DeleteMemberSmarthome(MemberSmarthomeInput input)
        {
            try
            {
                var member = await _homeMemberRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == input.SmarthomeCode && x.UserId == input.UserId);
                var admin = await _homeMemberRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == input.SmarthomeCode && x.UserId == AbpSession.UserId);
                if (member != null && admin.IsAdmin)
                {
                    await _homeMemberRepos.DeleteAsync(member);
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
        }

        #region Voice Controll

        public async Task<object> GetHomeDevice(long id)
        {
            try
            {
                var result = await _homeDeviceRepos.GetAsync(id);
                var data = DataResult.ResultCode(result, "Get success", 200);
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllDeviceActions()
        {
            try
            {
                var result = await _homeDeviceRepos.GetAllListAsync(x => x.CreatorUserId == AbpSession.UserId);
                var data = DataResult.ResultCode(result, "Get success", 200);
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message);
                throw;
            }
        }



        #endregion

        #region Home Gateway
        public async Task<object> GetAllHomeGatewayAsync(string code)
        {
            try
            {
                var result = await _homeGatewayRepos.GetAllListAsync(x => x.SmarthomeCode == code);

                var data = DataResult.ResultCode(result, "Get success", 200);
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreateHomeGatewayAsync(CreateHomeGatewayInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                //var isAdmin = AbpSession.UserId;
                var smh = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == input.SmarthomeCode);

                if (smh != null)
                {
                    var gtw = new HomeGateway()
                    {
                        IpAddress = input.IpAddress,
                        Properties = input.Properties,
                        SmarthomeCode = input.SmarthomeCode,
                        TenantId = AbpSession.TenantId
                    };
                    gtw.Id = await _homeGatewayRepos.InsertAndGetIdAsync(gtw);
                    mb.statisticMetris(t1, 0, "is_member");
                    var data = DataResult.ResultCode(gtw, "Create gateway success !", 200);
                    return data;
                }
                else
                {
                    mb.statisticMetris(t1, 0, "is_member");
                    var data = DataResult.ResultCode(null, "Smarthome is null", 201);
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

        public async Task<object> UpdateHomeGatewayAsync(CreateHomeGatewayInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                //var isAdmin = AbpSession.UserId;
                var smh = await _smartHomeRepos.FirstOrDefaultAsync(x => x.SmartHomeCode == input.SmarthomeCode);
                var gtw = await _homeGatewayRepos.FirstOrDefaultAsync(x => x.SmarthomeCode == input.SmarthomeCode);
                if (smh != null && gtw != null)
                {
                    gtw.Properties = input.Properties;
                    gtw.IpAddress = input.IpAddress;
                    await _homeGatewayRepos.UpdateAsync(gtw);
                    mb.statisticMetris(t1, 0, "ud_gateway");
                    var data = DataResult.ResultCode(gtw, "Create gateway success !", 200);
                    return data;
                }
                else
                {
                    mb.statisticMetris(t1, 0, "ud_gateway");
                    var data = DataResult.ResultCode(null, "Smarthome is null", 201);
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

        public async Task<object> DeleteHomeGatewayAsync(long id)
        {
            try
            {
                var gtw = await _homeGatewayRepos.GetAsync(id);
                if (gtw != null)
                {
                    await _homeGatewayRepos.DeleteAsync(gtw);

                    var data = DataResult.ResultSuccess("Xóa thành công !");
                    return data;
                }
                else
                {
                    var data = DataResult.ResultFail("Gateway không tồn tại !");
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

        #endregion
    }
}