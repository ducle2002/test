using Yootek.EntityDb;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{

    public class ADJsonPropertyModel
    {
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public ADPropertyType Type { get; set; }
        public int OrderNumber { get; set; }
    }

    public class ADConfigJsonModel
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long ADTypeId { get; set; }
        public List<ADJsonPropertyModel> Properties { get; set; }
        public string Title { get; set; }
    }


    public class ADJsonModel
    {
        public int? TenantId { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long ADTypeId { get; set; }
        public List<ADJsonPropertyModel> Properties { get; set; }
        public int State { get; set; }
    }
}
