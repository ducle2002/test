using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace Yootek.EntityDb
{
    [Table("BillStatistics")]
    public class BillStatistic: Entity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        [StringLength(1000)]
        public string ApartmentCode {  get; set; }
        public DateTime Period { get; set; }
        public double TotalManagementPaid { get; set; }
        public double TotalElectrictPaid { get; set; }
        public double TotalWaterPaid { get; set; }
        public double TotalParkingPaid { get; set; }
        public double TotalCarPaid { get; set; }
        public double TotalMotorPaid { get; set; }
        public double TotalBikePaid { get; set; }
        public double TotalOtherVehiclePaid { get; set; }
        public double TotalOtherBillPaid { get; set; }
        public double TotalResidenceBillPaid { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
