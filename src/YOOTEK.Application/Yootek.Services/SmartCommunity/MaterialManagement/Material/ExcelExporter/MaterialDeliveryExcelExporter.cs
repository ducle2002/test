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
    public interface IMaterialDeliveryExcelExporter
    {
        FileDto ExportToFile(List<MaterialDeliveryDto> input, bool isDelivery);
    }
    public class MaterialDeliveryExcelExporter : NpoiExcelExporterBase, IMaterialDeliveryExcelExporter
    {
        public MaterialDeliveryExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }
        public FileDto ExportToFile(List<MaterialDeliveryDto> input, bool isDelivery)
        {
            return CreateExcelPackage("materialDelivery.xlsx", package =>
            {
                var sheet = package.CreateSheet("Delivery");
                AddHeader(sheet,
                    "Mã phiếu",
                    "Ngày",
                    "Đơn vị " + (isDelivery ? "nhận": "trả"),
                    "Số loại tài sản",
                    "Tổng số",
                    "Tổng tiền",
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
                foreach(var delivery in input)
                {
                    var subCount = delivery.MaterialViews.Count;
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
                        CellRangeAddress cellDeptName = new CellRangeAddress(firstRMarker, finalRMarker, 2, 2);
                        sheet.AddMergedRegion(cellDeptName);
                        CellRangeAddress celCountMaterialMerge = new CellRangeAddress(firstRMarker, finalRMarker, 3, 3);
                        sheet.AddMergedRegion(celCountMaterialMerge);
                        CellRangeAddress celTotalAmountMerge = new CellRangeAddress(firstRMarker, finalRMarker, 4, 4);
                        sheet.AddMergedRegion(celTotalAmountMerge);
                        CellRangeAddress celAllPriceMerge = new CellRangeAddress(firstRMarker, finalRMarker, 5, 5);
                        sheet.AddMergedRegion(celAllPriceMerge);
                        CellRangeAddress celDescMerge = new CellRangeAddress(firstRMarker, finalRMarker, 6, 6);
                        sheet.AddMergedRegion(celDescMerge);
                    }

                    row.CreateCell(0);
                    row.GetCell(0).SetCellValue(delivery.Code);
                    row.CreateCell(1);
                    row.GetCell(1).SetCellValue(delivery.DeliveryDate.HasValue ? delivery.DeliveryDate.Value.ToString() : "");
                    row.CreateCell(2);
                    row.GetCell(2).SetCellValue(delivery.DepartmentName);
                    row.CreateCell(3);
                    row.GetCell(3).SetCellValue(delivery.CountMaterial ?? 0);
                    row.CreateCell(4);
                    row.GetCell(4).SetCellValue(delivery.TotalAmount ?? 0);
                    row.CreateCell(5);
                    row.GetCell(5).SetCellValue(delivery.AllPrice ?? 0);
                    row.CreateCell(6);
                    row.GetCell(6).SetCellValue(delivery.Description);

                    SetCellDataFormat(row.GetCell(1), "dd/mm/yyyy");

                    var subRowMarker = firstRMarker;

                    foreach(var view in delivery.MaterialViews)
                    {
                        if(subRowMarker == firstRMarker)
                        {
                            var rowSub = sheet.GetRow(subRowMarker);
                            rowSub.CreateCell(7);
                            rowSub.GetCell(7).SetCellValue(view.MaterialCode);

                            rowSub.CreateCell(8);
                            rowSub.GetCell(8).SetCellValue(view.MaterialName);

                            rowSub.CreateCell(9);
                            rowSub.GetCell(9).SetCellValue(view.UnitName);

                            rowSub.CreateCell(10);
                            rowSub.GetCell(10).SetCellValue(view.Amount ?? 0);

                            rowSub.CreateCell(11);
                            rowSub.GetCell(11).SetCellValue(view.Price ?? 0);

                            rowSub.CreateCell(12);
                            rowSub.GetCell(12).SetCellValue(view.Price.HasValue && view.Amount.HasValue ? view.Price.Value * view.Amount.Value : 0);
                        } else
                        {
                            var rowSub = sheet.CreateRow(subRowMarker);
                            rowSub.CreateCell(7);
                            rowSub.GetCell(7).SetCellValue(view.MaterialCode);

                            rowSub.CreateCell(8);
                            rowSub.GetCell(8).SetCellValue(view.MaterialName);

                            rowSub.CreateCell(9);
                            rowSub.GetCell(9).SetCellValue(view.UnitName);

                            rowSub.CreateCell(10);
                            rowSub.GetCell(10).SetCellValue(view.Amount ?? 0);

                            rowSub.CreateCell(11);
                            rowSub.GetCell(11).SetCellValue(view.Price ?? 0);

                            rowSub.CreateCell(12);
                            rowSub.GetCell(12).SetCellValue(view.Price.HasValue && view.Amount.HasValue ? view.Price.Value * view.Amount.Value : 0);
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
