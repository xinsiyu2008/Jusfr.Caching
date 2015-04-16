using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Redis {
    public class RedisCacheProvider : CacheProvider, IHttpRuntimeCacheProvider, IRegion {
        private static ConnectionMultiplexer _connection;
        public String Region { get; private set; }

        static RedisCacheProvider() {
            String connectionString = ConfigurationManager.AppSettings["redis"];
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new Exception("AppSettings \"redis\" missing");
            }
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public RedisCacheProvider()
            : this(null) {
        }

        public RedisCacheProvider(String region) {
            Region = region;
        }

        private IDatabase GetDatabase() {
            if (!_connection.IsConnected) {
                _connection = ConnectionMultiplexer.Connect(_connection.Configuration);
            }
            return _connection.GetDatabase();
        }

        protected override String BuildCacheKey(String key) {
            return Region == null ? key : String.Concat(Region, "_", key);
        }

        private Boolean InnerTryGet(string key, out RedisValue entry) {
            var database = GetDatabase();
            entry = database.StringGet(key);
            return entry != RedisValue.Null;
        }

        public override bool TryGet<T>(string key, out T entry) {
            String cacheKey = BuildCacheKey(key);
            RedisValue cache;
            var exist = InnerTryGet(cacheKey, out cache);
            if (exist) {
                if (cache.HasValue) {
                    entry = BinarySerializer.Deserialize<T>((Byte[])cache);
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

        public override void Expire(string key) {
            var database = GetDatabase();
            database.KeyDelete(BuildCacheKey(key));
        }

        public T GetOrCreate<T>(string key, Func<T> function, TimeSpan slidingExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, slidingExpiration);
            return value;
        }

        public T GetOrCreate<T>(string key, Func<T> function, DateTime absoluteExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, absoluteExpiration);
            return value;
        }

        public override void Overwrite<T>(string key, T value) {
            var database = GetDatabase();
            database.StringSet(key, BinarySerializer.Serialize(value));
        }

        public void Overwrite<T>(string key, T value, TimeSpan slidingExpiration) {
            var database = GetDatabase();
            database.StringSet(key, BinarySerializer.Serialize(value), expiry: slidingExpiration);
        }

        public void Overwrite<T>(string key, T value, DateTime absoluteExpiration) {
            var database = GetDatabase();
            database.StringSet(key, BinarySerializer.Serialize(value), expiry: absoluteExpiration.Subtract(DateTime.UtcNow));
        }

        internal class BinarySerializer {
            public static byte[] Serialize(Object obj) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream()) {
                    formatter.Serialize(stream, obj);
                    return stream.ToArray();
                }
            }

            public static T Deserialize<T>(Byte[] buffer) {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream(buffer)) {
                    return (T)formatter.Deserialize(stream);
                }
            }
        }
    }
}
