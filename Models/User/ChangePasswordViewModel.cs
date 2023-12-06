using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Models.User
{
    public class ChangePasswordViewModel : ForceChangePasswordViewModel
    {

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password *")]
        public string CurrentPassword { get; set; }
    }
}
