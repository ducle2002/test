using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using JetBrains.Annotations;
namespace Yootek.Services
{
    [AutoMap(typeof(ERPSeller))]
    public class ERPSellerDto : EntityDto<long>, IMayHaveTenant
    {
        /// <summary>
		/// Tên cửa hàng
		/// </summary>
		public string Title { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Địa chỉ Email
        /// </summary>
        [CanBeNull] public string Email { get; set; }
        /// <summary>
        /// Loại
        /// </summary>
        public int Types { get; set; }
        /// <summary>
        /// Loại hình kinh doanh
        /// </summary>
        public long? BusinessTypeId { get; set; }
        /// <summary>
        /// Đơn vị tiền tệ
        /// </summary>
        public long? CurrencyUnitId { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Tỉnh/ Thành phố
        /// </summary>
        [CanBeNull] public string ProvinceCode { get; set; }
        /// <summary>
        /// Quận/ Huyện
        /// </summary>
        [CanBeNull] public string DistrictCode { get; set; }
        /// <summary>
        /// Phường/ Xã
        /// </summary>
        [CanBeNull] public string WardCode { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public ERPSellerDto()
        {
        }
    }
    [AutoMap(typeof(ERPSeller))]
    public class CreateERPSellerDto
    {
        /// <summary>
		/// Tên cửa hàng
		/// </summary>
		public string Title { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Địa chỉ Email
        /// </summary>
        [CanBeNull] public string Email { get; set; }
        /// <summary>
        /// Loại
        /// </summary>
        public ESellerType Types { get; set; }
        /// <summary>
        /// Loại hình kinh doanh
        /// </summary>
        public long? BusinessTypeId { get; set; }
        /// <summary>
        /// Đơn vị tiền tệ
        /// </summary>
        public long? CurrencyUnitId { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Tỉnh/ Thành phố
        /// </summary>
        [CanBeNull] public string ProvinceCode { get; set; }
        /// <summary>
        /// Quận/ Huyện
        /// </summary>
        [CanBeNull] public string DistrictCode { get; set; }
        /// <summary>
        /// Phường/ Xã
        /// </summary>
        [CanBeNull] public string WardCode { get; set; }
    }
    [AutoMap(typeof(ERPSeller))]
    public class UpdateERPSellerDto : EntityDto<long>
    {
        /// <summary>
		/// Tên cửa hàng
		/// </summary>
		public string Title { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Địa chỉ Email
        /// </summary>
        [CanBeNull] public string Email { get; set; }
        /// <summary>
        /// Loại
        /// </summary>
        public ESellerType Types { get; set; }
        /// <summary>
        /// Loại hình kinh doanh
        /// </summary>
        public long? BusinessTypeId { get; set; }
        /// <summary>
        /// Đơn vị tiền tệ
        /// </summary>
        public long? CurrencyUnitId { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Tỉnh/ Thành phố
        /// </summary>
        [CanBeNull] public string ProvinceCode { get; set; }
        /// <summary>
        /// Quận/ Huyện
        /// </summary>
        [CanBeNull] public string DistrictCode { get; set; }
        /// <summary>
        /// Phường/ Xã
        /// </summary>
        [CanBeNull] public string WardCode { get; set; }
    }
    public class GetListERPSellerInput : CommonInputDto
    {
    }
}
