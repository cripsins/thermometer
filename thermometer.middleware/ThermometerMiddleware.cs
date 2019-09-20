using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text;

namespace thermometer.middleware
{
    public class ThermometerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _memoryCache;

        public ThermometerMiddleware(RequestDelegate next, IMemoryCache memoryCache)
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var start = Stopwatch.GetTimestamp();

            var thermo = new TemperatureCalculations(_memoryCache);
            var calc = thermo.GetCalculations();

            try
            {
                //Continue down the Middleware pipeline, eventually returning to this class
                await _next(httpContext);

                if(httpContext.Response != null && 
                    !string.IsNullOrWhiteSpace(httpContext.Response.ContentType) && 
                    httpContext.Response.ContentType.Contains("html"))
                {
                    byte[] test = Encoding.UTF8.GetBytes(string.Format("<!-- Thermometer middleware: Min: {0} Max: {1} Avg: {2} -->", calc.Min, calc.Max, calc.Average));
                    await httpContext.Response.Body.WriteAsync(test, 0, test.Length);
                }

                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                CalculateTemperature(thermo, elapsedMs);
            }
            catch (Exception)
                // Still get the developer page
                when (CalculateTemperature(thermo, GetElapsedMilliseconds(start, Stopwatch.GetTimestamp())))
            {

            }
        }

        bool CalculateTemperature(TemperatureCalculations thermo, double elapsedMs)
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