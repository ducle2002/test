using Abp.Application.Services;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IFeedbackListExcelExporter
    {
        FileDto ExportToFile(List<CitizenReflectDto> userFeedbackDtos);
    }

    public class CitizenReflectListExcelExporter : NpoiExcelExporterBase, IFeedbackListExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public CitizenReflectListExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<CitizenReflectDto> userFeedbackDtos)
        {
            var file = CreateExcelPackage(
                "FeedbackCitizens.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("Phản ánh cư dân"));

                    AddHeader(
                        sheet,
                        L("STT"),
                        L("Người phản ánh"),
                        L("Thời gian tạo"),
                        L("Tên phản ánh"),
                        L("Nội dung phản ánh"),
                        L("Đường dẫn file đính kèm")
                    );

                    AddObjects(
                        sheet, userFeedbackDtos,
                        _ => _.Id,
                        _ => _.FullName,
                        _ => _timeZoneConverter.Convert(_.CreationTime, _abpSession.TenantId, _abpSession.GetUserId()),
                        _ => _.Name,
                        _ => _.Data,
                        _ => _.FileUrl
                        );

                    for (var i = 1; i <= userFeedbackDtos.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[0], "yyyy-mm-dd hh:mm:ss");
                    }

                    for (var i = 0; i < 10; i++)
                    {
                        if (i.IsIn(4, 9)) //Don't AutoFit Parameters and Exception
                        {
                            continue;
                        }

                        sheet.AutoSizeColumn(i);
                    }
                });
            return file;
        }
    }
}
