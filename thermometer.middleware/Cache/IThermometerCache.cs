namespace thermometer.middleware.cache
{
    public interface IThermometerCache
    {
        bool TryGetValue(string key, out string value);
        string Set(string key, string value);
    }
}