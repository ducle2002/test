using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.Common;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public class UserVehicleDto
    {
    }

    public class GetAllVehicleInput 
    {
        public string ApartmentCode { get; set; }
    }

    [AutoMap(typeof(UserVehicle))]
    public class CreateUserVehicleDto
    {
        public string VehicleName { get; set; }
        public VehicleType VehicleType { get; set; }
        public string ImageUrl { get; set; }
        public string VehicleCode { get; set; }
        public string ApartmentCode { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public int? TenantId { get; set; }
    }

    [AutoMap(typeof(UserVehicle))]
    public class UpdateUserVehicleDto: EntityDto<long>
    {
        public string VehicleName { get; set; }
        public VehicleType VehicleType { get; set; }
        public string ImageUrl { get; set; }
        public string VehicleCode { get; set; }
        public string ApartmentCode { get; set; }
        public string Description { get; set; }
        public int? TenantId { get; set; }
    }
}
