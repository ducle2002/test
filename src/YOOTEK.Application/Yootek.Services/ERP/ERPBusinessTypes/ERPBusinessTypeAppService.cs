using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;
using System.Net;
namespace Yootek.Services
{
    public interface IERPBusinessTypeAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetListERPBusinessTypeInput input);
        Task<DataResult> GetByIdAsync(long id);
        Task<DataResult> CreateAsync(CreateERPBusinessTypeDto input);
        Task<DataResult> UpdateAsync(UpdateERPBusinessTypeDto input);
        Task<DataResult> DeleteAsync(long id);
    }
    public class ERPBusinessTypeAppService : YootekAppServiceBase, IERPBusinessTypeAppService
    {
        private readonly IRepository<ERPBusinessType, long> _eRPBusinessTypeRepository;
        public ERPBusinessTypeAppService(
             IRepository<ERPBusinessType, long> eRPBusinessTypeRepository
            )
        {
            _eRPBusinessTypeRepository = eRPBusinessTypeRepository;
        }
        public async Task<DataResult> GetAllAsync(GetListERPBusinessTypeInput input)
        {
            try
            {
                IQueryable<ERPBusinessTypeDto> query = (from eRPBusinessType in _eRPBusinessTypeRepository.GetAll()
                                                        select new ERPBusinessTypeDto
                                                        {
                                                            Id = eRPBusinessType.Id,
                                                            Types = eRPBusinessType.Types,
                                                            Title = eRPBusinessType.Title,
                                                            Description = eRPBusinessType.Description,
                                                        })
                    .ApplySearchFilter(input.Keyword, x => x.Title, x => x.Description)
                    ;
                List<ERPBusinessTypeDto> result = await query
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateAsync(CreateERPBusinessTypeDto input)
        {
            try
            {
                ERPBusinessType eRPBusinessType = ObjectMapper.Map<ERPBusinessType>(input);
                await _eRPBusinessTypeRepository.InsertAsync(eRPBusinessType);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateAsync(UpdateERPBusinessTypeDto input)
        {
            try
            {
                ERPBusinessType updateData = await _eRPBusinessTypeRepository.FirstOrDefaultAsync(x => x.Id == input.Id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPBusinessType not found!");
                ObjectMapper.Map(input, updateData);
                await _eRPBusinessTypeRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteAsync(long id)
        {
            try
            {
                ERPBusinessType eRPBusinessType = await _eRPBusinessTypeRepository.FirstOrDefaultAsync(x => x.Id == id)
                                        ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                  "ERPBusinessType not found!");
                await _eRPBusinessTypeRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetByIdAsync(long id)
        {
            try
            {
                ERPBusinessType eRPBusinessType = await _eRPBusinessTypeRepository.FirstOrDefaultAsync(x => x.Id == id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPBusinessType not found!");
                ERPBusinessTypeDto eRPBusinessTypeDto = ObjectMapper.Map<ERPBusinessTypeDto>(eRPBusinessType);
                return DataResult.ResultSuccess(eRPBusinessTypeDto, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
