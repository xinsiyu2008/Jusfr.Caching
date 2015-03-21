using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using System.Configuration;

namespace Jusfr.Caching.Mongodb {
    public class MongodbCacheProvider : CacheProvider, IHttpRuntimeCacheProvider, IRegion {
        private static readonly MongoClient _client;
        private readonly String _cacheCollection = "cache";

        public String Region { get; private set; }

        static MongodbCacheProvider() {
            String connectionString = ConfigurationManager.AppSettings["mongodb"];
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new Exception("AppSettings \"mongodb\" missing");
            }
            _client = new MongoClient(connectionString);
        }

        public MongodbCacheProvider()
            : this(null) {
        }

        public MongodbCacheProvider(String region) {
            Region = region;
        }

        private MongoCollection<BsonDocument> GetCacheCollection() {
            var server = _client.GetServer();
            var database = server.GetDatabase(_cacheCollection);
            return database.GetCollection(Region ?? "default");
        }

        protected override String BuildCacheKey(String key) {
            return key;
        }

        protected override Boolean InnerTryGet(String key, out Object entry) {
            entry = null;
            var exist = false;
            var caches = GetCacheCollection();
            var cacheBson = caches.FindOne(Query.EQ("_id", key));
            var cache = BsonSerializer.Deserialize<Cache>(cacheBson);

            if (cache != null) {
                if (cache.AbsoluteExpiration.HasValue) {
                    if (cache.AbsoluteExpiration.Value <= DateTime.UtcNow) {
                        caches.Remove(Query<Cache>.EQ(e => e.Id, key));
                    }
                    else {
                        //注意这是一个 BSON 而非 T, 后续类型检查会失败
                        entry = cacheBson.GetElement("Entry");
                        exist = true;
                    }
                }
                else if (cache.SlidingExpiration.HasValue) {
                    if (cache.CreateTime.Add(cache.SlidingExpiration.Value) < DateTime.UtcNow) {
                        caches.Remove(Query<Cache>.EQ(e => e.Id, key));
                    }
                    else {
                        cache.CreateTime = DateTime.UtcNow;
                        caches.Save(cache);
                        //注意这是一个 BSON 而非 T, 后续类型检查会失败
                        entry = cacheBson.GetElement("Entry");
                        exist = true;
                    }
                }
                else {
                    //注意这是一个 BSON 而非 T, 后续类型检查会失败
                    entry = cacheBson.GetElement("Entry");
                    exist = true;
                }
            }
            return exist;
        }

        private Boolean InnerTryGet<T>(String key, out T entry) {
            entry = default(T);
            var exist = false;
            var caches = GetCacheCollection();
            var cache = caches.FindOneByIdAs<Cache<T>>(key);

            if (cache != null) {
                if (cache.AbsoluteExpiration.HasValue) {
                    if (cache.AbsoluteExpiration.Value <= DateTime.UtcNow) {
                        caches.Remove(Query<Cache>.EQ(e => e.Id, key));
                    }
                    else {
                        entry = cache.Entry;
                        exist = true;
                    }
                }
                else if (cache.SlidingExpiration.HasValue) {
                    if (cache.CreateTime.Add(cache.SlidingExpiration.Value) < DateTime.UtcNow) {
                        caches.Remove(Query<Cache>.EQ(e => e.Id, key));
                    }
                    else {
                        cache.CreateTime = DateTime.UtcNow;
                        caches.Save(cache);
                        entry = cache.Entry;
                        exist = true;
                    }
                }
                else {
                    entry = cache.Entry;
                    exist = true;
                }
            }
            return exist;
        }

        public override Boolean TryGet<T>(String key, out T entry) {
            String cacheKey = BuildCacheKey(key);
            //只调用了泛型版本的 InnerTryGet
            Boolean exist = InnerTryGet<T>(cacheKey, out entry);
            if (exist) {
                if (entry == null) {
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
            var cache = new Cache<T> {
                Id = key,
                CreateTime = DateTime.UtcNow,
                AbsoluteExpiration = null,
                Entry = value,
                SlidingExpiration = null
            };
            SaveCache(cache);
        }

        private void SaveCache(Cache cache) {
            var caches = GetCacheCollection();
            caches.Save(cache);
        }

        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            var cache = new Cache<T> {
                Id = key,
                CreateTime = DateTime.UtcNow,
                AbsoluteExpiration = null,
                Entry = value,
                SlidingExpiration = slidingExpiration
            };
            SaveCache(cache);
        }

        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            var cache = new Cache<T> {
                Id = key,
                CreateTime = DateTime.UtcNow,
                AbsoluteExpiration = absoluteExpiration,
                Entry = value,
                SlidingExpiration = null
            };
            SaveCache(cache);
        }

        public override void Expire(String key) {
            var caches = GetCacheCollection();
            caches.Remove(Query.EQ("_id", key));
        }

        public class Cache {
            public String Id { get; set; }
            public DateTime CreateTime { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
            public DateTime? AbsoluteExpiration { get; set; }
        }

        public class Cache<TEntry> : Cache {
            public TEntry Entry { get; set; }
        }
    }
}
