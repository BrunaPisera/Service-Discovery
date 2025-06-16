using Consul;
using Microsoft.AspNetCore.Mvc;

namespace ServiceA.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceAController : ControllerBase
    {
        private readonly IConsulClient _consulClient;
        private readonly HttpClient _httpClient;

        public ServiceAController(IConsulClient consulClient, IHttpClientFactory httpClientFactory)
        {
            _consulClient = consulClient;
            _httpClient = httpClientFactory.CreateClient();
        }

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

        [HttpGet("call-serviceb")]
        public async Task<IActionResult> CallServiceB()
        {
            var healthyServices = await _consulClient.Health.Service("service-b", tag: null, passingOnly: true);
            var serviceEntry = healthyServices.Response.FirstOrDefault();

            if (serviceEntry == null)
                return NotFound("No healthy instance of ServiceB found");

            var address = serviceEntry.Service.Address;
            var port = serviceEntry.Service.Port;
            var url = $"http://{address}:{port}/health";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return Ok(new { called = url, response = content });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error calling ServiceB: {ex.Message}");
            }
        }
    }
}