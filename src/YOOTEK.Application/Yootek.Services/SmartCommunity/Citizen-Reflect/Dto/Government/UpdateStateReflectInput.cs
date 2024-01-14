using Abp.Application.Services.Dto;
using System;
using static Yootek.Common.Enum.UserFeedbackEnum;

namespace Yootek.Services
{
    public class UpdateStateReflectInput : EntityDto<long>
    {
        public STATE_FEEDBACK State { get; set; }
        public string Note { get; set; }
        public string FileOfNote { get; set; }
        public DateTime? FinishTime { get; set; }
    }
    public class UpdateStateReflectGovernmentInput : EntityDto<long>
    {
        public int State { get; set; }
        public string Note { get; set; }
        public string FileOfNote { get; set; }
    }

    public class UpdateHandleReportnput : EntityDto<long>
    {
        public string ReflectReport { get; set; }
        public string ReportName { get; set; }
    }
}
