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
    public static class HttpRuntimeCacheProviderExtensions {

        public static void ExpireAll(this HttpRuntimeCacheProvider cacheProvider) {
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>()
                .Where(cacheProvider.Hit);
            foreach (var entry in entries) {
                HttpRuntime.Cache.Remove((String)entry.Key);
            }
        }

        public static Int32 Count(this HttpRuntimeCacheProvider cacheProvider) {
            return HttpRuntime.Cache.OfType<DictionaryEntry>()
                .Where(cacheProvider.Hit).Count();
        }

        public static String Dump(this HttpRuntimeCacheProvider cacheProvider) {
            var builder = new StringBuilder(1024);
            builder.AppendLine("--------------------HttpRuntimeCacheProvider.Dump--------------------------");
            builder.AppendFormat("EffectivePercentagePhysicalMemoryLimit: {0}\r\n", HttpRuntime.Cache.EffectivePercentagePhysicalMemoryLimit);
            builder.AppendFormat("EffectivePrivateBytesLimit: {0}\r\n", HttpRuntime.Cache.EffectivePrivateBytesLimit);
            builder.AppendFormat("Count: {0}\r\n", HttpRuntime.Cache.Count);
            builder.AppendLine();
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>()
                .Where(cacheProvider.Hit).OrderBy(de => de.Key);
            foreach (var entry in entries) {
                builder.AppendFormat("{0,-20} {1}\r\n", entry.Key, entry.Value.GetType().FullName);
            }
            builder.AppendLine("--------------------HttpRuntimeCacheProvider.Dump--------------------------");
            Debug.WriteLine(builder.ToString());
            return builder.ToString();
        }
    }
}
