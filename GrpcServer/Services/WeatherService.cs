using Grpc.Core;
using WeatherServer;

namespace GrpcServer.Services;

public class WeatherServiceImpl : WeatherService.WeatherServiceBase
{
    private readonly Random _random = new Random();

    public override Task<WeatherResponse> GetCurrentWeather(
        WeatherRequest request, ServerCallContext context)
    {
        return Task.FromResult(new WeatherResponse
        {
            City = request.City,
            Temperature = _random.Next(10, 30),
            Description = "Солнечно",
            Timestamp = DateTime.UtcNow.ToString("O")
        });
    }

    public override async Task MonitorWeather(
        WeatherRequest request,
        IServerStreamWriter<WeatherResponse> responseStream,
        ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var weather = new WeatherResponse
            {
                City = request.City,
                Temperature = _random.Next(10, 30),
                Description = "Солнечно",
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            await responseStream.WriteAsync(weather);
            await Task.Delay(1000, context.CancellationToken);
        }
    }
}