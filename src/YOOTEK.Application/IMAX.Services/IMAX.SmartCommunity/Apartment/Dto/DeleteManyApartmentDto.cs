using System.Collections.Generic;

namespace IMAX.Services.Dto
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
