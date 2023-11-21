using Abp.AutoMapper;
using IMAX.Common;
using IMAX.IMAX.EntityDb.IMAX.DichVu.BusinessReg;

namespace IMAX.IMAX.Services.IMAX.DichVu.Business.BusinessRegisterService
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
