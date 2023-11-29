using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using trnservice.Models;
using trnservice.Services;
using trnservice.Services.Authorize;

namespace trnservice.Controllers
{
    [Authorize(Roles = Role.Admin + ", " + Role.User)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITRNService _trnService;

        public HomeController(ILogger<HomeController> logger, ITRNService trnService)
        {
            _logger = logger;
            _trnService = trnService;
        }

        [HasPermission(Permissions.Enum.CanDoIndividualQuery)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(TrnSearchRequestViewModel trnDTO)
        {
            if(!ModelState.IsValid)
            {
                return View("Index", trnDTO);
            }
            // Retreive Memory Stream of the TRN Search results
            return _trnService.SingleTRNValidation(trnDTO);
        }


        [HasPermission(Permissions.Enum.CanDoBulkQuery)]
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
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
