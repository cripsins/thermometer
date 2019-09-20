using Microsoft.Extensions.Caching.Memory;

namespace thermometer.middleware
{
    public static class CacheKeys
    {
        public static string Min { get { return "_Min"; } }
        public static string Max { get { return "_Max"; } }
        public static string Average { get { return "_Average"; } }
        public static string Count { get { return "_Count"; } }
        public static string Sum { get { return "_Sum"; } }
    }

    public class Calculations
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Average { get; set; }
    }

    public interface ITemperatureCalculation
    {
        void AddMeasure(double measure);
        Calculations GetCalculations();
    }

    public class TemperatureCalculations : ITemperatureCalculation
    {
        private int _count = 0;

        private double _sum = 0;
        private double _min = 0;
        private double _max = 0;
        private double _average = 0;
        private IMemoryCache _cache;

        public TemperatureCalculations(IMemoryCache cache)
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
                Min = _min,
                Max = _max,
                Average = _average
            };
        }

        private void StoreInCache(IMemoryCache cache)
        {
            StoreKey(cache, CacheKeys.Average, _average.ToString());
            StoreKey(cache, CacheKeys.Count, _count.ToString());
            StoreKey(cache, CacheKeys.Max, _max.ToString());
            StoreKey(cache, CacheKeys.Min, _min.ToString());
            StoreKey(cache, CacheKeys.Sum, _sum.ToString());
        }

        private string GetKey(IMemoryCache cache, string key)
        {
            string value = "0";
            cache.TryGetValue(key, out value);
            
            return value;
        }

        private void StoreKey(IMemoryCache cache, string key, string value)
        {
            // Save data in cache.
            cache.Set(key, value);
        }

        private void RestoreFromCache(IMemoryCache cache)
        {
            double.TryParse(GetKey(cache, CacheKeys.Average), out _average);
            int.TryParse(GetKey(cache, CacheKeys.Count), out _count);
            double.TryParse(GetKey(cache, CacheKeys.Max), out _max);
            double.TryParse(GetKey(cache, CacheKeys.Min), out _min);
            double.TryParse(GetKey(cache, CacheKeys.Sum), out _sum);
        }
    }
}