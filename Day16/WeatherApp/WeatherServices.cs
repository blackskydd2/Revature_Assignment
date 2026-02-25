using System;
using System.Collections.Generic;
namespace WeatherApp
{


    public interface IWeatherServices
    {
        IEnumerable<int> GetTemperature(string city);
    }
    public class WeatherServices : IWeatherServices
    {
        public IEnumerable<int> GetTemperature(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                throw new Exception("City name cannot be empty");

            }
            yield return 25;
            yield return 26;
            yield return 27;
            yield return 28;
            yield return 29;
        }

    }

}