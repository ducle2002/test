﻿using Abp.Application.Services.Dto;
using IMAX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    public class GetItemInputDto : CommonInputDto
    {

        public long? Id { get; set; }
        public long? ObjectPartnerId { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; } // Kiểu get dữ liệu
        public int? Type { get; set; }
        public int? TypeGoods { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }

        public bool IsLoadmore { get; set; }

        public string PageSession { get; set; }
        public string Timelife { get; set; }
        public float? RateNumber { get; set; }
        public ComboQuery Combo { get; set; }
    }

    public class ComboQuery
    {
        public bool IsActive { get; set; }
        public int[] Types { get; set; }
        public int[] FormIds { get; set; }
        public bool IsUnionFormId { get; set; }

    }

}
