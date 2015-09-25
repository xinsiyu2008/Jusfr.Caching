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
            return _client.Exists(key) == 1L;
        }

        public Boolean KeyDelete(RedisField key) {
            return _client.Del(key) == 1L;
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

        public Int64 HashLength(RedisField key) {
            return _client.HLen(key);
        }

        public RedisField HashGet(RedisField key, RedisField hashField) {
            return _client.HGet(key, hashField);
        }

        public Int64 HashSet(RedisField key, RedisField hashField, RedisField value) {
            return _client.HSet(key, hashField, value);
        }

        public Int64 HashSet(RedisField key, RedisEntry hash) {
            return _client.HSet(key, hash.Name, hash.Value);
        }

        public void HashSet(RedisField key, IList<RedisEntry> pairs) {
            var hashFields = pairs.Select(p => (Byte[])p.Name).ToArray();
            var values = pairs.Select(p => (Byte[])p.Value).ToArray();
            _client.HMSet(key, hashFields, values);
        }

        public RedisEntry[] HashGetAll(RedisField key) {
            var hash = _client.HGetAll(key);
            if (hash.Length == 0) {
                return null;
            }
            var list = new RedisEntry[hash.Length / 2];
            for (int i = 0; i < list.Length; i++) {
                list[i] = new RedisEntry(hash[2 * i], hash[2 * i + 1]);
            }
            return list;
        }

        public Boolean HashDelete(RedisField key, RedisField hashField) {
            return _client.HDel(key, hashField) == 1;
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

        public RedisField[] ListRange(RedisField key, Int32 startingFrom, Int32 endingAt) {
            return _client.LRange(key, startingFrom, endingAt)
                .Select(r => (RedisField)r)
                .ToArray();
        }

        public Int64 ListRightPush(RedisField key, RedisField value) {
            return _client.RPush(key, value);
        }

        public RedisField ListRightPop(RedisField key) {
            return _client.RPop(key);
        }

        public Int64 SortedSetLength(RedisField key) {
            return _client.ZCard(key);
        }

        public RedisField[] SortedSetRangeByRank(RedisField key, Int32 startPosition = 0, Int32 stopPosition = -1) {
            return _client.ZRange(key, startPosition, stopPosition)
                .Select(r => (RedisField)r)
                .ToArray();
        }

        public Int64? SortedSetRank(RedisField key, RedisField member) {
            var value = _client.ZRank(key, member);
            if (value == -1) {
                return null;
            }
            return value;
        }

        public long SortedSetAdd(RedisField key, RedisField value, double score) {
            return _client.ZAdd(key, score, value);
        }

        public bool SortedSetRemove(RedisField key, RedisField member) {
            return _client.ZRem(key, member) == 1;
        }

        public long SortedSetRemoveRangeByRank(RedisField key, Int32 startPosition, Int32 stopPosition) {
            return _client.ZRemRangeByRank(key, startPosition, stopPosition);
        }

        public long SortedSetRemoveRangeByScore(RedisField key, double startScore, double stopScore) {
            return _client.ZRemRangeByScore(key, startScore, stopScore);
        }
    }
}
