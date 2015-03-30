using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching {
    public abstract class CacheProvider : ICacheProvider {
        protected virtual String BuildCacheKey(String key) {
            return key;
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public abstract Boolean TryGet<T>(String key, out T entry);        

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
