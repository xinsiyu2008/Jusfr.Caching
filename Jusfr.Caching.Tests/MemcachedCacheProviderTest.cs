using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jusfr.Caching;
using Jusfr.Caching.Memcached;
using System.Threading;

namespace Jusfr.Tests.Infrastructure.Caching {
    [TestClass]
    public class MemcachedCacheProviderTest {
        [TestMethod]
        public void TryGet() {
            var key = Guid.NewGuid().ToString();
            Guid val;

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);

        }

        [TestMethod]
        public void GetOrCreate() {
            var key = Guid.NewGuid().ToString();
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
        public void GetOrCreateWithslidingExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, TimeSpan.FromSeconds(2D));
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);
            Assert.AreEqual(result, val);

            Thread.Sleep(4000);
            exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);
        }

        [TestMethod]
        public void GetOrCreateWithAbsoluteExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, DateTime.UtcNow.AddSeconds(2D));
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);
            Assert.AreEqual(result, val);

            Thread.Sleep(TimeSpan.FromSeconds(4D));
            exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);
        }

        [TestMethod]
        public void Overwrite() {
            var key = Guid.NewGuid().ToString();
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
        public void OverwriteWithslidingExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, TimeSpan.FromSeconds(2D));

            Thread.Sleep(4000);
            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void OverwriteWithAbsoluteExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, DateTime.UtcNow.AddSeconds(2D));

            Thread.Sleep(4000);
            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void Expire() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new MemcachedCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            cacheProvider.Expire(key);
            Guid val2;
            var exist = cacheProvider.TryGet<Guid>(key, out val2);
            Assert.IsFalse(exist);
            Assert.AreEqual(val2, Guid.Empty);
        }
    }
}
