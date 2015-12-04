using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Redis {
    
    public interface IRedis : IDistributedLock {
        //Key
        Boolean KeyExists(RedisField key);
        Boolean KeyDelete(RedisField key);
        Boolean KeyExpire(RedisField key, TimeSpan expiry);
        Boolean KeyExpire(RedisField key, DateTime expiry);
        RedisField KeyRandom();

        //String
        RedisField StringGet(RedisField key);
        void StringSet(RedisField key, RedisField value);
        Int64 StringIncrement(RedisField key, Int32 value = 1);
        Double StringIncrement(RedisField key, Double value);

        //Hash
        RedisField HashGet(RedisField key, RedisField hashField);
        RedisField[] HashGet(RedisField key, IList<RedisField> hashFields);
        RedisEntry[] HashGetAll(RedisField key);
        Int64 HashLength(RedisField key);
        Int64 HashIncrement(RedisField key, RedisField hashField, Int32 value = 1);
        Double HashIncrement(RedisField key, RedisField hashField, Double value);
        Int64 HashSet(RedisField key, RedisField hashField, RedisField value);
        Int64 HashSet(RedisField key, RedisEntry hash);
        void HashSet(RedisField key, IList<RedisEntry> pairs);
        Boolean HashDelete(RedisField key, RedisField hashField);

        //List
        Int64 ListLength(RedisField key);
        RedisField[] ListRange(RedisField key, Int32 startingFrom, Int32 endingAt);
        Int64 ListLeftPush(RedisField key, RedisField value);
        RedisField ListLeftPop(RedisField key);
        Int64 ListRightPush(RedisField key, RedisField value);
        RedisField ListRightPop(RedisField key);

        //ZSet
        Int64 SortedSetLength(RedisField key);
        Double? SortedSetScore(RedisField key, RedisField member);
        RedisField[] SortedSetRangeByRank(RedisField key, Int64 startPosition = 0, Int64 stopPosition = -1, Order order = Order.Ascending);
        RedisField[] SortedSetRangeByScore(RedisField key, Double startScore = Double.NegativeInfinity, Double stopScore = Double.PositiveInfinity, Int64 skip = 0, Int64 take = -1, Order order = Order.Ascending);
        RedisEntry[] SortedSetRangeByRankWithScores(RedisField key, Int64 startPosition = 0, Int64 stopPosition = -1, Order order = Order.Ascending);
        RedisEntry[] SortedSetRangeByScoreWithScores(RedisField key, Double startScore = Double.NegativeInfinity, Double stopScore = Double.PositiveInfinity, Int64 skip = 0, Int64 take = -1, Order order = Order.Ascending);
        Int64? SortedSetRank(RedisField key, RedisField member);
        Int64 SortedSetAdd(RedisField key, RedisField value, Double score);
        Boolean SortedSetRemove(RedisField key, RedisField member);
        Int64 SortedSetRemoveRangeByRank(RedisField key, Int64 startPosition, Int64 stopPosition);
        Int64 SortedSetRemoveRangeByScore(RedisField key, Double startScore, Double stopScore);
        Double SortedSetIncrement(RedisField key, RedisField member, Double value);
    }

    public enum Order {
        Ascending = 0,
        Descending = 1
    }
}
