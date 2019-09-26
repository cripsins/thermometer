using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text;
using thermometer.middleware.calculations;

namespace thermometer.middleware
{
    public class ThermometerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITemperatureCalculation _temperatureCalculation;

        public ThermometerMiddleware(RequestDelegate next, ITemperatureCalculation temperatureCalculation)
        {
            _next = next;
            _temperatureCalculation = temperatureCalculation;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var start = Stopwatch.GetTimestamp();

            try
            {
                //Continue down the Middleware pipeline, eventually returning to this class
                await _next(httpContext);

                if(httpContext.Response != null && 
                    !string.IsNullOrWhiteSpace(httpContext.Response.ContentType) && 
                    httpContext.Response.ContentType.Contains("html"))
                {
                    byte[] test = Encoding.UTF8.GetBytes(_temperatureCalculation.GetOutput());
                    await httpContext.Response.Body.WriteAsync(test, 0, test.Length);
                }

                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                CalculateTemperature(_temperatureCalculation, elapsedMs);
            }
            catch (Exception)
                // Still get the developer page
                when (CalculateTemperature(_temperatureCalculation, GetElapsedMilliseconds(start, Stopwatch.GetTimestamp())))
            {

            }
        }

        //bool CalculateTemperature(TemperatureCalculations thermo, double elapsedMs)
        bool CalculateTemperature(ITemperatureCalculation thermo, double elapsedMs)
        {
            thermo.AddMeasure(elapsedMs);
            
            return false;
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }
    }
}