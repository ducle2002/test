using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Services.Dto;
using Yootek.Storage;

namespace Yootek.Yootek.Services.Yootek.Clb.QnA
{
    public interface IClbAdminQuestionAnswerExcelExporter
    {
        FileDto ExportToFile(List<ClbExportQuestionAnswerDto> input);
    }

    public class ClbAdminQuestionAnswerExcelExporter : NpoiExcelExporterBase, IClbAdminQuestionAnswerExcelExporter
    {
    public ClbAdminQuestionAnswerExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
    {
    }

    public FileDto ExportToFile(List<ClbExportQuestionAnswerDto> input)
    {
        return CreateExcelPackage("QuestionsAndAnswers.xlsx",
            excelPackage =>
            {
                var sheet = excelPackage.CreateSheet("Q&A");
                AddHeader(sheet,
                    L("Creator"),
                    L("Title"),
                    L("Content"),
                    L("ForumResponseTime"),
                    L("Status"),
                    "Các câu trả lời");
                AddObjects(sheet, input,
                    _ => _.CreatorName,
                    _ => _.ThreadTitle,
                    _ => HTMLToText(_.Content),
                    _ => _.CreationTime,
                    _ => QAStatus(_.State),
                    _ => GetChatLog(_.Comments));

                var cellStyle = sheet.Workbook.CreateCellStyle();
                cellStyle.WrapText = true;
                for (var i = 1; i <= input.Count; i++)
                {
                    //Formatting cells
                    SetCellDataFormat(sheet.GetRow(i).Cells[3], "dd-mm-yyyy hh:mm");

                    sheet.GetRow(i).Cells[2].CellStyle = cellStyle;
                    sheet.GetRow(i).Cells[5].CellStyle = cellStyle;
                }

                for (var i = 0; i < 5; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
            });
    }

    protected string QAStatus(int input)
    {
        switch (input)
        {
            case 1: return L("new");
            case 2: return L("accepted");
            case 3: return L("denied");
            case 4: return L("cancelled");
            default: return "";
        }
    }

    // This function converts HTML code to plain text
    // Any step is commented to explain it better
    // You can change or remove unnecessary parts to suite your needs
    protected string HTMLToText(string HTMLCode)
    {
        // Remove new lines since they are not visible in HTML
        //HTMLCode = HTMLCode.Replace("\n", " ");

        // Remove tab spaces
        //HTMLCode = HTMLCode.Replace("\t", " ");

        // Remove multiple white spaces from HTML
        //HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");

        // Remove HEAD tag
        HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", ""
            , RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove any JavaScript
        HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", ""
            , RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Replace special characters like &, <, >, " etc.
        StringBuilder sbHTML = new StringBuilder(HTMLCode);
        // Note: There are many more special characters, these are just
        // most common. You can add new characters in this arrays if needed
        string[] OldWords =
        {
            "&nbsp;", "&amp;", "&quot;", "&lt;",
            "&gt;", "&reg;", "&copy;", "&bull;", "&trade;",
            "&agrave;", "&aacute;", "&atilde;", "&acirc;",
            "&egrave;", "&eacute;", "&ecirc;",
            "&igrave;", "&iacute;",
            "&ograve;", "&oacute;", "&otilde;", "&ocirc;",
            "&ugrave;", "&uacute;",
            "&yacute;",
            "&Agrave;", "&Aacute;", "&Atilde;", "&Acirc;",
            "&Egrave;", "&Eacute;", "&Ecirc;",
            "&Igrave;", "&Iacute;",
            "&Ograve;", "&Oacute;", "&Otilde;", "&Ocirc;",
            "&Ugrave;", "&Uacute;",
            "&Yacute;"
        };
        string[] NewWords =
        {
            " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢",
            "à", "á", "ã", "â",
            "è", "é", "ê",
            "ì", "í",
            "ò", "ó", "õ", "ô",
            "ù", "ú",
            "ý",
            "À", "Á", "Ã", "Â",
            "È", "É", "Ê",
            "Ì", "Í",
            "Ò", "Ó", "Õ", "Ô",
            "Ù", "Ú",
            "Ý"
        };
        for (int i = 0; i < OldWords.Length; i++)
        {
            sbHTML.Replace(OldWords[i], NewWords[i]);
        }

        // Check if there are line breaks (<br>) or paragraph (<p>)
        sbHTML.Replace("<br>", "\n<br>");
        sbHTML.Replace("<br ", "\n<br ");
        sbHTML.Replace("<p ", "\n<p ");

        // Finally, remove all HTML tags and return plain text
        return System.Text.RegularExpressions.Regex.Replace(
            sbHTML.ToString(), "<[^>]*>", "");
    }

    protected string GetChatLog(List<ClbCommentForumDto> input)
    {
        var data = "";
        foreach (var msg in input)
        {
            data = data + msg.CreatorName + ": " + msg.Comment + "\n";
        }

        return data;
    }
    }
}
