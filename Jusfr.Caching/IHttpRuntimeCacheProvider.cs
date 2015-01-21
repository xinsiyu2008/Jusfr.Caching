using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching {
    public interface IHttpRuntimeCacheProvider : ICacheProvider {
        T GetOrCreate<T>(String key, Func<T> function, TimeSpan slidingExpiration);
        T GetOrCreate<T>(String key, Func<String, T> factory, TimeSpan slidingExpiration);
        T GetOrCreate<T>(String key, Func<T> function, DateTime absoluteExpiration);
        T GetOrCreate<T>(String key, Func<String, T> factory, DateTime absoluteExpiration);
        void Overwrite<T>(String key, T value, TimeSpan slidingExpiration);
        void Overwrite<T>(String key, T value, DateTime absoluteExpiration);
        void ExpireAll();
        Int32 Count { get; }
        String Dump();
    }
}
