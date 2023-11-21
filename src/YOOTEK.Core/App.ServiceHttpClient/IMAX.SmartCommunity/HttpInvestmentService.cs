using Abp.Application.Services.Dto;
using Abp.Runtime.Session;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IMAX.App.ServiceHttpClient.IMAX.SmartCommunity
{
    public interface IHttpInvestmentService
    {
        #region IHttpInvestmentPackageService
        Task<MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>>> GetListInvestmentPackageByPartner(GetAllInvestmentPackagesDto input);
        Task<MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>>> GetListInvestmentPackageByUser(GetAllInvestmentPackagesDto input);
        Task<MicroserviceResultDto<InvestmentPackageDto>> GetInvestmentPackageById(long id);
        Task<MicroserviceResultDto<bool>> CreateInvestmentPackage(CreateInvestmentPackageDto input);
        Task<MicroserviceResultDto<bool>> UpdateInvestmentPackage(UpdateInvestmentPackageDto input);
        Task<MicroserviceResultDto<bool>> UpdateStateInvestmentPackage(UpdateStateInvestmentPackageDto input);
        Task<MicroserviceResultDto<bool>> DeleteInvestmentPackage(DeleteInvestmentPackageDto input);
        Task<MicroserviceResultDto<bool>> DeleteManyInvestmentPackage(DeleteManyInvestmentPackageDto input);
        #endregion

        #region IHttpInvestmentRegisterService
        Task<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>> GetListInvestmentRegisterByPartner(GetAllInvestmentRegistersDto input);
        Task<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>> GetListInvestmentRegisterByPackage(
            long input);
        Task<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>> GetListInvestmentRegisterByUser(GetAllInvestmentRegistersDto input);
        Task<MicroserviceResultDto<InvestmentRegisterDto>> GetInvestmentRegisterById(long id);
        Task<MicroserviceResultDto<long>> CreateInvestmentRegister(CreateInvestmentRegisterDto input);
        Task<MicroserviceResultDto<bool>> UpdateInvestmentRegister(UpdateInvestmentRegisterDto input);
        Task<MicroserviceResultDto<bool>> UpdateStateInvestmentRegister(UpdateStateInvestmentRegisterDto input);
        Task<MicroserviceResultDto<bool>> DeleteInvestmentRegister(DeleteInvestmentRegisterDto input);
        Task<MicroserviceResultDto<bool>> DeleteManyInvestmentRegister(DeleteManyInvestmentRegistersDto input);
        #endregion

        #region IHttpInvestmentPaymentService
        Task<MicroserviceResultDto<PagedResultDto<InvestmentPaymentDto>>> GetListInvestmentPayments(GetAllInvestmentPaymentsDto input);
        Task<MicroserviceResultDto<InvestmentPaymentDto>> GetInvestmentPaymentById(long id);
        Task<MicroserviceResultDto<bool>> CreateInvestmentPayment(CreateInvestmentPaymentDto input);
        Task<MicroserviceResultDto<bool>> UpdateStateInvestmentPayment(UpdateStateInvestmentPaymentDto input);
        #endregion

        #region IHttpInvestmentItemService
        Task<MicroserviceResultDto<PagedResultDto<InvestmentItemDto>>> GetListInvestmentItems(GetAllInvestmentItemsDto input);
        #endregion
        
        #region IHttpInvestmentPackageActivityService
        Task<MicroserviceResultDto<PagedResultDto<InvestmentPackageActivityDto>>> GetListInvestmentPackageActivity(GetAllInvestmentPackageActivitiesDto input);
        Task<MicroserviceResultDto<long>> CreateInvestmentPackageActivity(CreateInvestmentPackageActivitiesDto input);
        Task<MicroserviceResultDto<bool>> UpdateInvestmentPackageActivity(UpdateInvestmentPackageActivityDto input);
        Task<MicroserviceResultDto<bool>> DeleteInvestmentPackageActivity(DeleteInvestmentPackageActivityDto input);
        Task<MicroserviceResultDto<bool>> DeleteManyInvestmentPackageActivities(DeleteManyInvestmentPackageActivityDto input);
        #endregion
    }
    public class HttpInvestmentService : IHttpInvestmentService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;

        public HttpInvestmentService(HttpClient client, IAbpSession session)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        #region InvestmentPackageService
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>>> GetListInvestmentPackageByPartner(GetAllInvestmentPackagesDto input)
        {
            var query = "api/v1/InvestmentPackages/get-list-investment-package-by-partner" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>>> GetListInvestmentPackageByUser(GetAllInvestmentPackagesDto input)
        {
            var query = "api/v1/InvestmentPackages/get-list-investment-package-by-user" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>>>();
        }
        public async Task<MicroserviceResultDto<InvestmentPackageDto>> GetInvestmentPackageById(long id)
        {
            var query = "api/v1/InvestmentPackages/get-investment-package-by-id?id=" + id;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<InvestmentPackageDto>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateInvestmentPackage(CreateInvestmentPackageDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/InvestmentPackages/create-investment-package"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> UpdateInvestmentPackage(UpdateInvestmentPackageDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InvestmentPackages/update-investment-package");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateStateInvestmentPackage(UpdateStateInvestmentPackageDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InvestmentPackages/update-state-investment-package");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteInvestmentPackage(DeleteInvestmentPackageDto input)
        {
            var query = "api/v1/InvestmentPackages/delete-investment-package" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteManyInvestmentPackage(DeleteManyInvestmentPackageDto input)
        {
            var query = "api/v1/InvestmentPackages/delete-many-investment-package" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region InvestmentRegisterService
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>> GetListInvestmentRegisterByPartner(GetAllInvestmentRegistersDto input)
        {
            var query = "api/v1/InvestmentRegisters/get-list-investment-register-by-partner" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>>
            GetListInvestmentRegisterByPackage(long input)
        {
            var query = "api/v1/InvestmentRegisters/get-list-investment-register-by-package" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>>();
        }
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>> GetListInvestmentRegisterByUser(GetAllInvestmentRegistersDto input)
        {
            var query = "api/v1/InvestmentRegisters/get-list-investment-register-by-user" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>>>();
        }
        public async Task<MicroserviceResultDto<InvestmentRegisterDto>> GetInvestmentRegisterById(long id)
        {
            var query = "api/v1/InvestmentRegisters/get-investment-register-by-id?id=" + id;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<InvestmentRegisterDto>>();
        }
        public async Task<MicroserviceResultDto<long>> CreateInvestmentRegister(CreateInvestmentRegisterDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/InvestmentRegisters/create-investment-register"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<long>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> UpdateInvestmentRegister(UpdateInvestmentRegisterDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InvestmentRegisters/update-investment-register");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> UpdateStateInvestmentRegister(UpdateStateInvestmentRegisterDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InvestmentRegisters/update-state-investment-register");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteInvestmentRegister(DeleteInvestmentRegisterDto input)
        {
            var query = "api/v1/InvestmentRegisters/delete-investment-register" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteManyInvestmentRegister(DeleteManyInvestmentRegistersDto input)
        {
            var query = "api/v1/InvestmentRegisters/delete-many-investment-register" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);

            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region InvestmentPaymentService
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentPaymentDto>>> GetListInvestmentPayments(GetAllInvestmentPaymentsDto input)
        {
            var query = "api/v1/InvestmentPayments/get-list-investment-payment" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentPaymentDto>>>();
        }
        public async Task<MicroserviceResultDto<InvestmentPaymentDto>> GetInvestmentPaymentById(long id)
        {
            var query = "api/v1/InvestmentPayments/get-investment-payment-by-id?id=" + id;
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<InvestmentPaymentDto>>();
        }
        public async Task<MicroserviceResultDto<bool>> CreateInvestmentPayment(CreateInvestmentPaymentDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/InvestmentPayments/create-investment-payment"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> UpdateStateInvestmentPayment(UpdateStateInvestmentPaymentDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InvestmentPayments/update-state-investment-payment");

            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

        #region InvestmentItemService
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentItemDto>>> GetListInvestmentItems(GetAllInvestmentItemsDto input)
        {
            var query = "api/v1/Items/get-list-investment-item-by-user" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentItemDto>>>();
        }
        #endregion
        
        #region InvestmentPackageActivityService
        public async Task<MicroserviceResultDto<PagedResultDto<InvestmentPackageActivityDto>>> GetListInvestmentPackageActivity(GetAllInvestmentPackageActivitiesDto input)
        {
            var query = "api/v1/InvestmentPackageActivities/get-list-investment-package-activity" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, query);

            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<PagedResultDto<InvestmentPackageActivityDto>>>();
        }
       
        public async Task<MicroserviceResultDto<long>> CreateInvestmentPackageActivity(CreateInvestmentPackageActivitiesDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/InvestmentPackageActivities/create-investment-package-activity"))
            {
                request.HandlePostAsJson(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<long>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> UpdateInvestmentPackageActivity(UpdateInvestmentPackageActivityDto input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/InvestmentPackageActivities/update-investment-package-activity");
        
            request.HandlePutAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
       
        public async Task<MicroserviceResultDto<bool>> DeleteInvestmentPackageActivity(DeleteInvestmentPackageActivityDto input)
        {
            var query = "api/v1/InvestmentPackageActivities/delete-investment-package-activity" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
        
            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> DeleteManyInvestmentPackageActivities(DeleteManyInvestmentPackageActivityDto input)
        {
            var query = "api/v1/InvestmentPackageActivities/delete-many-investment-package-activity" + input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Delete, query);
        
            request.HandleDeleteAsJson(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion

    }
}
