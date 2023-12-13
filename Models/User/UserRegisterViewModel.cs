using System.ComponentModel.DataAnnotations;

namespace trnservice.Models
{
    public class UserRegisterViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First Name *")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name *")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Username *")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email Address *")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
