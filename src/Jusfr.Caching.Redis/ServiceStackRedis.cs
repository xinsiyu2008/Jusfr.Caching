using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace Jusfr.Caching.Redis {

    public class ServiceStackRedis : IDistributedLock, IRedis, IDisposable {
        private static ServiceStackRedis _default;
        private IRedisClientsManager _redisFactory;
        private readonly Byte[] _mutexBytes;
        
        private ServiceStackRedis() {
            _mutexBytes = Encoding.UTF8.GetBytes("Lock");
        }

        public static IRedis Default {
            get {
                if (_default == null) {
                    var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
                    if (String.IsNullOrWhiteSpace(connectionString)) {
                        throw new ArgumentOutOfRangeException("cache:redis", "Configuration \"cache:redis\" missing");
                    }
                    _default = new ServiceStackRedis(connectionString);
                }
                return _default;
            }
        }

        public ServiceStackRedis(String connectionString) {
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new ArgumentOutOfRangeException("connectionString");
            }
            _redisFactory = new PooledRedisClientManager(connectionString) { ConnectTimeout = 100 };
        }

        //注意，用完需要dispose
        public IRedisNativeClient GetRedisClient() {
            if (_redisFactory == null) {
                throw new ArgumentException("Configuration \"cache:redis\" miss or method Initialize() not invoke");
            }
            return (IRedisNativeClient)_redisFactory.GetClient();
        }

        public void Dispose() {
            if (_redisFactory != null) {
                _redisFactory.Dispose();
            }
        }

        public void Excute(Action<IRedisNativeClient> action) {
            if (_redisFactory == null) {
                throw new ArgumentException("Configuration \"cache:redis\" miss or method Initialize() not invoke");
            }
            using (var client = (IRedisNativeClient)_redisFactory.GetClient()) {
                action(client);
            }
        }

        public TResult Excute<TResult>(Func<IRedisNativeClient, TResult> func) {
            if (_redisFactory == null) {
                throw new ArgumentException("Configuration \"cache:redis\" miss or method Initialize() not invoke");
            }
            using (var client = (IRedisNativeClient)_redisFactory.GetClient()) {
                return func(client);
            }
        }

        //Key api

        public Boolean KeyExists(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.Exists(key) == 1L;
            }
        }

        public Boolean KeyDelete(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.Del(key) == 1L;
            }
        }

        public Boolean KeyExpire(RedisField key, TimeSpan expiry) {
            using (var client = GetRedisClient()) {
                return client.Expire(key, (Int32)expiry.TotalSeconds);
            }
        }

        public Boolean KeyExpire(RedisField key, DateTime expiry) {
            using (var client = GetRedisClient()) {
                return client.ExpireAt(key, expiry.ToUnixTime());
            }
        }

        public RedisField KeyRandom() {
            return Excute(c => c.RandomKey());
        }

        public Boolean KeyRename(RedisField key, RedisField newKey) {
            return Excute(c => c.RenameNx(key, newKey));
        }

        //String api

        public RedisField StringGet(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.Get(key);
            }
        }

        public void StringSet(RedisField key, RedisField value) {
            using (var client = GetRedisClient()) {
                client.Set(key, value);
            }
        }

        public Int64 StringIncrement(RedisField key, Int32 value = 1) {
            using (var client = GetRedisClient()) {
                return client.IncrBy(key, value);
            }
        }

        public Double StringIncrement(RedisField key, Double value) {
            using (var client = GetRedisClient()) {
                return client.IncrByFloat(key, value);
            }
        }

        //Hash api

        public Int64 HashLength(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.HLen(key);
            }
        }

        public Int64 HashIncrement(RedisField key, RedisField hashField, Int32 value = 1) {
            using (var client = GetRedisClient()) {
                return client.HIncrby(key, hashField, value);
            }
        }

        public Double HashIncrement(RedisField key, RedisField hashField, Double value) {
            using (var client = GetRedisClient()) {
                return client.HIncrbyFloat(key, hashField, value);
            }
        }

        public RedisField HashGet(RedisField key, RedisField hashField) {
            using (var client = GetRedisClient()) {
                return client.HGet(key, hashField);
            }
        }

        public RedisField[] HashGet(RedisField key, IList<RedisField> hashFields) {
            using (var client = GetRedisClient()) {
                var bytes = client.HMGet(key, hashFields.Select(h => (Byte[])h).ToArray());
                if (bytes == null) {
                    return null;
                }
                return bytes.Select(x => (RedisField)x).ToArray();
            }
        }

        public RedisEntry[] HashGetAll(RedisField key) {
            using (var client = GetRedisClient()) {
                var bytes = client.HGetAll(key);
                if (bytes == null) {
                    return null;
                }
                var list = new RedisEntry[bytes.Length / 2];
                for (int i = 0; i < list.Length; i++) {
                    list[i] = new RedisEntry(bytes[2 * i], bytes[2 * i + 1]);
                }
                return list;
            }
        }

        public Int64 HashSet(RedisField key, RedisField hashField, RedisField value) {
            using (var client = GetRedisClient()) {
                return client.HSet(key, hashField, value);
            }
        }

        public Int64 HashSet(RedisField key, RedisEntry hash) {
            using (var client = GetRedisClient()) {
                return client.HSet(key, hash.Name, hash.Value);
            }
        }

        public void HashSet(RedisField key, IList<RedisEntry> pairs) {
            using (var client = GetRedisClient()) {
                var hashFields = pairs.Select(p => (Byte[])p.Name).ToArray();
                var values = pairs.Select(p => (Byte[])p.Value).ToArray();
                client.HMSet(key, hashFields, values);
            }
        }

        public Boolean HashDelete(RedisField key, RedisField hashField) {
            using (var client = GetRedisClient()) {
                return client.HDel(key, hashField) == 1;
            }
        }

        //List api

        public Int64 ListLength(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.LLen(key);
            }
        }

        public Int64 ListLeftPush(RedisField key, RedisField value) {
            using (var client = GetRedisClient()) {
                return client.LPush(key, value);
            }
        }

        public Int64 ListLeftPush(RedisField key, IList<RedisField> values) {
            using (var client = (IRedisClient)GetRedisClient()) {
                client.AddRangeToList((String)key,
                    values.Reverse().Select(x => (String)x).ToList());
            }
            return 1; //因 ServiceStack.Redis 未返回数值
        }

        public RedisField ListLeftPop(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.LPop(key);
            }
        }

        public RedisField[] ListRange(RedisField key, Int32 startingFrom, Int32 endingAt) {
            using (var client = GetRedisClient()) {
                return client.LRange(key, startingFrom, endingAt)
                    .Select(r => (RedisField)r)
                    .ToArray();
            }
        }

        public Int64 ListRightPush(RedisField key, RedisField value) {
            using (var client = GetRedisClient()) {
                return client.RPush(key, value);
            }
        }

        public Int64 ListRightPush(RedisField key, IList<RedisField> values) {
            using (var client = (IRedisClient)GetRedisClient()) {
                client.AddRangeToList((String)key,
                    values.Select(x => (String)x).ToList());
            }
            return 1; //因 ServiceStack.Redis 未返回数值
        }

        public RedisField ListRightPop(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.RPop(key);
            }
        }

        //SortedSet api

        public Int64 SortedSetLength(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.ZCard(key);
            }
        }

        public Double? SortedSetScore(RedisField key, RedisField member) {
            using (var client = GetRedisClient()) {
                var value = client.ZScore(key, member);
                if (Double.IsNaN(value)) {
                    return null;
                }
                return value;
            }
        }

        public RedisField[] SortedSetRangeByRank(RedisField key, Int64 startPosition = 0, Int64 stopPosition = -1, Order order = Order.Ascending) {
            using (var client = GetRedisClient()) {
                Byte[][] bytes = order == Order.Ascending
                    ? client.ZRange(key, (int)startPosition, (int)stopPosition)
                    : client.ZRevRange(key, (int)startPosition, (int)stopPosition);
                if (bytes == null) {
                    return null;
                }
                return bytes.Select(r => (RedisField)r)
                    .ToArray();
            }
        }

        public RedisField[] SortedSetRangeByScore(RedisField key, Double startScore = Double.NegativeInfinity, Double stopScore = Double.PositiveInfinity, Int64 skip = 0, Int64 take = -1, Order order = Order.Ascending) {
            using (var client = GetRedisClient()) {
                Byte[][] bytes = order == Order.Ascending
                    ? client.ZRangeByScore(key, startScore, stopScore, (int)skip, (int)take)
                    : client.ZRevRangeByScore(key, startScore, stopScore, (int)skip, (int)take);
                if (bytes == null) {
                    return null;
                }
                return bytes.Select(r => (RedisField)r)
                    .ToArray();
            }
        }

        public RedisEntry[] SortedSetRangeByRankWithScores(RedisField key, Int64 startPosition = 0, Int64 stopPosition = -1, Order order = Order.Ascending) {
            using (var client = GetRedisClient()) {
                Byte[][] bytes = order == Order.Ascending
                    ? client.ZRangeWithScores(key, (int)startPosition, (int)stopPosition)
                    : client.ZRevRangeWithScores(key, (int)startPosition, (int)stopPosition);
                if (bytes == null) {
                    return null;
                }
                var list = new RedisEntry[bytes.Length / 2];
                for (int i = 0; i < list.Length; i++) {
                    list[i] = new RedisEntry(bytes[2 * i], bytes[2 * i + 1]);
                }
                return list;
            }
        }

        public RedisEntry[] SortedSetRangeByScoreWithScores(RedisField key, Double startScore = Double.NegativeInfinity, Double stopScore = Double.PositiveInfinity, Int64 skip = 0, Int64 take = -1, Order order = Order.Ascending) {
            using (var client = GetRedisClient()) {
                Byte[][] bytes = order == Order.Ascending
                    ? client.ZRangeByScoreWithScores(key, startScore, stopScore, (int)skip, (int)take)
                    : client.ZRevRangeByScoreWithScores(key, startScore, stopScore, (int)skip, (int)take);
                if (bytes == null) {
                    return null;
                }
                var list = new RedisEntry[bytes.Length / 2];
                for (int i = 0; i < list.Length; i++) {
                    list[i] = new RedisEntry(bytes[2 * i], bytes[2 * i + 1]);
                }
                return list;
            }
        }

        public Int64? SortedSetRank(RedisField key, RedisField member) {
            using (var client = GetRedisClient()) {
                var value = client.ZRank(key, member);
                if (value == -1) {
                    return null;
                }
                return value;
            }
        }

        public Int64 SortedSetAdd(RedisField key, RedisField value, Double score) {
            using (var client = GetRedisClient()) {
                return client.ZAdd(key, score, value);
            }
        }

        public Int64 SortedSetRemove(RedisField key, RedisField member) {
            using (var client = GetRedisClient()) {
                return client.ZRem(key, member);
            }
        }

        public Int64 SortedSetRemoveRangeByRank(RedisField key, Int64 startPosition, Int64 stopPosition) {
            using (var client = GetRedisClient()) {
                return client.ZRemRangeByRank(key, (int)startPosition, (int)stopPosition);
            }
        }

        public Int64 SortedSetRemoveRangeByScore(RedisField key, Double startScore, Double stopScore) {
            using (var client = GetRedisClient()) {
                return client.ZRemRangeByScore(key, startScore, stopScore);
            }
        }

        public Double SortedSetIncrement(RedisField key, RedisField member, Double value) {
            using (var client = GetRedisClient()) {
                return client.ZIncrBy(key, value, member);
            }
        }

        public IDisposable ReleasableLock(String key, Int32 expire = DistributedLockTime.DisposeMillisecond) {
            while (!TryLock(key, expire)) {
                Thread.Sleep(DistributedLockTime.IntervalMillisecond);
            }
            return new RedisLockReleaser(this, key);
        }

        public void Lock(String key, Int32 expire) { 
            while (!TryLock(key, expire)) {
                Thread.Sleep(DistributedLockTime.IntervalMillisecond);
            }
        }

        public Boolean TryLock(String key, Int32 expire) {
            if (Excute(x => x.SetNX(key, _mutexBytes) == 0L)) {
                return false;
            }
            if (expire > 0) {
                KeyExpire(key, TimeSpan.FromMilliseconds(expire));
            }
            return true;
        }

        public void UnLock(String key) {
            KeyDelete(key);
        }

        private struct RedisLockReleaser : IDisposable {
            private IRedis _redis;
            private String _key;

            public RedisLockReleaser(IRedis redis, String key) {
                _redis = redis;
                _key = key;
            }

            public void Dispose() {
                _redis.UnLock(_key);
            }
        }
    }
}
