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
using DocumentFormat.OpenXml.Drawing;

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
                "WorkList.xlsx",
                excelPackage =>
                {
                    ISheet sheet = excelPackage.CreateSheet("WorkList");
                    sheet.SetColumnWidth(0, 22 * 256);
                    sheet.SetColumnWidth(1, 18 * 256);
                    sheet.SetColumnWidth(2, 18 * 256);
                    sheet.SetColumnWidth(3, 18 * 256);

                    int rowIndex = 0;
                    int columnIndex = 0;
                    // header row
                    IRow aptRow = sheet.CreateRow(rowIndex);
                    CreateClsExcel(sheet, rowIndex, columnIndex, "DANH SÁCH CÔNG VIỆC", HorizontalAlignment.Center, VerticalAlignment.Center, true, false, false, 12, "Times New Roman", true, rowIndex, rowIndex, columnIndex, columnIndex + 3);
                    rowIndex += 2;
                    int index = 0;

                    foreach (var item in items)
                    {
                        index++;
                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, index + ". " + item.Title, HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 12, "Times New Roman", true, rowIndex, rowIndex, columnIndex, columnIndex + 3, null, IndexedColors.LightYellow.Index); rowIndex++;

                        //Thông tin công việc
                        string time = (item.DateStart.HasValue ? item.DateStart.Value.ToString("dd/MM/yyyy HH:mm") : "") + "-" + (item.DateExpected.HasValue ? item.DateExpected.Value.ToString("dd/MM/yyyy HH:mm") : "");
                        string nhacviec = GetValueFrequency(item.Frequency, item.FrequencyOption);

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Nội dung", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, item.Content, HorizontalAlignment.Left, VerticalAlignment.Center, false, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3); rowIndex++;

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Người tạo", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, item.CreatorUser.FullName, HorizontalAlignment.Left, VerticalAlignment.Center, false, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3); rowIndex++;

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Người xử lý", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, string.Join(", ", item.RecipientUsers.Select(x => x.FullName)), HorizontalAlignment.Left, VerticalAlignment.Center, false, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3); rowIndex++;

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Người giám sát", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, string.Join(", ", item.SupervisorUsers.Select(x => x.FullName)), HorizontalAlignment.Left, VerticalAlignment.Center, false, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3); rowIndex++;

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Thời gian xử lý", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, time, HorizontalAlignment.Left, VerticalAlignment.Center, false, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3); rowIndex++;

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Nhắc việc", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, nhacviec, HorizontalAlignment.Left, VerticalAlignment.Center, false, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3); rowIndex++;

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Trạng thái công việc", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, statusList.Find(x => x.Value == item.Status.Value)?.Label, HorizontalAlignment.Left, VerticalAlignment.Center, false, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex + 1, columnIndex + 3); rowIndex++;

                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Danh sách công việc chi tiết", HorizontalAlignment.Left, VerticalAlignment.Center, true, false, false, 11, "Times New Roman", true, rowIndex, rowIndex, columnIndex, columnIndex + 3); rowIndex++;


                        //Công việc chi tiết
                        aptRow = sheet.CreateRow(rowIndex);
                        CreateClsExcel(sheet, rowIndex, columnIndex, "Tên công việc", HorizontalAlignment.Center, VerticalAlignment.Center, true, true, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 1, "Mô tả", HorizontalAlignment.Center, VerticalAlignment.Center, true, true, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 2, "Logtime HT", HorizontalAlignment.Center, VerticalAlignment.Center, true, true, false, 11);
                        CreateClsExcel(sheet, rowIndex, columnIndex + 3, "Số lần logtime", HorizontalAlignment.Center, VerticalAlignment.Center, true, true, false, 11);
                        rowIndex++;
                        foreach (var workDetail in item.ListWorkDetails)
                        {
                            aptRow = sheet.CreateRow(rowIndex);
                            CreateClsExcel(sheet, rowIndex, columnIndex, workDetail.Name, HorizontalAlignment.Left, VerticalAlignment.Center, false, true, false, 11);
                            CreateClsExcel(sheet, rowIndex, columnIndex + 1, workDetail.Description, HorizontalAlignment.Left, VerticalAlignment.Center, false, true, false, 11);
                            CreateClsExcel(sheet, rowIndex, columnIndex + 2, item.WorkLogTimes.Count(x => x.Status == LogTimeStatus.COMPLETED && x.WorkDetailId == workDetail.Id).ToString(), HorizontalAlignment.Center, VerticalAlignment.Center, false, true, false, 11);
                            CreateClsExcel(sheet, rowIndex, columnIndex + 3, item.WorkLogTimes.Count(x => x.WorkDetailId == workDetail.Id).ToString(), HorizontalAlignment.Center, VerticalAlignment.Center, false, true, false, 11);
                            rowIndex++;
                        }
                        rowIndex++;
                    }
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
