using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using Jusfr.Caching;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class HttpContextCacheProviderTest {
        [TestInitialize]
        public void Initialize() {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://localhost", null), new HttpResponse(null));
        }

        [TestMethod]
        public void NullCache() {
            var key = "key-null";
            HttpContext.Current.Items.Add(key, null);
            Assert.IsTrue(HttpContext.Current.Items.Contains(key));
            Assert.IsNull(HttpContext.Current.Items[key]);
        }

        [TestMethod]
        public void ValueType() {
            var key = "key-guid";
            ICacheProvider cache = new HttpContextCacheProvider();
            var id1 = Guid.NewGuid();
            var id2 = cache.GetOrCreate(key, () => id1);
            Assert.AreEqual(id1, id2);

            cache.Expire(key);
            Guid id3;
            var exist = cache.TryGet(key, out id3);
            Assert.IsFalse(exist);
            Assert.AreNotEqual(id1, id3);
            Assert.AreEqual(id3, Guid.Empty);
        }

        [TestMethod]
        public void ReferenceType() {
            var key = "key-object";
            ICacheProvider cache = new HttpContextCacheProvider();
            var id1 = new Object();
            var id2 = cache.GetOrCreate(key, () => id1);
            Assert.AreEqual(id1, id2);

            cache.Expire(key);
            Object id3;
            var exist = cache.TryGet(key, out id3);
            Assert.IsFalse(exist);
            Assert.AreNotEqual(id1, id3);
            Assert.AreEqual(id3, null);
        }

        [TestMethod]
        public void ReferenceTypeValueChangeToNull() {
            var key = "key-object-null";
            ICacheProvider cache = new HttpContextCacheProvider();
            var id1 = new Object();
            var id2 = cache.GetOrCreate(key, () => id1);
            Assert.AreEqual(id1, id2);

            id1 = null;
            Object id3;
            var exist = cache.TryGet(key, out id3);
            Assert.IsTrue(exist);
            Assert.AreEqual(id2, id3);
        }
    }
}
