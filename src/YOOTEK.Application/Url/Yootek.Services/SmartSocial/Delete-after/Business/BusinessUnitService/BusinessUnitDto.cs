using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Yootek.DichVu.BusinessReg;

namespace Yootek.Yootek.Services.Yootek.DichVu.Business.BusinessRegisterService
{
    [AutoMap(typeof(BusinessUnit))]
    public class BusinessUnitDto : BusinessUnit
    {
    }

    public class FindBusinessUnit : CommonInputDto
    {
        public long? CreatorUserId { get; set; }
        public int? FormId { get; set; }
        public long OrganizationUnitId { get; set; }
    }
}
