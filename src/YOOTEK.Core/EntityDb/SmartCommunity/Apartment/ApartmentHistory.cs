#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace Yootek.Yootek.EntityDb.SmartCommunity.Apartment
{
    [Table("ApartmentHistories")]
    public class ApartmentHistory : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long ApartmentId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
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

    public enum EApartmentHistoryType
    {
        Repair = 1,
        ForRent = 2,
        ForSale = 3,
        Violation = 4,
        LostAsset = 5,
        HandoverProperty = 6,
        Other = 7,
        Vehicle = 8,
        Service = 9
    }
}