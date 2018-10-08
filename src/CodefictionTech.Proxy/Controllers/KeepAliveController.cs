using Microsoft.AspNetCore.Mvc;

namespace CodefictionTech.Proxy.Controllers
{
    [Route("api/[controller]")]
    public class KeepAliveController : Controller
    {
        [HttpGet]
        [Route("health_check")]
        public IActionResult HealthCheck()
        {
            return Ok("OK!");
        }
    }
}
