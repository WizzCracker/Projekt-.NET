using System.Text.Json.Serialization;

public class WeatherData
{
    public Wind Wind { get; set; }
    public List<WeatherCondition> Weather { get; set; }
    public Rain? Rain { get; set; }
}

public class Wind
{
    public double Speed { get; set; } // w m/s
}

public class Rain
{
    [JsonPropertyName("1h")]
    public double? OneHour { get; set; }
}

public class WeatherCondition
{
    public string Main { get; set; } // np. Rain, Thunderstorm
}
