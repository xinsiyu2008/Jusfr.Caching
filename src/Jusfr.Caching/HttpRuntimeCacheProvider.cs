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
    public class HttpRuntimeCacheProvider : CacheProvider, IHttpRuntimeCacheProvider, IRegion {
        private static readonly Object _nullEntry = new Object();
        private const String _prefix = "HRCP_";

        public virtual String Region { get; private set; }

        public HttpRuntimeCacheProvider() {
        }

        public HttpRuntimeCacheProvider(String region) {
            Region = region;
        }

        private Boolean InnerTryGet(String key, out object entry) {
            entry = HttpRuntime.Cache.Get(key);
            return entry != null;
        }

        public override bool TryGet<T>(string key, out T entry) {
            String cacheKey = BuildCacheKey(key);
            Object cacheEntry;
            Boolean exist = InnerTryGet(cacheKey, out cacheEntry);
            if (exist) {
                if (cacheEntry != null) {
                    if (!(cacheEntry is T)) {
                        throw new InvalidOperationException(String.Format("缓存项`[{0}]`类型错误, {1} or {2} ?",
                            key, cacheEntry.GetType().FullName, typeof(T).FullName));
                    }
                    entry = (T)cacheEntry;
                }
                else {
                    entry = (T)((Object)null);
                }
            }
            else {
                entry = default(T);
            }
            return exist;
        }

        protected override String BuildCacheKey(String key) {
            //Region 为空将被当作  String.Empty 处理
            return Region == null
                ? String.Concat(_prefix, key)
                : String.Concat(_prefix, Region, "_", key);
        }

        private Object BuildCacheEntry<T>(T value) {
            Object entry = value;
            if (value == null) {
                entry = _nullEntry;
            }
            return entry;
        }


        public T GetOrCreate<T>(String key, Func<T> function, TimeSpan slidingExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, slidingExpiration);
            return value;
        }

        public T GetOrCreate<T>(String key, Func<T> function, DateTime absoluteExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, absoluteExpiration);
            return value;
        }

        public override void Overwrite<T>(String key, T value) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheEntry<T>(value));
        }

        //slidingExpiration 时间内无访问则过期
        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheEntry<T>(value), null,
                Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        //absoluteExpiration 时过期
        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheEntry<T>(value), null,
                absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public override void Expire(String key) {
            HttpRuntime.Cache.Remove(BuildCacheKey(key));
        }

        internal Boolean Hit(DictionaryEntry entry) {
            return (entry.Key is String) && ((String)entry.Key).StartsWith(BuildCacheKey(String.Empty));
        }
    }
}
