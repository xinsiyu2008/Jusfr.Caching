using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace Jusfr.Tests.Infrastructure.Caching {
    [TestClass]
    public class MemcachedTest {
        [TestMethod]
        public void TryGet() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                String key = "key-get";
                Object value = client.Get(key);
                Assert.IsNull(value);

                var exist = client.TryGet(key, out value);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void NullEntry() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                String key = "key-null";
                Object value = null;
                client.Store(StoreMode.Set, key, value);
                Assert.IsNull(value);

                var exist = client.TryGet(key, out value);
                Assert.IsTrue(exist);
            }
        }
    }
}

