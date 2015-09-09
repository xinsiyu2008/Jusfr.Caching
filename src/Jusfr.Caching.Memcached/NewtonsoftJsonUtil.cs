using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Jusfr.Caching.Memcached {
    internal class NewtonsoftJsonUtil {
        private static readonly JsonSerializerSettings _jsonSettings;

        static NewtonsoftJsonUtil() {
            _jsonSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
#if DEBUG
                Formatting = Formatting.Indented,
#else
                    Formatting = Formatting.None,
#endif
            };
        }

        public static String Stringify(Object value) {
            return JsonConvert.SerializeObject(value, _jsonSettings);
        }

        public static String Stringify(Object value, Boolean formatting = false) {
            return JsonConvert.SerializeObject(value,
                formatting ? Formatting.Indented : Formatting.None, _jsonSettings);
        }

        public static Object Parse(String value) {
            return JsonConvert.DeserializeObject(value, _jsonSettings);
        }

        public static T Parse<T>(String value) {
            return JsonConvert.DeserializeObject<T>(value, _jsonSettings);
        }

        public static Object Parse(String value, Type type) {
            return JsonConvert.DeserializeObject(value, type, _jsonSettings);
        }


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
