using Jusfr.Caching.Memcached;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Enyim.Caching.Memcached;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class NewtonsoftJsonTranscoderTest {
        [TestMethod]
        public void DefaultSerialize() {
            var person = new Person {
                Id = 2,
                Name = "Rattz",
                Address = new Address {
                    Line1 = "Haidin Shuzhoujie",
                    Line2 = "Beijing China"
                }
            };

            var serializeObject = typeof(DefaultTranscoder).GetMethod("SerializeObject", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(serializeObject);
            var buffer = (ArraySegment<Byte>)serializeObject.Invoke(new DefaultTranscoder(), new Object[] { person });

            var deserializeObject = typeof(NewtonsoftJsonTranscoder).GetMethod("DeserializeObject", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(serializeObject);
            var binary = deserializeObject.Invoke(new NewtonsoftJsonTranscoder(), new Object[] { buffer });
            Assert.IsTrue(binary is Person);
        }

        [TestMethod]
        public void NewtonsoftSerialize() {
            var person = new Person {
                Id = 2,
                Name = "Rattz",
                Address = new Address {
                    Line1 = "Haidin Shuzhoujie",
                    Line2 = "Beijing China"
                }
            };

            var transcoder = new NewtonsoftJsonTranscoder();
            var transcoderType = typeof(NewtonsoftJsonTranscoder);
            var serializeObject = transcoderType.GetMethod("SerializeObject", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(serializeObject);
            var buffer = (ArraySegment<Byte>)serializeObject.Invoke(transcoder, new Object[] { person });

            var deserializeObject = transcoderType.GetMethod("DeserializeObject", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(serializeObject);
            var json = deserializeObject.Invoke(transcoder, new Object[] { buffer });
            Assert.IsTrue(json is JObject);
        }
    }
}
