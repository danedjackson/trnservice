using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace trnservice.Models
{
    public class UserLoginViewModel
    {
        
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
