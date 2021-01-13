using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Suscruiter.DataStore;
using Suscruiter.Services;

namespace Suscruiter {
    public class Suscruiter {

        private const string CONFIG_FILE = "config.json";

        public IConfigurationRoot Configuration { get; }

        public Suscruiter(string[] args) {
            string currDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var builder = new ConfigurationBuilder().SetBasePath(currDir);

            if (!File.Exists(Path.Combine(currDir, CONFIG_FILE))) {
                File.WriteAllText(CONFIG_FILE, "{}");
            }

            Configuration = builder.Add((Action<WritableJsonConfigurationSource>)(s => {
                 s.FileProvider   = null;
                 s.Path           = CONFIG_FILE;
                 s.Optional       = false;
                 s.ReloadOnChange = true;
                 s.ResolveFileProvider();
             })).Build();
        }

        public static async Task RunAsync(string[] args) {
            var startup = new Suscruiter(args);
            await startup.RunAsync();
        }

        public async Task RunAsync() {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandlerService>();

            await provider.GetRequiredService<StartupService>().StartAsync();
            await provider.GetRequiredService<NagService>().StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services) {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {
                         LogLevel         = LogSeverity.Verbose,
                         MessageCacheSize = 1000
                     }))
                    .AddSingleton(new CommandService(new CommandServiceConfig {
                         LogLevel       = LogSeverity.Verbose,
                         DefaultRunMode = RunMode.Async,
                     }))
                    .AddSingleton<CommandHandlerService>()
                    .AddSingleton<ReactHandlerService>()
                    .AddSingleton<ActivityService>()
                    .AddSingleton<StartupService>()
                    .AddSingleton<LoggingService>()
                    .AddSingleton<NagService>()
                    .AddSingleton(Configuration);
        }

    }
}
