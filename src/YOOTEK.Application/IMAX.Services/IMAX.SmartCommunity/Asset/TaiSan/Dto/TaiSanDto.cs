using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using IMAX.EntityDb;
using IMAX.Common;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services
{
    [AutoMap(typeof(TaiSan))]
    public class TaiSanDto : TaiSan
    {
        public string NhomTaiSanText { get; set; }
        public string LoaiTaiSanText { get; set; }
        public string NhaSanXuatText { get; set; }
        public string DonViTinhText { get; set; }
        public string KhoTaiSanText { get; set; }
    }
   
    public class GetAllTaiSanInputDto : CommonInputDto
    {
        public string Code { get; set; }
        public long NhomTaiSanId { get; set; }
        public long LoaiTaiSanId { get; set; }
        public long NhaSanXuatId { get; set; }
        public long DonViTinhId { get; set; }
        public long KhoTaiSanId { get; set; }
        public int TrangThai { get; set; }
        public FieldSortTaiSan? OrderBy { get; set; }
    }
    public class TaiSanNhapXuatDto
    {
        public long Id { get; set; }       
        public int SoLuong { get; set; }       
        public decimal? DonGia { get; set; }
        public string DonViTinh { get; set; }        
        public decimal? ThanhTien { get; set; }       
        public int? TongSoLuong { get; set; }
        public int Type { get; set; }
        public DateTime Created { get; set; }
    }
    public enum FieldSortTaiSan
    {
        [FieldName("Id")]
        ID = 1,
    }

    public class ImportTaiSanInput
    {
        public IFormFile File { get; set; }
    }
}
