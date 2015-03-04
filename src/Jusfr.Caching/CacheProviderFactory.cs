using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Jusfr.Caching {
    public static class CacheProviderFactory {
        public static ICacheProvider GetHttpContextCache() {
            return new HttpContextCacheProvider();
        }

        public static IHttpRuntimeCacheProvider GetHttpRuntimeCache() {
            return new HttpRuntimeCacheProvider();
        }

        public static IHttpRuntimeCacheProvider GetHttpRuntimeCache(String region) {
            return new HttpRuntimeCacheProvider(region);
        }

        public static String DumpHttpRuntimCache() {
            var builder = new StringBuilder(1024);
            builder.AppendLine("--------------------HttpRuntimeCacheProvider.Dump--------------------------");
            builder.AppendFormat("EffectivePercentagePhysicalMemoryLimit: {0}\r\n", HttpRuntime.Cache.EffectivePercentagePhysicalMemoryLimit);
            builder.AppendFormat("EffectivePrivateBytesLimit: {0}\r\n", HttpRuntime.Cache.EffectivePrivateBytesLimit);
            builder.AppendFormat("Count: {0}\r\n", HttpRuntime.Cache.Count);
            builder.AppendLine();
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>().OrderBy(de => de.Key);
            foreach (var entry in entries) {
                builder.AppendLine(String.Format("{0}\r\n    {1}", entry.Key, entry.Value.GetType().FullName));
            }
            builder.AppendLine("--------------------HttpRuntimeCacheProvider.Dump--------------------------");
            Debug.WriteLine(builder.ToString());
            return builder.ToString();
        }
    }
}
