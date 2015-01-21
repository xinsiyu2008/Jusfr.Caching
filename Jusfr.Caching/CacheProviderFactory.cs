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
        //请求级别缓存
        public static ICacheProvider GetHttpContextCache() {
            return new HttpContextCacheProvider();
        }

//#if DEBUG
        public static IHttpRuntimeCacheProvider GetHttpRuntimeCache() {
            return new HttpRuntimeCacheProvider();
        }

        public static IHttpRuntimeCacheProvider GetHttpRuntimeCache(String region) {
            return new HttpRuntimeCacheProvider(region);
        }

        //开发环境，退化成 进程Cache
        public static IHttpRuntimeCacheProvider GetDistributedCache() {
            return new HttpRuntimeCacheProvider();
        }

        public static IHttpRuntimeCacheProvider GetDistributedCache(String region) {
            return new HttpRuntimeCacheProvider(region);
        }


//#else
//        //正式环境，使用 Memcached
//        public static IHttpRuntimeCacheProvider GetDistributedCache() {
//            return new MemcachedCacheProvider();
//        }

//        public static IHttpRuntimeCacheProvider GetDistributedCache(String region) {
//            return new MemcachedCacheProvider(region);
//        }
//#endif

        public static String Dump() {
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
