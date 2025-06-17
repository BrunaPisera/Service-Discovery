using Consul;
using System.Net;

namespace ServiceB.Infrastructure
{
    public class ConsulServiceRegistration
    {
        private readonly IConsulClient _consulClient;
        private readonly ConsulConfig _config;
        private string _registrationId;

        public ConsulServiceRegistration(IConsulClient consulClient, ConsulConfig config)
        {
            _consulClient = consulClient;
            _config = config;
        }

        public async Task RegisterAsync()
        {        
            var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? _config.ServiceName;

            var address = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .ToString();

            _registrationId = $"{_config.ServiceName}-{Guid.NewGuid()}";       

            var registration = new AgentServiceRegistration()
            {
                ID = _registrationId,
                Name = serviceName,
                Address = address,
                Port = _config.ServicePort,
                Check = new AgentServiceCheck
                {
                    HTTP = $"http://{address}:{_config.ServicePort}/health",
                    Interval = TimeSpan.FromSeconds(1),
                    Timeout = TimeSpan.FromSeconds(1),               
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(1)
                }
            };

            await _consulClient.Agent.ServiceRegister(registration);
        }

        public async Task DeregisterAsync()
        {
            if (!string.IsNullOrEmpty(_registrationId))
            {
                await _consulClient.Agent.ServiceDeregister(_registrationId);
            }
        }
    }
}
