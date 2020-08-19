using System;
using System.Collections.Generic;
using System.Text;
#if NETSTANDARD
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif

namespace PieterP.ScoreSheet.Model.Database {
    public class DataSerializer {
#if NETSTANDARD
        public static string Serialize<T>(T value) { 
            return Encoding.UTF8.GetString(SerializeToBytes(value));
        }
        public static byte[] SerializeToBytes<T>(T value) {
#if DEBUG
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
#else
            JsonSerializerOptions options = null;
#endif
            return JsonSerializer.SerializeToUtf8Bytes<T>(value, options);
        }
        public static T Deserialize<T>(string value) {
            var options = new JsonSerializerOptions() {
                AllowTrailingCommas = true
            };
            return JsonSerializer.Deserialize<T>(value, options);
        }
#else
        public static string Serialize<T>(T value) {
#if DEBUG
            var formatting = Formatting.Indented;
#else
            var formatting = Formatting.None;
#endif
            return JsonConvert.SerializeObject(value, formatting);
        }
        public static byte[] SerializeToBytes<T>(T value) {
            return Encoding.UTF8.GetBytes(Serialize(value));
        }
        public static T Deserialize<T>(string value) {
            return JsonConvert.DeserializeObject<T>(value);
        }
#endif
        }
}
