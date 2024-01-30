using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.DocumentTypes.dto;
using Yootek.Organizations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Services
{
    public interface IUserDocumentsAppService : IApplicationService
    {
        Task<DataResult> GetAllDocumentsAsync(GetAllDocumentsByUserInput input);
        Task<DataResult> GetAllDocumentsTypes(GetAllDocumentTypesByUserDto input);
    }
    public class UserDocumentsAppService : YootekAppServiceBase, IUserDocumentsAppService
    {
        private readonly IRepository<Documents, long> _documentsRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnit;
        private readonly IRepository<DocumentTypes,long> _documentTypesRepository;
        public UserDocumentsAppService(
            IRepository<Documents, long> documentsRepository,
            IRepository<AppOrganizationUnit, long> organizationUnit,
            IRepository<DocumentTypes, long> documentTypesRepository
        )
        {
            _documentsRepository = documentsRepository;
            _organizationUnit = organizationUnit;
            _documentTypesRepository = documentTypesRepository;
        }
        public async Task<DataResult> GetAllDocumentsAsync(GetAllDocumentsByUserInput input)
        {
            try
            {
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
                             })
                        .Where(u => u.Scope == DocumentScope.App || u.Scope == DocumentScope.All)
                        .WhereIf(input.BuildingId.HasValue, x=>x.BuildingId == input.BuildingId || x.BuildingId == null)
                        .WhereIf(input.UrbanId.HasValue,x=>x.UrbanId == input.UrbanId || x.UrbanId == null)
                        .WhereIf(input.TypeId.HasValue, u => u.DocumentTypeId == input.TypeId)
                        .ApplySearchFilter(input.Keyword, x => x.DisplayName);

                List<DocumentsDto> result = await query
                .ApplySort(input.OrderBy, input.SortBy)
                .ApplySort(OrderByDocument.DISPLAY_NAME)
                .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<DataResult> GetAllDocumentsTypes(GetAllDocumentTypesByUserDto input)
        {
            try
            {
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
                                                      .Where(x=>x.Scope == DocumentScope.App || x.Scope == DocumentScope.All)
                                                      .ApplySearchFilter(input.Keyword, x => x.DisplayName);

                List<DocumentTypesDto> result = await query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByDocumentType.DISPLAY_NAME, SortBy.DESC)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}