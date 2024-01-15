using System.Threading.Tasks;
using Abp;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.App.ServiceHttpClient.Social;
using Yootek.Common.DataResult;

namespace Yootek.Yootek.Services.Yootek.SmartSocial.YooSocialMedia
{
    public interface ISocialMediaReport
    {
        Task<object> GetListReportAsync(GetListReportDto input);
        Task<object> CreateReportAsync(CreateReportDto input);
    }
    public class SocialMediaReportAppService : YootekAppServiceBase, ISocialMediaReport
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