using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jusfr.Caching.Redis;

namespace Jusfr.Caching.Tests {
    [TestClass]
    public class RedisTest {

        [TestMethod]
        public void RedisField_Equal_Test() {
            RedisField f1 = new RedisField();
            RedisField f2 = new RedisField();
            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1.Equals(f2));

            String str = Guid.NewGuid().ToString();
            f1 = str;
            f2 = str;
            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1.Equals(f2));

            Object f3 = f1;
            Assert.IsTrue(f3.Equals(f2));
            Assert.IsTrue(f2.Equals(f3));

            f2 = Guid.NewGuid().ToString();
            Assert.IsTrue(f1 != f2);
            Assert.IsTrue(!f1.Equals(f2));

            Assert.IsTrue(!f3.Equals(f2));
            Assert.IsTrue(!f2.Equals(f3));

        }
    }
}
