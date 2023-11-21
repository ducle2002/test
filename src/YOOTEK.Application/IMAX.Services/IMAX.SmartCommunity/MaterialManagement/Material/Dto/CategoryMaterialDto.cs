using Abp.AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using IMAX.EntityDb;

namespace IMAX.Services
{
    [AutoMap(typeof(MaterialCategory))]
    public class CategoryMaterialDto : MaterialCategory
    {
    }
}
