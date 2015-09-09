using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Redis {
    public class ServiceStackRedis : IRedis {
        private readonly IRedisNativeClient _client;

        public ServiceStackRedis(IRedisNativeClient client) {
            _client = client;
        }

        public ServiceStackRedis() {
            var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new Exception("AppSettings \"redis\" missing");
            }

            var redisManager = new BasicRedisClientManager(connectionString);
            redisManager.ConnectTimeout = 100;
            _client = (IRedisNativeClient)redisManager.GetClient();
        }

        public Boolean KeyExists(RedisField key) {
            return _client.Exists(key) > 0;
        }

        public Int64 KeyDelete(RedisField key) {
            return _client.Del(key);
        }

        public Boolean KeyExpire(RedisField key, TimeSpan expiry) {
            return _client.Expire(key, (Int32)expiry.TotalSeconds);
        }

        public Boolean KeyExpire(RedisField key, DateTime expiry) {
            return _client.ExpireAt(key, expiry.ToUnixTime());
        }

        public RedisField StringGet(RedisField key) {
            return _client.Get(key);
        }

        public void StringSet(RedisField key, RedisField value) {
            _client.Set(key, value);
        }

        public RedisField HashGet(RedisField key, RedisField hashField) {
            return _client.HGet(key, hashField);
        }

        public Int64 HashSet(RedisField key, RedisField hashField, RedisField value) {
            return _client.HSet(key, hashField, value);
        }

        public void HashSet(RedisField key, IList<KeyValuePair<RedisField, RedisField>> pairs) {
            var hashFields = pairs.Select(p =>(Byte[]) p.Key).ToArray();
            var values = pairs.Select(p => (Byte[])p.Value).ToArray();
            _client.HMSet(key, hashFields, values);
        }

        public KeyValuePair<RedisField, RedisField>[] HashGetAll(RedisField key) {
            var hash = _client.HGetAll(key);
            if (hash.Length == 0) {
                return null;
            }
            var list = new KeyValuePair<RedisField, RedisField>[hash.Length / 2];
            for (int i = 0; i < list.Length; i++) {
                list[i] = new KeyValuePair<RedisField, RedisField>(hash[2 * i], hash[2 * i + 1]);
            }
            return list;
        }

        public Int64 HashDelete(RedisField key, RedisField hashField) {
            return _client.HDel(key, hashField);
        }

        public Int64 ListLength(RedisField key) {
            return _client.LLen(key);
        }

        public Int64 ListLeftPush(RedisField key, RedisField value) {
            return _client.LPush(key, value);
        }

        public RedisField ListLeftPop(RedisField key) {
            return _client.LPop(key);
        }

        public Int64 ListRightPush(RedisField key, RedisField value) {
            return _client.RPush(key, value);
        }

        public RedisField ListRightPop(RedisField key) {
            return _client.RPop(key);
        }
    }
}
