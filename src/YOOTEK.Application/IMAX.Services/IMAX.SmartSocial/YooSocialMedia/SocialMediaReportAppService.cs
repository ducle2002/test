using System.Threading.Tasks;
using Abp;
using IMAX.App.ServiceHttpClient.Dto.Imax.Social.Forum;
using IMAX.App.ServiceHttpClient.Imax.Social;
using IMAX.Common.DataResult;

namespace IMAX.IMAX.Services.IMAX.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaReport
    {
        Task<object> GetListReportAsync(GetListReportDto input);
        Task<object> CreateReportAsync(CreateReportDto input);
    }
    public class SocialMediaReportAppService : IMAXAppServiceBase, ISocialMediaReport
    {
        private readonly IHttpSocialMediaService _httpSocialMediaService;
        
        public SocialMediaReportAppService(IHttpSocialMediaService httpSocialMediaService)
        {
            _httpSocialMediaService = httpSocialMediaService;
        }
        public async Task<object> GetListReportAsync(GetListReportDto input)
        {
            try 
            {
                var getListResult = await _httpSocialMediaService.GetListReport(input);
                return DataResult.ResultSuccess(getListResult.Result.Items, getListResult.Message, getListResult.Result.TotalCount);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<object> CreateReportAsync(CreateReportDto input)
        {
            try 
            {
                var createResult = await _httpSocialMediaService.CreateReport(input);
                return DataResult.ResultSuccess(createResult.Result, createResult.Message);
            }
            catch (System.Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
    }
}