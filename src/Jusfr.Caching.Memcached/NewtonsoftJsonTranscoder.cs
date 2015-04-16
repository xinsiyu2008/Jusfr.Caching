using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Memcached {
    public class NewtonsoftJsonTranscoder : DefaultTranscoder {
        private static readonly Byte[] _donetBytes;

        static NewtonsoftJsonTranscoder() {
            _donetBytes = new[] { 0, 1, 0, 0, 0, 255, 255, 255, 255 }
                .Select(x => Convert.ToByte(x)).ToArray();
        }

        protected override object DeserializeObject(ArraySegment<byte> value) {
            if (value.Array.Length >= _donetBytes.Length) {
                var equal = _donetBytes.Select((b, i) => value.Array[i] == b).Any(x => !x);
                if (!equal) {
                    return base.DeserializeObject(value);
                }
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (MemoryStream memoryStream = new MemoryStream(value.Array, value.Offset, value.Count))
            using (TextReader textReader = new StreamReader(memoryStream))
            using (JsonReader jsonReader = new JsonTextReader(textReader)) {
                return serializer.Deserialize(jsonReader);
            }
        }

        protected override ArraySegment<byte> SerializeObject(object value) {
            //return base.SerializeObject(value);

            JsonSerializer serializer = new JsonSerializer();
            using (MemoryStream memoryStream = new MemoryStream())
            using (TextWriter textWriter = new StreamWriter(memoryStream))
            using (JsonWriter jsonWriter = new JsonTextWriter(textWriter)) {
                serializer.Serialize(jsonWriter, value);
                jsonWriter.Flush();
                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }
    }
}
