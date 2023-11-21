using Abp.AutoMapper;
using Abp.Domain.Entities;
using IMAX.EntityDb;
using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity;
using JetBrains.Annotations;

namespace IMAX.Services
{
    [AutoMap(typeof(Meter))]
    public class MeterDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string? ApartmentCode { get; set; }
        [StringLength(1000)] public string Name { get; set; }
        [StringLength(2000)] public string QrCode { get; set; }
        [StringLength(2000)] public string Code { get; set; }
        public long? MeterTypeId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [CanBeNull] public string UrbanName { get; set; }
        [CanBeNull] public string BuildingName { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        
        public QRObjectDto QRObject { get; set; }
        
        public string QRAction { get; set; }
    }
}