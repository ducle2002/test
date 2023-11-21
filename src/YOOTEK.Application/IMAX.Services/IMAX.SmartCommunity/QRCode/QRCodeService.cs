using Abp.Domain.Repositories;
using Abp.UI;
using IMAX.App.ServiceHttpClient;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos;
using IMAX.App.ServiceHttpClient.IMAX.SmartCommunity;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using System;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public interface IQRCodeService
    {
        Task<object> CreateQRObjectAsync(CreateQRObjectDto input);
        Task<object> GetListQRObjectAsync(GetListQRObjectDto input);
        Task<object> GetListQRObjectByListCodeAsync(GetListQRObjectByListCodeInput input);
        Task<object> GetQRObjectByCodeAsync(GetQRObjectByCodeDto input);
        Task<object> GetInformationByQRcodeAsync(GetInformationByQRCodeInput input);
        Task<object> UpdateQRObjectAsync(UpdateQRObjectDto input);
        Task<object> DeleteQRObjectAsync(DeleteQRObjectDto input);
    }

    public class QRCodeService : IMAXAppServiceBase, IQRCodeService
    {
        private readonly IHttpQRCodeService _httpQRCodeService;
        private readonly IRepository<Parking, long> _parkingRepository;
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;

        public QRCodeService(
            IHttpQRCodeService httpQRCodeService,
            IRepository<Parking, long> parkingRepository,
            IHttpWorkAssignmentService httpWorkAssignmentService
            )
        {
            _httpQRCodeService = httpQRCodeService;
            _parkingRepository = parkingRepository;
            _httpWorkAssignmentService = httpWorkAssignmentService;
        }

        public async Task<object> CreateQRObjectAsync(CreateQRObjectDto input)
        {
            try
            {
                var createQRCodeResult = await _httpQRCodeService.CreateQRObject(input);
                return DataResult.ResultSuccess(createQRCodeResult.Result, createQRCodeResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetListQRObjectAsync(GetListQRObjectDto input)
        {
            try
            {
                var listQRCodeResult = await _httpQRCodeService.GetListQRObject(input);
                return DataResult.ResultSuccess(listQRCodeResult.Result, listQRCodeResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetListQRObjectByListCodeAsync(GetListQRObjectByListCodeInput input)
        {
            try
            {
                var listQRCodeResult = await _httpQRCodeService.GetListQRObjectByListCode(input);
                return DataResult.ResultSuccess(listQRCodeResult.Result, listQRCodeResult.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetQRObjectByCodeAsync(GetQRObjectByCodeDto input)
        {
            try
            {
                var result = await _httpQRCodeService.GetQRObjectByCode(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetInformationByQRcodeAsync(GetInformationByQRCodeInput input)
        {
            try
            {
                switch (input.ActionType)
                {
                    case QRCodeActionType.CarParking:
                        var parking = await _parkingRepository.FirstOrDefaultAsync(x => x.QrCode == input.Code);
                        return DataResult.ResultSuccess(parking, "Get success !");
                    case QRCodeActionType.Work:
                        MicroserviceResultDto<WorkDetailDto> work = await _httpWorkAssignmentService.GetWorkByQrCode(input.Code);
                        return DataResult.ResultSuccess(work.Result, work.Message);
                    default:
                        return DataResult.ResultFail("Acction invalid");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> GetImformationByQRcodeAsync(GetInformationByQRCodeInput input)
        {
            try
            {
                switch (input.ActionType)
                {
                    case QRCodeActionType.CarParking:
                        var parking = await _parkingRepository.FirstOrDefaultAsync(x => x.QrCode == input.Code);
                        return DataResult.ResultSuccess(parking, "Get success !");
                    case QRCodeActionType.Work:
                        MicroserviceResultDto<WorkDetailDto> work = await _httpWorkAssignmentService.GetWorkByQrCode(input.Code);
                        return DataResult.ResultSuccess(work.Result, work.Message);
                    default:
                        return DataResult.ResultFail("Acction invalid");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }


        public async Task<object> UpdateQRObjectAsync(UpdateQRObjectDto input)
        {
            try
            {
                var result = await _httpQRCodeService.UpdateQRObject(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<object> DeleteQRObjectAsync(DeleteQRObjectDto input)
        {
            try
            {
                var result = await _httpQRCodeService.DeleteQRObject(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }
    }
}
