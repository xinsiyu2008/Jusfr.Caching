using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Jusfr.Caching {
    internal class HttpContextCacheProvider : ICacheProvider {
        protected virtual String BuildCacheKey(String key) {
            return String.Concat("HttpContextCacheProvider_", key);
        }

        public Boolean TryGet<T>(String key, out T value) {
            key = BuildCacheKey(key);
            Boolean exist = false;
            if (HttpContext.Current.Items.Contains(key)) {
                exist = true;
                Object entry = HttpContext.Current.Items[key];
                if (entry != null && !(entry is T)) {
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

        public T GetOrCreate<T>(String key, Func<T> function) {
            T value;
            if (TryGet(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value);
            return value;
        }

        public T GetOrCreate<T>(String key, Func<String, T> factory) {
            T value;
            if (TryGet(key, out value)) {
                return value;
            }
            value = factory(key);
            Overwrite(key, value);
            return value;
        }

        public void Overwrite<T>(String key, T value) {
            key = BuildCacheKey(key);
            HttpContext.Current.Items[key] = value;
        }

        public void Expire(String key) {
            key = BuildCacheKey(key);
            HttpContext.Current.Items.Remove(key);
        }
    }
}
