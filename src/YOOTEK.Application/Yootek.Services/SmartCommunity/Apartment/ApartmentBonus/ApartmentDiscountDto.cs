using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services
{
    [AutoMap(typeof(ApartmentDiscount))]
    public class ApartmentDiscountDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        [StringLength(256)]
        public string Name { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public DateTime? StartPeriod { get; set; }
        public int NumberPeriod { get; set; }
        public DiscountType DiscountType { get; set; }
        public BillType BillType { get; set; }
        public bool IsChecked { get; set; }
        public decimal Value { get; set; }
    }

    public class CreateOrUpdateApartmentDiscountInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public List<ApartmentDiscountDto> Discounts { get; set; }
    }
}
