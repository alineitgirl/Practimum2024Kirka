using Grpc.Core;
using Grpc.Net.Client;
using WeatherClient;
using GrpcServer.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Diagnostics;

var httpHandler = new HttpClientHandler();
httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

using var channel = GrpcChannel.ForAddress("https://localhost:7098", new GrpcChannelOptions
{
    HttpHandler = httpHandler
});

var grpcClient = new WeatherService.WeatherServiceClient(channel);

var currentWeather = await grpcClient.GetCurrentWeatherAsync(new WeatherRequest { City = "Москва" });
Console.WriteLine($"GRPC погода: {currentWeather.City}, {currentWeather.Temperature}°C, {currentWeather.Description}");

var currentWeatherList = await grpcClient.GetCurrentWeatherArrayAsync(new EmptyRequest());
foreach (var item in currentWeatherList.Data)
{
    Console.WriteLine(item);
}

Console.WriteLine("Начинаем мониторинг погоды...");

using var monitoring = grpcClient.MonitorWeather(new WeatherRequest { City = "Москва" });

try
{
    await foreach (var weather in monitoring.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Обновление: {weather.City}, {weather.Temperature}°C, {weather.Timestamp}");
    }
}
catch (RpcException ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}

//REST 
using var restClient = new HttpClient(httpHandler);
var url = "https://localhost:7098/api/weather/current?city=Moscow";

var response = await restClient.GetFromJsonAsync<WeatherResponse>(url);

if (response != null)
{
    Console.WriteLine($"REST API: Погода в {response.City}: {response.Temperature}°C, {response.Description}");
}
else
{
    Console.WriteLine("Ошибка при получении данных от REST API.");
}

var urlClient = "https://localhost:7098/api/weather/forecast?";
var responseClient = await restClient.GetFromJsonAsync<IEnumerable<WeatherResponse>>(urlClient);

if (responseClient != null)
{
	Console.WriteLine("REST API: Отправляем массив на клиент");
    foreach (var item in responseClient) 
	{
		Console.WriteLine($"{item.City}, {item.Temperature}°C, {item.Description}, {item.Timestamp}");
	}
}
else
{
    Console.WriteLine("Ошибка при получении данных от REST API.");
}

//измерение времени отправки
Console.WriteLine("Сравниваем время отправки 10000 сообщений: ");
var sw = Stopwatch.StartNew();
for (var i =0; i < 10000; i++)
{
	var tempResponse = await restClient.GetFromJsonAsync<WeatherResponse>(url);
}
sw.Stop();
Console.WriteLine($"REST - Время для 10000 сообщений: {sw.ElapsedMilliseconds} мс");

sw = Stopwatch.StartNew();
for (var i = 0; i < 10000; i++)
{
	var tempResponse = await grpcClient.GetCurrentWeatherAsync(new WeatherRequest { City = "Москва" });
}
sw.Stop();
Console.WriteLine($"GRPC - Время для 10000 сообщений: {sw.ElapsedMilliseconds} мс");

public class WeatherResponse
{
    public string City { get; set; }
    public int Temperature { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
}
