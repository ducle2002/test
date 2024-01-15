using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Yootek.Common;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using JetBrains.Annotations;

namespace Yootek.Services
{
    public class GetAllApartmentHistoryDto : Entity<long>
    {
        public int? TenantId { get; set; }
        public long ApartmentId { get; set; }
        
        [CanBeNull] public string ApartmentCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public EApartmentHistoryType Type { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<string>? FileUrls { get; set; }
        public string ExecutorName { get; set; }
        public string? ExecutorPhone { get; set; }
        public string? ExecutorEmail { get; set; }
        public string? SupervisorName { get; set; }
        public string? SupervisorPhone { get; set; }
        public string? SupervisorEmail { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? ReceiverEmail { get; set; }
        public long? Cost { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }

    public class GetAllApartmentHistoryInput : CommonInputDto
    {
        public long ApartmentId { get; set; }
        public EApartmentHistoryType? Type { get; set; }
        public OrderByApartmentHistory? OrderBy { get; set; }
    }
    
    public enum OrderByApartmentHistory
    {
        [YootekServiceBase.FieldNameAttribute("Title")]
        TITLE = 1,
        [YootekServiceBase.FieldNameAttribute("CreationTime")]
        CREATION_TIME = 2,
    }
}