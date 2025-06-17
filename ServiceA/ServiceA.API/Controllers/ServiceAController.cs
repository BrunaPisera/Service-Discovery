using Consul;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

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

        [HttpGet("call/serviceb")]
        public async Task<IActionResult> CallServiceB()
        {
            // Get healthy instances of "serviceb" from Consul
            var healthyServices = await _consulClient.Health.Service("serviceb", tag: null, passingOnly: true);
            var healthyInstances = healthyServices.Response;

            // Check if there are any healthy instances available
            if (healthyInstances == null || healthyInstances.Length == 0)
                return NotFound("No healthy instances found for 'serviceb'");

            // Pick a random healthy instance
            var random = new Random();
            var chosenInstance = healthyInstances[random.Next(healthyInstances.Length)];
            
            // Build the target URL to call
            var address = chosenInstance.Service.Address;
            var port = chosenInstance.Service.Port;
            var url = $"http://{address}:{port}/health";

            try
            {
                // Make the HTTP request to the chosen instance
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                // Return information about the called instance and its response
                return Ok(new { 
                    called = url,
                    calledInstance = new
                    {
                        name = chosenInstance.Service,                    
                    },
                    response = content 
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error calling {chosenInstance.Service}: {ex.Message}");
            }
        }
    }
}