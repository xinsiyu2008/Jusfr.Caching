using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Memcached {
    internal class NewtonsoftJsonUtil {
        public static T EnsureObjectType<T>(Object obj) {
            if (obj is T) {
                return (T)obj;
            }
            else if (obj is JObject) {
                return ((JObject)obj).ToObject<T>();
            }
            else {
                //return (T)Convert.ChangeType(obj, typeof(T));  // Guid 类型将失败
                return JToken.FromObject(obj).ToObject<T>();
            }
        }
    }
}
