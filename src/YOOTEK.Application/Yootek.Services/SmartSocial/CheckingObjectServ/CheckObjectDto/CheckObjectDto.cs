using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Yootek.DichVu.CheckingObj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.DichVu.CheckingObjectServ.CheckObjectDto
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
