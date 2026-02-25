using Xunit;
using System.Linq;
using WeatherApp;

public class WeatherServicesTest
{
    [Fact]
    public void GetTemperature_ReturnsCorrectSequence()
    {
        var service = new WeatherServices();

        var temperatures = service.GetTemperature("London").ToList();

        Assert.Equal(new[] { 25, 26, 27, 28, 29 }, temperatures);
    }
}