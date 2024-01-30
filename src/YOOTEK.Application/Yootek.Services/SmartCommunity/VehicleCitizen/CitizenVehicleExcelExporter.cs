using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Services;
using Yootek.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        "Mã khu đô thị",
                        "Mã toà nhà",
                        L("ApartmentCode"),
                        "Khách hàng",
                        "Số thẻ",
                        L("VehicleType"),
                        L("VehicleCode"),
                        L("VehicleName"),
                        "Mã bãi đỗ",
                        L("Description"));
                    AddObjects(sheet, vehicles,
                        _ => _.UrbanCode,
                        _ => _.BuildingCode,
                        _ => _.ApartmentCode,
                        _ => _.OwnerName,
                        _ => _.CardNumber,
                        _ => GetVehicleTypeTxt(_.VehicleType),
                        _ => _.VehicleCode,
                        _ => _.VehicleName,
                        _ => _.ParkingName,
                        _ => _.Description
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
