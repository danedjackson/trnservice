using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Services.Utils;

namespace trnservice.Areas.Identity.Data
{
    public class ApplicationPlatformUser
    {

        public int PlatformId { get; set; }
        public ApplicationPlatform Platform { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
