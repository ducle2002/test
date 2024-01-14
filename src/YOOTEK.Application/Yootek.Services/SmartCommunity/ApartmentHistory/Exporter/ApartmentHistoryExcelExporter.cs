using System;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Storage;
using System.Collections.Generic;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Yootek.Services.Exporter
{
    public interface IApartmentHistoryExcelExporter
    {
        FileDto ExportApartmentHistoryToFile(List<ApartmentHistoryExcelOutput> apartmentHistories);
    }
    
    public class ApartmentHistoryExcelExporter : NpoiExcelExporterBase, IApartmentHistoryExcelExporter
    {
        public ApartmentHistoryExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }

        public FileDto ExportApartmentHistoryToFile(List<ApartmentHistoryExcelOutput> apartmentHistories)
        {
            return CreateExcelPackage(
                "apartmentHistories.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Lịch sử căn hộ");
                    AddHeader(
                        sheet,
                        "Mã căn hộ",
                        "Tiêu đề",
                        "Mô tả",
                        "Loại",
                        "Ảnh",
                        "File",
                        "Người thực hiện",
                        "Số điện thoại người thực hiện",
                        "Email người thực hiện",
                        "Người giám sát",
                        "Số điện thoại người giám sát",
                        "Email người giám sát",
                        "Người nhận",
                        "Số điện thoại người nhận",
                        "Email người nhận",
                        "Chi phí",
                        "Ngày bắt đầu",
                        "Ngày kết thúc");
                    AddObjects(sheet, apartmentHistories,
                        _ => _.ApartmentId,
                        _ => _.Title,
                        _ => _.Description,
                        _ => _.Type,
                        _ => _.ImageUrls,
                        _ => _.FileUrls,
                        _ => _.ExecutorName,
                        _ => _.ExecutorPhone,
                        _ => _.ExecutorEmail,
                        _ => _.SupervisorName,
                        _ => _.SupervisorPhone,
                        _ => _.SupervisorEmail,
                        _ => _.ReceiverName,
                        _ => _.ReceiverPhone,
                        _ => _.ReceiverEmail,
                        _ => _.Cost,
                        _ => _.DateStart,
                        _ => _.DateEnd
                    );
                    // SetColumnStyle(sheet, 0, 0, 0, new StyleCellDto()
                    // {
                    //     AlignmentHorizontal = HorizontalAlignment.Center,
                    //     AlignmentVertical = VerticalAlignment.Center,
                    //     ColumnWidth = 25,
                    // });
                    // SetAutoSizeColumn(sheet, 1, sheet.GetRow(0).LastCellNum);
                    // SetRowStyle(sheet, 0, 0, new StyleCellDto()
                    // {
                    //     AlignmentHorizontal = HorizontalAlignment.Center,
                    //     AlignmentVertical = VerticalAlignment.Center,
                    //     FontBold = true,
                    //     FontColor = IndexedColors.White.Index,
                    //     FillForegroundColor = IndexedColors.Black.Index,
                    //     FillPattern = FillPattern.SolidForeground,
                    // });
                }
            );
        }
    }
}