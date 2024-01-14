using System.ComponentModel.DataAnnotations;

namespace Yootek.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}