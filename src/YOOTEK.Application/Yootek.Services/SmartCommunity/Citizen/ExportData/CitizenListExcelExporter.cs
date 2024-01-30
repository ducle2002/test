using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Storage;
using System.Collections.Generic;
using System.Linq;

namespace Yootek.Services.Dto
{
    public interface ICitizenListExcelExporter
    {
        FileDto ExportToFile(List<CitizenDto> citizenDtos);
        FileDto ExportToFile(List<SmarthomeByTenantDto> smarthomes);
    }

    public class CitizenListExcelExporter : NpoiExcelExporterBase, ICitizenListExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public CitizenListExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager)
            : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<CitizenDto> citizenDtos)
        {
            return CreateExcelPackage(
                "verifiedCitizens.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("ResidentTable"));

                    AddHeader(
                        sheet,
                        L("Fullname"),
                        L("ApartmentCode"),
                        L("BuildingCode"),
                        L("Birthday"),
                        L("Gender"),
                        L("Status"),
                        L("PhoneNo"),
                        L("Email"),
                        L("CMND"),
                        L("Nationality")
                    );

                    AddObjects(sheet, citizenDtos,
                        _ => _.FullName,
                        _ => _.ApartmentCode,
                        _ => _.BuildingCode,
                        _ => _.DateOfBirth,
                        _ => _.Gender,
                        _ => _.State,
                        _ => _.PhoneNumber,
                        _ => _.Email,
                        _ => _.CitizenCode,
                        _ => _.Nationality);

                    //AddObjects(
                    //    sheet, citizenDtos,
                    //    _ => _.Id,
                    //    _ => _.FullName,
                    //    _ => _timeZoneConverter.Convert(_.CreationTime, _abpSession.TenantId, _abpSession.GetUserId()),
                    //    _ => _.Name,
                    //    _ => _.Data,
                    //    _ => _.FileUrl
                    //    );

                    for (var i = 1; i <= citizenDtos.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[3], "dd-mm-yyyy");
                    }

                    for (var i = 0; i < 10; i++)
                    {
                        if (i.IsIn(4, 9)) //Don't AutoFit Parameters and Exception
                        {
                            continue;
                        }

                        sheet.AutoSizeColumn(i);
                    }
                });
        }

        public FileDto ExportToFile(List<SmarthomeByTenantDto> smarthomes)
        {
            return CreateExcelPackage("apartments.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet(L("ResidentTable"));

                    AddHeader(
                        sheet,
                        L("ApartmentCode"),
                        L("ApartmentOwner"),
                        L("Contact"),
                        L("ApartmentRenter"),
                        L("ApartmentSize"),
                        L("Nationality"),
                        L("UrbanAreaName")
                    );

                    AddObjects(sheet, smarthomes,
                        _ => _.ApartmentCode,
                        _ => _.Citizens.Where(x => x.RelationShip == RELATIONSHIP.Contractor
                                || x.RelationShip == RELATIONSHIP.Husband).Select(x => x.Name).FirstOrDefault(),
                        _ => _.Citizens.Where(x => x.RelationShip == RELATIONSHIP.Contractor || x.RelationShip == RELATIONSHIP.Husband)
                                .Select(x => x.Contact)
                                .FirstOrDefault(),
                        _ => (_.Citizens.Where(x => x.RelationShip == RELATIONSHIP.Guest).Select(x => x.Name).FirstOrDefault() ??
                            _.Citizens.Where(x => x.RelationShip == RELATIONSHIP.Contractor || x.RelationShip == RELATIONSHIP.Husband)
                            .Select(x => x.Name)
                            .FirstOrDefault()),
                        _ => _.ApartmentAreas,
                        _ => _.Citizens.Where(x => x.RelationShip == RELATIONSHIP.Guest
                            || x.RelationShip == RELATIONSHIP.Contractor
                            || x.RelationShip == RELATIONSHIP.Husband).Select(x => x.Nationality).FirstOrDefault(),
                        _ => _.UrbanName
                        );

                    for (var i = 0; i < 10; i++)
                    {
                        if (i.IsIn(4, 9)) //Don't AutoFit Parameters and Exception
                        {
                            continue;
                        }

                        sheet.AutoSizeColumn(i);
                    }
                });
        }
    }
}
