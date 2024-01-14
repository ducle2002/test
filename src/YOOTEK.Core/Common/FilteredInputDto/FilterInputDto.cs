using JetBrains.Annotations;

namespace Yootek.Common
{
    public interface IFilteredInputDto
    {
        [CanBeNull] public string Keyword { get; set; }
    }
    public class FilteredInputDto : IFilteredInputDto
    {
        [CanBeNull] public string Keyword { get; set; }
    }
}
