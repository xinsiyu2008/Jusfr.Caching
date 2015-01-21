using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching {
    public abstract class CacheProvider : ICacheProvider {
        protected virtual String BuildCacheKey(String key) {
            return key;
        }
        
        protected abstract Boolean InnerTryGet(String key, out Object entry);

        public virtual Boolean TryGet<T>(String key, out T entry) {
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

        public virtual T GetOrCreate<T>(String key, Func<T> function) {
            T entry;
            if (TryGet(key, out entry)) {
                return entry;
            }
            entry = function();
            Overwrite(key, entry);
            return entry;
        }

        public virtual T GetOrCreate<T>(String key, Func<String, T> factory) {
            T entry;
            if (TryGet(key, out entry)) {
                return entry;
            }
            entry = factory(key);
            Overwrite(key, entry);
            return entry;
        }

        public abstract void Overwrite<T>(String key, T value);

        public abstract void Expire(String key);
    }
}
