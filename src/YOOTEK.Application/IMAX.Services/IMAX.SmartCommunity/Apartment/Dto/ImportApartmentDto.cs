using Microsoft.AspNetCore.Http;

namespace IMAX.Services.Dto
{
    public class ImportApartmentInput
    {
        public IFormFile File { get; set; }
    }
}
