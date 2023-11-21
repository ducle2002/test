﻿using Abp.AutoMapper;
using IMAX.EntityDb;
using System.ComponentModel.DataAnnotations;

namespace IMAX.Services
{
    [AutoMap(typeof(DocumentTypes))]
    public class CreateDocumentTypesInput
    {
        [StringLength(512)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string Icon { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public DocumentScope? Scope { get; set; }
    }
    [AutoMap(typeof(DocumentTypes))]
    public class CreateDocumentTypesByUserInput
    {
        [StringLength(512)]
        public string DisplayName { get; set; }
        [StringLength(2000)]
        public string Icon { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public DocumentScope? Scope { get; set; }
    }
}