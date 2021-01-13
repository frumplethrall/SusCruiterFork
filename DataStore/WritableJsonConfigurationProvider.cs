using System.IO;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;

namespace Suscruiter.DataStore {
    public class WritableJsonConfigurationProvider : JsonConfigurationProvider {
        public WritableJsonConfigurationProvider(JsonConfigurationSource source) : base(source) { /* NOOP */ }

        public override void Set(string key, string value) {
            base.Set(key, value);

            var     fileFullPath = base.Source.FileProvider.GetFileInfo(base.Source.Path).PhysicalPath;
            string  json         = File.ReadAllText(fileFullPath);
            dynamic jsonObj      = JsonConvert.DeserializeObject(json);
            jsonObj[key] = value;
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(fileFullPath, output);
        }
    }
}
