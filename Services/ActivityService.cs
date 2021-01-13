using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Suscruiter.Services {
    public class ActivityService {

        private readonly DiscordSocketClient _discord;
        private readonly IConfigurationRoot  _config;

        public ActivityService(DiscordSocketClient discord, IConfigurationRoot config) {
            _discord = discord;
            _config  = config;

            UpdateStatus();
        }

        public async void UpdateStatus() {
            await _discord.SetGameAsync(_config["activity_game"]);
        }

    }
}
