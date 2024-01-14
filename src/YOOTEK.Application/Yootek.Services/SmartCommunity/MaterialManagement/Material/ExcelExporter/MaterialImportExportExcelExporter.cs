using Abp.Extensions;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Services;
using Yootek.Storage;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.MaterialManagement.Material.ExcelExporter
{
    public interface IMaterialExportImportExcelExporter
    {
        FileDto ExportToFile(List<InventoryImportExportDto> input);
    }

    public class MaterialExportImportExcelExporter : NpoiExcelExporterBase, IMaterialExportImportExcelExporter
    {
        public MaterialExportImportExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }

        public FileDto ExportToFile(List<InventoryImportExportDto> input)
        {
            return CreateExcelPackage("materialImportExport.xlsx", excelPackage =>
            {
                var sheet = excelPackage.CreateSheet("Materials");
                AddHeader(sheet,
                    "Mã phiếu nhập",
                    "Ngày",
                    "Số loại tài sản",
                    "Tổng số",
                    "Đơn giá",
                    "Lý do",
                    "Mã tài sản",
                    "Tên tài sản",
                    "Đơn vị",
                    "Số lượng",
                    "Đơn giá",
                    "Thành tiền"
                );

                var totalRows = 0;
                var rowMarker = 1;

                foreach (var inventoryImportExport in input)
                {
                    var subCount = inventoryImportExport.MaterialViews.Count;
                    var firstRMarker = rowMarker;
                    var finalRMarker = rowMarker + subCount - 1;
                    var row = sheet.CreateRow(rowMarker);

                    totalRows += subCount;

                    if (finalRMarker > firstRMarker)
                    {
                        CellRangeAddress celCodeMerge = new CellRangeAddress(firstRMarker, finalRMarker, 0, 0);
                        sheet.AddMergedRegion(celCodeMerge);
                        CellRangeAddress celDateMerge = new CellRangeAddress(firstRMarker, finalRMarker, 1, 1);
                        sheet.AddMergedRegion(celDateMerge);
                        CellRangeAddress celCountTypeMerge = new CellRangeAddress(firstRMarker, finalRMarker, 2, 2);
                        sheet.AddMergedRegion(celCountTypeMerge);
                        CellRangeAddress celTotalAmountMerge = new CellRangeAddress(firstRMarker, finalRMarker, 3, 3);
                        sheet.AddMergedRegion(celTotalAmountMerge);
                        CellRangeAddress celPriceMerge = new CellRangeAddress(firstRMarker, finalRMarker, 4, 4);
                        sheet.AddMergedRegion(celPriceMerge);
                        CellRangeAddress celDescMerge = new CellRangeAddress(firstRMarker, finalRMarker, 5, 5);
                        sheet.AddMergedRegion(celDescMerge);
                    }

                    row.CreateCell(0);
                    row.GetCell(0).SetCellValue(inventoryImportExport.Code);
                    row.CreateCell(1);
                    row.GetCell(1).SetCellValue(inventoryImportExport.ImportExportDate.HasValue ? inventoryImportExport.ImportExportDate.Value.ToString() : "");
                    row.CreateCell(2);
                    row.GetCell(2).SetCellValue(inventoryImportExport.CountMaterial ?? 0);
                    row.CreateCell(3);
                    row.GetCell(3).SetCellValue(inventoryImportExport.TotalAmount ?? 0);
                    row.CreateCell(4);
                    row.GetCell(4).SetCellValue(inventoryImportExport.AllPrice ?? 0);
                    row.CreateCell(5);
                    row.GetCell(5).SetCellValue(inventoryImportExport.Description ?? "");

                    SetCellDataFormat(row.GetCell(1), "dd/mm/yyyy");

                    var subRowMarker = firstRMarker;

                    foreach (var i in inventoryImportExport.MaterialViews)
                    {
                        if (subRowMarker == firstRMarker)
                        {
                            var rowSub = sheet.GetRow(subRowMarker);
                            rowSub.CreateCell(6);
                            rowSub.GetCell(6).SetCellValue(i.MaterialCode);

                            rowSub.CreateCell(7);
                            rowSub.GetCell(7).SetCellValue(i.MaterialName);

                            rowSub.CreateCell(8);
                            rowSub.GetCell(8).SetCellValue(i.UnitName);

                            rowSub.CreateCell(9);
                            rowSub.GetCell(9).SetCellValue(i.Amount);

                            rowSub.CreateCell(10);
                            rowSub.GetCell(10).SetCellValue(i.Price ?? 0);

                            rowSub.CreateCell(11);
                            rowSub.GetCell(11).SetCellValue(i.Price.HasValue ? i.Price.Value * i.Amount : 0);
                        }
                        else
                        {
                            var rowSub = sheet.CreateRow(subRowMarker);
                            rowSub.CreateCell(6);
                            rowSub.GetCell(6).SetCellValue(i.MaterialCode);

                            rowSub.CreateCell(7);
                            rowSub.GetCell(7).SetCellValue(i.MaterialName);

                            rowSub.CreateCell(8);
                            rowSub.GetCell(8).SetCellValue(i.UnitName);

                            rowSub.CreateCell(9);
                            rowSub.GetCell(9).SetCellValue(i.Amount);

                            rowSub.CreateCell(10);
                            rowSub.GetCell(10).SetCellValue(i.Price.HasValue ? i.Price.Value : 0);

                            rowSub.CreateCell(11);
                            rowSub.GetCell(11).SetCellValue(i.Price.HasValue ? i.Price.Value * i.Amount : 0);
                        }
                        subRowMarker++;
                    }
                    rowMarker += subCount;
                }

                for (var i = 0; i < 11; i++)
                {
                    if (i.IsIn(1)) //Don't AutoFit Parameters and Exception
                    {
                        continue;
                    }

                    sheet.AutoSizeColumn(i);
                }

            });
        }
    }
}
