using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jusfr.Caching.Redis;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using ServiceStack.Redis;
using System.Diagnostics;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class RedisTest {

        [TestMethod]
        public void RedisField_Equal_Test() {
            //StackExchange.Redis.IDatabase d;
            //d.SortedSetRangeByRankWithScores;
            //d.SortedSetRangeByScoreWithScores;

            RedisField f1 = new RedisField();
            RedisField f2 = new RedisField();
            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1.Equals(f2));

            String str = Guid.NewGuid().ToString();
            f1 = str;
            f2 = str;
            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1.Equals(f2));

            Object f3 = f1;
            Assert.IsTrue(f3.Equals(f2));
            Assert.IsTrue(f2.Equals(f3));

            f2 = Guid.NewGuid().ToString();
            Assert.IsTrue(f1 != f2);
            Assert.IsTrue(!f1.Equals(f2));

            Assert.IsTrue(!f3.Equals(f2));
            Assert.IsTrue(!f2.Equals(f3));
        }

        [TestMethod]
        public void RedisEntry_Equal_Test() {
            var e1 = new RedisEntry();
            var e2 = new RedisEntry();
            Assert.IsTrue(e1 == e2);
            Assert.IsTrue(e1.Equals(e2));

            KeyValuePair<RedisField, RedisField> e3 = e2;
            Assert.IsTrue(e1 == e2);
            Assert.IsTrue(e1.Equals(e2));

            Object e4 = e2;
            Assert.IsTrue(e1.Equals(e4));
            Assert.IsTrue(e4.Equals(e1));

            var e5 = new RedisEntry(Guid.NewGuid().ToString(), e2.Value);
            Assert.IsTrue(e1 != e5);
            Assert.IsTrue(!e1.Equals(e5));
        }

        [TestMethod]
        public void StringTest() {
            var cacheKey = Guid.NewGuid().ToString();
            IRedis redis = new ServiceStackRedis();

            //StringGet
            var cacheField = redis.StringGet(cacheKey);
            Assert.IsFalse(cacheField.HasValue);
            Assert.AreEqual((String)cacheField, null);

            //StringSet
            var cacheValue = Guid.NewGuid().ToString();
            redis.StringSet(cacheKey, cacheValue);

            //StringGet again
            cacheField = redis.StringGet(cacheKey);
            Assert.IsTrue(cacheField.HasValue);
            Assert.AreEqual((String)cacheField, cacheValue);

            //KeyDelete
            redis.KeyDelete(cacheKey);
        }

        [TestMethod]
        public void ListTest() {
            var cacheKey = Guid.NewGuid().ToString();
            IRedis redis = new ServiceStackRedis();
            var linkList = new LinkedList<String>();
            const Int32 listLength = 4;

            Action init = () => {
                redis.KeyDelete(cacheKey);
                linkList.Clear();

                for (int i = 0; i < listLength; i++) {
                    var cacheValue = Guid.NewGuid().ToString();

                    if ((Guid.NewGuid().GetHashCode() & 1) == 0) {
                        linkList.AddFirst(cacheValue);
                        //ListLeftPush
                        redis.ListLeftPush(cacheKey, linkList.First.Value);
                    }
                    else {
                        linkList.AddLast(cacheValue);
                        //ListLeftPush
                        redis.ListRightPush(cacheKey, linkList.Last.Value);
                    }
                }
            };

            init();
            Assert.AreEqual(linkList.Count, redis.ListLength(cacheKey));


            for (int i = 0; i < listLength; i++) {
                RedisField cacheItem;
                if ((Guid.NewGuid().GetHashCode() & 1) == 0) {
                    cacheItem = redis.ListLeftPop(cacheKey);
                    Assert.AreEqual(linkList.First.Value, (String)cacheItem);
                    linkList.RemoveFirst();
                }
                else {
                    cacheItem = redis.ListRightPop(cacheKey);
                    Assert.AreEqual(linkList.Last.Value, (String)cacheItem);
                    linkList.RemoveLast();
                }

                Assert.AreEqual(linkList.Count, redis.ListLength(cacheKey));
            }

            var cacheEists = redis.KeyExists(cacheKey);
            Assert.IsFalse(cacheEists);
        }

        [TestMethod]
        public void HashTest() {
            var cacheKey = Guid.NewGuid().ToString();
            IRedis redis = new ServiceStackRedis();

            var count = 10;
            var names = new String[count].ToList();
            var values = new String[count];

            for (int i = 0; i < count; i++) {
                names[i] = Guid.NewGuid().ToString();
                values[i] = Guid.NewGuid().ToString();
            }
            var list = Enumerable.Range(0, count)
                .Select(i => new RedisEntry(names[i], values[i]))
                .ToArray();

            redis.HashSet(cacheKey, list);
            Assert.AreEqual(redis.HashLength(cacheKey), count);

            var array = redis.HashGet(cacheKey, names.Select(x => (RedisField)x).ToArray());
            for (int i = 0; i < count; i++) {
                Assert.IsTrue(array[i] == values[i]);
            }

            var hash = redis.HashGetAll(cacheKey);
            Assert.AreEqual(hash.Length, count);
            for (int i = 0; i < count; i++) {
                Assert.IsTrue(hash[i].Name == names[i]);
                Assert.IsTrue(hash[i].Value == values[i]);
            }

            for (int i = 0; i < count; i++) {
                var cacheItem = redis.HashGet(cacheKey, names[i]);
                Assert.IsTrue((String)cacheItem == values[i]);
            }

            for (int i = 0; i < count; i++) {
                var deleted = redis.HashDelete(cacheKey, names[i]);
                Assert.IsTrue(deleted);
            }

            var exist = redis.KeyExists(cacheKey);
            Assert.IsFalse(exist);
        }

        [TestMethod]
        public void SortSertTest() {
            var cacheKey = Guid.NewGuid().ToString();
            IRedis redis = new ServiceStackRedis();

            var random = new Random();
            var list = Enumerable.Repeat(0, 20).Select(r => random.Next(100)).Distinct().ToList();

            for (int i = 0; i < list.Count; i++) {
                redis.SortedSetAdd(cacheKey, list[i].ToString(), i);
                var len = redis.SortedSetLength(cacheKey);
                Assert.AreEqual(len, i + 1);
            }

            var values = redis.SortedSetRangeByRank(cacheKey, 0, -1);
            Assert.AreEqual(values.Length, list.Count);
            for (int i = 0; i < list.Count; i++) {
                Assert.AreEqual((String)values[i], list[i].ToString());
            }

            for (int i = 0; i < 3; i++) {
                var index1 = random.Next(list.Count);
                var index2 = redis.SortedSetRank(cacheKey, list[index1].ToString());
                Assert.AreEqual(index1, index2);
            }

            for (int i = 0; i < 3; i++) {
                var index = random.Next(list.Count);
                var value = list[index];
                list.RemoveAt(index);
                var removed = redis.SortedSetRemove(cacheKey, value.ToString());
                Assert.IsTrue(removed);
                var len = redis.SortedSetLength(cacheKey);
                Assert.AreEqual(len, list.Count);
            }

            Assert.IsTrue(redis.SortedSetLength(cacheKey) > 3);
            var removedByRank = redis.SortedSetRemoveRangeByRank(cacheKey, 0, 2);
            Assert.AreEqual(removedByRank, 3);
        }

        [TestMethod]
        public void SortSertOrderTest() {
            var cacheKey = Guid.NewGuid().ToString();
            IRedis redis = new ServiceStackRedis();

            var random = new Random();
            var list = Enumerable.Repeat(0, 4).Select(r => random.Next(100)).ToList();
            list.ForEach(i => redis.SortedSetAdd(cacheKey, i.ToString(), (double)i));

            var list1 = redis.SortedSetRangeByRank(cacheKey, order: Order.Ascending);
            Assert.AreEqual(list1.Length, list.Count);

            var array1 = list.ToArray();
            Array.Sort(array1);
            for (int i = 0; i < list1.Length; i++) {
                Assert.AreEqual(Convert.ToInt32( list1[i]), array1[i]);
            }

            var list2 = redis.SortedSetRangeByRank(cacheKey, order: Order.Descending);
            Assert.AreEqual(list2.Length, list.Count);

            var array2 = array1.Reverse().ToArray();
            for (int i = 0; i < list2.Length; i++) {
                Assert.AreEqual(Convert.ToInt32(list2[i]), array2[i]);
            }

        }


        [TestMethod]
        public void RedisParallelTest() {
            //StackExchange.Redis.IDatabase d;
            var key = "RedisParallelTest";
            var redis = new ServiceStackRedis();
            redis.KeyDelete(key);

            Action action = () => Console.WriteLine(redis.StringIncrement(key));
            Parallel.Invoke(Enumerable.Repeat(1, 100).Select(i => action).ToArray());
        }

        [TestMethod]
        public void RedisNativeClientTest1() {
            var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
            var key = "RedisParallelTest1";

            using (var redisManager = new PooledRedisClientManager(connectionString)) {
                redisManager.ConnectTimeout = 100;
                using (var client = (IRedisNativeClient)redisManager.GetClient()) {
                    client.Del(key);
                }
            }

            var actions = Enumerable.Repeat(1, 100).Select(i => new Action(() => {
                using (var redisManager = new BasicRedisClientManager(connectionString)) {
                    redisManager.ConnectTimeout = 100;
                    using (var client = (IRedisNativeClient)redisManager.GetClient()) {
                        client.Incr(key);
                    }
                }
            })).ToArray();

            Parallel.Invoke(actions);
        }

        [TestMethod]
        public void RedisNativeClientTest2() {
            var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
            var key = "RedisParallelTest2";

            using (var redisManager = new BasicRedisClientManager(connectionString)) {
                redisManager.ConnectTimeout = 100;
                using (var client = (IRedisNativeClient)redisManager.GetCacheClient()) {
                    client.Del(key);
                }
            }

            var actions = Enumerable.Repeat(1, 100).Select(i => new Action(() => {
                using (var redisManager = new BasicRedisClientManager(connectionString)) {
                    redisManager.ConnectTimeout = 100;
                    using (var client = (IRedisNativeClient)redisManager.GetCacheClient()) {
                        client.Incr(key);
                    }
                }
            })).ToArray();

            Parallel.Invoke(actions);
        }

        [TestMethod]
        public void RedisNativeClientTest3() {
            var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
            var key = "RedisParallelTest3";

            var redisManager = new BasicRedisClientManager(connectionString);

            redisManager.ConnectTimeout = 100;
            using (var client = (IRedisNativeClient)redisManager.GetCacheClient()) {
                client.Del(key);
            }

            var actions = Enumerable.Repeat(1, 100).Select(i => new Action(() => {
                redisManager.ConnectTimeout = 100;
                using (var client = (IRedisNativeClient)redisManager.GetCacheClient()) {
                    client.Incr(key);
                }
            })).ToArray();

            Parallel.Invoke(actions);
        }

        [TestMethod]
        public void HMGet() {
            var redis = new ServiceStackRedis();
            var key = "/Work/0/100";
            redis.HashSet(key, "Read", "2");
            redis.HashSet(key, "Share", "3");
            var hash = redis.HashGet(key, new RedisField[] { "Id", "Read", "Share" });

        }

        [TestMethod]
        public void RedisNativeClientTest4() {
            var redis = new ServiceStackRedis();
            var key = "RedisParallelTest4";
            redis.KeyDelete(key);
            var actions = Enumerable.Repeat(1, 100).Select(i => new Action(() => redis.StringIncrement(key))).ToArray();
            Parallel.Invoke(actions);
        }
    }
}
