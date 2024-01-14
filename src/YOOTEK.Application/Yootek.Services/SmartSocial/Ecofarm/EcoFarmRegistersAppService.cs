using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Yootek.Services.SmartSocial.Ecofarm
{
    public interface IEcoFarmRegistersAppService : IApplicationService
    {
        Task<object> GetListByPartner([FromQuery] GetAllEcofarmRegistersDto input);
        Task<object> GetListByUser([FromQuery] GetAllEcofarmRegistersDto input);
        Task<object> GetById([FromQuery] GetItemEcofarmByIdDto input);
        Task<object> Create([FromBody] CreateEcofarmRegisterDto input);
        Task<object> Update([FromBody] UpdateEcofarmRegisterDto input);
        Task<object> UpdateStatus([FromBody] UpdateStatusEcofarmRegisterDto input);
        Task<object> Delete([FromQuery] DeleteEcofarmRegisterDto input);
    }

    public class EcoFarmRegistersAppService : YootekAppServiceBase, IEcoFarmRegistersAppService
    {
        private readonly IAbpSession _abpSession;
        private readonly BaseHttpClient _httpClient;
        private readonly UserManager _userManager;

        public EcoFarmRegistersAppService(IAbpSession abpSession, IConfiguration configuration, UserManager userManager)
        {
            _abpSession = abpSession;
            _httpClient = new BaseHttpClient(abpSession, configuration["ApiSettings:Business.Item"]);
            _userManager = userManager;
        }

        public async Task<object> GetListByPartner([FromQuery] GetAllEcofarmRegistersDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmRegisterDto>>(
                    "/api/v1/EcoFarmRegisters/partner/get-list", HttpMethod.Get, input);
                if (result.Success)
                    return DataResult.ResultSuccess(await AttachUserInfoDto(result.Data.Items.ToList()), "",
                        result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetListByUser(GetAllEcofarmRegistersDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<PagedResultDto<EcofarmRegisterDto>>(
                    "/api/v1/EcoFarmRegisters/user/get-list", HttpMethod.Get, input);
                if (result.Success)
                    return DataResult.ResultSuccess(await AttachUserInfoDto(result.Data.Items.ToList()), "",
                        result.Data.TotalCount);
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> GetById([FromQuery] GetItemEcofarmByIdDto input)
        {
            try
            {
                var result = await _httpClient.SendSync<EcofarmRegisterDto>("/api/v1/EcoFarmRegisters/get-detail",
                    HttpMethod.Get, input);
                if (result.Success && result?.Data != null)
                    return DataResult.ResultSuccess(await AttachUserInfoDto(result.Data), "");
                return result;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Create(
            [FromBody] CreateEcofarmRegisterDto input)
        {
            try
            {
                return await _httpClient.SendSync<long>("/api/v1/EcoFarmRegisters/create", HttpMethod.Post, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Update(
            [FromBody] UpdateEcofarmRegisterDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmRegisters/update", HttpMethod.Put, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> UpdateStatus(
            [FromBody] UpdateStatusEcofarmRegisterDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmRegisters/update-status", HttpMethod.Put,
                    input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> Delete(
            [FromQuery] DeleteEcofarmRegisterDto input)
        {
            try
            {
                return await _httpClient.SendSync<bool>("/api/v1/EcoFarmRegisters/delete", HttpMethod.Delete, input);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        private async Task<object> AttachUserInfoDto(List<EcofarmRegisterDto> ecofarmRegisters)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    foreach (var ecofarmRegister in ecofarmRegisters)
                    {
                        ecofarmRegister.UserInfo =
                            ObjectMapper.Map<UserInfoDto>(
                                _userManager.Users.FirstOrDefault(x => x.Id == ecofarmRegister.CreatorUserId));
                        ecofarmRegister.PartnerInfo =
                            ObjectMapper.Map<UserInfoDto>(
                                _userManager.Users.FirstOrDefault(x => x.Id == ecofarmRegister.PartnerId));
                    }

                    return ecofarmRegisters;
                }
            });
        }

        private async Task<object> AttachUserInfoDto(EcofarmRegisterDto ecofarmRegister)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    ecofarmRegister.UserInfo =
                        ObjectMapper.Map<UserInfoDto>(
                            _userManager.Users.FirstOrDefault(x => x.Id == ecofarmRegister.CreatorUserId));
                    ecofarmRegister.PartnerInfo =
                        ObjectMapper.Map<UserInfoDto>(
                            _userManager.Users.FirstOrDefault(x => x.Id == ecofarmRegister.PartnerId));
                    return ecofarmRegister;
                }
            });
        }
    }
}