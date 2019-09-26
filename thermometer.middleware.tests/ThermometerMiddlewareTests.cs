using thermometer.middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using thermometer.middleware.calculations;
using thermometer.middleware.cache;

namespace thermometer.middleware.tests
{
    public class ThermometerMiddlewareTests
    {
        private Mock<HttpContext> GetHttpContext(string contentType)
        {
            var requestMock = new Mock<HttpResponse>();        
            requestMock.Setup(x => x.ContentType).Returns(contentType);
            requestMock.Setup(x => x.Body).Returns(new MemoryStream());

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(x => x.Response).Returns(requestMock.Object);

            return contextMock;
        }

        private IThermometerCache GetMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            var memoryCache = serviceProvider.GetService<IMemoryCache>();

            return new MemoryThermometerCache(memoryCache);
        }

        [Fact]
        public async Task It_Should_Append_Calculations()
        {
            var temperatureCalculation = new TemperatureCalculations(GetMemoryCache());
            var thermometerMiddleware = new ThermometerMiddleware(next: (innerHttpContext) => Task.FromResult(0), temperatureCalculation: temperatureCalculation);
            Mock<HttpContext> httpContextMoq = GetHttpContext("html/text");

            await thermometerMiddleware.Invoke(httpContextMoq.Object);

            httpContextMoq.Object.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContextMoq.Object.Response.Body);
            var streamText = reader.ReadToEnd();

            Assert.True(streamText != null && streamText.Contains("<!-- Thermometer middleware:"));
        }

        [Fact]
        public async Task It_Should_NOT_Append_Calculations()
        {
            var temperatureCalculation = new TemperatureCalculations(GetMemoryCache());
            var thermometerMiddleware = new ThermometerMiddleware(next: (innerHttpContext) => Task.FromResult(0), temperatureCalculation: temperatureCalculation);
            Mock<HttpContext> httpContextMoq = GetHttpContext("text");

            await thermometerMiddleware.Invoke(httpContextMoq.Object);

            httpContextMoq.Object.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContextMoq.Object.Response.Body);
            var streamText = reader.ReadToEnd();

            Assert.True(string.IsNullOrWhiteSpace(streamText));
        }

        [Fact]
        public void It_Should_Calculate_Zero()
        {
            var memoryCache = GetMemoryCache();
           
            var thermo = new TemperatureCalculations(memoryCache);
            var calc = thermo.GetCalculations();

            Assert.True(calc.Min == Convert.ToDouble(int.MaxValue) && calc.Max == 0 && calc.Average == 0);
        }

        [Fact]
        public void It_Should_Calculate_Correct()
        {
            var memoryCache = GetMemoryCache();
           
            var thermo = new TemperatureCalculations(memoryCache);
            thermo.AddMeasure(10);
            thermo.AddMeasure(20);
            var calc = thermo.GetCalculations();

            Assert.True(calc.Min == 10);
            Assert.True(calc.Max == 20);
            Assert.True(calc.Average == 15);
        }
    }
}
