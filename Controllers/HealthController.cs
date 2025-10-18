using Microsoft.AspNetCore.Mvc;

namespace be_justthread.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("JustThread is healthy!");
        }
    }
}
