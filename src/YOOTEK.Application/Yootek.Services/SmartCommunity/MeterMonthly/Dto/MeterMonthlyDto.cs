using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Yootek.Common.Enum;
using Yootek.Services.Dto;
using Yootek.Organizations.Interface;

namespace Yootek.Services
{
    [AutoMap(typeof(MeterMonthly))]
    public class MeterMonthlyDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        public long? MeterId { get; set; }

        public long? MeterTypeId { get; set; }

        [StringLength(1000)] public DateTime? Period { get; set; }
        [StringLength(2000)] public int? Value { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        [CanBeNull] public string CreatorUserName { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        public string Name { get; set; }

        public long? UrbanId { get; set; }

        public long? BuildingId { get; set; }

        [CanBeNull] public string UrbanName { get; set; }
        [CanBeNull] public string BuildingName { get; set; }
        public string ApartmentCode { get; set; }
        public string UrbanCode { get; set; }
        public string BuildingCode { get; set; }
        public long? PriceDeviation { get; set; }
        public int? State { get; set; }
        public BillType BillType { get; set; }
        public string? BillConfig { get; set; }
        public List<GetAllBillConfigDto>? ListBillConfig { get; set; }
        public string? CustomerName { get; set; }
        public int? FirstValue { get; set; }
        public bool? IsClosed { get; set; }
    }


    public class ExportMeterMonthlyDto
    {
        [CanBeNull] public List<long> Ids { get; set; }
        public long? MeterTypeId { get; set; }
    }
    public class ImportMeterMonthlyInput
    {
        public IFormFile File { get; set; }

    }

    public class CreateBillMeterMonthlyInput
    {
        public int? TenantId { get; set; }
        public long? MeterId { get; set; }
        public long? BuildingId { get; set; }
        //public string? ApartmentCode { get; set; }
        public long UrbanId { get; set; }
        public DateTime Period { get; set; }
        public int Value { get; set; }
        public int? FirstValue { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }
}