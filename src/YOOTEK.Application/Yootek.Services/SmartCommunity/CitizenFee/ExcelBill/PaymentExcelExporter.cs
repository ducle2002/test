using Abp.Extensions;
using DocumentFormat.OpenXml.Drawing.Charts;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.Common.Enum;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Storage;
using Newtonsoft.Json;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yootek.Services
{
    public interface IPaymentExcelExporter
    {
        FileDto ExportToFile(List<AdminUserBillPaymentOutputDto> payments);
    }
    public class PaymentExcelExporter : NpoiExcelExporterBase, IPaymentExcelExporter
    {
        public PaymentExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {

        }
        public FileDto ExportToFile(List<AdminUserBillPaymentOutputDto> payments)
        {
            return CreateExcelPackage(
                "payments.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("payments");

                    AddHeader(
                        sheet,
                        L("ApartmentCode"),
                        L("CustomerName"),
                        "Thời gian",
                        "Mã thanh toán",
                        "Phương thức thanh toán",
                        "Tổng tiền",
                        "Loại hóa đơn",
                        "Tháng",
                        "Chi phí",
                        "Số tiền thanh toán"
                    );

                    sheet.ShiftRows(0, 0, 1);

                    var cellStyle = sheet.Workbook.CreateCellStyle();
                    var font = sheet.Workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;
                    cellStyle.SetFont(font);

                    var totalRows = 0;
                    var rowMarker = 2;
                    //double totalPrice = 0;

                    foreach (var b in payments)
                    {

                        if (!b.BillPaymentInfo.IsNullOrEmpty())
                        {
                            try
                            {
                                var infos = JsonConvert.DeserializeObject<BillPaymentInfo>(b.BillPaymentInfo);
                                b.BillList = infos.BillList;
                                b.BillListDebt = infos.BillListDebt;
                                b.BillListPrepayment = infos.BillListPrepayment;
                            }
                            catch { }
                        }

                        var listBills = new List<BillPaidDto>();
                        if (b.BillList != null && b.BillList.Count > 0)
                        {
                            listBills =  listBills.Concat(b.BillList).ToList();
                        }

                        if (b.BillListDebt != null && b.BillListDebt.Count > 0)
                        {
                            listBills = listBills.Concat(b.BillListDebt).ToList();
                        }

                        if (b.BillListPrepayment != null && b.BillListPrepayment.Count > 0)
                        {
                            listBills = listBills.Concat(b.BillListPrepayment).ToList();
                        }

                        var subBillCount = listBills.Count;
                        var firstRMarker = rowMarker;
                        var finalRMarker = rowMarker + subBillCount - 1;
                        var aptRow = sheet.CreateRow(rowMarker);
                        double totalPriceEach = 0;

                        totalRows += subBillCount;

                        for(var i =  1; i < 10; i ++)
                        {
                            sheet.SetColumnWidth(i, 15 * 256);
                        }

                        aptRow.CreateCell(0);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celAptMerge = new CellRangeAddress(firstRMarker, finalRMarker, 0, 0);
                            sheet.AddMergedRegion(celAptMerge);
                        }
                        if (b.ApartmentCode != null) aptRow.GetCell(0).SetCellValue(b.ApartmentCode.Trim());
                        else aptRow.GetCell(0).SetCellValue("");

                        string cName = b.CustomerName;
                        
                        aptRow.CreateCell(1);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celCustomerMerge = new CellRangeAddress(firstRMarker, finalRMarker, 1, 1);
                            sheet.AddMergedRegion(celCustomerMerge);
                        }
                        aptRow.GetCell(1).SetCellValue(cName);

                        aptRow.CreateCell(2);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celDueMerge = new CellRangeAddress(firstRMarker, finalRMarker, 2, 2);
                            sheet.AddMergedRegion(celDueMerge);
                        }
                        aptRow.GetCell(2).SetCellValue(b.CreationTime.ToString("dd/MM/yyyy"));

                        aptRow.CreateCell(3);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celCustomerMerge = new CellRangeAddress(firstRMarker, finalRMarker,3, 3);
                            sheet.AddMergedRegion(celCustomerMerge);
                        }
                        aptRow.GetCell(3).SetCellValue(b.PaymentCode);

                        aptRow.CreateCell(4);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celCustomerMerge = new CellRangeAddress(firstRMarker, finalRMarker, 4, 4);
                            sheet.AddMergedRegion(celCustomerMerge);
                        }
                        aptRow.GetCell(4).SetCellValue(GetPaymentMethod(b.Method));

                        aptRow.CreateCell(5);
                        if (finalRMarker > firstRMarker)
                        {
                            CellRangeAddress celCustomerMerge = new CellRangeAddress(firstRMarker, finalRMarker, 5, 5);
                            sheet.AddMergedRegion(celCustomerMerge);
                        }
                        aptRow.GetCell(5).SetCellValue(FormatCost(b.Amount));

                      
                        rowMarker += subBillCount;

                        var subBillRowMarker = firstRMarker;

                        foreach (var bi in listBills)
                        {
                            var rowSB = sheet.GetRow(subBillRowMarker);
                            if (subBillRowMarker != firstRMarker)
                            {
                                rowSB = sheet.CreateRow(subBillRowMarker);

                            }

                            rowSB.CreateCell(6);
                            rowSB.GetCell(6).SetCellValue(BillTypeString(bi.BillType));

                            rowSB.CreateCell(7);
                            rowSB.GetCell(7).SetCellValue(bi.Period.HasValue? bi.Period.Value.ToString("MM/yyyy") : "");

                            rowSB.CreateCell(8);
                            rowSB.GetCell(8).SetCellValue(FormatCost(bi.LastCost));

                            rowSB.CreateCell(9);
                            rowSB.GetCell(9).SetCellValue(FormatCost(bi.PayAmount));

                            totalPriceEach += bi.LastCost.Value;

                            subBillRowMarker++;
                        }



                    }

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

        private string GetPaymentMethod(UserBillPaymentMethod value)
        {
            switch (value)
            {
                case UserBillPaymentMethod.Momo:
                    return "Thanh toán Momo";
                case UserBillPaymentMethod.Banking:
                    return "Thanh toán chuyển khoản";
                case UserBillPaymentMethod.Direct:
                    return "Thanh toán tiền mặt";
                case UserBillPaymentMethod.OnePay:
                    return "Thanh toán VNPay";
                case UserBillPaymentMethod.ZaloPay:
                    return "Thanh toán ZaloPay";
                default:
                    return "";
            }
        }



    }
}
