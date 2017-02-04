using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReptileDemo.Infrastructure
{
    public class JsonHelper
    {
        public static void WriteJsonFile(List<string> lists)
        {
            var fileStream = new FileStream(Path.Combine("JsonFile", "NewBlog.json"), FileMode.OpenOrCreate);
            var streamWriter = new StreamWriter(fileStream);
            var jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new JavaScriptDateTimeConverter() }
            };
            JsonWriter jsonWriter = new JsonTextWriter(streamWriter);
            jsonSerializer.Serialize(jsonWriter, lists);
            streamWriter.Flush();
            jsonWriter.Flush();
        }
    }
}
