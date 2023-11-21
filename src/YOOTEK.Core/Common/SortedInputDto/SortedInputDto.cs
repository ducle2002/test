using static IMAX.Common.Enum.CommonENum;

namespace IMAX.Common
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
