using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.Common.Enum.HandbookEnums;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.Guides
{
    public interface IHandbookService : IApplicationService
    {
        Task<object> CreateOrUpdateHandbookAsync(HandbookDto input);
        Task<object> GetHandbooksAsync(FindInHandbook input);
        Task<object> DeleteHandbookAsync(long id);
    }
    public class HandbookService : YootekAppServiceBase, IHandbookService
    {
        private readonly IRepository<Handbook, long> _handbookRepo;
        public HandbookService(IRepository<Handbook, long> handbookRepo)
        {
            _handbookRepo = handbookRepo;
        }

      
        public async Task<object> CreateOrUpdateHandbookAsync(HandbookDto input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateData = await _handbookRepo.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        //call back
                        await _handbookRepo.UpdateAsync(updateData);
                    }
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;


                }
                else
                {
                    var inputHandbook = input.MapTo<Handbook>();
                    var data = await _handbookRepo.InsertAsync(inputHandbook);
                    var result = DataResult.ResultSuccess(inputHandbook, "Inserted");
                    return result;
                }
            }
            catch (Exception e)
            {
                var result = DataResult.ResultError(e.Message, "Failed!");
                Logger.Fatal(e.StackTrace);
                return result;
            }

        }

        public async Task<object> DeleteHandbookAsync(long id)
        {
            try
            {
                await _handbookRepo.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Deleted!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.Message, "Failed!");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        protected IQueryable<HandbookDto> GetAllHandbooks()
        {
            var query = (from handbook in _handbookRepo.GetAll()
                         select new HandbookDto
                         {
                             Id = handbook.Id,
                             Title = handbook.Title,
                             Content = handbook.Content,
                             CreationTime = handbook.CreationTime,
                             CreatorUserId = handbook.CreatorUserId,
                             DeleterUserId = handbook.DeleterUserId,
                             DeletionTime = handbook.DeletionTime,
                             IsDeleted = handbook.IsDeleted,
                             LastModificationTime = handbook.LastModificationTime,
                             LastModifierUserId = handbook.LastModifierUserId,
                             TenantId = handbook.TenantId,
                             Type = handbook.Type,
                             OrganizationUnitId = handbook.OrganizationUnitId,
                             FileUrl = handbook.FileUrl
                         }).AsQueryable();
            return query;
        }

        protected IQueryable<HandbookDto> QueryFormId(IQueryable<HandbookDto> query, FindInHandbook input, int? FormId)
        {
            switch (FormId)
            {
                case (int)HandbookEnums.FORM_ID_OBJECTS.FORM_ADMIN_GET_OBJECT_GETALL:
                    query = query.OrderBy(x => x.CreationTime);
                    break;
                case (int)HandbookEnums.FORM_ID_OBJECTS.FORM_PARTNER_OBJECT_GETALL:
                    query = query.Where(x => x.CreatorUserId == input.CreatorUserId).OrderBy(x => x.CreationTime);
                    break;
                case (int)HandbookEnums.FORM_ID_OBJECTS.FORM_USER_OBJECT_GETALL:
                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId).OrderBy(x => x.CreationTime);
                    break;
                case (int)HandbookEnums.FORM_ID_OBJECTS.FORM_SEARCHING_OBJECT:
                    query = query.Where(x => (x.Title.Contains(input.Keyword) || x.Content.Contains(input.Keyword))).OrderBy(x => x.CreationTime);
                    break;
                case (int)HandbookEnums.FORM_ID_OBJECTS.FORM_GET_ALL_UNIT_ID_OBJECT:
                    query = query.Where(x => x.OrganizationUnitId == input.OrganizationUnitId).OrderBy(x => x.CreationTime);
                    break;
                case (int)HandbookEnums.FORM_ID_OBJECTS.FORM_SEARCHING_UNIT_ID_OBJECT:
                    query = query.Where(x => (x.Title.Contains(input.Keyword) || x.Content.Contains(input.Keyword)) && x.OrganizationUnitId == input.OrganizationUnitId).OrderBy(x => x.CreationTime);
                    break;
            }
            return query;
        }

        public async Task<object> GetHandbooksAsync(FindInHandbook input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var obj = new Object();
                    var query = QueryFormId(GetAllHandbooks(), input, input.FormId);
                    var list = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                    var count = query.Count();
                    obj = list;
                    //var list = query.ToList();
                    var data = DataResult.ResultSuccess(obj, "Success", count);
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.Message, "Error");
                Logger.Error(e.Message);
                return data;
            }
        }
    }
}
