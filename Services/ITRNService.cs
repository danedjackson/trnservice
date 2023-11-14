using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trnservice.Models;

namespace trnservice.Services
{
    interface ITRNService
    {
        public void SingleTRNValidation(TrnDTO trnDTO);
        public void MultipleTRNValidation(IFormFile formFile);
    }
}
