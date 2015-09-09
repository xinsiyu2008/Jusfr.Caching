﻿using ServiceStack.Redis;
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

        RedisField HashGet(RedisField key, RedisField hashField);
        Int64 HashLength(RedisField key);
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
    }
}
