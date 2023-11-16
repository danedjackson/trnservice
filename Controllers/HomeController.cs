using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using trnservice.Models;
using trnservice.Services;

namespace trnservice.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITRNService _trnService;

        public HomeController(ILogger<HomeController> logger, ITRNService trnService)
        {
            _logger = logger;
            _trnService = trnService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(TrnViewModel trnDTO)
        {
            if(!ModelState.IsValid)
            {
                return View("Index", trnDTO);
            }
            _logger.LogInformation("Initiating process to search for individual TRN");
            // Retreive Memory Stream of the TRN Search results
            return _trnService.SingleTRNValidation(trnDTO);
        }


        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            _logger.LogInformation("Initiating process to search for multiple TRN");
            if (file == null || file.Length == 0 || !file.FileName.EndsWith(".csv"))
            {
                return Content("No csv file selected");
            }
            return _trnService.MultipleTRNValidation(file);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
