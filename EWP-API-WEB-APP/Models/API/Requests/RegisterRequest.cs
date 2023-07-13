using System.ComponentModel.DataAnnotations;

namespace EWP_API_WEB_APP.Models.API.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "You must insert a user name!")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Your name must be at least 3 characters!")]
        public string userName { get; set; }

        [Required(ErrorMessage = "You must insert a valid name.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Your name must be at least 3 characters!")]
        public string name { get; set; }

        [Required(ErrorMessage = "You must insert a valid email.")]
        public string email { get; set; }

        public string password { get; set; }

        public string cellphone { get; set; }

        public int accountype { get; set; }
    }
}
