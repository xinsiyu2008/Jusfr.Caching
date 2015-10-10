using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

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

        public static void Flush(this HttpRuntimeCacheProvider cacheProvider, Func<String, Boolean> predicate) {
            var cacheFilename = GetCacheFilePath(cacheProvider);            
            using (var stream = new FileStream(cacheFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(stream)) {
                var prefix = cacheProvider.BuildCacheKey(null);

                stream.SetLength(0L);
                var entries = HttpRuntime.Cache.OfType<DictionaryEntry>()
                    .Where(cacheProvider.Hit)
                    .Where(r => predicate(((String)r.Key).Substring(prefix.Length)));
                var json = new JavaScriptSerializer();
                foreach (var entry in entries) {
                    writer.WriteLine(json.Serialize(entry));
                }
                writer.Flush();
            }
        }

        private static string GetCacheFilePath(HttpRuntimeCacheProvider cacheProvider) {
            var cacheFilename = AppDomain.CurrentDomain.BaseDirectory;
            if (HttpRuntime.AppDomainAppId != null) {
                cacheFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            }
            cacheFilename = Path.Combine(cacheFilename, String.Format("HRCP_{0}.cache", cacheProvider.Region));
            return cacheFilename;
        }

        public static T Restore<T>(this HttpRuntimeCacheProvider cacheProvider, String key) {
            var cacheFilename = GetCacheFilePath(cacheProvider);
            if (!File.Exists(cacheFilename)) {
                return default(T);
            }
            //{"Key":"HRCP_??","Value":??}
            var cacheLineStart = String.Format("{{\"Key\":\"{0}\",\"Value\":", cacheProvider.BuildCacheKey(key));
            var cacheCollection = File.ReadLines(cacheFilename);
            var cacheLine = cacheCollection.FirstOrDefault(c => c.StartsWith(cacheLineStart));
            if (cacheLine == null) {
                return default(T);
            }

            cacheLine = cacheLine.Substring(cacheLineStart.Length, cacheLine.Length - cacheLineStart.Length - 1);
            var json = new JavaScriptSerializer();
            return json.Deserialize<T>(cacheLine);
        }
    }
}
