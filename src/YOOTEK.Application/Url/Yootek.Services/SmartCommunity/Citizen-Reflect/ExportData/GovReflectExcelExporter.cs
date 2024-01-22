using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Services;
using Yootek.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.Citizen_Reflect.ExportData
{
    public interface IGovReflectExcelExporter
    {
        FileDto ExportToExcel(List<CitizenReflectDto> input);
    }
    public class GovReflectExcelExporter : NpoiExcelExporterBase, IGovReflectExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;
        public GovReflectExcelExporter(
            ITimeZoneConverter timeZoneConverter, 
            ITempFileCacheManager tempFileCacheManager, 
            IAbpSession abpSession
        ) : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }
        public FileDto ExportToExcel(List<CitizenReflectDto> input)
        {
            var file = CreateExcelPackage(
                "FeedbackCitizens.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Phản ánh số");

                    AddHeader(
                        sheet,
                        "STT",
                        "Người phản ánh",
                        L("PhoneNo"),
                        L("Address"),
                        "Tên phản ánh",
                        "Nội dung phản ánh",
                        "Thời gian phản ánh",
                        "Phòng ban tiếp nhận",
                        "Người xử lý",
                        "Đường dẫn file đính kèm",
                        L("Status"),
                        L("Rating")
                    );

                    AddObjects(
                        sheet, input,
                        _ => _.Id,
                        _ => _.FullName,
                        _ => _.Phone,
                        _ => _.Address,
                        _ => _.Name,
                        _ => _.Data,
                        _ => _timeZoneConverter.Convert(_.CreationTime, _abpSession.TenantId, _abpSession.GetUserId()),
                        _ => _.OrganizationUnitName,
                        _ => _.HandlerName,
                        _ => _.FileUrl,
                        _ => GetStateValue(_.State.HasValue ? _.State.Value : 0),
                        _ => _.Rating.HasValue ? _.Rating : ""
                        );

                    var cellStyle = sheet.Workbook.CreateCellStyle();
                    cellStyle.WrapText = true;

                    for (var i = 1; i <= input.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[6], "yyyy-mm-dd");
                    }

                    for (var i = 0; i < 11; i++)
                    {
                        if (i.IsIn(9)) continue;
                        sheet.AutoSizeColumn(i);
                    }
                });
            return file;
        }

        protected string GetStateValue(int state)
        {
            switch (state)
            {
                case 1: return L("NewFeedback");
                case 2: return L("Processing");
                case 3: return "Admin đã xử lý";
                case 4: return "Đã xác nhận";
                case 5: return "Đã đánh giá";
                case 6: return "Đã phân công";
                default: return "Phản ánh mới";
            }
        }
    }
}
