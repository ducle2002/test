using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Services.Dto;
using Yootek.Services.SmartCommunity.ExcelBill.Dto;
using Yootek.Storage;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yootek.Services
{
    public interface IBillExcelExporter
    {
        FileDto ExportToFile(List<UserBill> bills);
        FileDto ExportMonthlyToFile(List<KeyValuePair<string, List<UserBill>>> bills);
        // FileDto ExportStatisticBill(List<DataStatisticUserBill> dataStatisticUserBills);
        FileDto ExportStatisticBill(List<DataStatisticScope> dataStatisticScopes);

        FileDto ExportAllDetailUserBills(List<ApartmentPaymentBillDetailDto> datas, List<DateTime> listMonths);
    }

    public class BillExcelExporter : NpoiExcelExporterBase, IBillExcelExporter
    {
        public BillExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {
        }

        public FileDto ExportMonthlyToFile(List<KeyValuePair<string, List<UserBill>>> bills)
        {
            return CreateExcelPackage(
                "bills.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Bill");

                    AddHeader(
                        sheet,
                        L("ApartmentCode"),
                        L("CustomerName"),
                        L("BillType"),
                        L("BillCost"),
                        L("BillPeriod"),
                        L("DueDate"),
                        L("Status"),
                        L("TotalAmount")
                    );

                    sheet.ShiftRows(0, 0, 1);

                    var cellStyle = sheet.Workbook.CreateCellStyle();
                    var font = sheet.Workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;
                    cellStyle.SetFont(font);

                    var titleRow = sheet.CreateRow(0);
                    titleRow.CreateCell(0);
                    CellRangeAddress cellMerge = new CellRangeAddress(0, 0, 0, 7);
                    sheet.AddMergedRegion(cellMerge);
                    titleRow.GetCell(0).CellStyle = cellStyle;
                    titleRow.GetCell(0).SetCellValue(bills[0].Value[0].Title);

                    var totalRows = 0;
                    var rowMarker = 2;
                    //double totalPrice = 0;

                    foreach (var b in bills)
                    {
                        var subBillCount = b.Value.Count;
                        var firstRMarker = rowMarker;
                        var finalRMarker = rowMarker + subBillCount - 1;
                        var aptRow = sheet.CreateRow(rowMarker);
                        double totalPriceEach = 0;

                        totalRows += subBillCount;

                        aptRow.CreateCell(0);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celAptMerge = new CellRangeAddress(firstRMarker, finalRMarker, 0, 0);
                            sheet.AddMergedRegion(celAptMerge);
                        }
                        if (b.Value[0].ApartmentCode != null) aptRow.GetCell(0).SetCellValue(b.Value[0].ApartmentCode.Trim());
                        else aptRow.GetCell(0).SetCellValue("");

                        string cName = "";
                        if (b.Value[0].Properties.Contains("customerName"))
                        {
                            cName = JsonConvert.DeserializeObject<Dictionary<String, object>>(b.Value[0].Properties)!["customerName"]?.ToString();
                        }
                        aptRow.CreateCell(1);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celCustomerMerge = new CellRangeAddress(firstRMarker, finalRMarker, 1, 1);
                            sheet.AddMergedRegion(celCustomerMerge);
                        }
                        aptRow.GetCell(1).SetCellValue(cName);

                        aptRow.CreateCell(4);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celDueMerge = new CellRangeAddress(firstRMarker, finalRMarker, 4, 4);
                            sheet.AddMergedRegion(celDueMerge);
                        }
                        aptRow.GetCell(4).SetCellValue(b.Value[0].Period.Value.ToString());
                        SetCellDataFormat(aptRow.GetCell(4), "mm/yyyy");

                        aptRow.CreateCell(5);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celDueMerge = new CellRangeAddress(firstRMarker, finalRMarker, 5, 5);
                            sheet.AddMergedRegion(celDueMerge);
                        }
                        aptRow.GetCell(5).SetCellValue(b.Value[0].DueDate.Value.ToString());
                        SetCellDataFormat(aptRow.GetCell(5), "dd/mm/yyyy");

                        var subBillRowMarker = firstRMarker;

                        foreach (var bi in b.Value)
                        {
                            if (subBillRowMarker == firstRMarker)
                            {
                                var rowSB = sheet.GetRow(subBillRowMarker);

                                rowSB.CreateCell(2);
                                rowSB.GetCell(2).SetCellValue(BillTypeString(bi.BillType));

                                rowSB.CreateCell(3);
                                rowSB.GetCell(3).SetCellValue(bi.LastCost ?? 0);

                                rowSB.CreateCell(6);
                                rowSB.GetCell(6).SetCellValue(BillStatusString(bi.Status));

                                totalPriceEach += bi.LastCost.Value;

                            }
                            else
                            {
                                var rowSB = sheet.CreateRow(subBillRowMarker);

                                rowSB.CreateCell(2);
                                rowSB.GetCell(2).SetCellValue(BillTypeString(bi.BillType));

                                rowSB.CreateCell(3);
                                rowSB.GetCell(3).SetCellValue(bi.LastCost ?? 0);

                                rowSB.CreateCell(6);
                                rowSB.GetCell(6).SetCellValue(BillStatusString(bi.Status));

                                totalPriceEach += bi.LastCost.Value;
                            }
                            subBillRowMarker++;
                        }

                        rowMarker += subBillCount;

                        aptRow.CreateCell(7);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celCustomerMerge = new CellRangeAddress(firstRMarker, finalRMarker, 7, 7);
                            sheet.AddMergedRegion(celCustomerMerge);
                        }
                        aptRow.GetCell(7).SetCellValue(totalPriceEach);

                        //totalPrice += totalPriceEach;
                    }

                    /*var totalRow = sheet.CreateRow(totalRows + 2);
                    totalRow.CreateCell(0);
                    CellRangeAddress celTotalMerge = new CellRangeAddress(totalRows+2, totalRows + 2, 0, 5);
                    sheet.AddMergedRegion(celTotalMerge);
                    totalRow.GetCell(0).CellStyle = cellStyle;
                    totalRow.GetCell(0).SetCellValue(L("TotalAmount"));

                    totalRow.CreateCell(6);
                    totalRow.GetCell(6).SetCellValue(totalPrice);*/
                });
        }

        public FileDto ExportStatisticBill(List<DataStatisticScope> dataStatisticScopes)
        {
            DataStatisticScope dataParent = dataStatisticScopes.FirstOrDefault();
            int numberOfMonth = dataParent.Data.Count;
            List<string> headerList = new() { "KĐT/Tòa nhà", "Tên tiêu chí" };
            headerList.AddRange(dataParent.Data.Keys);

            return CreateExcelPackage(
                "StatisticBill.xlsx",
                excelPackage =>
                {
                    ISheet sheet = excelPackage.CreateSheet("Statistic Bill");
                    string nameArea = string.Empty;

                    // header row
                    AddHeaderRow(sheet, 0, 0,
                        new StyleCellDto()
                        {
                            IsBold = true,
                            HeightInPoints = 35,
                            FillForegroundColor = IndexedColors.Green.Index,
                            AlignmentHorizontal = HorizontalAlignment.Center,
                            AlignmentVertical = VerticalAlignment.Center,
                            Pattern = FillPattern.SolidForeground,
                            Border = new(),
                        },
                        headerList.ToArray()
                    );

                    int rowIndex = 1;
                    // data
                    foreach (var data in dataStatisticScopes)
                    {
                        Dictionary<string, DataStatisticBillTenantDto> dataScope = data.Data;
                        nameArea = data.ScopeName;
                        CreateCellMerge(sheet, nameArea, rowIndex, rowIndex + 31, 0, 0, new StyleCellDto()
                        {
                            AlignmentHorizontal = HorizontalAlignment.Center,
                            AlignmentVertical = VerticalAlignment.Center,
                            FillForegroundColor = rowIndex == 1? IndexedColors.Yellow.Index : IndexedColors.White.Index,
                            WrapText = true,
                            ColumnWidth = 15,
                            Border = new(),
                            HeightInPoints = 30,
                        });
                        var normalFont = new StyleCellDto()
                        {
                            FillForegroundColor = rowIndex == 1 ? IndexedColors.LightYellow.Index : IndexedColors.White.Index,
                            ColumnWidth = 25,
                            HeightInPoints = 30,
                        };
                        var boldFont = new StyleCellDto()
                        {
                            FillForegroundColor = rowIndex == 1 ? IndexedColors.LightYellow.Index : IndexedColors.White.Index,
                            ColumnWidth = 25,
                            HeightInPoints = 30,
                            IsBold = true,
                        };
                        AddHeaderCol(sheet, 1, rowIndex,
                           boldFont,
                           ("Tổng tiền hóa đơn")
                        );
                        AddHeaderCol(sheet, 1, rowIndex + 7,
                          boldFont,
                          ("Tổng tiền công nợ")
                        );
                        AddHeaderCol(sheet, 1, rowIndex + 14,
                          boldFont,
                          ("Tổng tiền hóa đơn thanh toán")
                        );
                        AddHeaderCol(sheet, 1, rowIndex + 21,
                          boldFont,
                          ("Tổng tiền thu hóa đơn")
                        );
                        AddHeaderCol(sheet, 1, rowIndex + 1,
                            normalFont,
                            ("Hóa đơn điện"),
                            ("Hóa đơn nước"),
                            ("Hóa đơn phí quản lý"),
                            ("Hóa đơn phí gửi xe tháng"),
                            ("Hóa đơn phí cư dân"),
                            ("Các hóa đơn khác")
                        );
                        AddHeaderCol(sheet, 1, rowIndex + 8,
                            normalFont,
                            ("Công nợ hóa đơn điện"),
                            ("Công nợ hóa đơn nước"),
                            ("Công nợ hóa đơn phí quản lý"),
                            ("Công nợ hóa đơn phí gửi xe tháng"),
                            ("Công nợ hóa đơn phí cư dân"),
                            ("Công nợ các hóa đơn khác")
                        );
                        AddHeaderCol(sheet, 1, rowIndex + 15,
                            normalFont,
                            ("Hóa đơn điện đã thanh toán"),
                            ("Hóa đơn nước thanh toán"),
                            ("Hóa đơn phí quản lý thanh toán"),
                            ("Hóa đơn phí gửi xe tháng thanh toán"),
                            ("Hóa đơn phí cư dân thanh toán"),
                            ("Các hóa đơn khác thanh toán")
                        );
                        AddHeaderCol(sheet, 1, rowIndex + 22,
                            normalFont,
                            ("Tiền thu trực tiếp"),
                            ("Tiền thu qua chuyển khoản ngân hàng"),
                            ("Tiền thu qua Momo"),
                            ("Tiền thu qua VnPay"),
                            ("Tiền thu qua ZaloPay"),
                            ("Tổng chỉ số điện"),
                            ("Tổng chỉ số nước"),
                            ("Số ô tô"),
                            ("Số xe máy"),
                            ("Số xe đạp")
                        );

                        AddObjectsCol(sheet, dataScope.Values.ToList(), 2, rowIndex,
                            _ => _.TotalCost,
                            _ => _.TotalCostElectric,
                            _ => _.TotalCostWater,
                            _ => _.TotalCostManager,
                            _ => _.TotalCostParking,
                            _ => _.TotalCostResidence,
                            _ => _.TotalCostOther,

                            _ => _.TotalDebt,
                            _ => _.TotalDebtElectric,
                            _ => _.TotalDebtWater,
                            _ => _.TotalDebtManager,
                            _ => _.TotalDebtParking,
                            _ => _.TotalDebtResidence,
                            _ => _.TotalDebtOther,

                            _ => _.TotalPaid,
                            _ => _.TotalPaidElectric,
                            _ => _.TotalPaidWater,
                            _ => _.TotalPaidManager,
                            _ => _.TotalPaidParking,
                            _ => _.TotalPaidResidence,
                            _ => _.TotalPaidOther,

                            _ => _.TotalPaymentIncome,
                            _ => _.TotalPaymentWithDirect,
                            _ => _.TotalPaymentWithBanking,
                            _ => _.TotalPaymentWithMomo,
                            _ => _.TotalPaymentWithVNPay,
                            _ => _.TotalPaymentWithZaloPay,
                            _ => _.TotalEIndex,
                            _ => _.TotalWIndex,
                            _ => _.CarNumber,
                            _ => _.MotorbikeNumber,
                            _ => _.BicycleNumber
                            );
                        rowIndex += 32;
                    }

                    // format
                    SetColumnStyle(sheet, 0, 0, 0, new StyleCellDto()
                    {
                        AlignmentHorizontal = HorizontalAlignment.Center,
                        AlignmentVertical = VerticalAlignment.Center,
                        ColumnWidth = 25,
                        HeightInPoints = 30,
                    });
                    SetAutoSizeColumn(sheet, 1, sheet.GetRow(0).LastCellNum);
                });
        }
        private string BillTypeString(Common.Enum.BillType value)
        {
            switch (value)
            {
                case Common.Enum.BillType.Electric:
                    return L("Electricity");
                case Common.Enum.BillType.Water:
                    return L("Water");
                case Common.Enum.BillType.Parking:
                    return L("Parking");
                case Common.Enum.BillType.Lighting:
                    return L("Lightning");
                case Common.Enum.BillType.Manager:
                    return L("Management");
                case Common.Enum.BillType.Residence:
                    return L("Residency");
                case Common.Enum.BillType.Other:
                    return L("Other");
                default:
                    return "";
            }
        }

        private string BillStatusString(UserBillStatus value)
        {
            switch (value)
            {
                case UserBillStatus.Paid:
                    return L("BillPaid");
                case UserBillStatus.Pending:
                    return L("BillNotPaid");
                case UserBillStatus.Debt:
                    return L("BillDebt");
                case UserBillStatus.WaitForConfirm:
                    return L("WaitForConfirmation");
                default:
                    return "";
            }
        }

        public FileDto ExportToFile(List<UserBill> bills)
        {
            return CreateExcelPackage(
                "bills.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Bill");

                    AddHeader(
                        sheet,
                        L("BillCode"),
                        L("Title"),
                        L("ApartmentCode"),
                        L("CustomerName"),
                        L("BillCost"),
                        L("BillPeriod"),
                        L("DueDate"),
                        L("Status")
                    );

                    AddObjects(
                        sheet, bills,
                        _ => _.Code,
                        _ => _.Title,
                        _ => _.ApartmentCode,
                        _ =>
                        {
                            if (_.Properties.Contains("customerName"))
                            {
                                return JsonConvert.DeserializeObject<Dictionary<String, object>>(_.Properties)!["customerName"];
                            }
                            else return "";
                        },
                        _ => _.LastCost,
                        _ => _.Period,
                        _ => _.DueDate,
                        _ => _.Status
                    );

                    for (var i = 1; i <= bills.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[5], "yyyy-mm-dd");
                        SetCellDataFormat(sheet.GetRow(i).Cells[6], "yyyy-mm-dd");
                    }

                    // for (var i = 0; i < 10; i++)
                    // {
                    //     if (i.IsIn(4, 9)) //Don't AutoFit Parameters and Exception
                    //     {
                    //         continue;
                    //     }
                    //
                    //     sheet.AutoSizeColumn(i);
                    // }
                });
        }


        public FileDto ExportAllDetailUserBills(List<ApartmentPaymentBillDetailDto> data, List<DateTime> listMonths)
        {
            try
            {
                 return CreateExcelPackage(
               "BillDetail.xlsx",
               excelPackage =>
               {
                   var sheet = excelPackage.CreateSheet("BillDetails");

                   for (var i = 0; i < 4; i++)
                   {
                       sheet.SetColumnWidth(i, 18 * 256);
                   }

                   for (var i = 5; i < 100; i++)
                   {
                       sheet.SetColumnWidth(i, 12 * 256);
                   }

                   var headFontNormal = new StyleCellDto()
                   {
                       WrapText = true,
                       HeightInPoints = 30,
                       AlignmentHorizontal = HorizontalAlignment.Center
                   };
                   var headFont10 = new StyleCellDto()
                   {
                       AlignmentHorizontal = HorizontalAlignment.Center,
                       AlignmentVertical = VerticalAlignment.Top,
                       WrapText = true,
                       HeightInPoints = 30,
                       IsBold = true,
                   };

                   var headFont30 = new StyleCellDto()
                   {
                       AlignmentHorizontal = HorizontalAlignment.Center,
                       AlignmentVertical = VerticalAlignment.Top,
                       WrapText = true,
                       HeightInPoints = 30,
                       IsBold = true,
                   };

                   var countMonth = listMonths.Count() - 1;

                   IRow aptRow1 = sheet.CreateRow(0);
                   IRow aptRow2 = sheet.CreateRow(1);
                   IRow aptRow3 = sheet.CreateRow(2);
                   CreateCellMergeHeader(sheet, aptRow1, "Mã căn hộ", 0, 2, 0, 0);
                   CreateCellMergeHeader(sheet, aptRow1, "Diện tích", 0, 2, 1, 1);
                   CreateCellMergeHeader(sheet, aptRow1, "Khu đô thị/tòa nhà", 0, 2, 2, 2);
                   CreateCellMergeHeader(sheet, aptRow1, "Loại căn hộ", 0, 2, 3, 3 );

                   CreateCellMergeHeader(sheet, aptRow1, "Phí quản lý dịch vụ (từ thời điểm bàn giao đến ngày)", 0, 0, 4, 9);
                   CreateCellMergeHeader(sheet, aptRow2, "Nợ đầu tháng", 1, 2, 4, 4);
                   CreateCellMergeHeader(sheet, aptRow2, "Phải thu trong tháng", 1, 2, 5, 5);
                   CreateCellMergeHeader(sheet, aptRow2, "Tổng phải thu", 1, 2, 6, 6);
                   CreateCellMergeHeader(sheet, aptRow2, "Đã thu trong tháng", 1, 2, 7, 7);
                   CreateCellMergeHeader(sheet, aptRow2, "Trả trước", 1, 2, 8, 8);
                   CreateCellMergeHeader(sheet, aptRow2, "Số dư đến cuối tháng", 1, 2, 9, 9);

                   CreateHeaderMhome(sheet, aptRow1, aptRow2, aptRow3, "Phí quản lý dịch vụ", listMonths, 10, 0);
                   CreateHeaderMhome(sheet, aptRow1, aptRow2, aptRow3, "Phí điện", listMonths, 16, 1);
                   CreateHeaderMhome(sheet, aptRow1, aptRow2, aptRow3, "Phí nước", listMonths, 22, 2);
                   CreateHeaderMhome(sheet, aptRow1, aptRow2, aptRow3, "Phí gửi xe tháng", listMonths, 28, 3);
                   CreateHeaderMhome(sheet, aptRow1, aptRow2, aptRow3, "Phí gửi xe ô tô", listMonths, 34, 4);
                   CreateHeaderMhome(sheet, aptRow1, aptRow2, aptRow3, "Phí gửi xe máy/ Xe máy điện/ Xe đạp điện", listMonths, 40, 5);
                   CreateHeaderMhome(sheet, aptRow1, aptRow2, aptRow3, "Phí gửi xe đạp", listMonths, 46, 6);

                   var index = 0;
                   foreach (var item in data)
                   {

                       var row = sheet.CreateRow(index + 3);
                       CreateCellString(row, 0, item.ApartmentCode);
                       CreateCellString(row, 1, item.Area);
                       CreateCellString(row, 2, item.UrbanName + "/ " + item.BuildingName);
                       CreateCellString(row, 3, item.ApartmentTypeName);

                       CreateCellCost(row, 4, item.ManagementDebtAll);
                       CreateCellCost(row, 5, item.ManagementCostAll);
                       CreateCellCost(row, 6, item.ManagementTotalAll);
                       CreateCellCost(row, 7, item.ManagementPaidAll);
                       CreateCellCost(row, 8, item.ManagementPrepayAll);
                       CreateCellCost(row, 9, item.ManagementBalanceAll);

                       CreateCellCost(row, 10, item.ManagementDebt);
                       CreateCellCost(row, 11, item.ManagementCost);
                       CreateCellCost(row, 12, item.ManagementTotal);
                       CreateCellCost(row, 14 + countMonth, item.ManagementPrepay);
                       CreateCellCost(row, 15 + countMonth, item.ManagementBalance);

                       CreateCellCost(row, 16 + countMonth, item.ElectricDebt);
                       CreateCellCost(row, 17 + countMonth, item.ElectricCost);
                       CreateCellCost(row, 18 + countMonth, item.ElectricTotal);
                       CreateCellCost(row, 20 + 2 * countMonth, item.ElectricPrepay);
                       CreateCellCost(row, 21 + 2 * countMonth, item.ElectricBalance);

                       CreateCellCost(row, 22 + 2 * countMonth, item.WaterDebt);
                       CreateCellCost(row, 23 + 2 * countMonth, item.WaterCost);
                       CreateCellCost(row, 24 + 2 * countMonth, item.WaterTotal);
                       CreateCellCost(row, 26 + 3 * countMonth, item.WaterPrepay);
                       CreateCellCost(row, 27 + 3 * countMonth, item.WaterBalance);

                       CreateCellCost(row, 28 + 3 * countMonth, item.ParkingDebt);
                       CreateCellCost(row, 29 + 3 * countMonth, item.ParkingCost);
                       CreateCellCost(row, 30 + 3 * countMonth, item.ParkingTotal);
                       CreateCellCost(row, 32 + 4 * countMonth, item.ParkingPrepay);
                       CreateCellCost(row, 33 + 4 * countMonth, item.ParkingBalance);

                       CreateCellCost(row, 34 + 4 * countMonth, item.CarDebt);
                       CreateCellCost(row, 35 + 4 * countMonth, item.CarCost);
                       CreateCellCost(row, 36 + 4 * countMonth, item.CarTotal);
                       CreateCellCost(row, 38 + 5 * countMonth, item.CarPrepay);
                       CreateCellCost(row, 39 + 5 * countMonth, item.CarBalance);

                       CreateCellCost(row, 40 + 5 * countMonth, item.MotorDebt);
                       CreateCellCost(row, 41 + 5 * countMonth, item.MotorCost);
                       CreateCellCost(row, 42 + 5 * countMonth, item.MotorTotal);
                       CreateCellCost(row, 44 + 6 * countMonth, item.MotorPrepay);
                       CreateCellCost(row, 45 + 6 * countMonth, item.MotorBalance);

                       CreateCellCost(row, 46 + 6 * countMonth, item.BikeDebt);
                       CreateCellCost(row, 47 + 6 * countMonth, item.BikeCost);
                       CreateCellCost(row, 48 + 6 * countMonth, item.BikeTotal);
                       CreateCellCost(row, 50 + 7 * countMonth, item.BikePrepay);
                       CreateCellCost(row, 51 + 7 * countMonth, item.BikeBalance);

                       for (var i = 0; i <= countMonth; i++)
                       {
                           CreateCellCost(row, 13 + i, item.ManagementCellPaidMonths[i]?.TotalPaid ?? 0);
                           CreateCellCost(row, 19 + i + countMonth, item.ElectricCellPaidMonths[i]?.TotalPaid ?? 0);
                           CreateCellCost(row, 25 + i + 2 * countMonth, item.WaterCellPaidMonths[i]?.TotalPaid ?? 0);
                           CreateCellCost(row, 31 + i + 3 * countMonth, item.ParkingCellPaidMonths[i]?.TotalPaid ?? 0);
                           CreateCellCost(row, 37 + i + 4 * countMonth, item.CarCellPaidMonths[i]?.TotalPaid ?? 0);
                           CreateCellCost(row, 43 + i + 5 * countMonth, item.MotorCellPaidMonths[i]?.TotalPaid ?? 0);
                           CreateCellCost(row, 49 + i + 6 * countMonth, item.BikeCellPaidMonths[i]?.TotalPaid ?? 0);
                       }

                       index++;
                   }

               });

            }
            catch (Exception e)
            {
                throw;
            }
        
        }

        private void CreateHeaderMhome(ISheet sheet, IRow aptRow1, IRow aptRow2, IRow aptRow3, string header, List<DateTime> listMonths, int startCol, int number)
        {
            var countMonth = listMonths.Count() - 1;
            CreateCellMergeHeader(sheet, aptRow1, header, 0, 0, startCol + number * countMonth, startCol + 4 + (number + 1) * countMonth);
            CreateCellMergeHeader(sheet, aptRow2, "Nợ đầu tháng", 1, 2, startCol + number * countMonth, startCol + number * countMonth);
            CreateCellMergeHeader(sheet, aptRow2, "Phải thu trong tháng", 1, 2, startCol + 1 + number * countMonth, startCol + 1 + number * countMonth);
            CreateCellMergeHeader(sheet, aptRow2, "Tổng phải thu", 1, 2, startCol + 2 + number * countMonth, startCol + 2 + number * countMonth);
            CreateCellMergeHeader(sheet, aptRow2, "Đã thu trong tháng", 1, 1, startCol + 3 + number * countMonth, startCol + 3 + (number + 1) * countMonth);
            CreateCellMergeHeader(sheet, aptRow2, "Trả trước", 1, 2, startCol + 4 + (number + 1) * countMonth, startCol + 4 + (number + 1) * countMonth);
            CreateCellMergeHeader(sheet, aptRow2, "Số dư đến cuối tháng", 1, 2, startCol + 5 + (number + 1) * countMonth, startCol + 5 + (number + 1) * countMonth);

            for (var i = 0; i <= countMonth; i++)
            {
                CreateCellHeader(sheet, aptRow3, listMonths[i].ToString("MM/yyyy"), startCol + 3 + number * countMonth + i);
            }
        }
    }
}