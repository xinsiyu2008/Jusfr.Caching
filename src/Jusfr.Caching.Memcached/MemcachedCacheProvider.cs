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
            if (!exist) {
                //不存在
                entry = default(T);
                return false;
            }
            if (cacheEntry == null) {
                //存在但为 null
                entry = (T)((Object)null);
                return true;
            }
            if (cacheEntry is T) {
                //存在，直接返回
                entry = (T)cacheEntry;
                return true;
            }

            if (!(cacheEntry is ExpirationWraper<T>)) {
                //类型不为 T 也不为 ExpirationWraper<T>，抛出异常
                throw new InvalidOperationException(String.Format("缓存项`[{0}]`类型错误, {1} or {2} ?",
                    key, cacheEntry.GetType().FullName, typeof(T).FullName));
            }

            var cacheWraper = (ExpirationWraper<T>)cacheEntry;
            //表示滑动过期缓存项
            if (cacheWraper.SlidingExpiration == Cache.NoSlidingExpiration) {
                //绝对时间过期，返回
                entry = cacheWraper.Value;
                return true;
            }

            var diffSpan = DateTime.Now.Subtract(cacheWraper.SettingTime);
            //当前时间-设置时间>滑动时间, 已经过期
            if (diffSpan > cacheWraper.SlidingExpiration) {
                Expire(key);
                entry = default(T);
                return false;
            }

            //当前时间-设置时间> 滑动时间/2, 更新缓存
            if (diffSpan.Add(diffSpan) > cacheWraper.SlidingExpiration) {
                entry = cacheWraper.Value;
                Overwrite(key, cacheWraper.Value, cacheWraper.SlidingExpiration);
            }
            entry = cacheWraper.Value;
            return true;
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
            _client.Store(StoreMode.Set, BuildCacheKey(key), cacheWraper,
                TimeSpan.FromSeconds(slidingExpiration.TotalSeconds * 1.5));
        }

        //absoluteExpiration UTC或本地时间均可
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
            public DateTime SettingTime { get; set; }

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
                SettingTime = DateTime.Now;
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