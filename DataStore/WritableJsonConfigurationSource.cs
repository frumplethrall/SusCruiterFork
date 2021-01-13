using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Suscruiter.DataStore {
    public class WritableJsonConfigurationSource : JsonConfigurationSource {
        public override IConfigurationProvider Build(IConfigurationBuilder builder) {
            this.EnsureDefaults(builder);
            return new WritableJsonConfigurationProvider(this);
        }
    }
}
