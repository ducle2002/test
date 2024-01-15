
using System.ComponentModel.DataAnnotations;


namespace Yootek.Users.Dto
{
    public class UpdateProfilePictureInput
    {
        [Required]
        [MaxLength(400)]
        public string FileName { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public class UpdateUserProfilePictureInput
    {
        public string ImageUrl { get; set; }

    }
}
