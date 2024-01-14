using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Services.Dto;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IImagesAppService : IApplicationService
    {
        #region Image

        Task<object> CreateOrUpdateImage(ImageDto input);
        Task<object> GetImageAsync(ImageInput input);
        #endregion
    }

    public class ImagesAppService : YootekAppServiceBase, IImagesAppService
    {
        private readonly IRepository<Images, long> _imagesRepos;
        public ImagesAppService(IRepository<Images, long> imagesRepos)
        {
            _imagesRepos = imagesRepos;
        }

      
        public async Task<object> CreateOrUpdateImage(ImageDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    if (input.Id > 0)
                    {
                        //update
                        var updateData = await _imagesRepos.GetAsync(input.Id);
                        if (updateData != null)
                        {
                            //input.MapTo(updateData);
                            updateData.Properties = input.Properties;
                            updateData.Type = input.Type;
                            //call back
                            await _imagesRepos.UpdateAsync(updateData);
                        }
                        mb.statisticMetris(t1, 0, "admin_ud_img");
                        var data = DataResult.ResultSuccess(updateData, "Update success !");
                        return data;
                    }
                    else
                    {
                        //Insert
                        var insertInput = input.MapTo<Images>();
                        long id = await _imagesRepos.InsertAndGetIdAsync(insertInput);

                        mb.statisticMetris(t1, 0, "admin_is_img");
                        var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                        return data;
                    }
                }

            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<object> GetImageAsync(ImageInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input.TenantId == 0)
                {
                    input.TenantId = null;
                }

                var result = new Images();
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    result = await _imagesRepos.FirstOrDefaultAsync(x => input.Type != null && x.Type == input.Type);
                }

                var data = DataResult.ResultSuccess(result, "Get success");
                mb.statisticMetris(t1, 0, "get_img");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }

        }
    }
}
