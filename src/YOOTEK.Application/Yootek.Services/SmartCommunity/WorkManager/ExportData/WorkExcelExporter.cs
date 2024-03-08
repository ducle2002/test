using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Storage;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using System;
using MimeKit;
using System.Linq;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Reflection.Emit;
using Yootek.EntityDb;

namespace Yootek.Services.ExportData
{
    public interface IWorkExcelExporter
    {
        FileDto ExportToFile(List<WorkExcelDto> items);
    }
    public class WorkExcelExporter : NpoiExcelExporterBase, IWorkExcelExporter
    {
        public WorkExcelExporter(
            ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
        }
        public FileDto ExportToFile(List<WorkExcelDto> items)
        {
            List<OptionItem> statusList = new List<OptionItem>();
            statusList.Add(new OptionItem() { Label = "Mới tạo", Value = 1 });
            statusList.Add(new OptionItem() { Label = "Đang xử lý", Value = 2 });
            statusList.Add(new OptionItem() { Label = "Quá hạn xử lý", Value = 3 });
            statusList.Add(new OptionItem() { Label = "Hoàn thành", Value = 4 });
            statusList.Add(new OptionItem() { Label = "Công việc đóng", Value = 5 });
            return CreateExcelPackage(
                "WorkList" + DateTime.Now.ToString() + ".xlsx",
                excelPackage =>
                {
                    ISheet sheet = excelPackage.CreateSheet("WorkList");
                    string nameArea = string.Empty;
                    int rowIndex = 0;
                    int columnIndex = 0;
                    // header row
                    AddHeaderRow(sheet, columnIndex, rowIndex,
                        new StyleCellDto()
                        {
                            IsBold = true,
                            HeightInPoints = 35,
                            //FillForegroundColor = IndexedColors.Green.Index,
                            AlignmentHorizontal = HorizontalAlignment.Center,
                            AlignmentVertical = VerticalAlignment.Center,
                            Pattern = FillPattern.SolidForeground,
                            Border = new(),
                        },
                        "DANH SÁCH CÔNG VIỆC"
                    );
                    rowIndex += 2;
                    int index = 0;
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
                    foreach (var item in items)
                    {
                        index++;
                        CreateCellMerge(sheet, index + "." + item.Title, rowIndex, rowIndex, columnIndex, columnIndex + 3, new StyleCellDto()
                        {
                            AlignmentHorizontal = HorizontalAlignment.Left,
                            AlignmentVertical = VerticalAlignment.Center,
                            FillForegroundColor = IndexedColors.Grey25Percent.Index,
                            WrapText = true,
                            Border = new(),
                            IsBold = true,
                        });
                        rowIndex++;
                        columnIndex = 0;
                        //Thông tin công việc
                        AddHeaderCol(sheet, columnIndex, rowIndex,
                            boldFont,
                            ("Nội dung"),
                            ("Người tạo"),
                            ("Người xử lý"),
                            ("Người giám sát"),
                            ("Thời gian xử lý"),
                            ("Nhắc việc"),
                            ("Trạng thái công việc"),
                            ("Công việc chi tiết")
                        );
                        //string time = (item.DateStart.HasValue ? item.DateStart.Value.ToString("dd/MM/yyyy HH:mm") : "") + "-" + (item.DateExpected.HasValue ? item.DateExpected.Value.ToString("dd/MM/yyyy HH:mm") : "");
                        //string nhacviec = GetValueFrequency(item.Frequency, item.FrequencyOption);
                        //CreateCellMerge(sheet, item.Content, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        //CreateCellMerge(sheet, item.CreatorUser.FullName, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        //CreateCellMerge(sheet, string.Join(", ", item.RecipientUsers.Select(x => x.FullName)), rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        //CreateCellMerge(sheet, string.Join(", ", item.SupervisorUsers.Select(x => x.FullName)), rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        //CreateCellMerge(sheet, time, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        //CreateCellMerge(sheet, nhacviec, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        //CreateCellMerge(sheet, statusList.Find(x => x.Value == item.Status.Value)?.Label, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        //CreateCellMerge(sheet, "", rowIndex, rowIndex, columnIndex + 1, columnIndex + 3, normalFont); rowIndex++;
                        ////Công việc chi tiết
                        //AddHeaderRow(sheet, columnIndex, rowIndex,
                        // new StyleCellDto()
                        // {
                        //     FillForegroundColor = IndexedColors.Grey25Percent.Index,
                        //     ColumnWidth = 25,
                        //     HeightInPoints = 30,
                        //     IsBold = true,
                        // },
                        // "Tên công việc",
                        // "Mô tả",
                        // "Số lần logtime",
                        // "Logtime hoàn thành"
                        //);
                        //rowIndex++;
                        //foreach (var workDetail in item.ListWorkDetails)
                        //{
                        //    AddHeaderRow(sheet, columnIndex, rowIndex,
                        //     new StyleCellDto()
                        //     {
                        //         FillForegroundColor = IndexedColors.White.Index,
                        //         ColumnWidth = 25,
                        //         HeightInPoints = 30,
                        //         IsBold = false,
                        //     },
                        //     (workDetail.Name),
                        //     (workDetail.Description),
                        //     (item.WorkLogTimes.Count(x => x.Status == LogTimeStatus.COMPLETED && x.WorkDetailId == workDetail.Id).ToString()),
                        //     (item.WorkLogTimes.Count(x => x.WorkDetailId == workDetail.Id).ToString())                             
                        //    );
                        //    rowIndex++;
                        //}
                        rowIndex++;
                    }

                    //// format
                    //SetColumnStyle(sheet, 0, 0, 0, new StyleCellDto()
                    //{
                    //    AlignmentHorizontal = HorizontalAlignment.Center,
                    //    AlignmentVertical = VerticalAlignment.Center,
                    //    ColumnWidth = 25,
                    //    HeightInPoints = 30,
                    //});
                    //SetAutoSizeColumn(sheet, 1, sheet.GetRow(0).LastCellNum);
                });
        }

        private string GetValueFrequency(int? frequency, string frequencyOption)
        {
            List<OptionItem> dayList = new List<OptionItem>();
            dayList.Add(new OptionItem() { Label = "Thứ hai", Value = 0 });
            dayList.Add(new OptionItem() { Label = "Thứ ba", Value = 1 });
            dayList.Add(new OptionItem() { Label = "Thứ tư", Value = 2 });
            dayList.Add(new OptionItem() { Label = "Thứ năm", Value = 3 });
            dayList.Add(new OptionItem() { Label = "Thứ sáu", Value = 4 });
            dayList.Add(new OptionItem() { Label = "Thứ bảy", Value = 5 });
            dayList.Add(new OptionItem() { Label = "Chủ nhật", Value = 6 });
            if (string.IsNullOrEmpty(frequencyOption) || !frequency.HasValue) return "";
            var strReturn = "";
            var data = JsonConvert.DeserializeObject<List<FreQuencyOption>>(frequencyOption);
            switch (frequency)
            {
                //Ngày
                case 1:
                    if (data.Count > 0)
                    {
                        strReturn = "Lặp lại hàng ngày: ";
                        strReturn += string.Join(", ", data.Select(x => x.timeText));
                    }
                    break;
                //Tuần
                case 2:
                    if (data.Count > 0)
                    {
                        strReturn = "Lặp lại hàng tuần: ";
                        strReturn += string.Join(", ", data.Select(x => dayList.Find(y => y.Value == x.dayOfWeek).Label + " " + x.timeText));
                    }
                    break;
                //Tháng
                case 3:
                    if (data.Count > 0)
                    {
                        strReturn = "Lặp lại hàng tháng: ";
                        strReturn += string.Join(", ", data.Select(x => "ngày " + x.day + " vào " + x.timeText));
                    }
                    break;
                //Năm
                case 4:
                    if (data.Count > 0)
                    {
                        strReturn = "Lặp lại hàng năm: ";
                        strReturn += string.Join(", ", data.Select(x => "ngày " + x.day + "-" + x.month + " vào " + x.timeText));
                    }
                    break;
            }
            return strReturn;
        }
    }
    public class FreQuencyOption
    {
        //public int dayOfWeek { get; set; }
        //public string timeText { get; set; }
        //public string time { get; set; }
        //public string month { get; set; }
        //public string day { get; set; }
        public int? dayOfWeek { get; set; }
        public string? time { get; set; }
        public int? month { get; set; }
        public int? day { get; set; }
        public string? timeText { get; set; }
    }
    public class OptionItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }
}
