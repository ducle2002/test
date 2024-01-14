using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{

    public interface IFeedbackAppAppService : IApplicationService
    {
        Task<object> GetAllFeedbackAppAsync(GetAllFeedbackAppInput input);
        Task<object> CreateOrUpdateFeedbackApp(FeedbackAppDto input);
        Task<object> DeleteFeedbackApp(long id);
    }
    public class FeedbackAppAppService : YootekAppServiceBase, IFeedbackAppAppService
    {
        private readonly IRepository<FeedbackApp, long> _feedbackAppRepos;
        private readonly IRepository<User, long> _userRepos;
        private readonly IFeedbackAppExcelExporter _feedbackAppExcelExporter;
        public FeedbackAppAppService(IRepository<FeedbackApp, long> feedbackAppRepos, 
            IRepository<User, long> userRepos,
            IFeedbackAppExcelExporter feedbackAppExcelExporter)
        {
            _feedbackAppRepos = feedbackAppRepos;
            _userRepos = userRepos;
            _feedbackAppExcelExporter = feedbackAppExcelExporter;
        }

      
        public async Task<object> CreateOrUpdateFeedbackApp(FeedbackAppDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _feedbackAppRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);

                        //call back
                        await _feedbackAppRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "ud_feeback");
                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<FeedbackApp>();
                    long id = await _feedbackAppRepos.InsertAndGetIdAsync(insertInput);

                    mb.statisticMetris(t1, 0, "is_feeback");
                    var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                    return data;
                }

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteFeedbackApp(long id)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                await _feedbackAppRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete Success");
                mb.statisticMetris(t1, 0, "del_feeback");
                return data;
            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Có lỗi");
                Logger.Fatal(ex.Message, ex);
                throw;
            }
        }

        public async Task<object> GetAllFeedbackAppAsync(GetAllFeedbackAppInput input)
        {
            try
            {
                var query = from fb in _feedbackAppRepos.GetAll()
                            join user in _userRepos.GetAll() on fb.CreatorUserId equals user.Id into tb_user
                            from user in tb_user.DefaultIfEmpty()
                            orderby fb.CreationTime descending
                            select new FeedbackAppOutputDto()
                            {
                                TenantId = fb.TenantId,
                                FileUrl = fb.FileUrl,
                                Feedback = fb.Feedback,
                                Id = fb.Id,
                                CreatorUserId = fb.CreatorUserId,
                                Name = user.FullName,
                                ImageUrl = user.ImageUrl
                            };

                var result = await query.PageBy(input).ToListAsync();
                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }

        }

        public async Task<object> ExportFeedbackAppToExcel(ExportExcelInput input)
        {
            try
            {
                var query = await (from fb in _feedbackAppRepos.GetAll() 
                                   join user in _userRepos.GetAll() on fb.CreatorUserId equals user.Id into tb_user
                                   from user in tb_user.DefaultIfEmpty()
                                   orderby fb.CreationTime descending
                                   select new FeedbackAppOutputDto()
                                   {
                                       TenantId = fb.TenantId,
                                       FileUrl = fb.FileUrl,
                                       Feedback = fb.Feedback,
                                       Id = fb.Id,
                                       CreatorUserId = fb.CreatorUserId,
                                       Name = user.FullName,
                                       ImageUrl = user.ImageUrl
                                   })
                                   .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id))
                                   .ToListAsync();
                var result = _feedbackAppExcelExporter.ExportToFile(query);
                return DataResult.ResultSuccess(result, "Export Success");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }

    public interface IFeedbackAppExcelExporter
    {
        FileDto ExportToFile(List<FeedbackAppOutputDto> input);
    }

    public class FeedbackAppExcelExporter : NpoiExcelExporterBase, IFeedbackAppExcelExporter
    {
        public FeedbackAppExcelExporter(ITempFileCacheManager tempFileCacheManager): base(tempFileCacheManager) { }
        public FileDto ExportToFile(List<FeedbackAppOutputDto> input)
        {
            return CreateExcelPackage("feedbackApp.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Feedback");
                    AddHeader(sheet,
                        L("Fullname"),
                        L("FeedbackDetail"),
                        L("CreationTime"));
                    AddObjects(sheet, input,
                        _ => _.Name, _ => _.Feedback, _ => _.CreationTime);
                    for (var i = 1; i <= input.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[2], "yyyy-mm-dd");
                    }
                });
        }
    }
}
