using System;
using System.ComponentModel.DataAnnotations;

namespace trnservice.Models
{
    public class TrnViewModel
    {
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }
        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "TRN is required.")]
        public string Trn {get; set;}
    }
}
