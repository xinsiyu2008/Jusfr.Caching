using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Web.Caching;

namespace Jusfr.Caching.Memcached {
    public class MemcachedCacheProvider : CacheProvider, IHttpRuntimeCacheProvider, IRegion {
        private static readonly MemcachedClient _client = new MemcachedClient("enyim.com/memcached");

        //EnyimMemcached 天然支持空缓存项，感觉没有必要添加一个“不支持空缓存项的特性”
        //另外过期时间不能算的太准，会发现时间刚刚到而这货还没来得及过期

        public String Region { get; private set; }

        public MemcachedCacheProvider() {
        }

        public MemcachedCacheProvider(String region) {
            Region = region;
        }

        protected override String BuildCacheKey(String key) {
            return Region == null ? key : String.Concat(Region, "_", key);
        }

        // Will not last expire time
        public Boolean TryGetObject(string key, out object entry) {
            return _client.TryGet(BuildCacheKey(key), out entry);
        }

        public override bool TryGet<T>(string key, out T entry) {            
            Object cacheEntry;
            Boolean exist = TryGetObject(key, out cacheEntry);
            if (exist) {
                if (cacheEntry != null) {
                    if (cacheEntry is ExpirationWraper<T>) {
                        var cacheWraper = (ExpirationWraper<T>)cacheEntry;
                        if (cacheWraper.AbsoluteExpiration == Cache.NoAbsoluteExpiration) {
                            //表示滑动过期缓存项，重新存储
                            Overwrite(key, cacheWraper.Value, cacheWraper.SlidingExpiration);
                        }
                        entry = cacheWraper.Value;
                    }
                    else if (!(cacheEntry is T)) {
                        throw new InvalidOperationException(String.Format("缓存项`[{0}]`类型错误, {1} or {2} ?",
                            key, cacheEntry.GetType().FullName, typeof(T).FullName));
                    }
                    else {
                        entry = (T)cacheEntry;
                    }
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
            _client.Store(StoreMode.Set, BuildCacheKey(key), value);
        }

        //slidingExpiration 时间内无访问则过期
        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            var cacheWraper = new ExpirationWraper<T>(value, slidingExpiration);
            _client.Store(StoreMode.Set, BuildCacheKey(key), cacheWraper, slidingExpiration);
        }

        //absoluteExpiration 时过期
        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            _client.Store(StoreMode.Set, BuildCacheKey(key), value, absoluteExpiration);
        }

        public override void Expire(String key) {
            _client.Remove(BuildCacheKey(key));  // Could check result
        }

        [Serializable]
        public class ExpirationWraper<T> {
            public T Value { get; private set; }
            public DateTime AbsoluteExpiration { get; private set; }
            public TimeSpan SlidingExpiration { get; private set; }

            public ExpirationWraper(T value, DateTime absoluteExpiration)
                : this(value, absoluteExpiration, Cache.NoSlidingExpiration) {
            }

            public ExpirationWraper(T value, TimeSpan slidingExpiration)
                : this(value, Cache.NoAbsoluteExpiration, slidingExpiration) {
            }

            private ExpirationWraper(T value, DateTime absoluteExpiration, TimeSpan slidingExpiration) {
                Value = value;
                AbsoluteExpiration = absoluteExpiration;
                SlidingExpiration = slidingExpiration;
            }
        }

        public struct NullableEntry<T> {
            private T entry;
            private Boolean isNull;

            public NullableEntry(T value) {
                entry = value;
                isNull = (value == null);
            }

            public static implicit operator NullableEntry<T>(T value) {
                return new NullableEntry<T>(value);
            }

            public static implicit operator T(NullableEntry<T> value) {
                if (value.isNull) {
                    return (T)((Object)null);
                }
                return value.entry;
            }
        }
    }
}