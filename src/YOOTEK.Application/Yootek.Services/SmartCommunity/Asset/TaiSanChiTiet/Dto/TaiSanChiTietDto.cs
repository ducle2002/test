using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Yootek.EntityDb;
using Yootek.Common;
using static Yootek.YootekServiceBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Yootek.Application;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity;

namespace Yootek.Services
{
    [AutoMap(typeof(TaiSanChiTiet))]
    public class TaiSanChiTietDto : TaiSanChiTiet
    {
        public string MaHeThongText { get; set; }
        public string NhomTaiSanText { get; set; }
        public string BlockText { get; set; }
        public string ApartmentText { get; set; }
        public string BuildingText { get; set; }
        public string FloorText { get; set; }
        public string HinhThucText
        {
            get
            {
                if (HinhThuc > 0 && HinhThuc <= 5)
                    return QueryMiddleware.GetEnumDesctiption((HinhThucTaiSanChiTietEnum)HinhThuc);
                return "";
            }
        }
        public string TrangThaiText
        {
            get
            {
                if (HinhThuc > 0 && HinhThuc <= 5)
                    return QueryMiddleware.GetEnumDesctiption((TrangThaiTaiSanChiTietEnum)TrangThai);
                return "";
            }
        }
        public string QRAction { get; set; }
    }

    [AutoMap(typeof(TaiSanChiTiet))]
    public class TaiSanChiTietGridDto : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public int HinhThuc { get; set; }
        public long MaHeThongId { get; set; }
        public long NhomTaiSanId { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public long? GiaTriTaiSan { get; set; }       
        public DateTime? NgayKetThuc { get; set; }
        public int? TenantId { get; set; }
        public long? BlockId { get; set; }
        public long? ApartmentId { get; set; }
        public long? BuildingId { get; set; }
        public long? FloorId { get; set; }

        public string NhomTaiSanText { get; set; }
        public string MaHeThongText { get; set; }
        public string BlockText { get; set; }
        public string ApartmentText { get; set; }
        public string BuildingText { get; set; }
        public string FloorText { get; set; }
        public string HinhThucText
        {
            get
            {
                if (HinhThuc > 0 && HinhThuc <= 5)
                    return QueryMiddleware.GetEnumDesctiption((HinhThucTaiSanChiTietEnum)HinhThuc);
                return "";
            }
        }
        public string TrangThaiText
        {
            get
            {
                if (HinhThuc > 0 && HinhThuc <= 5)
                    return QueryMiddleware.GetEnumDesctiption((TrangThaiTaiSanChiTietEnum)TrangThai);
                return "";
            }
        }

        
        
        public string QrCode { get; set; }
        public QRObjectDto QRObject { get; set; }
        public string QRAction { get; set; }

    }

    public class GetAllTaiSanChiTietInputDto : CommonInputDto
    {
        public string Code { get; set; }
        public int HinhThuc { get; set; }
        public long NhomTaiSanId { get; set; }
        public int TrangThai { get; set; }
        public long MaHeThongId { get; set; }
        public long ApartmentId { get; set; }
        public FieldSortTaiSanChiTiet? OrderBy { get; set; }
    }
    public class GetAllTaiSanChiTietInputExcelDto
    {
        public string Keyword { get; set; }
        public string Code { get; set; }
        public int HinhThuc { get; set; }
        public long NhomTaiSanId { get; set; }
        public int TrangThai { get; set; }
        public FieldSortTaiSanChiTiet? OrderBy { get; set; }
    }

    [AutoMap(typeof(TaiSanChiTiet))]
    public class TaiSanChiTietExportDto : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public int HinhThuc { get; set; }
        public int TrangThai { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public int? TenantId { get; set; }
        public string NhomTaiSanText { get; set; }
        public string MaHeThongText { get; set; }
        public string BlockText { get; set; }
        public string ApartmentCode { get; set; }
        public string BuildingText { get; set; }
        public string FloorText { get; set; }
        public string MaSoBaoHanh { get; set; }
        public string GhiChu { get; set; }
        public string SoLuong { get; set; }
        public long? GiaTriTaiSan { get; set; }
        public long NhomTaiSanId { get; set; }

        public string HinhThucText
        {
            get
            {
                if (HinhThuc > 0 && HinhThuc <= 5)
                    return QueryMiddleware.GetEnumDesctiption((HinhThucTaiSanChiTietEnum)HinhThuc);
                return "";
            }
        }
        public string TrangThaiText
        {
            get
            {
                if (HinhThuc > 0 && HinhThuc <= 5)
                    return QueryMiddleware.GetEnumDesctiption((TrangThaiTaiSanChiTietEnum)TrangThai);
                return "";
            }
        }

    }
    public enum FieldSortTaiSanChiTiet
    {        
        [FieldName("Code")]
        CODE = 1,
        [FieldName("Title")]
        TITLE = 2,
        [FieldName("NhomTaiSanId")]
        NHOMTAISAN = 3,
        [FieldName("TrangThai")]
        TRANGTHAI = 4,
        [FieldName("CreationTime")]
        CREATION_TIME = 5,

    }
    public enum HinhThucTaiSanChiTietEnum
    {
        [FieldName("BQL")]
        [Description("Ban quản lý")]
        BQL = 1,
        [FieldName("BQT")]
        [Description("Ban quản trị")]
        BQT = 2,
        [FieldName("CDT")]
        [Description("Chủ đầu tư")]
        CDT = 3,
        [FieldName("TSCH")]
        [Description("Tài sản căn hộ")]
        TSCH = 4,
        [FieldName("CDVK")]
        [Description("Các đơn vị khác")]
        CDVK = 5
    }


    public enum TrangThaiTaiSanChiTietEnum
    {
        [FieldName("SD")]
        [Description("Đang sử dụng")]
        SD = 1,
        [FieldName("NSD")]
        [Description("Ngưng sử dụng")]
        NSD = 2,
    }
    public class ImportTaiSanChiTietInput
    {
        public IFormFile File { get; set; }
        public bool Replace { get; set; }
    }
}
