using WeatherApp;
class Program
{
    static void Main()
    {
        IWeatherServices weather = new WeatherServices();

        foreach(var temp in weather.GetTemperature("NewYork"))
        {
            System.Console.WriteLine(temp);
        }
    }
}