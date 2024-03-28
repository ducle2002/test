using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Organizations.Interface;
using Yootek.Services;
using YOOTEK.EntityDb.IMAX.DichVu.DigitalServices;

namespace YOOTEK.Yootek.Services
{
    [AutoMap(typeof(DigitalServicePayment))]
    public class DigitalServicePaymentDto: EntityDto<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        public int? TenantId { get; set; }
        public long OrderId { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public decimal Amount { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [StringLength(2000)]
        public string Note { get; set; }
        public string Properties { get; set; }
        public long ServiceId { get; set; }
        public DigitalServicePaymentMethod Method { get; set; }
        public DigitalServicePaymentStatus Status { get; set; }
        public DateTime CreationTime { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string ServicesText { get; set; }
        public List<DigitalServiceDetailsGridDto> ArrServiceDetails { get; set; }
        public string ServiceDetails { get; set; }
    }

    [AutoMap(typeof(DigitalServicePayment))]
    public class CreateDigitalServicePaymentDto
    {
        public long OrderId { get; set; }
        [RegularExpression(@"^\d+(\.\d{1,2})?$")]
        [Range(0, 9999999999999999.99)]
        public decimal Amount { get; set; }
        [StringLength(2000)]
        public string Note { get; set; }
        public DigitalServicePaymentMethod Method { get; set; }
    }

    [AutoMap(typeof(DigitalServicePayment))]
    public class UpdateDigitalServicePaymentDto : EntityDto<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        public int? TenantId { get; set; }
        public long OrderId { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public decimal Amount { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [StringLength(2000)]
        public string Note { get; set; }
        public string Properties { get; set; }
        public long ServiceId { get; set; }
        public DigitalServicePaymentMethod Method { get; set; }
        public DigitalServicePaymentStatus Status { get; set; }
    }
}
