using Grpc.Core;
using WeatherServer;

namespace GrpcServer.Services;

public class WeatherServiceImpl : WeatherService.WeatherServiceBase
{
    private readonly Random _random = new Random();
	private readonly List<string> cities = new List<string> 
	{
		"Москва",
		"Нижний Новгород",
		"Казань",
		"Тюмень",
		"Санкт-Петербург"
	};
	private readonly List<string> descriptions = new List<string> 
	{
		"Солнечно",
		"Пасмурно",
		"Облачно",
		"Снег",
		"Дождь"
	};

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

	public override Task<WeatherArrayResponse> GetCurrentWeatherArray (EmptyRequest request, ServerCallContext context) 
	{
		var response = new WeatherArrayResponse();
		var weatherList = new List<WeatherResponse>();
		for (var i =0; i < 3; i++)
		{
			weatherList.Add(new WeatherResponse 
				{
					City = cities[_random.Next(0, 5)],
					Temperature = _random.Next(10, 30),
					Description = descriptions[_random.Next(0, 5)],
					Timestamp = DateTime.UtcNow.ToString("O")
				});
		}
		response.Data.AddRange(weatherList);
		return Task.FromResult(response);	
	}

    public override async Task MonitorWeather(
        WeatherRequest request,
        IServerStreamWriter<WeatherResponse> responseStream,
        ServerCallContext context)
    {
        var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    	var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
        context.CancellationToken, timeoutCancellationTokenSource.Token);
    	try
    		{
        		while (!linkedCancellationTokenSource.Token.IsCancellationRequested)
        		{
            		var weather = new WeatherResponse
           			 {
                		City = request.City,
                		Temperature = _random.Next(10, 30),
                		Description = "Солнечно",
                		Timestamp = DateTime.UtcNow.ToString("O")
            		 };
           		await responseStream.WriteAsync(weather);
         	 	await Task.Delay(1000, linkedCancellationTokenSource.Token);
        		}
			}
    	catch (OperationCanceledException)
    	{
        	Console.WriteLine("Мониторинг погоды завершен или был отменен.");
    	}
    }
}