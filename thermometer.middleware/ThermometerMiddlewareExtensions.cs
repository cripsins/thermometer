using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using thermometer.middleware.cache;
using thermometer.middleware.calculations;

namespace thermometer.middleware
{
    public static class ThermometerMiddlewareExtensions
    {
        public static IApplicationBuilder UseThermometer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThermometerMiddleware>();
        }

        public static IServiceCollection AddThermometer(this IServiceCollection services)
        {
            services.AddTransient<ITemperatureCalculation, TemperatureCalculations>();
            services.AddTransient<IThermometerCache, MemoryThermometerCache>();
            return services;
        }
    }
}