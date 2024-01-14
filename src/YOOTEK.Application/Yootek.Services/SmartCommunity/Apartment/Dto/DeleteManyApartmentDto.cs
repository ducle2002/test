using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    public class DeleteManyApartmentDto
    {
        public List<long> Ids { get; set; }
    }
    public class DeleteManyApartmentByUserDto
    {
        public List<long> Ids { get; set; }
    }
}
