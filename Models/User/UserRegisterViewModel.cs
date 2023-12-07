﻿using System.ComponentModel.DataAnnotations;

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

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password *")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password *")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string Message { get; set; }
    }
}