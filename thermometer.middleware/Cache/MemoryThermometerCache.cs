using System;
using Microsoft.Extensions.Caching.Memory;

namespace thermometer.middleware.cache
{
    public class MemoryThermometerCache : IThermometerCache
    {
        private readonly IMemoryCache _cache;
        public MemoryThermometerCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        string IThermometerCache.Set(string key, string value)
        {
            return _cache.Set(key, value);
        }

        bool IThermometerCache.TryGetValue(string key, out string value)
        {
           return _cache.TryGetValue(key, out value);
        }
    }
}