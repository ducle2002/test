using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Yootek.Common.DataResult;
using Yootek.Yootek.EntityDb.Yootek.DichVu.CheckingObj;
using Yootek.Yootek.Services.Yootek.DichVu.CheckingObjectServ.CheckObjectDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.DichVu.CheckingObjectServ
{
    public interface ICheckObjService : IApplicationService
    {
        Task CreateCheckObjDataAsync(CheckingObjectDto input);
        Task<int> GetTotalCheckObject(GetAllObjectDto input);
    }
    public class CheckingObjService : YootekAppServiceBase, ICheckObjService
    {
        private readonly IRepository<CheckingObject, long> _checkingObjectRepos;

        public CheckingObjService(IRepository<CheckingObject, long> checkingObjectRepos)
        {
            _checkingObjectRepos = checkingObjectRepos;
        }

      
        public async Task CreateCheckObjDataAsync(CheckingObjectDto input)
        {

            try
            {
                //input.ObjectId = 0;
                //input.TenantId = AbpSession.TenantId;
                var insertInput = input.MapTo<CheckingObject>();
                await _checkingObjectRepos.InsertAsync(insertInput);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
            }
        }

        public Task<int> GetTotalCheckObject(GetAllObjectDto input)
        {
            try
            {
                var query = _checkingObjectRepos.GetAll().Where(s => s.ObjectId == input.objectId && s.Type == input.type).ToList();
                if (input.objectId == null)
                {
                    query = _checkingObjectRepos.GetAll().Where(s => s.Type == input.type).ToList();
                }
                if (input.type == null)
                {
                    query = _checkingObjectRepos.GetAll().Where(s => s.ObjectId == input.objectId).ToList();
                }

                var count = query.Count();
                return Task.FromResult(count);

            }
            catch (Exception e)
            {
                return Task.FromResult(0);
            }
        }

        protected bool checkQuery(CheckingObject rs, GetAllObjectDto input)
        {

            if (input.objectId <= 0) return rs.Type == input.type;
            if (input.type <= 0) return rs.ObjectId == input.objectId;
            return (rs.ObjectId == input.objectId && rs.Type == input.type);
        }
    }
}
