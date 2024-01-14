using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.DocumentTypes.dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Services
{
    public interface IAdminDocumentTypesAppService : IApplicationService
    {
        Task<DataResult> GetAllDocumentTypesAsync(GetAllDocumentTypesDto input);
        Task<DataResult> CreateDocumentType(CreateDocumentTypesInput input);
        Task<DataResult> UpdateDocumentType(UpdateDocumentTypesInput input);
        Task<DataResult> DeleteDocumentType(long id);
        Task<DataResult> DeleteManyDocumentTypes([FromBody] List<long> ids);
    }

    public class AdminDocumentTypesAppService : YootekAppServiceBase, IAdminDocumentTypesAppService
    {
        private readonly IRepository<DocumentTypes, long> _documentTypesRepository;

        public AdminDocumentTypesAppService(
            IRepository<DocumentTypes, long> documentTypesRepository
        )
        {
            _documentTypesRepository = documentTypesRepository;
        }


        public async Task<DataResult> GetAllDocumentTypesAsync(GetAllDocumentTypesDto input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var tenantId = AbpSession.TenantId;
                    IQueryable<DocumentTypesDto> query = (from sm in _documentTypesRepository.GetAll()
                                                          select new DocumentTypesDto
                                                          {
                                                              Id = sm.Id,
                                                              TenantId = sm.TenantId,
                                                              DisplayName = sm.DisplayName,
                                                              Description = sm.Description,
                                                              CreationTime = sm.CreationTime,
                                                              CreatorUserId = sm.CreatorUserId ?? 0,
                                                              Icon = sm.Icon,
                                                              Scope = sm.Scope,
                                                              IsGlobal = sm.IsGlobal,
                                                          })
                                                          .WhereIf(input.isGlobal != null, x => x.IsGlobal == input.isGlobal)
                                                          .WhereIf(tenantId != null, x => x.TenantId == tenantId)
                                                          .WhereIf(tenantId == null && input.isGlobal == null, x => x.IsGlobal == true)
                                                          .ApplySearchFilter(input.Keyword, x => x.DisplayName, x => x.Description);

                    List<DocumentTypesDto> result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByDocumentType.DISPLAY_NAME, SortBy.DESC)
                        .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                    return DataResult.ResultSuccess(result, "Get success!", query.Count());
                }

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> CreateDocumentType(CreateDocumentTypesInput input)
        {
            try
            {
                DocumentTypes? documentTypesOrg = await _documentTypesRepository.FirstOrDefaultAsync(x => x.DisplayName == input.DisplayName);
                if (documentTypesOrg != null) throw new UserFriendlyException(409, "DisplayName is exist");
                DocumentTypes documentTypes = input.MapTo<DocumentTypes>();
                documentTypes.TenantId = AbpSession.TenantId;
                documentTypes.IsGlobal = false;
                if (documentTypes.TenantId == null)
                {
                    documentTypes.IsGlobal = true;
                }
                await _documentTypesRepository.InsertAsync(documentTypes);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateDocumentType(UpdateDocumentTypesInput input)
        {
            try
            {
                DocumentTypes? updateData = await _documentTypesRepository.FirstOrDefaultAsync(input.Id)
                                         ?? throw new Exception("DocumentTypes not found!");
                input.MapTo(updateData);
                await _documentTypesRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteDocumentType(long id)
        {
            try
            {
                await _documentTypesRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteManyDocumentTypes([FromBody] List<long> ids)
        {
            try
            {
                await _documentTypesRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
