#nullable enable
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
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
    public interface IAdminDocumentsAppService : IApplicationService
    {
        Task<DataResult> GetAllDocumentsAsync(GetAllDocumentsInput input);
        Task<DataResult> CreateDocument(CreateDocumentsInput input);
        Task<DataResult> UpdateDocument(UpdateDocumentsInput input);
        Task<DataResult> DeleteDocument(long id);
        Task<DataResult> DeleteManyDocuments([FromBody] List<long> ids);
    }

    public class AdminDocumentsAppService : YootekAppServiceBase, IAdminDocumentsAppService
    {
        private readonly IRepository<Documents, long> _documentsRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnit;
        private readonly IRepository<DocumentTypes, long> _documentTypesRepository;


        public AdminDocumentsAppService(
            IRepository<Documents, long> documentsRepository,
            IRepository<AppOrganizationUnit, long> organizationUnit,
            IRepository<DocumentTypes, long> documentTypesRepository

        )
        {
            _documentsRepository = documentsRepository;
            _organizationUnit = organizationUnit;
            _documentTypesRepository = documentTypesRepository;
        }

        public async Task<DataResult> GetAllDocumentsGlobalAsync(GetAllDocumentsInput input)
        {
            try
            {
                return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.SetTenantId(null))
                    {
                        var query = (from sm in _documentsRepository.GetAll()
                                     join type in _documentTypesRepository.GetAll() on sm.DocumentTypeId equals type.Id
                                     select new DocumentsDto

                                     {
                                         Id = sm.Id,
                                         TenantId = sm.TenantId,
                                         DisplayName = sm.DisplayName,
                                         BuildingId = sm.BuildingId,
                                         BuildingName = _organizationUnit.GetAll().Where(x => x.Id == sm.BuildingId)
                                             .Select(x => x.DisplayName).FirstOrDefault(),
                                         UrbanName = _organizationUnit.GetAll().Where(x => x.Id == sm.UrbanId)
                                             .Select(x => x.DisplayName).FirstOrDefault(),
                                         CreationTime = sm.CreationTime,
                                         CreatorUserId = sm.CreatorUserId ?? 0,
                                         DocumentTypeId = sm.DocumentTypeId,
                                         DocumentTypeName = type.DisplayName,
                                         FileUrl = sm.FileUrl,
                                         Scope = type.Scope,
                                         IsGlobal = type.IsGlobal,
                                     })
                            .Where(u => u.IsGlobal == true)
                            .WhereIf(input.Scope.HasValue, u => u.Scope == input.Scope)
                            .WhereIf(input.DocumentTypeId.HasValue, u => u.DocumentTypeId == input.DocumentTypeId)
                            .ApplySearchFilter(input.Keyword, x => x.DisplayName);
                        if (input.Arrange == true)
                        {
                            List<DocumentsDto> result = await query
                            .ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByDocument.DISPLAY_NAME)
                            .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                            return DataResult.ResultSuccess(result, "Get success!", query.Count());
                        }
                        else if (input.Arrange == false)
                        {
                            List<DocumentsDto> result = await query
                            .ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByDocument.DISPLAY_NAME, SortBy.DESC)
                                .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                            return DataResult.ResultSuccess(result, "Get success!", query.Count());
                        }
                        else
                        {
                            List<DocumentsDto> result = await query
                            .ApplySort(input.OrderBy, input.SortBy)
                            .ApplySort(OrderByDocument.BUILDING_ID)
                            .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                            return DataResult.ResultSuccess(result, "Get success!", query.Count());
                        }
                    }

                });
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetAllDocumentsAsync(GetAllDocumentsInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                {
                    var tenantId = AbpSession.TenantId;
                    var query = (from sm in _documentsRepository.GetAll()
                                 join type in _documentTypesRepository.GetAll() on sm.DocumentTypeId equals type.Id into tbl_sm
                                 from tbl in tbl_sm.DefaultIfEmpty()
                                 select new DocumentsDto
                                 {
                                     Id = sm.Id,
                                     TenantId = sm.TenantId,
                                     DisplayName = sm.DisplayName,
                                     BuildingId = sm.BuildingId,
                                     BuildingName = _organizationUnit.GetAll().Where(x => x.Id == sm.BuildingId)
                                         .Select(x => x.DisplayName).FirstOrDefault(),
                                     UrbanId = sm.UrbanId,
                                     UrbanName = _organizationUnit.GetAll().Where(x => x.Id == sm.UrbanId).Select(x => x.DisplayName)
                                         .FirstOrDefault(),
                                     CreationTime = sm.CreationTime,
                                     CreatorUserId = sm.CreatorUserId ?? 0,
                                     DocumentTypeId = tbl.Id,
                                     DocumentTypeName = tbl.DisplayName,
                                     FileUrl = sm.FileUrl,
                                     Scope = tbl.Scope,
                                     IsGlobal = tbl.IsGlobal,
                                     Link = sm.Link
                                 })
                            .Where(u => u.IsGlobal == false)
                            .WhereIf(tenantId != null, x => x.TenantId == tenantId)
                            .WhereIf(input.Scope.HasValue, u => u.Scope == input.Scope)
                            .WhereIf(input.DocumentTypeId.HasValue, u => u.DocumentTypeId == input.DocumentTypeId)
                            .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                            .ApplySearchFilter(input.Keyword, x => x.DisplayName);
                    if (input.Arrange == true)
                    {
                        List<DocumentsDto> result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByDocument.DISPLAY_NAME)
                        .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                        return DataResult.ResultSuccess(result, "Get success!", query.Count());
                    }
                    else if (input.Arrange == false)
                    {
                        List<DocumentsDto> result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByDocument.DISPLAY_NAME, SortBy.DESC)
                            .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                        return DataResult.ResultSuccess(result, "Get success!", query.Count());
                    }
                    else
                    {
                        List<DocumentsDto> result = await query
                        .ApplySort(input.OrderBy, input.SortBy)
                        .ApplySort(OrderByDocument.BUILDING_ID)
                        .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                        return DataResult.ResultSuccess(result, "Get success!", query.Count());
                    }
                }

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<DataResult> CreateDocument(CreateDocumentsInput input)
        {
            try
            {
                Documents? documentTypesOrg = await _documentsRepository.FirstOrDefaultAsync(x => x.DisplayName == input.DisplayName);
                if (documentTypesOrg != null) throw new UserFriendlyException(409, "DisplayName is exist");
                Documents documents = input.MapTo<Documents>();
                documents.TenantId = AbpSession.TenantId;
                await _documentsRepository.InsertAsync(documents);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateDocument(UpdateDocumentsInput input)
        {
            try
            {
                Documents? updateData = await _documentsRepository.FirstOrDefaultAsync(input.Id)
                                        ?? throw new Exception("Documents not found!");
                input.MapTo(updateData);
                await _documentsRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteDocument(long id)
        {
            try
            {
                await _documentsRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteManyDocuments([FromBody] List<long> ids)
        {
            try
            {
                await _documentsRepository.DeleteAsync(x => ids.Contains(x.Id));
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
