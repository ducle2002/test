using Abp.AutoMapper;
using IMAX.Common;
using IMAX.IMAX.EntityDb.IMAX.DichVu.CheckingObj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.IMAX.Services.IMAX.DichVu.CheckingObjectServ.CheckObjectDto
{

    [AutoMap(typeof(CheckingObject))]
    public class CheckingObjectDto : CheckingObject
    {
    }

    public class GetAllObjectDto : CommonInputDto
    {
        public long? objectId { get; set; }
        public int? type { get; set; }
    }

}
