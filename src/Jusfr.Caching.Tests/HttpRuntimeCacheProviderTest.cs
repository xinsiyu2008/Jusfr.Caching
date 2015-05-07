using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Jusfr.Caching;
using System.Web.Caching;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class HttpRuntimeCacheProviderTest {

        [TestMethod]
        public void NullCache() {
            var key = Guid.NewGuid().ToString();
            Object val;

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var exist = cacheProvider.TryGet<Object>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, null);

            cacheProvider.Overwrite(key, val);
            exist = cacheProvider.TryGet<Object>(key, out val);
            Assert.IsNull(val);
        }

        [TestMethod]
        public void TryGet() {
            var key = Guid.NewGuid().ToString();
            Guid val;

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);

        }

        [TestMethod]
        public void GetOrCreate() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            var cacheProvider = new HttpRuntimeCacheProvider();
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

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, TimeSpan.FromSeconds(1.5D));
            Assert.AreEqual(result, val);
            {
                Thread.Sleep(1000);
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }
            {
                Thread.Sleep(1000);
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }
            {
                Thread.Sleep(2000);
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsFalse(exist);
                Assert.AreEqual(val, Guid.Empty);
            }
        }

        [TestMethod]
        public void GetOrCreateWithAbsoluteExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val, DateTime.UtcNow.AddSeconds(2D));
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);
            Assert.AreEqual(result, val);

            Thread.Sleep(2000);
            exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);
        }

        [TestMethod]
        public void Overwrite() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
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

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, TimeSpan.FromSeconds(1D));

            Thread.Sleep(2000);
            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void OverwriteWithAbsoluteExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, DateTime.UtcNow.AddSeconds(1D));

            Thread.Sleep(2000);
            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void Expire() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);

            cacheProvider.Expire(key);
            Guid val2;
            var exist = cacheProvider.TryGet<Guid>(key, out val2);
            Assert.IsFalse(exist);
            Assert.AreEqual(val2, Guid.Empty);
        }

        [TestMethod]
        public void ExpireAll() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            HttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, () => val);
            Assert.AreEqual(result, val);
            Assert.IsTrue(cacheProvider.Count() > 0);


            cacheProvider.ExpireAll();
            Guid val2;
            var exist = cacheProvider.TryGet<Guid>(key, out val2);
            Assert.IsFalse(exist);
            Assert.AreEqual(val2, Guid.Empty);

            Assert.IsTrue(cacheProvider.Count() == 0);
        }


        [TestMethod]
        public void Callback() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            HttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var expireCallback = new CacheItemUpdateCallback(Callback);
            cacheProvider.Overwrite(key, val, DateTime.Now.AddSeconds(4D), expireCallback);
            Thread.Sleep(5000);
        }

        private void Callback(string key, CacheItemUpdateReason reason, out object expensiveObject, out CacheDependency dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration) {
            expensiveObject = null;
            dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = Cache.NoSlidingExpiration;
            Console.WriteLine("{0} key expired", key);
        }
    }
}
