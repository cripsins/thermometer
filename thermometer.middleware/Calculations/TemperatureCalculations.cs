using System;
using Microsoft.Extensions.Caching.Memory;
using thermometer.middleware.cache;
using thermometer.middleware.common;

namespace thermometer.middleware.calculations
{
    public class TemperatureCalculations : ITemperatureCalculation
    {
        private int _count = 0;

        private double _sum = 0;
        private double _min = Convert.ToDouble(int.MaxValue);
        private double _max = 0;
        private double _average = 0;
        private IThermometerCache _cache;
        public TemperatureCalculations(IThermometerCache cache)
        {
            if(cache != null)
            {
                _cache = cache;
                RestoreFromCache(cache);
            }
        }

        public void AddMeasure(double measure)
        {
            _count++;

            if(measure > _max)
                _max = measure;
            if(measure < _min)
                _min = measure;

            _sum += measure;

            _average = _sum / _count;

            StoreInCache(_cache);
        }

        public Calculations GetCalculations()
        {
            return new Calculations() {
                Min = _count == 0 ? 0 : _min,
                Max = _max,
                Average = _average
            };
        }

        public string GetOutput()
        {
            var calc = GetCalculations();
            return string.Format("<!-- Thermometer middleware: Min: {0} Max: {1} Avg: {2} -->", calc.Min, calc.Max, calc.Average);
        }

        private void StoreInCache(IThermometerCache cache)
        {
            StoreKey(cache, CacheKeys.Average, _average.ToString());
            StoreKey(cache, CacheKeys.Count, _count.ToString());
            StoreKey(cache, CacheKeys.Max, _max.ToString());
            StoreKey(cache, CacheKeys.Min, _min.ToString());
            StoreKey(cache, CacheKeys.Sum, _sum.ToString());
        }

        private string GetKey(IThermometerCache cache, string key)
        {
            string value = string.Empty;

            var result = cache.TryGetValue(key, out value);
            if(!result && key == CacheKeys.Min)
                value = Convert.ToDouble(int.MaxValue).ToString();
            
            return value;
        }

        private void StoreKey(IThermometerCache cache, string key, string value)
        {
            // Save data in cache.
            cache.Set(key, value);
        }

        private void RestoreFromCache(IThermometerCache cache)
        {
            double.TryParse(GetKey(cache, CacheKeys.Average), out _average);
            int.TryParse(GetKey(cache, CacheKeys.Count), out _count);
            double.TryParse(GetKey(cache, CacheKeys.Max), out _max);
            double.TryParse(GetKey(cache, CacheKeys.Min), out _min);            
            double.TryParse(GetKey(cache, CacheKeys.Sum), out _sum);
        }
    }
}