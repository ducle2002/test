using Abp.Extensions;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Services;
using Yootek.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.City_Vote
{
    public interface ICityVoteExcelExporter
    {
        FileDto ExportToFile(List<CityVoteDto> input);
    }
    public class CityVoteExcelExporter : NpoiExcelExporterBase, ICityVoteExcelExporter
    {
        public CityVoteExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }
        public FileDto ExportToFile(List<CityVoteDto> input)
        {
            return CreateExcelPackage(
                "Survey.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Data");

                    AddHeader(
                        sheet,
                        L("SurveyName"),
                        L("SurveyContent"),
                        L("SurveyCreateDate"),
                        L("SurveyEndDate")
                    );

                    AddObjects(sheet, input,
                        _ => _.Name,
                        _ => OutputVoteOptions(_.VoteOptions, _.TotalUsers, _.UserVoted),
                        _ => _.StartTime,
                        _ => _.FinishTime
                        );

                    //AddObjects(
                    //    sheet, citizenDtos,
                    //    _ => _.Id,
                    //    _ => _.FullName,
                    //    _ => _timeZoneConverter.Convert(_.CreationTime, _abpSession.TenantId, _abpSession.GetUserId()),
                    //    _ => _.Name,
                    //    _ => _.Data,
                    //    _ => _.FileUrl
                    //    );

                    var cellStyle = sheet.Workbook.CreateCellStyle();
                    cellStyle.WrapText = true;

                    for (var i = 1; i <= input.Count; i++)
                    {
                        sheet.GetRow(i).Cells[1].CellStyle = cellStyle;
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[2], "hh:mm dd-mm-yyyy");
                        SetCellDataFormat(sheet.GetRow(i).Cells[3], "hh:mm dd-mm-yyyy");
                    }

                    for (var i = 0; i < 10; i++)
                    {
                        if (i.IsIn(0, 4)) //Don't AutoFit Parameters and Exception
                        {
                            continue;
                        }

                        sheet.AutoSizeColumn(i);
                    }
                });
        }

        //protected string OutputVoteOptions(List<VoteOption> input, long total)
        //{
        //    string data = "";
        //    long undecided = total;
        //    float undecidedPercent = 100;
        //    foreach (var i in input)
        //    {
        //        string choice = i.Option + ": " + i.CountVote + "/" + total + " (" + i.Percent * 100 + "%) \n";
        //        undecided -= i.CountVote;
        //        undecidedPercent -= i.Percent * 100;
        //        data += choice;
        //    }
        //    data = data + L("NotParticipatedSurvey") + ": " + undecided + "/" + total + " (" + undecidedPercent + "%)";
        //    return data;
        //}

        protected string OutputVoteOptions(List<VoteOption> input, long total, List<UserVoteDto>? userVotes)
        {
            string data = "";
            long undecided = total;
            float undecidedPercent = 100;

            for (int i = 0; i < input.Count; i++)
            {
                var voteOption = input[i];

                string choice = $"{voteOption.Option}: {voteOption.CountVote}/{total} ({voteOption.Percent * 100}%) \n";

                if (voteOption.CountVote > 0)
                {
                    for (int v = 0; v < userVotes.Count; v++)
                    {
                        var userVote = userVotes[v];
                        if (userVote.OptionVoteId == voteOption.Id)
                        {
                            choice += $"Mã căn hộ: {userVote.ApartmentCode} ; Họ tên: {userVote.FullName}\n";
                        }

                    }

                }

                data += choice;
                undecided -= voteOption.CountVote;
                undecidedPercent -= voteOption.Percent * 100;
            }

            data += L("NotParticipatedSurvey") + ": " + undecided + "/" + total + " (" + undecidedPercent + "%)";
            return data;
        }


    }
}
