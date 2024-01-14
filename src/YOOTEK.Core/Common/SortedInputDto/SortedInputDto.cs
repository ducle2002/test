using static Yootek.Common.Enum.CommonENum;

namespace Yootek.Common
{
    public interface ISortedInputDto
    {
        public SortBy SortBy { get; set; }
    }
    public class SortedInputDto : ISortedInputDto
    {
        public SortBy SortBy { get; set; }

        public SortedInputDto()
        {
            SortBy = SortBy.ASC;
        }
    }
}
