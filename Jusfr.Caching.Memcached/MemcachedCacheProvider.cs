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
namespace Jusfr.Caching.Memcached {
    public class MemcachedCacheProvider : IHttpRuntimeRegionCacheProvider {
        private static readonly MemcachedClient _client = new MemcachedClient("enyim.com/memcached");

        //EnyimMemcached 天然支持空缓存项，感觉没有必要添加一个“不支持空缓存项的特性”
        //另外过期时间不能算的太准，会发现时间刚刚到而这货还没来得及过期
                
        public String Region { get; private set; }

        public MemcachedCacheProvider()
            : this(String.Empty) {
        }

        public MemcachedCacheProvider(String region) {
            Region = region;
        }

        protected virtual String BuildCacheKey(String key) {
            return String.Concat(Region, "_", key);
        }

        public Boolean TryGet<T>(String key, out T value) {
            Object entry;
            Boolean exist = _client.TryGet(BuildCacheKey(key), out entry);
            if (exist) {
                if (!(entry is T)) {
                    throw new InvalidOperationException(String.Format("缓存项`[{0}]`类型错误, {1} or {2} ?",
                        key, entry.GetType().FullName, typeof(T).FullName));
                }
                value = (T)entry;
            }
            else {
                value = default(T);
            }
            return exist;
        }

        public T GetOrCreate<T>(String key, Func<String, T> factory) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = factory(key);
            Overwrite(key, value);
            return value;
        }

        public T GetOrCreate<T>(String key, Func<T> function) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value);
            return value;
        }

        public T GetOrCreate<T>(String key, Func<String, T> factory, TimeSpan slidingExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = factory(key);
            Overwrite(key, value, slidingExpiration);
            return value;
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

        public T GetOrCreate<T>(String key, Func<String, T> factory, DateTime absoluteExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = factory(key);
            Overwrite(key, value, absoluteExpiration);
            return value;
        }

        public void Overwrite<T>(String key, T value) {
            _client.Store(StoreMode.Set, BuildCacheKey(key), value);
        }

        //slidingExpiration 时间内无访问则过期
        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            _client.Store(StoreMode.Set, BuildCacheKey(key), value, slidingExpiration);
        }

        //absoluteExpiration 时过期
        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            _client.Store(StoreMode.Set, BuildCacheKey(key), value, absoluteExpiration);
        }

        public void Expire(String key) {
            _client.Remove(BuildCacheKey(key));
        }

        public void ExpireAll() {
            throw new NotSupportedException();
        }

        public int Count {
            get {
                throw new NotSupportedException();
            }
        }

        public string Dump() {
            throw new NotSupportedException();
        }
    }
}