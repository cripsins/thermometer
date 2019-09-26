using Microsoft.Extensions.Caching.Memory;

namespace thermometer.middleware.calculations
{
    public interface ITemperatureCalculation
    {
        void AddMeasure(double measure);
        Calculations GetCalculations();

        string GetOutput();
    }
}