using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Redis {
    public interface IRedis {
        Boolean KeyExists(RedisField key);
        Boolean KeyDelete(RedisField key);
        Boolean KeyExpire(RedisField key, TimeSpan expiry);
        Boolean KeyExpire(RedisField key, DateTime expiry);

        RedisField StringGet(RedisField key);
        void StringSet(RedisField key, RedisField value);
        Int64 StringIncrement(RedisField key, Int32 value = 1);
        Double StringIncrement(RedisField key, Double value);

        RedisField HashGet(RedisField key, RedisField hashField);
        Int64 HashLength(RedisField key);
        Int64 HashIncrement(RedisField key, RedisField hashField, Int32 value = 1);
        Double HashIncrement(RedisField key, RedisField hashField, Double value);
        Int64 HashSet(RedisField key, RedisField hashField, RedisField value);
        Int64 HashSet(RedisField key, RedisEntry hash);
        void HashSet(RedisField key, IList<RedisEntry> pairs);
        RedisEntry[] HashGetAll(RedisField key);
        Boolean HashDelete(RedisField key, RedisField hashField);

        Int64 ListLength(RedisField key);
        RedisField[] ListRange(RedisField key, Int32 startingFrom, Int32 endingAt);
        Int64 ListLeftPush(RedisField key, RedisField value);
        RedisField ListLeftPop(RedisField key);
        Int64 ListRightPush(RedisField key, RedisField value);
        RedisField ListRightPop(RedisField key);

        Int64 SortedSetLength(RedisField key);
        RedisField[] SortedSetRangeByRank(RedisField key, Int32 startPosition = 0, Int32 stopPosition = -1);
        RedisField[] SortedSetRangeByScore(RedisField key, double startScore = double.NegativeInfinity, double stopScore = double.PositiveInfinity, Int32 skip = 0, Int32 take = -1);
        Int64? SortedSetRank(RedisField key, RedisField member);
        Int64 SortedSetAdd(RedisField key, RedisField value, Double score);
        Boolean SortedSetRemove(RedisField key, RedisField member);
        Int64 SortedSetRemoveRangeByRank(RedisField key, Int32 startPosition, Int32 stopPosition);
        Int64 SortedSetRemoveRangeByScore(RedisField key, Double startScore, Double stopScore);
    }
}
