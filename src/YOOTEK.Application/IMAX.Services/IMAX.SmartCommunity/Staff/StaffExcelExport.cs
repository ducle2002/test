using IMAX.Core.Dto;
using IMAX.DataExporting.Excel.NPOI;
using IMAX.EntityDb;
using IMAX.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IMAX.Services
{
    public interface IStaffExcelExport
    {
        FileDto ExportStaffToExcel(List<Staff> staffList);
    }
    public class StaffExcelExport : NpoiExcelExporterBase, IStaffExcelExport
    {
        public StaffExcelExport(ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
        }
        public FileDto ExportStaffToExcel(List<Staff> staffList)
        {
            return CreateExcelPackage(
                "staff_list.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Staff");

                    AddHeader(
                        sheet,
                        L("UserName"),
                        L("Name"),
                        L("Surname"),
                        L("Email"),
                        L("Birthday")
                    );

                    AddObjects(
                        sheet, staffList,
                        _ => _.AccountName,
                        _ => _.Name,
                        _ => _.Surname,
                        _ => _.Email,
                        _ => _.DateOfBirth?.ToString("yyyy-MM-dd") // Định dạng ngày tháng
                    );
                });
        }

    }
}
