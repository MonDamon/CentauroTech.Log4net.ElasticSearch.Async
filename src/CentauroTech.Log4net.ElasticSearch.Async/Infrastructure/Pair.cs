namespace CentauroTech.Log4net.ElasticSearch.Async.Infrastructure
{
    using System.Collections.Generic;

    internal static class Pair
    {
        public static KeyValuePair<TKey, TValue> For<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}