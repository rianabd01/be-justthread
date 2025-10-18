using Microsoft.AspNetCore.Mvc;

namespace be_justthread.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Halo dari .NET!");
        }
    }
}
