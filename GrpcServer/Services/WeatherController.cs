using Microsoft.AspNetCore.Mvc;

namespace GrpcServer.Services;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
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
    
    [HttpGet("current")]
    public ActionResult<WeatherResponse> GetCurrentWeather([FromQuery] string city)
    {
        var weather = new WeatherResponse
        {
            City = city,
            Temperature = _random.Next(10, 30),
            Description = "Солнечно",
            Timestamp = DateTime.UtcNow 
        };
        return Ok(weather);
    }

    [HttpGet("forecast")]
    public ActionResult<IEnumerable<WeatherResponse>> GetWeatherForecast()
    {
        var weatherList = new List<WeatherResponse>();
        for (var i = 0; i < 3; i++)
        {
            weatherList.Add(new WeatherResponse 
            {
                City = cities[_random.Next(0, cities.Count)],
                Temperature = _random.Next(10, 30),
                Description = descriptions[_random.Next(0, descriptions.Count)],
                Timestamp = DateTime.UtcNow 
            });
        }
        return Ok(weatherList);
    }
    
    public class WeatherResponse
    {
        public string City { get; set; }
        public int Temperature { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } 
    }
}