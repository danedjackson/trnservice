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
    //[Authorize(Roles = Role.Admin + ", " + Role.User)]
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
        // TODO: Make a default index page so that this Index can be annotated with HasPermission
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [HasPermission(Permissions.CanDoIndividualQuery)]
        public IActionResult Index(TrnSearchRequestViewModel trnDTO)
        {
            if(!ModelState.IsValid)
            {
                return View("Index", trnDTO);
            }
            // Retreive Memory Stream of the TRN Search results
            if (_trnService.SingleTRNValidation(trnDTO))
            {
                trnDTO.IsMatched = true;
                trnDTO.Message = $"TRN Information MATCHED for {trnDTO.FirstName} {trnDTO.LastName}";
                return View("Index", trnDTO);
            }
            trnDTO.Message = $"TRN Information MISMATCHED for {trnDTO.FirstName} {trnDTO.LastName}";
            return View("Index", trnDTO);
        }


        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [HasPermission(Permissions.CanDoBulkQuery)]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0 || !file.FileName.EndsWith(".csv"))
            {
                ModelState.AddModelError(string.Empty, "No csv file selected");
                return View("Upload");
            }

            var result = _trnService.MultipleTRNValidation(file);


            return result;

        }


        public IActionResult Reset()
        {
            return View("Index", new TrnSearchRequestViewModel());
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
