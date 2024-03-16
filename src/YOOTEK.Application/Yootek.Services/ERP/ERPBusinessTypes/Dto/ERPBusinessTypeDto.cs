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
    [AutoMap(typeof(ERPBusinessType))]
    public class ERPBusinessTypeDto : EntityDto<long>
    {
        /// <summary>
		/// Loại
		/// </summary>
		public int Types { get; set; }
        /// <summary>
        /// Tiêu đề
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        public ERPBusinessTypeDto()
        {
        }
    }
    [AutoMap(typeof(ERPBusinessType))]
    public class CreateERPBusinessTypeDto
    {
        /// <summary>
		/// Loại
		/// </summary>
		public ESellerType Types { get; set; }
        /// <summary>
        /// Tiêu đề
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
    }
    [AutoMap(typeof(ERPBusinessType))]
    public class UpdateERPBusinessTypeDto : EntityDto<long>
    {
        /// <summary>
		/// Loại
		/// </summary>
		public ESellerType Types { get; set; }
        /// <summary>
        /// Tiêu đề
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
    }
    public class GetListERPBusinessTypeInput : CommonInputDto
    {
    }
}
