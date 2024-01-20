using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services.QuanLyDoThi.TinTuc
{
    public interface INewsAppService : IApplicationService
    {
        Task<object> GetAll();
        Task<object> GetNewsSlide();
        Task<object> GetById(long id);
        Task<object> Create(NewsServiceDto dto);
        Task<object> Update(NewsServiceDto dto);
        Task<object> Delete(long id);
    }
}
