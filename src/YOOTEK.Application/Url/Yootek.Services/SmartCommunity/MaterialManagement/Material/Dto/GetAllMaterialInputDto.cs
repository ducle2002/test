using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Yootek.Services
{
    public class GetAllMaterialInputDto : CommonInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public long? LocationId { get; set; }
        public CategoryType? CategoryType { get; set; }
        public long? TypeId { get; set; }
        public string? Status { get; set; }
        public int? Amount { get; set; }

    }

    public class QueryMaterialDto : FilteredInputDto, IMayHaveUrban, IMayHaveBuilding
    {
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public CategoryType? CategoryType { get; set; }
        public int FormQuery { get; set; }
        public long? TypeId { get; set; }
        public long? GroupId { get; set; }
        public long? UnitId { get; set; }
        public long? ProducerId { get; set; }
        public long? LocationId { get; set; }
        public string? Status { get; set; }
        public int? Amount { get; set; }
    }

    public class GetAllCategoryMaterialInputDto : CommonInputDto
    {
        public CategoryType Type { get; set; }
    }
    public class MaterialStatisticsDto
    {
        public int NumberRange { get; set; }
        public QueryCaseMaterial QueryCase { get; set; }
    }
    public class MaterialStatisticsOutput
    {
        //public int? Month { get; set; }
        //public int? Year { get; set; }
        public int? MaterialCount { get; set; }
        public int? InventoryMaterialCount { get; set; }
        public int? UnInventoryMaterialCount { get; set; }
        public long? LocationId { get; set; }
    }
    public class DeliveryStatisticsOutput
    {
        public int? ReceiveCount { get; set; }
        public int? SendCount { get; set; }
    }
    public enum QueryCaseMaterial
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4
    }

    public class MaterialExelExportInput : GetAllMaterialInputDto
    {
        [CanBeNull] public List<long> Ids { get; set; }
    }
}
