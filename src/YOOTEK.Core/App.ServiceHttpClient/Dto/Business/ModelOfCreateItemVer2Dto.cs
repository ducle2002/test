using System.Collections.Generic;
using JetBrains.Annotations;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class ModelOfCreateItemVer2Dto
    {
        public string? Sku { get; set; }
        public int? Stock { get; set; }
        public double? OriginalPrice { get; set; }
        public double? CurrentPrice { get; set; }
        public List<int> TierIndex { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }
}