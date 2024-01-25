using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Yootek.Services
{
    public interface IMeterMonthlyExcelExport
    {
        FileDto ExportMeterMonthlyToExcel(List<MeterMonthlyDto> meterMonthlyList);
    }
    public class MeterMonthlyExcelExport : NpoiExcelExporterBase, IMeterMonthlyExcelExport
    {
        public MeterMonthlyExcelExport(ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
        }
        public FileDto ExportMeterMonthlyToExcel(List<MeterMonthlyDto> meterMonthlyList)
        {
            return CreateExcelPackage(
                "meterMonthly_list.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("MeterMonthly");

                    AddHeader(
                        sheet,
                        "Tên",
                        "Kỳ",
                        "Chỉ số cũ",
                        "Chỉ số mới",
                        "Khu đô thị",
                        "Toà nhà",
                        "Mã căn hộ",
                        "Người ghi chỉ số",
                        "Ngày ghi chỉ số"
                    );

                    AddObjects(
                        sheet, meterMonthlyList,
                        _ => _.Name,
                        _ => _.Period,
                        _ => _.FirstValue,
                        _ => _.Value,
                        _ => _.UrbanName,
                        _ => _.BuildingName,
                        _ => _.ApartmentCode,
                        _ => _.CreatorUserName,
                        _ => _.CreationTime

                    );
                    for (var i = 1; i <= meterMonthlyList.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[1], "mm-yyyy");
                        SetCellDataFormat(sheet.GetRow(i).Cells[7], "dd-mm-yyyy hh:mm");
                    }
                });
        }

    }
}
