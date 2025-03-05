using Grpc.Core;
using Grpc.Net.Client;
using WeatherClient;

var httpHandler = new HttpClientHandler();
            
httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

using var channel = GrpcChannel.ForAddress("https://localhost:7276", new GrpcChannelOptions
{
    HttpHandler = httpHandler
});

var client = new WeatherService.WeatherServiceClient(channel);

var currentWeather = await client.GetCurrentWeatherAsync(
    new WeatherRequest { City = "Москва" });

Console.WriteLine($"Текущая погода: {currentWeather.City}, " +
                  $"{currentWeather.Temperature}°C, {currentWeather.Description}");


Console.WriteLine("Начинаем мониторинг погоды...");
using var monitoring = client.MonitorWeather(new WeatherRequest { City = "Москва" });

try
{
    await foreach (var weather in monitoring.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Обновление: {weather.City}, " +
                          $"{weather.Temperature}°C, {weather.Timestamp}");
    }
}
catch (RpcException ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}