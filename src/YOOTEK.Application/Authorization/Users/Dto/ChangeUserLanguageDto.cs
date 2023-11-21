using System.ComponentModel.DataAnnotations;

namespace IMAX.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}