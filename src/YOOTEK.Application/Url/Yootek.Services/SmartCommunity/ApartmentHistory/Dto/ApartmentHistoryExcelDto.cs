using System;
using System.Collections.Generic;
using Yootek.Yootek.EntityDb.SmartCommunity.Apartment;
using Microsoft.AspNetCore.Http;

namespace Yootek.Services
{
    public class ImportApartmentHistoryExcelDto
    { 
        public IFormFile Form { get; set; }
    }
    
    public class ExportApartmentHistoryExcelDto
    {
        public List<long> ApartmentId { get; set; }
        public EApartmentHistoryType? Type { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }

    public class ApartmentHistoryExcelOutput
    {
        public long ApartmentId { get; set; }
        public string ApartmentCode { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public EApartmentHistoryType Type { get; set; }
        public string? ImageUrls { get; set; }
        public string? FileUrls { get; set; }
        public string ExecutorName { get; set; }
        public string? ExecutorPhone { get; set; }
        public string? ExecutorEmail { get; set; }
        public string? SupervisorName { get; set; }
        public string? SupervisorPhone { get; set; }
        public string? SupervisorEmail { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? ReceiverEmail { get; set; }
        public long? Cost { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}