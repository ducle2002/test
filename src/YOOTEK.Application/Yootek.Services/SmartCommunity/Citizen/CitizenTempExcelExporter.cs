using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.EntityDb;
using Yootek.Storage;
using System.Collections.Generic;

namespace Yootek.Services
{
    public interface ICitizenTempExcelExporter
    {
        FileDto ExportToFile(List<CitizenTemp> citizens);
    }
    public class CitizenTempExcelExporter : NpoiExcelExporterBase, ICitizenTempExcelExporter
    {
        public CitizenTempExcelExporter(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager)
        {

        }
        public FileDto ExportToFile(List<CitizenTemp> citizens)
        {
            return CreateExcelPackage("citizenTemps.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Citizens");
                    AddHeader(sheet,
                        L("Fullname"),
                        L("Birthday"),
                        L("Gender"),
                        L("CMND"),
                        L("Email"),
                        L("PhoneNumber"),
                        L("CitizenCode"),
                        L("Roles"),
                        L("ApartmentCode"),
                        "Đời chủ"
                    );
                    AddObjects(sheet, citizens,
                        _ => _.FullName,
                        _ => _.DateOfBirth,
                        _ =>
                        {
                            return GetGenderString(_.Gender);

                        },
                        _ => _.IdentityNumber,
                        _ => _.Email,
                        _ => _.PhoneNumber,
                        _ => _.CitizenCode,
                        _ => { return GetRelationshipText(_.RelationShip); },
                        _ => _.ApartmentCode,
                        _ => "F" + (_.OwnerGeneration > 0 ? _.OwnerGeneration : 0));

                    for (var i = 1; i <= citizens.Count; i++)
                    {
                        //Formatting cells
                        SetCellDataFormat(sheet.GetRow(i).Cells[1], "yyyy-mm-dd");
                    }
                });
        }
        protected string GetRelationshipText(RELATIONSHIP? id)
        {
            switch (id)
            {
                case RELATIONSHIP.Contractor: return L("Host");
                case RELATIONSHIP.Wife: return L("Wife");
                case RELATIONSHIP.Husband: return L("Husband");
                case RELATIONSHIP.Daughter: return L("Daughter");
                case RELATIONSHIP.Son: return L("Son");
                case RELATIONSHIP.Family: return L("Relative");
                case RELATIONSHIP.Father: return L("Father");
                case RELATIONSHIP.Mother: return L("Mother");
                case RELATIONSHIP.Grandfather: return L("Grandfather");
                case RELATIONSHIP.Grandmother: return L("Grandmother");
                case RELATIONSHIP.Guest: return L("Guest");
                case RELATIONSHIP.Wife_Guest: return L("GuestWife");
                case RELATIONSHIP.Husband_Guest: return L("GuestHusband");
                case RELATIONSHIP.Daughter_Guest: return L("GuestDaughter");
                case RELATIONSHIP.Son_Guest: return L("GuestSon");
                case RELATIONSHIP.Family_Guest: return L("GuestRelative");
                case RELATIONSHIP.Father_Guest: return L("GuestFather");
                case RELATIONSHIP.Mother_Guest: return L("GuestMother");
                case RELATIONSHIP.Grandfather_Guest: return L("GuestGrandfather");
                case RELATIONSHIP.Grandmother_Guest: return L("GuestGrandmother");
                case RELATIONSHIP.Staff_Guest: return L("Staff");
                default: return "";
            }
        }

        protected string GetGenderString(string txt)
        {
            switch (txt)
            {
                case "Nam": return L("Male");
                case "Nữ": return L("Female");
                default: return "";
            }
        }
    }
}
