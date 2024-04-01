using System.Collections.Generic;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Storage;


namespace Yootek.Services.Dto
{
    public interface ICarCardListExcelExporter
    {
        FileDto ExportToFile(List<CarCardDto> carCards);
    }

    public class CarCardListExcelExporter : NpoiExcelExporterBase, ICarCardListExcelExporter
    {
        public CarCardListExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }

        public FileDto ExportToFile(List<CarCardDto> carCards)
        {
            return CreateExcelPackage(
                "carCards.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Thẻ xe");
                    AddHeader(
                        sheet,
                         "Thẻ xe",
                        "Bãi đỗ xe",
                        "khu đô thị",
                        "Toà nhà",
                        L("ApartmentCode"),
                        "Chủ xe",
                        L("VehicleType"),
                        L("VehicleCode"),
                        L("VehicleName"),
                        L("Description"),
                        "Phí gửi xe",
                        "Ngày đăng ký",
                        "Ngày hết hạn",
                        "Trạng thái");
                    AddObjects(sheet, carCards,
                         _ => _.VehicleCardCode,
                        _ => _.ParkingName,
                        _ => _.UrbanName,
                        _ => _.BuildingName,
                        _ => _.ApartmentCode,
                        _ => _.OwnerName,
                        _ => GetVehicleTypeTxt(_.VehicleType),
                        _ => _.VehicleCode,
                        _ => _.VehicleName,
                        _ => _.Description,
                         _ => _.Cost,
                         _ => _.RegistrationDate,
                         _ => _.ExpirationDate,
                         _ => GetVehicleStateTxt(_.State)
                        );
                }
                );
        }

        protected string GetVehicleTypeTxt(VehicleType? type)
        {
            switch (type)
            {
                case VehicleType.Car: return L("Car");
                case VehicleType.Motorbike: return L("Motorbike");
                case VehicleType.Bicycle: return L("Bicycle");
                case VehicleType.ElectricCar: return L("Electric car");
                case VehicleType.ElectricBike: return L("Electric bike");
                case VehicleType.ElectricMotor: return L("Electric motor");
                case VehicleType.Other: return L("Other");
                case null: return "Thẻ chưa được đăng kí xe";
                default: return L("Other");
            }
        }
        protected string GetVehicleStateTxt(CitizenVehicleState? state)
        {
            switch (state)
            {
                case CitizenVehicleState.WAITING: return "Đã đăng kí xe chờ phê duyệt";
                case CitizenVehicleState.ACCEPTED: return "Đã đăng kí xe thành công";
                case CitizenVehicleState.REJECTED: return "Thẻ xe đã hủy";
                case CitizenVehicleState.OVERDUE: return "Thẻ xe đã quá hạn";
                case null: return "Thẻ chưa được đăng kí xe";
                default: return "Thẻ chưa đăng kí xe";
            }
        }
    }
}
