using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;


namespace Yootek.Services
{
    [AutoMap(typeof(Documents))]
    public class UpdateDocumentsInput : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(1000)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        [StringLength(2000)]
        public string Link { get; set; }
        public long DocumentTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }

    [AutoMap(typeof(Documents))]
    public class UpdateDocumentsByUserInput : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(1000)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        [StringLength(2000)]
        public string Link { get; set; }

        public long DocumentTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}