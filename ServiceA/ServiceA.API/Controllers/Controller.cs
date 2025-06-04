using Microsoft.AspNetCore.Mvc;

namespace ServiceA.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceAController : ControllerBase
    {      
        [HttpGet("hello")]
        public IActionResult GetHello()
        {
            return Ok("Hello from ServiceA!");
        }

        [HttpGet("time")]
        public IActionResult GetTime()
        {
            return Ok(DateTime.UtcNow.ToString("o"));
        }
    }
}