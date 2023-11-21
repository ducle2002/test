using Abp.AutoMapper;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;
using System.ComponentModel.DataAnnotations;

namespace IMAX.Services
{
    [AutoMap(typeof(Documents))]
    public class CreateDocumentsInput : IMayHaveUrban, IMayHaveBuilding
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
    public class CreateDocumentsByUserInput : IMayHaveUrban, IMayHaveBuilding
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