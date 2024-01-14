using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [AutoMap(typeof(FeedbackApp))]
    public class FeedbackAppDto : FeedbackApp
    {
    }

    public class GetAllFeedbackAppInput : CommonInputDto
    {
    }

    public class FeedbackAppOutputDto : FeedbackApp
    {
        public string? Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ExportExcelInput
    {
        [CanBeNull] public List<long> Ids { get; set; }
        public int FormId { get; set; }
    }
    public class ImportCitizenExcelDto
    {
        public IFormFile Form { get; set; }
    }
}
