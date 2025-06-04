using Microsoft.AspNetCore.Mvc;

namespace ServiceB.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceBController : ControllerBase
    {
        [HttpGet("hello")]
        public IActionResult GetHello()
        {
            return Ok("Hello from ServiceB!");
        }

        [HttpGet("time")]
        public IActionResult GetTime()
        {
            return Ok(DateTime.UtcNow.ToString("o"));
        }
    }
}
