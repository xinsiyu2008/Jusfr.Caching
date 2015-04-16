using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jusfr.Caching;
using Jusfr.Caching.Memcached;
using System.Threading;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class MemcachedCacheProviderTest {
        [TestMethod]
        public void GetOrCreateTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            {
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }

            {
                var result2 = cacheProvider.GetOrCreate<Guid>(key, () => {
                    Assert.Fail();
                    return Guid.NewGuid();
                });
                Assert.AreEqual(result2, val);
            }
        }

        [TestMethod]
        public void GetOrCreateWithslidingExpirationTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, TimeSpan.FromSeconds(6D));
            Assert.AreEqual(result, val);

            for (var i = 0; i < 2; i++) {
                Thread.Sleep(TimeSpan.FromSeconds(4D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }

            {
                Thread.Sleep(TimeSpan.FromSeconds(8D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void GetOrCreateWithAbsoluteExpirationTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, DateTime.UtcNow.AddSeconds(6D));
            Assert.AreEqual(result, val);

            {
                Thread.Sleep(TimeSpan.FromSeconds(4D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }

            {
                Thread.Sleep(TimeSpan.FromSeconds(8D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsFalse(exist);
            }

        }

        [TestMethod]
        public void OverwriteTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2);

            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsTrue(exist);
            Assert.AreEqual(val3, val2);
        }

        [TestMethod]
        public void OverwriteWithslidingExpirationTest() {
            var key = Guid.NewGuid().ToString("n");
            key = "ecc6cf4d923f48b3a98f11b9641549fd";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            cacheProvider.Overwrite(key, val, TimeSpan.FromSeconds(6D));

            Guid result;
            for (var i = 0; i < 2; i++) {
                Thread.Sleep(TimeSpan.FromSeconds(4D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }

            {
                Thread.Sleep(TimeSpan.FromSeconds(8D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void OverwriteWithAbsoluteExpirationTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var t1 = DateTime.Now.AddSeconds(5D).ToTimestamp();
            var t2 = DateTime.UtcNow.AddSeconds(5D).ToTimestamp();
            Assert.AreEqual(t1, t2);

            cacheProvider.Overwrite(key, val, DateTime.Now.AddSeconds(6D));
            Guid result = Guid.NewGuid();

            {
                Thread.Sleep(TimeSpan.FromSeconds(4D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }

            {
                Thread.Sleep(TimeSpan.FromSeconds(8D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void ExpireTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);

            cacheProvider.Expire(key);
            Guid val2;
            exist = cacheProvider.TryGet<Guid>(key, out val2);
            Assert.IsFalse(exist);
            Assert.AreEqual(val2, Guid.Empty);
        }
    }

    public static class Util {
        //((DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000).Dump();
        //((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).Dump();
        //((Int64)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTime.Now.Kind)).TotalSeconds).Dump();
        public static Int64 ToTimestamp(this DateTime time) {
            return (Int64)(time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds;
        }

        public static DateTime FromTimestamp(this Int64 timestamp) {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
        }
    }
}

