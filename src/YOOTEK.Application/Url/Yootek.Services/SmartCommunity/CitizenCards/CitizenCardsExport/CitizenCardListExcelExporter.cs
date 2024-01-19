using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Storage;
using System;
using System.Collections.Generic;

namespace Yootek.Services
{
    public interface ICitizenCardListExcelExporter
    {
        FileDto ExportToFile(List<CitizenCardDto> citizenCardDtos);
    }
    public class CitizenCardListExcelExporter : NpoiExcelExporterBase, ICitizenCardListExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;
        public CitizenCardListExcelExporter(ITimeZoneConverter timeZoneConverter, IAbpSession abpSession, ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }
        public FileDto ExportToFile(List<CitizenCardDto> citizenCardDtos)
        {
            return CreateExcelPackage(
                "CitizenCardList.xls", excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Danh sách thẻ cư dân");
                    AddHeader(sheet,
                        L("STT"),
                        L("Mã cư dân"));
                    AddObjects(sheet, citizenCardDtos, _ => _.Id, _ => _.CitizenCode);
                    for (var i = 1; i <= citizenCardDtos.Count; i++)
                    {
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
        }
    }
}
