using Abp.AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Yootek.EntityDb;

namespace Yootek.Services
{
    [AutoMap(typeof(MaterialCategory))]
    public class CategoryMaterialDto : MaterialCategory
    {
    }
}
