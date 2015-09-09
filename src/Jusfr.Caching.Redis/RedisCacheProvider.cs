using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Redis {

    public class RedisCacheProvider : CacheProvider, IHttpRuntimeCacheProvider, IRegion {
        private readonly IRedis _redis;

        public String Region { get; private set; }

        public IRedis Redis {
            get { return _redis; }
        }

        public RedisCacheProvider(IRedis redis)
            : this(redis, null) {
        }

        public RedisCacheProvider(IRedis redis, String region) {
            _redis = redis;
            Region = region;
        }

        protected override String BuildCacheKey(String key) {
            return Region == null ? key : String.Concat(Region, "_", key);
        }

        public override void Expire(String key) {
            _redis.KeyDelete(BuildCacheKey(key));
        }

        public override bool TryGet<T>(String key, out T entry) {
            var val = _redis.StringGet(BuildCacheKey(key));
            if (!val.HasValue) {
                entry = default(T);
                return false;
            }
            entry = NewtonsoftJsonUtil.Parse<T>(val);
            return true;

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

        public T GetOrCreate<T>(String key, Func<T> function, TimeSpan slidingExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, slidingExpiration);
            return value;
        }

        public override void Overwrite<T>(String key, T value) {
            _redis.StringSet(BuildCacheKey(key), NewtonsoftJsonUtil.Stringify(value));
        }

        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            var key2 = BuildCacheKey(key);
            _redis.StringSet(key2, NewtonsoftJsonUtil.Stringify(value));
            _redis.KeyExpire(key2, absoluteExpiration);
        }

        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            var key2 = BuildCacheKey(key);
            _redis.StringSet(key2, NewtonsoftJsonUtil.Stringify(value));
            _redis.KeyExpire(key2, slidingExpiration);
        }
    }
}
