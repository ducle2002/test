using System.Collections.Generic;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Services;
using Yootek.Storage;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.VehicleCitizen
{
    public interface ICitizenVehicleExcelExporter
    {
        FileDto ExportParkingToFile(List<ExportToExcelOutputDto> parkings);
        FileDto ExportCitizenVehicleToFile(List<CitizenVehicleExcelOutputDto> vehicles);
    }
    public class CitizenVehicleExcelExporter : NpoiExcelExporterBase, ICitizenVehicleExcelExporter
    {
        public CitizenVehicleExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }

        public FileDto ExportCitizenVehicleToFile(List<CitizenVehicleExcelOutputDto> vehicles)
        {
            return CreateExcelPackage(
                "citizenVehicles.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Phương tiện");
                    AddHeader(
                        sheet,
                        "Id",
                        "Mã khu đô thị",
                        "Mã toà nhà",
                        L("ApartmentCode"),
                        "Khách hàng",
                        "Số thẻ",
                        L("VehicleType"),
                        L("VehicleCode"),
                        L("VehicleName"),
                        "Mã bãi đỗ",
                        L("Description"),
                        "Phí gửi xe",
                        "Ngày đăng ký",
                        "Ngày hết hạn",
                        "Mã bảng giá",
                        "Trạng thái"
                        );
                    AddObjects(sheet, vehicles,
                        _ => _.Id,
                        _ => _.UrbanCode,
                        _ => _.BuildingCode,
                        _ => _.ApartmentCode,
                        _ => _.OwnerName,
                        _ => _.CardNumber,
                        _ => GetVehicleTypeTxt(_.VehicleType),
                        _ => _.VehicleCode,
                        _ => _.VehicleName,
                        _ => _.ParkingCode,
                        _ => _.Description,
                        _ => _.Cost,
                        _ => _.RegistrationDate.HasValue ? _.RegistrationDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                        _ => _.ExpirationDate.HasValue ? _.ExpirationDate.Value.ToString("dd/MM/yyyy") : string.Empty,
                        _ => _.BillConfigCode,
                        _ => GetVehicleStateTxt(_.State)
                        );
                }
                );
        }

        protected string GetVehicleTypeTxt(VehicleType type)
        {
            switch (type)
            {
                case VehicleType.Car: return L("Car");
                case VehicleType.Motorbike: return L("Motorbike");
                case VehicleType.Bicycle: return L("Bicycle");
                case VehicleType.Other: return L("Other");
                default: return L("Other");
            }
        }
        protected string GetVehicleStateTxt(CitizenVehicleState? state)
        {
            switch (state)
            {
                case CitizenVehicleState.ACCEPTED: return "Hoạt động";
                case CitizenVehicleState.REJECTED: return "Không hoạt động";
                case CitizenVehicleState.OVERDUE: return "Hết hạn";
                default: return "Không hoạt động";
            }
        }

        public FileDto ExportParkingToFile(List<ExportToExcelOutputDto> parkings)
        {
            return CreateExcelPackage(
                "parkingLots.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Bãi đỗ xe");

                    AddHeader(
                        sheet,
                        "Mã khu đo thị",
                        "Mã toà nhà",
                        L("ParkingLotName"),
                        L("ParkingLotCode"),
                        L("Description")
                    );

                    AddObjects(
                        sheet, parkings,
                        _ => _.UrbanCode,
                        _ => _.BuildingCode,
                        _ => _.ParkingName,
                        _ => _.ParkingCode,
                        _ => _.Description
                    );
                });
        }
    }
}
