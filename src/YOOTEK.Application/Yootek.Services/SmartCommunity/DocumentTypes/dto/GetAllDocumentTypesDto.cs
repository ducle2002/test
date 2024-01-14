using Yootek.Common;
using Yootek.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Yootek.YootekServiceBase;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.DocumentTypes.dto
{
    public class GetAllDocumentTypesDto: CommonInputDto
    {
        public OrderByDocumentType? OrderBy { get; set; }
        public bool? isGlobal { get; set; }
    }
    public enum OrderByDocumentType
    {
        [FieldName("DisplayName")]
        DISPLAY_NAME = 1,
    }
    public class GetAllDocumentTypesByUserDto : CommonInputDto {
        public OrderByDocumentType? OrderBy { get; set; }
    }
}
