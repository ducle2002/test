using Microsoft.AspNetCore.Http;

namespace Yootek.Services.Dto
{
    public class ImportApartmentInput
    {
        public IFormFile File { get; set; }
    }
}
