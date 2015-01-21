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
    internal class HttpRuntimeCacheProvider : IHttpRuntimeCacheProvider {
        private static readonly Object _sync = new Object();
        private static readonly Object _nullEntry = new Object();
        private Boolean _supportNull = true;

        public HttpRuntimeCacheProvider(Boolean supportNull = true) {
            _supportNull = supportNull;
        }

        protected virtual String BuildCacheKey(String key) {
            return String.Concat("HttpRuntimeCacheProvider_", key);
        }

        protected virtual Object BuildCacheEntry<T>(T value) {
            Object entry = value;
            if (value == null) {
                if (_supportNull) {
                    entry = _nullEntry;
                }
                else {
                    throw new InvalidOperationException(String.Format("Null cache item not supported, try ctor with paramter 'supportNull = true' "));
                }
            }
            return entry;
        }

        public Boolean TryGet<T>(String key, out T value) {
            Object entry = HttpRuntime.Cache.Get(BuildCacheKey(key));
            Boolean exist = false;
            if (entry != null) {
                exist = true;
                if (!(entry is T)) {
                    if (_supportNull && !(entry == _nullEntry)) {
                        throw new InvalidOperationException(String.Format("缓存项`[{0}]`类型错误, {1} or {2} ?",
                            key, entry.GetType().FullName, typeof(T).FullName));
                    }
                    value = (T)((Object)null);
                }
                else {
                    value = (T)entry;
                }
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

        public T GetOrCreate<T>(String key, Func<T> function, TimeSpan slidingExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, slidingExpiration);
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

        public void Expire(String key) {
            HttpRuntime.Cache.Remove(BuildCacheKey(key));
        }

        protected virtual Boolean Hit(DictionaryEntry entry) {
            return (entry.Key is String) && ((String)entry.Key).StartsWith("HttpRuntimeCacheProvider_");
        }

        //todo: lock 的必要性?
        public void ExpireAll() {
            lock (_sync) {
                var entries = HttpRuntime.Cache.OfType<DictionaryEntry>().Where(Hit);
                foreach (var entry in entries) {
                    HttpRuntime.Cache.Remove((String)entry.Key);
                }
            }
        }

        public Int32 Count {
            get {
                lock (_sync) {
                    return HttpRuntime.Cache.OfType<DictionaryEntry>().Where(Hit).Count();
                }
            }
        }

        public String Dump() {
            var builder = new StringBuilder(1024);
            builder.AppendLine("--------------------HttpRuntimeCacheProvider.Dump--------------------------");
            builder.AppendFormat("EffectivePercentagePhysicalMemoryLimit: {0}\r\n", HttpRuntime.Cache.EffectivePercentagePhysicalMemoryLimit);
            builder.AppendFormat("EffectivePrivateBytesLimit: {0}\r\n", HttpRuntime.Cache.EffectivePrivateBytesLimit);
            builder.AppendFormat("Count: {0}\r\n", HttpRuntime.Cache.Count);
            builder.AppendLine();
            lock (_sync) {
                var entries = HttpRuntime.Cache.OfType<DictionaryEntry>().Where(Hit).OrderBy(de => de.Key);
                foreach (var entry in entries) {
                    builder.AppendFormat("{0}\r\n    {1}\r\n", entry.Key, entry.Value.GetType().FullName);
                }
            }
            builder.AppendLine("--------------------HttpRuntimeCacheProvider.Dump--------------------------");
            Debug.WriteLine(builder.ToString());
            return builder.ToString();
        }
    }
}
