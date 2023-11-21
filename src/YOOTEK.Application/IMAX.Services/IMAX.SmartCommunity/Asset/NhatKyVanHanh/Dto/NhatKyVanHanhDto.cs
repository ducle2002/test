using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
using static IMAX.IMAXServiceBase;
using System.ComponentModel;
using IMAX.Application;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos;
using JetBrains.Annotations;

namespace IMAX.Services
{
    [AutoMap(typeof(NhatKyVanHanh))]
    public class NhatKyVanHanhDto : NhatKyVanHanh
    {
        public string TaiSanText { get; set; }
        public string NguoiKiemTraText { get; set; }
        public string BlockText { get; set; }
        public string ApartmentText { get; set; }
        public string BuildingText { get; set; }
        public string FloorText { get; set; }
        public string TrangThaiText
        {
            get
            {
                if (TrangThai > 0)
                    return QueryMiddleware.GetEnumDesctiption((TrangThaiNhatKyVanHanh)TrangThai);
                return "";
            }
        }
    }

    [AutoMap(typeof(NhatKyVanHanh))]
    public class NhatKyVanHanhCreateDto : NhatKyVanHanh
    {
        public List<long> LstIdTaiSan { get; set; }
        public long MaHeThongId { get; set; }
        public long NhomTaiSanId { get; set; }
    }
    public class GetAllNhatKyVanHanhInputDto : CommonInputDto
    {
        public long TaiSanId { get; set; }
        public long NguoiKiemTraId { get; set; }
        public int TrangThai { get; set; }
        public FieldSortNhatKyVanHanh? OrderBy { get; set; }
    }
    public enum FieldSortNhatKyVanHanh
    {
        [FieldName("Id")]
        ID = 1,
        [FieldName("TaiSanId")]
        TaiSanId = 2,
    }

    public enum TrangThaiNhatKyVanHanh
    {
        [Description("Tạo mới")]
        TaoMoi = 1,
        [Description("Đã xử lý")]
        DaXuLy = 2,
    }
}
