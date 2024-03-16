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
    [AutoMap(typeof(ERPStaff))]
    public class ERPStaffDto : EntityDto<long>, IMayHaveTenant
    {
        /// <summary>
		/// Họ và tên
		/// </summary>
		public string Fullname { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Cửa hàng ID
        /// </summary>
        public long SellerId { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        [CanBeNull] public string Notes { get; set; }
        /// <summary>
        /// Tenant
        /// </summary>
        public int? TenantId { get; set; }
        public ERPStaffDto()
        {
        }
    }
    [AutoMap(typeof(ERPStaff))]
    public class CreateERPStaffDto
    {
        /// <summary>
		/// Họ và tên
		/// </summary>
		public string Fullname { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Cửa hàng ID
        /// </summary>
        public long SellerId { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        [CanBeNull] public string Notes { get; set; }
    }
    [AutoMap(typeof(ERPStaff))]
    public class UpdateERPStaffDto : EntityDto<long>
    {
        /// <summary>
		/// Họ và tên
		/// </summary>
		public string Fullname { get; set; }
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Cửa hàng ID
        /// </summary>
        public long SellerId { get; set; }
        /// <summary>
        /// Ghi chú
        /// </summary>
        [CanBeNull] public string Notes { get; set; }
    }
    public class GetListERPStaffInput : CommonInputDto
    {
    }
}
