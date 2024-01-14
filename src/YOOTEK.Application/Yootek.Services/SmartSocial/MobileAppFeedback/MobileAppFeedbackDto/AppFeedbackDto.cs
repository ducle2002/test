using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Yootek.MobileAppFeedback;

namespace Yootek.Yootek.Services.Yootek.MobileAppFeedback.MobileAppFeedbackDto
{
    public class AppFeedbackDto : AppFeedback
    {
    }

    public class GetAllFeedbackDto : CommonInputDto
    {
        public long? CreatorUserId { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; }
    }
}
