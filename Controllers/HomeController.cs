using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using trnservice.Models;
using trnservice.Services;

namespace trnservice.Controllers
{
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
        public async Task<FileResult> Index(TrnDTO trnDTO)
        {
            _logger.LogInformation(trnDTO.DateOfBirth.ToString());
            // Retreive Memory Stream of the TRN Search results
            return _trnService.SingleTRNValidation(trnDTO);
        }

        public IActionResult Success()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
