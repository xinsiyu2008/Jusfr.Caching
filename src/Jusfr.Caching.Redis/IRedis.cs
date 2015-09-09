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
        Int64 KeyDelete(RedisField key);
        Boolean KeyExpire(RedisField key, TimeSpan expiry);
        Boolean KeyExpire(RedisField key, DateTime expiry);

        RedisField StringGet(RedisField key);
        void StringSet(RedisField key, RedisField value);

        RedisField HashGet(RedisField key, RedisField hashField);
        Int64 HashSet(RedisField key, RedisField hashField, RedisField value);
        void HashSet(RedisField key, IList<KeyValuePair<RedisField, RedisField>> pairs);
        KeyValuePair<RedisField, RedisField>[] HashGetAll(RedisField key);
        Int64 HashDelete(RedisField key, RedisField hashField);

        Int64 ListLength(RedisField key);
        Int64 ListLeftPush(RedisField key, RedisField value);
        RedisField ListLeftPop(RedisField key);
        Int64 ListRightPush(RedisField key, RedisField value);
        RedisField ListRightPop(RedisField key);
    }
}
