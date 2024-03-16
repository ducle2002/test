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
    [AutoMap(typeof(ERPBranch))]
    public class ERPBranchDto : EntityDto<long>, IMayHaveTenant
    {
        /// <summary>
		/// Chi nhánh mặc định
		/// </summary>
		public bool? IsDefault { get; set; }
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Mã chi nhánh
        /// </summary>
        [CanBeNull] public string Code { get; set; }
        /// <summary>
        /// Cửa hàng ID
        /// </summary>
        public long SellerId { get; set; }
        /// <summary>
        /// Mã bưu điện
        /// </summary>
        [CanBeNull] public string ZipCode { get; set; }
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
        public ERPBranchDto()
        {
        }
    }
    [AutoMap(typeof(ERPBranch))]
    public class CreateERPBranchDto
    {
        /// <summary>
		/// Chi nhánh mặc định
		/// </summary>
		public bool? IsDefault { get; set; }
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Mã chi nhánh
        /// </summary>
        [CanBeNull] public string Code { get; set; }
        /// <summary>
        /// Cửa hàng ID
        /// </summary>
        public long SellerId { get; set; }
        /// <summary>
        /// Mã bưu điện
        /// </summary>
        [CanBeNull] public string ZipCode { get; set; }
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
    [AutoMap(typeof(ERPBranch))]
    public class UpdateERPBranchDto : EntityDto<long>
    {
        /// <summary>
		/// Chi nhánh mặc định
		/// </summary>
		public bool? IsDefault { get; set; }
        /// <summary>
        /// Tên chi nhánh
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Mã chi nhánh
        /// </summary>
        [CanBeNull] public string Code { get; set; }
        /// <summary>
        /// Cửa hàng ID
        /// </summary>
        public long SellerId { get; set; }
        /// <summary>
        /// Mã bưu điện
        /// </summary>
        [CanBeNull] public string ZipCode { get; set; }
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
    public class GetListERPBranchInput : CommonInputDto
    {
    }
}
