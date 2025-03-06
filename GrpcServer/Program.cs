using GrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7098, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core
            .HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps();
    });
});

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddSingleton<WeatherServiceImpl>();
builder.Services.AddSingleton<WeatherController>();

var app = builder.Build();
app.MapControllers();

app.MapGrpcService<WeatherServiceImpl>();
app.Run();