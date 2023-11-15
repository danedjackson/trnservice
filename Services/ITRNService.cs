using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using trnservice.Models;

namespace trnservice.Services
{
    public interface ITRNService
    {
        public FileResult SingleTRNValidation(TrnViewModel trnDTO);
        public FileResult MultipleTRNValidation(IFormFile formFile);
    }
}
