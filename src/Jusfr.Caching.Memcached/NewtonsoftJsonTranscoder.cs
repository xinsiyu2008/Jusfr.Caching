using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Memcached {
    public class NewtonsoftJsonTranscoder : DefaultTranscoder {
        private static readonly Byte[] _donetBytes;

        static NewtonsoftJsonTranscoder() {
            //_donetBytes = new[] { 0, 0, 1, 1, 0, 0, 0, 0, 255 }
            //_donetBytes = new[] { 0, 1, 0, 0, 0, 255, 255, 255, 255 }
            //.Select(x => Convert.ToByte(x)).ToArray();
            _donetBytes = new[] { (Byte)0, (Byte)1, (Byte)255 };
        }

        private static Object JsonDeserialize(Byte[] buffer) {
            JsonSerializer serializer = JsonSerializer.CreateDefault();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            using (TextReader textReader = new StreamReader(memoryStream))
            using (JsonReader jsonReader = new JsonTextReader(textReader)) {
                return serializer.Deserialize(jsonReader);
            }
        }

        protected override object DeserializeObject(ArraySegment<byte> value) {
            Boolean isJson = false;
            if (value.Array[0] == 123 && value.Array[value.Array.Length - 1] == 125) {
                isJson = true;
            }
            if (!isJson) {
                var isOrignalObjectByte = value.Array.Take(10).Distinct().All(_donetBytes.Contains);
                isJson = !isOrignalObjectByte;
            }

            if (isJson) {
                return JsonDeserialize(value.Array);
            }
            else {
                try {
                    return base.DeserializeObject(value);
                }
                catch (SerializationException) {
                    // Log or something
                    return JsonDeserialize(value.Array);
                }
            }
        }

        protected override ArraySegment<byte> SerializeObject(object value) {
            JsonSerializer serializer = JsonSerializer.CreateDefault();
            using (MemoryStream memoryStream = new MemoryStream())
            using (TextWriter textWriter = new StreamWriter(memoryStream))
            using (JsonWriter jsonWriter = new JsonTextWriter(textWriter)) {
                serializer.Serialize(jsonWriter, value);
                jsonWriter.Flush();
                memoryStream.Seek(0L, SeekOrigin.Begin);
                return new ArraySegment<byte>(memoryStream.ToArray());
            }
        }
    }
}
