using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMAX.App.ServiceHttpClient.Dto
{

    public class OrderDto : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public int Type { get; set; }
        public long OrdererId { get; set; }
        [StringLength(256)]
        public string OrderCode { get; set; }
        public double TotalPrice { get; set; }
        public int State { get; set; }
        public string ProviderName { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Properties { get; set; }
    }

    public class GetOrdersDto
    {
        [CanBeNull] public string Search { get; set; }
        public long? ProviderId { get; set; }
        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? Type { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? SkipCount { get; set; }
        public int? MaxResultCount { get; set; }
    }
}
