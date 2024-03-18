using System.ComponentModel.DataAnnotations;

namespace YamoriMovieApis.Models.DTO
{
    public class ChangePasswordModel
    {
        [Required]
        public string UserName { get; set; }
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        //here we will use the compare data anotation to compare the entered NewPassword and the ConfirmNewNewPassword
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}
