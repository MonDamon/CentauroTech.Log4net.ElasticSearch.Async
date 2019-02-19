namespace CentauroTech.Log4net.ElasticSearch.Async
{
    using System;
    using System.Collections.Generic;

    using System.Data.Common;
    using System.Linq;

    using log4net.Core;
    using CentauroTech.Log4net.ElasticSearch.Async.Infrastructure;
    using log4net.Util;

#if NETSTANDARD || NETSTANDARD2_0
    using Newtonsoft.Json;
#else
    using System.Web.Script.Serialization;
#endif

    internal static class ExtensionMethods
    {
        public static void Do<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self)
            {
                action(item);
            }
        }

        public static string With(this string self, params object[] args)
        {
            return string.Format(self, args);
        }

        public static IEnumerable<KeyValuePair<string, string>> Properties(this LoggingEvent self)
        {
            return self.GetProperties().AsPairs();
        }

#if NETSTANDARD || NETSTANDARD2_0
        public static string ToJson<T>(this T self)
        {
            return JsonConvert.SerializeObject(self);
        }
#else
        public static string ToJson<T>(this T self)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Serialize(self);
        }
#endif

        public static bool Contains(this CaseInsensitiveStringDictionary<string> self, string key)
        {
            return self.ContainsKey(key) && !self[key].IsNullOrEmpty();
        }


        public static string Get(this CaseInsensitiveStringDictionary<string> self, string key)
        {
            self.TryGetValue(key, out string value);
            return value;
        }

        public static bool ToBool(this string self)
        {
            return bool.Parse(self);
        }

        /// <summary>
        /// Take the full connection string and break it into is constituent parts
        /// </summary>
        /// <param name="self">The connection string itself</param>
        /// <returns>A dictionary of all the parts</returns>
        public static CaseInsensitiveStringDictionary<string> ConnectionStringParts(this string self)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = self.Replace("{", "\"").Replace("}", "\"")
            };

            var parts = new CaseInsensitiveStringDictionary<string>();
            foreach (string key in builder.Keys)
            {
                parts[key] = Convert.ToString(builder[key]);
            }
            return parts;
        }

        static IEnumerable<KeyValuePair<string, string>> AsPairs(this ReadOnlyPropertiesDictionary self)
        {
            return self.GetKeys().Select(key => Pair.For(key, self[key].ToStringOrNull()));
        }

        static string ToStringOrNull(this object self)
        {
            return self != null ? self.ToString() : null;
        }

        static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }
    }
}
