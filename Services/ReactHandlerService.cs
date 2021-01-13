using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Suscruiter.Services {
    public class ReactHandlerService {

        private readonly DiscordSocketClient _discord;
        private readonly IConfigurationRoot  _config;
        private readonly NagService          _nag;

        public ReactHandlerService(DiscordSocketClient discord, IConfigurationRoot config, NagService nag) {
            _discord = discord;
            _config  = config;
            _nag     = nag;

            _discord.ReactionAdded += OnReactionAdded;

            UpdateEmotes();
        }

        private Emote _ackYes;
        private Emote _ackNo;

        public void UpdateEmotes() {
            if (_config[NagService.EMOTE_ACKYES] != null && Emote.TryParse(_config[NagService.EMOTE_ACKYES], out var ackYes)) _ackYes = ackYes;
            if (_config[NagService.EMOTE_ACKNO]  != null && Emote.TryParse(_config[NagService.EMOTE_ACKNO],  out var ackNo)) _ackNo   = ackNo;
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
            async Task RevokeReaction(IUserMessage message, IUser user) {
                await message.RemoveReactionAsync(arg3.Emote, user);
            }

            if (Equals(arg3.Emote, _ackYes)) {
                var associatedMessage = await arg1.GetOrDownloadAsync();

                if (associatedMessage == null || associatedMessage.Id != _nag.LastNagMessage.Id) return;

                foreach (var user in await (await arg1.GetOrDownloadAsync()).GetReactionUsersAsync(_ackYes, 20).FlattenAsync()) {
                    if (user.Id == _discord.CurrentUser.Id) continue; // Ignore us

                    await RevokeReaction(associatedMessage, user);
                    await _nag.AddAckUser(user);
                }
            } else if (Equals(arg3.Emote, _ackNo)) {
                var associatedMessage = await arg1.GetOrDownloadAsync();

                if (associatedMessage == null || associatedMessage.Id != _nag.LastNagMessage.Id) return;

                foreach (var user in await (await arg1.GetOrDownloadAsync()).GetReactionUsersAsync(_ackNo, 20).FlattenAsync()) {
                    if (user.Id == _discord.CurrentUser.Id) continue; // Ignore us

                    await RevokeReaction(associatedMessage, user);
                    await _nag.RemoveAckUser(user);
                }
            }
        }

    }
}
