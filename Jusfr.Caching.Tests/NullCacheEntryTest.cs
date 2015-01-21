using Jusfr.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Tests.Infrastructure.Caching {
    [TestClass]
    public class NullCacheEntryTest {
        [TestMethod]
        public void NullableEntry() {
            {
                Person person = new Person {
                    Id = 2,
                    Name = "Rattz"
                };
                NullableEntry<Person> obj = person;
                Person person2 = obj;
                Assert.IsNotNull(person2);
                Assert.ReferenceEquals(person, person2);
            }

            {
                Person person = null;
                NullableEntry<Person> obj = person;
                Person person2 = obj;
                Assert.IsNull(person2);

            }

            {
                Int32 id = 2;
                NullableEntry<Int32> obj = id;
                Int32 id2 = id;
                Assert.AreEqual(id, id2);
            }

            {
                Int32 id = 0;
                NullableEntry<Int32> obj = id;
                Int32 id2 = id;
                Assert.AreEqual(id, id2);
            }
        }

        //[TestMethod]
        //public void NullCacheError {
        //    var key = "key-null";
        //    Person person = null;
        //    IHttpRuntimeCacheProvider cacheProvider = CacheProviderFactory.GetHttpRuntimeCache(false);
        //    try {
        //        cacheProvider.GetOrCreate<Person>(key, () => person); //error
        //        Assert.Fail();
        //    }
        //    catch (Exception ex) {
        //        Assert.IsTrue(ex is InvalidOperationException);
        //    }

        //    Person person2;
        //    var exist = cacheProvider.TryGet(key, out person2);
        //    Assert.IsFalse(exist);
        //    Assert.AreEqual(person2, null);
        //}

        [TestMethod]
        public void NullableCache() {
            var key = "key-nullable";
            Person person = null;
            IHttpRuntimeCacheProvider cacheProvider = CacheProviderFactory.GetHttpRuntimeCache();
            cacheProvider.GetOrCreate<Person>(key, () => person);
            Person person2;
            var exist = cacheProvider.TryGet(key, out person2);
            Assert.IsTrue(exist);
            Assert.AreEqual(person2, null);
        }

        class Person {
            public Int32 Id { get; set; }
            public String Name { get; set; }
        }
    }
}
