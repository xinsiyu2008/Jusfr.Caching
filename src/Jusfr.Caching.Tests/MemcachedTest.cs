using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using System.Reflection;
using Jusfr.Caching.Memcached;
using System.Collections.Generic;
using Newtonsoft.Json;

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

                value = new Person {
                    Id = 2,
                    Name = "Rattz",
                    Address = new Address {
                        Line1 = "Haidin Shuzhoujie",
                        Line2 = "Beijing China"
                    }
                };
                client.Store(StoreMode.Set, key, value);
                exist = client.TryGet(key, out value);
                Assert.IsTrue(exist);
                Assert.IsNotNull(value);
            }
        }

        [TestMethod]
        public void NullCache() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                String key = Guid.NewGuid().ToString("n");
                Object value = null;
                client.Store(StoreMode.Set, key, value);
                var exist = client.TryGet(key, out value);
                Assert.IsTrue(exist);
                Assert.IsNull(value);
            }
        }

        [TestMethod]
        public void Compatibility() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                var array = new List<Object>();
                array.Add(new Object());
                array.Add(Guid.NewGuid().GetHashCode());
                array.Add(1.1m);
                array.Add(Guid.NewGuid());
                array.Add(Guid.NewGuid().GetHashCode().ToString());
                array.Add(new[] { 1, 2 });
                array.Add(new[] { Guid.NewGuid().ToString() });
                array.Add(new Person {
                    Id = 2,
                    Name = "Rattz",
                    Address = new Address {
                        Line1 = "Haidin Shuzhoujie",
                        Line2 = "Beijing China"
                    }
                });

                var transcoderProp = typeof(MemcachedClient).GetField("transcoder", BindingFlags.Instance | BindingFlags.NonPublic);
                var region = Guid.NewGuid().ToString("n");
                region = "Compatibility";
                for (var i = 0; i < array.Count; i++) {
                    if ((Guid.NewGuid().GetHashCode() % 2) == 1) {
                        transcoderProp.SetValue(client, new NewtonsoftJsonTranscoder());
                    }
                    else {
                        transcoderProp.SetValue(client, new DefaultTranscoder());
                    }

                    var key = region + "_" + i;
                    client.Store(StoreMode.Set, key, array[i]);

                    transcoderProp.SetValue(client, new NewtonsoftJsonTranscoder());
                    Object cache;
                    Assert.IsTrue(client.TryGet(key, out cache));
                    //Assert.AreEqual(array[i].GetType(), cache.GetType());

                    Assert.AreEqual(JsonConvert.SerializeObject(array[i]),
                        JsonConvert.SerializeObject(cache));
                }
            }
        }

        [TestMethod]
        public void Compatibility2() {
            var cache = new MemcachedCacheProvider("Test");
            var person = new Person {
                Id = 2,
                Name = "Rattz",
                Address = new Address {
                    Line1 = "Haidin Shuzhoujie",
                    Line2 = "Beijing China"
                }
            };

            var key = "Person";
            cache.Overwrite(key, person, TimeSpan.FromHours(1D));
            Person personOut;
            var exist = cache.TryGet<Person>(key, out personOut);
            Assert.IsTrue(exist);
            Assert.IsNotNull(personOut);
        }

    }

    [Serializable]
    public class Person {
        public int Id { get; set; }
        public String Name { get; set; }
        public Address Address { get; set; }
    }

    [Serializable]
    public class Address {
        public String Line1 { get; set; }
        public String Line2 { get; set; }
    }
}

