using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Newtonsoft.Json;

namespace Yootek.Services
{
    [AutoMap(typeof(DigitalServiceOrder))]
    public class DigitalServiceOrderDto : DigitalServiceOrder
    {
        public string ServiceText { get; set; }
        public string CreatorName { get; set; }
        public long? WorkTypeId { get; set; }
        public List<DigitalServiceDetailsGridDto> ArrServiceDetails { get; set; }
    }
    [AutoMap(typeof(DigitalServiceOrder))]
    public class DigitalServiceOrderViewDto : DigitalServiceOrder
    {
        public string ServiceText { get; set; }
        public Citizen CreatorCitizen { get; set; }
        public List<DigitalServiceDetailsGridDto> ArrServiceDetails { get; set; }
        public List<WorkDto> WorkOrder { get; set; }
    }
    [AutoMap(typeof(DigitalServiceOrder))]
    public class DigitalServiceOrderCrearteDto : DigitalServiceOrder
    {
        public List<DigitalServiceDetailsGridDto> ArrServiceDetails { get; set; }
    }
    public class GetAllDigitalServiceOrderInputDto : CommonInputDto
    {
        public string Keyword { get; set; }
        public int Status { get; set; }
        /// <summary>
        /// 1. Mới tạo
        /// 2. Đang xử lý
        /// 3. Hoàn thành
        /// </summary>
        public int StatusTab { get; set; }
        public long ServiceId { get; set; }
        public FieldSortDigitalServiceOrder? OrderBy { get; set; }
    }
    public class UpdateStateDigitalServiceOrderDto
    {
        public long Id { get; set; }
        public TypeActionUpdateStateServiceOrder TypeAction { get; set; }
    }
    public class UpdateFeedbackDigitalServiceOrderDto
    {
        public long Id { get; set; }
        public string ResponseContent { get; set; }
        public long TotalAmount { get; set; }
    }
    public class UpdateRateDigitalServiceOrderDto
    {
        public long Id { get; set; }
        public int RatingScore { get; set; }
        public string Comments { get; set; }
    }
    
    public enum TypeActionUpdateStateServiceOrder
    {
        CREATE = 0,
        RECEIVE = 1,
        START_DOING,
        FEEDBACK,
        COMPLETE,
        CANCEL,
        PAIDED,
        PAIDEDDEBT,
    }
    public enum FieldSortDigitalServiceOrder
    {
        [FieldName("Id")]
        ID = 1,
    }
}
