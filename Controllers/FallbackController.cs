using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace DatingApp.API.Controllers
{
    [AllowAnonymous]
    // we derive from the controller because we want an controller that has a view support.
    // index is effectively a view that we are returning here
    // What we are doing here is when the server doesn`t know about an endpoint we return this instead
    public class FallbackController : Controller
    {
        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot", "index.html"), "text/html");
        }
    }
}