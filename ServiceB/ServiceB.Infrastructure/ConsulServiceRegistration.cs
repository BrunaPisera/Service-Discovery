using Consul;

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

        public async Task RegisterAsync(string address)
        {
            _registrationId = $"{_config.ServiceName}";
            //_registrationId = $"{_config.ServiceName}-{Guid.NewGuid()}";

            var registration = new AgentServiceRegistration()
            {
                ID = _registrationId,
                Name = _config.ServiceName,
                Address = address,
                Port = _config.ServicePort,
                Check = new AgentServiceCheck
                {
                    HTTP = $"http://{address}:{_config.ServicePort}/health",
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
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
