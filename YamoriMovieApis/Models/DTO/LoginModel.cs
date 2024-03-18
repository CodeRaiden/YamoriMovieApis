using System.ComponentModel.DataAnnotations;

namespace YamoriMovieApis.Models.DTO
{
    public class LoginModel
    {
        //new public string? Email { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
