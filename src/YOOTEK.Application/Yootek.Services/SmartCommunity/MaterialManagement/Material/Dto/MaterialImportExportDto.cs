using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(InventoryImportExport))]
    public class InventoryImportExportDto : InventoryImportExport
    {
        public string Id { get; set; }
        public int? TotalAmount { get; set; }
        public int? AllPrice { get; set; }
        public int? CountMaterial { get; set; }
        public string DepartmentName { get; set; }
        public List<MaterialDto> Materials { get; set; }
        public List<ImportExportViewDto> MaterialViews { get; set; }
        public List<Guid> DeleteElements { get; set; }
        public List<Guid> UpdateElements { get; set; }
        public List<Guid> AddElements { get; set; }
    }

    [AutoMap(typeof(InventoryImportExportDto))]
    public class ImportExportViewDto
    {
        public long Id { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        public long? StaffId { get; set; }
        public DateTime? ImportExportDate { get; set; }
        public long? FromInventoryId { get; set; }
        public long? ToInventoryId { get; set; }
        public bool IsImport { get; set; }
        public int Amount { get; set; }
        public long MaterialId { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public int? TenantId { get; set; }
        public Guid Key { get; set; }
        public int? TotalPrice { get; set; }
        public string UnitName { get; set; }
        public string InventoryName { get; set; }
        public int? Price { get; set; }
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public bool? IsCheckDelete { get; set; }
        public bool? IsCheckUpdate { get; set; }
        public bool? IsApproved { get; set; }
    }

    public class GetAllInventoryImportExportInput : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? StaffId { get; set; }
        public long? LocationId { get; set; }
        public bool IsImport { get; set; }
        public bool? IsApproved { get; set; }
    }

    public class ExportInventoryImportExportInput
    {
        public List<string> Ids { get; set; }
        public bool IsImport { get; set; }
    }
    public class ApproveMaterialImportExportInputDto
    {
        public string Id { get; set; }
        public bool IsImport { get; set; }
    }
}
