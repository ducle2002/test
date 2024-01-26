using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.Yootek.EntityDb.Yootek.MobileAppFeedback;
using Yootek.Yootek.Services.Yootek.MobileAppFeedback.MobileAppFeedbackDto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.MobileAppFeedback
{
    public interface IMobileAppFeedbackService : IApplicationService
    {
        Task<object> CreateOrUpdateMobileAppFeedback(AppFeedbackDto input);
        Task<object> GetMobileAppFeedback(GetAllFeedbackDto input);
    }

    public class MobileAppFeedbackService : YootekAppServiceBase, IMobileAppFeedbackService
    {
        private readonly IRepository<AppFeedback, long> _appFeedbackRepo;

        public MobileAppFeedbackService(IRepository<AppFeedback, long> appFeedbackRepo)
        {
            _appFeedbackRepo = appFeedbackRepo;
        }

      
        public async Task<object> CreateOrUpdateMobileAppFeedback(AppFeedbackDto input)
        {
            try
            {
                if (input.Id > 0)
                {
                    //Logger.Debug(input.Feedback + " - " + input.ImgUrl);
                    var updateFeedback = await _appFeedbackRepo.GetAsync(input.Id);
                    if (updateFeedback != null)
                    {
                        input.MapTo(updateFeedback);
                        await _appFeedbackRepo.UpdateAsync(updateFeedback);
                    }
                    var result = DataResult.ResultSuccess(updateFeedback, "Updated successfully!");
                    return result;
                }
                else
                {
                    var inputFeedback = input.MapTo<AppFeedback>();
                    var data = await _appFeedbackRepo.InsertAsync(inputFeedback);
                    var result = DataResult.ResultSuccess(inputFeedback, "Created successfully!");
                    return result;
                }

            }
            catch (Exception e)
            {
                var result = DataResult.ResultError(e.Message, "Process failed!");
                Logger.Fatal(e.Message);
                return result;
            }
        }

        protected IQueryable<AppFeedbackDto> GetAllMobileAppFeedback()
        {
            var query = (from f in _appFeedbackRepo.GetAll()
                         select new AppFeedbackDto
                         {
                             Id = f.Id,
                             Feedback = f.Feedback,
                             FileUrl = f.FileUrl,
                             IsDeleted = f.IsDeleted,
                             DeleterUserId = f.DeleterUserId,
                             DeletionTime = f.DeletionTime,
                             LastModificationTime = f.LastModificationTime,
                             LastModifierUserId = f.LastModifierUserId,
                             CreationTime = f.CreationTime,
                             CreatorUserId = f.CreatorUserId
                         }).AsQueryable();
            return query;
        }

        protected IQueryable<AppFeedbackDto> QueryFormId(IQueryable<AppFeedbackDto> query, GetAllFeedbackDto input, int? formId)
        {
            switch (formId)
            {
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_ADMIN_GET_OBJECT_GETALL:
                    query = query.OrderBy(x => x.CreationTime);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_PARTNER_OBJECT_GETALL:
                    query = query.Where(x => x.CreatorUserId == input.CreatorUserId).OrderBy(x => x.CreationTime);
                    break;
                case (int)CommonENumObject.FORM_ID_OBJECT.FORM_USER_OBJECT_GETALL:
                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId).OrderBy(x => x.CreationTime);
                    break;
            }
            return query;
        }

        public async Task<object> GetMobileAppFeedback(GetAllFeedbackDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var obj = new Object();
                    var query = QueryFormId(GetAllMobileAppFeedback(), input, input.FormId);
                    var count = 0;
                    if (input.FormCase == null || input.FormCase == (int)CommonENumObject.FORMCASE_GET_DATA.OBJECT_GETALL)
                    {
                        if (input.FormId == 4)
                        {
                            count = query.Count();
                        }
                        else
                        {
                            count = query.Count();
                            var list = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                            obj = list;
                        }
                    }
                    else if (input.FormCase == (int)CommonENumObject.FORMCASE_GET_DATA.OBJECT_DETAIL)
                    {
                        obj = await query.FirstOrDefaultAsync();
                        count = 1;
                    }
                    else
                    {
                        count = query.Count();
                    }
                    var data = DataResult.ResultSuccess(obj, "Success!", count);
                    return data;

                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.Message, "Failed!");
                return data;
            }
        }
    }
}
