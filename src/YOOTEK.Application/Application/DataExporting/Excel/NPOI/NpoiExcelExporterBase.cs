﻿using Abp.Collections.Extensions;
using Abp.Dependency;
using DocumentFormat.OpenXml.Spreadsheet;
using Yootek.Core.Dto;
using Yootek.Net.MimeTypes;
using Yootek.Storage;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using IndexedColors = NPOI.SS.UserModel.IndexedColors;
using MailKit;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Yootek.DataExporting.Excel.NPOI
{
    public abstract class NpoiExcelExporterBase : YootekServiceBase, ITransientDependency
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;

        private IWorkbook _workbook;

        private readonly Dictionary<string, ICellStyle> _dateCellStyles = new();
        private readonly Dictionary<string, IDataFormat> _dateDateDataFormats = new();

        private ICellStyle GetDateCellStyle(ICell cell, string dateFormat)
        {
            if (_workbook != cell.Sheet.Workbook)
            {
                _dateCellStyles.Clear();
                _dateDateDataFormats.Clear();
                _workbook = cell.Sheet.Workbook;
            }

            if (_dateCellStyles.ContainsKey(dateFormat))
            {
                return _dateCellStyles.GetValueOrDefault(dateFormat);
            }

            var cellStyle = cell.Sheet.Workbook.CreateCellStyle();
            _dateCellStyles.Add(dateFormat, cellStyle);
            return cellStyle;
        }

        private IDataFormat GetDateDataFormat(ICell cell, string dateFormat)
        {
            if (_workbook != cell.Sheet.Workbook)
            {
                _dateDateDataFormats.Clear();
                _workbook = cell.Sheet.Workbook;
            }

            if (_dateDateDataFormats.ContainsKey(dateFormat))
            {
                return _dateDateDataFormats.GetValueOrDefault(dateFormat);
            }

            var dataFormat = cell.Sheet.Workbook.CreateDataFormat();
            _dateDateDataFormats.Add(dateFormat, dataFormat);
            return dataFormat;
        }

        protected NpoiExcelExporterBase(ITempFileCacheManager tempFileCacheManager)
        {
            _tempFileCacheManager = tempFileCacheManager;
        }

        protected FileDto CreateExcelPackage(string fileName, Action<XSSFWorkbook> creator)
        {
            var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
            var workbook = new XSSFWorkbook();

            creator(workbook);

            Save(workbook, file);

            return file;
        }

        protected void AddHeader(ISheet sheet, params string[] headerTexts)
        {
            if (headerTexts.IsNullOrEmpty())
            {
                return;
            }

            sheet.CreateRow(0);

            for (var i = 0; i < headerTexts.Length; i++)
            {
                AddHeader(sheet, i, headerTexts[i]);
            }
        }

        protected void AddHeaderCol(ISheet sheet, int colIndex, int rowIndex, StyleCellDto styleCell, params string[] headerTexts)
        {
            if (headerTexts.IsNullOrEmpty() && colIndex > 0 && rowIndex > 0)
            {
                return;
            }

            for (int i = 0; i < headerTexts.Length; i++)
            {
                AddHeaderCell(sheet, colIndex, i + rowIndex, headerTexts[i], styleCell);
            }
        }
        protected void AddHeaderRow(ISheet sheet, int colIndex, int rowIndex, StyleCellDto styleCell, params string[] headerTexts)
        {
            if (headerTexts.IsNullOrEmpty() && colIndex > 0 && rowIndex > 0)
            {
                return;
            }
            sheet.CreateRow(rowIndex);
            for (int i = 0; i < headerTexts.Length; i++)
            {
                AddHeaderCell(sheet, colIndex + i, rowIndex, headerTexts[i], styleCell);
            }
        }
        private void AddHeader(ISheet sheet, int columnIndex, string headerText)
        {
            var cell = sheet.GetRow(0).CreateCell(columnIndex);
            cell.SetCellValue(headerText);
            var cellStyle = sheet.Workbook.CreateCellStyle();
            var font = sheet.Workbook.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 12;
            cellStyle.SetFont(font);
            cell.CellStyle = cellStyle;
        }
        protected void AddObjects<T>(ISheet sheet, IList<T> items, params Func<T, object>[] propertySelectors)
        {
            if (items.IsNullOrEmpty() || propertySelectors.IsNullOrEmpty())
            {
                return;
            }

            for (var i = 1; i <= items.Count; i++)
            {
                var row = sheet.CreateRow(i);

                for (var j = 0; j < propertySelectors.Length; j++)
                {
                    var cell = row.CreateCell(j);
                    var value = propertySelectors[j](items[i - 1]);
                    if (value != null)
                    {
                        cell.SetCellValue(value.ToString());
                    }
                }
            }
        }


        protected void AddObjectsCol<T>(ISheet sheet, IList<T> items, int colIndex, int rowIndex, params Func<T, object>[] propertySelectors)
        {
            if (items.IsNullOrEmpty() || propertySelectors.IsNullOrEmpty())
            {
                return;
            }

            for (int i = colIndex; i < items.Count + colIndex; i++) // cột i
            {
                for (int j = rowIndex; j < propertySelectors.Length + rowIndex; j++) // hàng j
                {
                    ICell cell = sheet.GetRow(j).CreateCell(i);
                    SetCellValue(cell, FormatCost(propertySelectors[j - rowIndex](items[i - colIndex])));
                }
            }
        }
        protected void Save(XSSFWorkbook excelPackage, FileDto file)
        {
            using (var stream = new MemoryStream())
            {
                excelPackage.Write(stream);
                _tempFileCacheManager.SetFile(file.FileToken, stream.ToArray());
            }
        }
        protected void SetCellDataFormat(ICell cell, string dataFormat)
        {
            if (cell == null)
                return;

            var dateStyle = GetDateCellStyle(cell, dataFormat);
            var format = GetDateDataFormat(cell, dataFormat);

            dateStyle.DataFormat = format.GetFormat(dataFormat);
            cell.CellStyle = dateStyle;
            if (DateTime.TryParse(cell.StringCellValue, out var datetime))
                cell.SetCellValue(datetime);
        }

        protected void CreateCellMerge(ISheet sheet, string value, int firstRow, int lastRow, int firstCol, int lastCol, StyleCellDto styleCell)
        {
            CreateMultipleRow(sheet, firstRow, lastRow);
            IRow row = sheet.GetRow(firstRow);
            ICell cell = row.GetCell(firstCol);
            cell ??= row.CreateCell(firstCol);

            CellRangeAddress celCustomerMerge = new(firstRow, lastRow, firstCol, lastCol);
            sheet.AddMergedRegion(celCustomerMerge);
            ApplyCellStyle(cell, styleCell);
            cell.SetCellValue(value);
        }

        protected void CreateCellString(IRow row, int col, object value)
        {
            var cell = row.CreateCell(col);
            if (value != null)
            {
                cell.SetCellValue(value.ToString());
            }
        }

        protected void CreateCellCost(IRow row, int col, decimal value)
        {
            var cell = row.CreateCell(col);
            if (value > 0)
            {
                cell.SetCellValue(FormatCost(Math.Round(value, 0)));
            }
            else cell.SetCellValue("0");
        }

        protected void CreateCellCost(IRow row, int col, double value)
        {
            var cell = row.CreateCell(col);
            if (value > 0)
            {
                cell.SetCellValue(FormatCost(Math.Round(value, 0)));
            }
            else cell.SetCellValue("0");
        }

        protected void CreateCellMergeHeader(ISheet sheet, IRow aptRow, string value, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            ICell cell = aptRow.CreateCell(firstCol);

            if (!(firstRow == lastRow && lastCol == firstCol))
            {
                CellRangeAddress celAptMerge = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
                sheet.AddMergedRegion(celAptMerge);
            }

            ICellStyle cellStyle = cell.Sheet.Workbook.CreateCellStyle();

            IFont font = cell.Sheet.Workbook.CreateFont();
            font.IsBold = true;
            font.FontName = "Times New Roman";
            cellStyle.SetFont(font);
            cellStyle.Alignment = HorizontalAlignment.Center;

            cellStyle.WrapText = true;

            cell.CellStyle = cellStyle;
            cell.SetCellValue(value);
        }
        
        protected void CreateClsExcel(ISheet sheet, int row, int col, string value, HorizontalAlignment horAlign = HorizontalAlignment.Left, VerticalAlignment verAlign = VerticalAlignment.Center, bool isBold = false, bool isBorder = true, bool isWrapText = false, int fontSize = 12, string fontName = "Times New Roman", bool isMersg = false, int firstRow = 0, int lastRow = 0, int firstCol = 0, int lastCol = 0, object color = null, object bgcolor = null)
        {
            IRow aptRow = sheet.GetRow(row);
            ICell cell = aptRow.CreateCell(col);
            CellRangeAddress celAptMerge = null;
            if (isMersg)
            {
                celAptMerge = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
                sheet.AddMergedRegion(celAptMerge);
            }
            ICellStyle cellStyle = cell.Sheet.Workbook.CreateCellStyle();

            // font
            IFont font = cell.Sheet.Workbook.CreateFont();
            font.IsBold = isBold;
            font.FontHeightInPoints = fontSize;
            font.FontName = fontName;            
            if (color != null)
                font.Color = (short)color;
            cellStyle.SetFont(font);
            if (bgcolor != null)
                cellStyle.FillForegroundColor = (short)bgcolor;
            // alignment
            cellStyle.Alignment = horAlign;
            cellStyle.VerticalAlignment = verAlign;
            // border 

            if (isBorder)
            {
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                if (isMersg)
                {
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, celAptMerge, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, celAptMerge, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, celAptMerge, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, celAptMerge, sheet);
                }   
            }

            // wrap
            cellStyle.WrapText = isWrapText;
            cell.CellStyle = cellStyle;
            cell.SetCellValue(value);

        }



        protected void CreateCellHeader(ISheet sheet, IRow aptRow, string value, int firstCol)
        {

            ICell cell = aptRow.CreateCell(firstCol);

            ICellStyle cellStyle = cell.Sheet.Workbook.CreateCellStyle();

            IFont font = cell.Sheet.Workbook.CreateFont();
            font.IsBold = false;
            font.FontName = "Times New Roman";
            cellStyle.SetFont(font);
            cellStyle.Alignment = HorizontalAlignment.Center;

            cellStyle.WrapText = true;

            cell.CellStyle = cellStyle;
            cell.SetCellValue(value);
        }

        protected void CreateMultipleRow(ISheet sheet, int firstRow, int lastRow)
        {
            for (int i = firstRow; i <= lastRow; i++)
            {
                sheet.CreateRow(i);
            }
        }

        #region style 
        protected void SetAutoSizeColumn(ISheet sheet, int colStart, int colEnd)
        {
            for (int i = colStart; i <= colEnd; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }
        protected void SetColumnStyle(ISheet sheet, int colIndex, int rowIndexStart, int rowIndexEnd, StyleCellDto styleCell)
        {
            if (styleCell.ColumnWidth != null)
            {
                sheet.SetColumnWidth(colIndex, (int)styleCell.ColumnWidth * 256);
            }
            for (int rowIndex = rowIndexStart; rowIndex < rowIndexEnd; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row != null)
                {
                    ICell cell = row.GetCell(rowIndex);

                    // Cell doesn't exist in this row, create it
                    cell ??= row.CreateCell(colIndex);

                    ApplyCellStyle(cell, styleCell);
                }
            }
        }
        protected void SetColumnStyle(ISheet sheet, int colIndex, StyleCellDto styleCell)
        {
            ICellStyle cellStyle = MapCellStyle(sheet, sheet.Workbook.CreateCellStyle(), styleCell);
            sheet.SetDefaultColumnStyle(colIndex, cellStyle);
        }
        protected void SetRowStyle(ISheet sheet, int rowIndex, int colIndexStart, int colIndexEnd, StyleCellDto styleCell)
        {
            IRow row = sheet.GetRow(rowIndex);
            row.HeightInPoints = (int)styleCell.HeightInPoints;
            for (int colIndex = colIndexStart; colIndex < colIndexEnd; colIndex++)
            {
                if (row != null)
                {
                    ICell cell = row.GetCell(colIndex);

                    // Cell doesn't exist in this row, create it
                    cell ??= row.CreateCell(colIndex);

                    ApplyCellStyle(cell, styleCell);
                }
            }
        }
        #endregion

        #region method helpders
        private ICellStyle MapCellStyle(ISheet sheet, ICellStyle cellStyle, StyleCellDto styleCell)
        {
            // font
            IFont font = sheet.Workbook.CreateFont();
            font.IsBold = (bool)styleCell.IsBold;
            font.FontHeightInPoints = (int)styleCell.FontHeightInPoints;
            cellStyle.SetFont(font);

            // alignment
            cellStyle.Alignment = (HorizontalAlignment)styleCell.AlignmentHorizontal;
            cellStyle.VerticalAlignment = (VerticalAlignment)styleCell.AlignmentVertical;

            // color (background + foreground)
            cellStyle.FillForegroundColor = (short)styleCell.FillForegroundColor;
            cellStyle.FillPattern = (FillPattern)styleCell.Pattern;

            // border 
            cellStyle.BorderTop = BorderStyle.Thin;

            // wrap
            cellStyle.WrapText = (bool)styleCell.WrapText;
            return cellStyle;
        }
        public string FormatCost(object value)
        {
            string stringValue = string.Format("{0:#,#.##}", value);
            return string.IsNullOrEmpty(stringValue) ? "0" : stringValue;
        }
        private void SetCellValue(ICell cell, object value)
        {
            cell.SetCellValue(value?.ToString() ?? "");
        }
        public void ApplyCellStyle(ICell cell, StyleCellDto styleCell)
        {
            ICellStyle cellStyle = cell.Sheet.Workbook.CreateCellStyle();

            // font
            IFont font = cell.Sheet.Workbook.CreateFont();
            font.IsBold = (bool)styleCell.IsBold;
            font.FontHeightInPoints = (int)styleCell.FontHeightInPoints;
            font.FontName = "Times New Roman";
            cellStyle.SetFont(font);

            // alignment
            cellStyle.Alignment = (HorizontalAlignment)styleCell.AlignmentHorizontal;
            cellStyle.VerticalAlignment = (VerticalAlignment)styleCell.AlignmentVertical;

            // color (background + foreground)
            cellStyle.FillForegroundColor = (short)styleCell.FillForegroundColor;
            cellStyle.FillPattern = (FillPattern)styleCell.Pattern;

            // border 

            cellStyle.BorderBottom = BorderStyle.Thin;

            // wrap
            cellStyle.WrapText = (bool)styleCell.WrapText;

            cell.CellStyle = cellStyle;
        }
        
        public void AddHeaderCell(ISheet sheet, int columnIndex, int rowIndex, string headerText, StyleCellDto styleCell)
        {
            IRow row = sheet.GetRow(rowIndex);
            ICell cell = row.CreateCell(columnIndex);
            ApplyCellStyle(cell, styleCell);
            SetCellValue(cell, headerText);
        }
        #endregion

        #region dto
        public class StyleCellDto
        {
            public int? FontHeightInPoints { get; set; } = 12;
            public int? HeightInPoints { get; set; } = 12;
            public bool? IsBold { get; set; } = false;
            public short? Color { get; set; } = IndexedColors.Black.Index;
            public HorizontalAlignment? AlignmentHorizontal { get; set; } = HorizontalAlignment.Left;
            public VerticalAlignment? AlignmentVertical { get; set; } = VerticalAlignment.Bottom;
            public short? FillForegroundColor { get; set; } = IndexedColors.White.Index;
            public int? ColumnWidth { get; set; } = 5;
            public bool? WrapText { get; set; } = false;
            public FillPattern? Pattern { get; set; } = FillPattern.SolidForeground;
            public BorderDto? Border { get; set; }

            public StyleCellDto()
            {

            }            
            public StyleCellDto(StyleCellDto data)
            {
                FontHeightInPoints = data.FontHeightInPoints;
                HeightInPoints = data.HeightInPoints;
                IsBold = data.IsBold;
                Color = data.Color;
                AlignmentHorizontal = data.AlignmentHorizontal;
                AlignmentVertical = data.AlignmentVertical;
                FillForegroundColor = data.FillForegroundColor;
                ColumnWidth = data.ColumnWidth;
                WrapText = data.WrapText;
                Pattern = data.Pattern;
                Border = data.Border;
            }
        }
        public class BorderDto
        {
            public BorderStyle? BorderTop { get; set; } = BorderStyle.Thin;
            public BorderStyle? BorderBottom { get; set; } = BorderStyle.Thin;
            public BorderStyle? BorderLeft { get; set; } = BorderStyle.Thin;
            public BorderStyle? BorderRight { get; set; } = BorderStyle.Thin;
        }

        #endregion
    }
}
