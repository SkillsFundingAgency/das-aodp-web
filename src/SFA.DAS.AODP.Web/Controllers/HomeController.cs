using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models;

namespace SFA.DAS.AODP.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FormBuilder()
        {
            return View();
        }

        //[HttpPost("/home/update")]
        //public IActionResult Update([FromBody] string value)
        //{
        //    try
        //    {
        //        Console.WriteLine(value);
        //        var x = 0;
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Update()
        {
            var rawRequestBody = await GetRawBodyAsync(Request);

            // Other code herereturn Ok();

            return Ok();
        }


        public async Task<string> GetRawBodyAsync(
            HttpRequest request,
            Encoding encoding = null)
        {
            if (!request.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                request.EnableBuffering();
            }

            request.Body.Position = 0;

            var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            request.Body.Position = 0;

            return body;
        }
    }
}
