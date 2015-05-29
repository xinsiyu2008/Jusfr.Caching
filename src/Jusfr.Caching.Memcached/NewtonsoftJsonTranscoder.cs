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
            _donetBytes = new[] { (Byte)0, (Byte)1, (Byte)255 };
        }

        private Object JsonDeserialize(Byte[] buffer) {
            JsonSerializer serializer = JsonSerializer.CreateDefault();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            using (TextReader textReader = new StreamReader(memoryStream))
            using (JsonReader jsonReader = new JsonTextReader(textReader)) {
                return serializer.Deserialize(jsonReader);
            }
        }

        protected override object DeserializeObject(ArraySegment<byte> value) {
            Byte[] buffer;
            if (value.Offset != 0) {
                buffer = new Byte[value.Count];
                Array.Copy(value.Array, value.Offset, buffer, 0, value.Count);
            }
            else {
                buffer = value.Array;
            }

            Boolean isJson = false;
            if (buffer[0] == 123 && buffer[buffer.Length - 1] == 125) {
                isJson = true;
            }
            if (!isJson) {
                var isOrignalObjectByte = buffer.Take(10).Distinct().All(_donetBytes.Contains);
                isJson = !isOrignalObjectByte;
            }

            if (isJson) {
                return JsonDeserialize(buffer);
            }
            else {
                try {
                    return base.DeserializeObject(value);
                }
                catch (SerializationException) {
                    // Log or something
                    return JsonDeserialize(buffer);
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
                return new ArraySegment<byte>(memoryStream.ToArray(), 0, (Int32)memoryStream.Length);
            }
        }
    }
}
