using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class MemcachedTest {
        [TestMethod]
        public void Online() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                String key = Guid.NewGuid().ToString("n");
                Object value = client.Get(key);
                Assert.IsNull(value);

                var exist = client.TryGet(key, out value);
                Assert.IsFalse(exist);
                Assert.IsNull(value);

                value=Guid.NewGuid();
                client.Store(StoreMode.Set, key, value);
                exist = client.TryGet(key, out value);
                Assert.IsTrue(exist);
                Assert.IsNotNull(value);
            }
        }

        [TestMethod]
        public void NullEntry() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                String key = Guid.NewGuid().ToString("n");
                Object value = null;
                client.Store(StoreMode.Set, key, value);
                var exist = client.TryGet(key, out value);
                Assert.IsTrue(exist);
                Assert.IsNull(value);
            }
        }
    }
}

