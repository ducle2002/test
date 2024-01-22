using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Storage;
using NPOI.SS.UserModel;
using System.Collections.Generic;

namespace Yootek.Services.ExportData
{
    public interface ITaiSanChiTietExcelExporter
    {
        FileDto ExportToFile(List<TaiSanChiTietExportDto> items);
    }
    public class TaiSanChiTietExcelExporter : NpoiExcelExporterBase, ITaiSanChiTietExcelExporter
    {
        public TaiSanChiTietExcelExporter(
            ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
        }
        public FileDto ExportToFile(List<TaiSanChiTietExportDto> items)
        {
            return CreateExcelPackage(
                "DanhSachTaiSan.xlsx",
                excelPackage =>
                {
                    ISheet sheet = excelPackage.CreateSheet("DanhSachTaiSan");
                    AddHeader(
                        sheet,
                        ("Mã tài sản"),
                        ("Tên tài sản"),
                        ("Hình thức"),
                        ("Mã hệ thống"),
                        ("Nhóm tài sản"),
                        ("Trạng thái"),
                        ("Block"),
                        ("Mã căn hộ"),
                        ("Mã số bảo hành"),
                        ("Giá trị tài sản"),
                        ("Ngày bắt đầu"),
                        ("Ngày kết thúc"),
                        ("Ghi chú"),
                        ("Tòa nhà"),
                        ("Thuộc tầng"),
                        ("Số lượng")
                    );
                    AddObjects(
                        sheet, items,
                        _ => _.Code,
                        _ => _.Title,
                        _ => _.HinhThucText,
                        _ => _.MaHeThongText,
                        _ => _.NhomTaiSanText,
                        _ => _.TrangThaiText,
                        _ => _.BlockText,
                        _ => _.ApartmentCode,
                        _ => _.MaSoBaoHanh,
                        _ => _.GiaTriTaiSan,
                        _ => _.NgayBatDau.HasValue ? _.NgayBatDau.Value.ToString("dd/MM/yyyy") : "",
                        _ => _.NgayKetThuc.HasValue?_.NgayKetThuc.Value.ToString("dd/MM/yyyy"):"",
                        _ => _.GhiChu,
                        _ => _.BuildingText,
                        _ => _.FloorText,
                        _ => _.SoLuong
                        );

                    for (int i = 0; i < 16; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
