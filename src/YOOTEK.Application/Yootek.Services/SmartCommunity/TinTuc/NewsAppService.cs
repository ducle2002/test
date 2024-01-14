using Abp.Authorization;
using Abp.Domain.Repositories;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.QuanLyDoThi.TinTuc
{
    [AbpAuthorize]
    public class NewsAppService : YootekAppServiceBase, INewsAppService
    {
        private readonly IRepository<News, long> _repository;

        public NewsAppService(IRepository<News, long> repository)
        {
            _repository = repository;
        }

        public async Task<object> GetAll()
        {
            try
            {
                var result = await _repository.GetAllListAsync();

                var data = DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess);
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultFail(e.Message);
                Logger.Fatal(e.Message);
                return null;
            }

        }

        public async Task<object> GetNewsSlide()
        {
            try
            {
                var result = (from news in _repository.GetAll()
                              orderby news.DatePost descending
                              select news).Take(5).ToList();
                var data = DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.Success);
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultFail(e.Message);
                Logger.Fatal(e.Message);
                return null;
            }
        }

        public async Task<object> GetById(long id)
        {
            try
            {
                var result = await _repository.GetAsync(id);

                var data = DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess);
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultFail(e.Message);
                Logger.Fatal(e.Message);
                return null;
            }

        }
        public async Task<object> Create(NewsServiceDto dto)
        {
            try
            {
                await _repository.InsertAsync(dto);

                var data = DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultFail(e.Message);
                Logger.Fatal(e.Message);
                return null;
            }
        }
        public async Task<object> Update(NewsServiceDto dto)
        {
            try
            {
                await _repository.UpdateAsync(dto);

                var data = DataResult.ResultSuccess(Common.Resource.QuanLyChung.UpdateSuccess);
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultFail(e.Message);
                Logger.Fatal(e.Message);
                return null;
            }
        }
        public async Task<object> Delete(long id)
        {
            try
            {
                await _repository.DeleteAsync(id);

                var data = DataResult.ResultSuccess(Common.Resource.QuanLyChung.DeleteSuccess);
                return data;
            }
            catch (Exception e)
            {
                DataResult.ResultFail(e.Message);
                Logger.Fatal(e.Message);
                return null;
            }
        }
    }
}
