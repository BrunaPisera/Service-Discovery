using Consul;
using Microsoft.Extensions.Options;
using ServiceA.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<ConsulConfig>(builder.Configuration.GetSection("Consul"));

builder.Services.AddSingleton<IConsulClient>(p =>
{
    var config = builder.Configuration.GetSection("Consul").Get<ConsulConfig>();
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Address));
});

builder.Services.AddSingleton<ConsulServiceRegistration>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ConsulConfig>>().Value;
    var consulClient = sp.GetRequiredService<IConsulClient>();
    return new ConsulServiceRegistration(consulClient, config);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

string address = "servicea";
var consulService = app.Services.GetRequiredService<ConsulServiceRegistration>();
await consulService.RegisterAsync(address);

app.Run();
