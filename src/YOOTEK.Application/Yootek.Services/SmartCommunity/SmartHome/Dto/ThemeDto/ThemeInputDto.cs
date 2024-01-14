

namespace Yootek.Services.Dto
{

    public class ThemeInput
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long? RoomSmartHomeId { get; set; }

        public long? HomeServerId { get; set; }

        public bool IsActive { get; set; }

        public string ImageUrl { get; set; }

        public int? NumberDevices { get; set; }

        public string Value { get; set; }
    }

    public class GetThemeInput
    {
        public int? FormCase { get; set; }
        public long Id { get; set; }
        public long? RoomId { get; set; }
    }
}
