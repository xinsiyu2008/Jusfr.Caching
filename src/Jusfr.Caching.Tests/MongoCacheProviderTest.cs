using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jusfr.Caching;
using Jusfr.Caching.Memcached;
using System.Threading;
using Jusfr.Caching.Mongo;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class MongoCacheProviderTest {
        [TestMethod]
        public void TryGetTest() {
            var key = "TryGetTest";
            Guid val;

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite(key, val2);
            exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);
            Assert.AreEqual(val, val2);
        }

        [TestMethod]
        public void GetOrCreateTest() {
            var key = "GetOrCreateTest";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
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
            var key = "GetOrCreateWithslidingExpirationTest";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, TimeSpan.FromSeconds(4D));
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);
            Assert.AreEqual(result, val);

            Thread.Sleep(TimeSpan.FromSeconds(8D));
            exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);
        }

        [TestMethod]
        public void GetOrCreateWithAbsoluteExpirationTest() {
            var key = "GetOrCreateWithAbsoluteExpirationTest";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, DateTime.UtcNow.AddSeconds(4D));
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);
            Assert.AreEqual(result, val);

            //注意服务器时间与本地时间的误差
            Thread.Sleep(TimeSpan.FromSeconds(8D));
            exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);
        }

        [TestMethod]
        public void OverwriteTest() {
            var key = "OverwriteTest";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
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
            var key = "OverwriteWithslidingExpirationTest";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, TimeSpan.FromSeconds(4D));

            Thread.Sleep(TimeSpan.FromSeconds(8D));
            Guid val3;
            exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void OverwriteWithAbsoluteExpirationTest() {
            var key = "OverwriteWithAbsoluteExpirationTest";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, DateTime.UtcNow.AddSeconds(4D));

            Thread.Sleep(TimeSpan.FromSeconds(8D));
            Guid val3;
            exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void ExpireTest() {
            var key = "ExpireTest";
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MongoCacheProvider();
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
}

