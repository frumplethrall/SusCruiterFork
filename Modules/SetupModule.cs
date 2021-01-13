using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Suscruiter.Services;

namespace Suscruiter.Modules {
    [Name("Moderator")]
    [RequireContext(ContextType.Guild)]
    public class SetupModule : ModuleBase<SocketCommandContext> {

        private readonly IConfigurationRoot  _config;
        private readonly ReactHandlerService _reactHandler;
        private readonly ActivityService     _activity;
        private readonly NagService          _nag;

        public SetupModule(IConfigurationRoot config, ReactHandlerService reactHandler, ActivityService activity, NagService nag) {
            _config       = config;
            _reactHandler = reactHandler;
            _activity     = activity;
            _nag          = nag;
        }

        private string FormatAsCommand(string command) {
            return $"{_config["command_prefix"]}{command}";
        }

        [Command("SetupHelp")]
        public async Task SetupHelp() {
            if (Context.Channel.Id.ToString() != _config["bot_channel"]) return;

            await ReplyAsync("**Setup:**\r\n\r\n" +
                             $":one:  {FormatAsCommand("SetupEmotes *<ack_emote> <featurerequest_emote> <bug_emote>*")}\r\n" +
                             $":two:  {FormatAsCommand("SetApprover *<user>*")}\r\n" +
                             $":three:  {FormatAsCommand("SetGame *\"<name of game>\"*")}");
        }

        private string FormatAsEmote(Emote emote) {
            return $"<:{emote.Name}:{emote.Id}>";
        }

        [Command("Play")]
        public async Task Play() {
            if (Context.Channel.Id.ToString() != _config["bot_channel"]) return;

            await _nag.SendPlayNag();
        }

        [Command("Recruit")]
        public async Task Recruit() {
            if (Context.Channel.Id.ToString() != _config["bot_channel"]) return;

            await _nag.SendStartRecruit();
        }

        [Command("Refresh")]
        public async Task Refresh() {
            if (Context.Channel.Id.ToString() != _config["bot_channel"]) return;

            await _nag.RefreshNag();
        }

        [Command("SetupEmotes")]
        public async Task SetupEmotes(Emote ackYes, Emote ackNo) {
            if (Context.Channel.Id.ToString() != _config["bot_channel"]) return;

            if (ackYes == null || ackNo == null) {
                await ReplyAsync("You didn't provide the correct emote params. :confused:");
                return;
            }

            _config[NagService.EMOTE_ACKYES] = FormatAsEmote(ackYes);
            _config[NagService.EMOTE_ACKNO]  = FormatAsEmote(ackNo);

            _reactHandler.UpdateEmotes();

            await ReplyAsync("Got it! :thumbsup:\r\n\r\n" +
                             $"{FormatAsEmote(ackYes)} => Ack Yes\r\n" +
                             $"{FormatAsEmote(ackNo)} => Ack No\r\n");
        }

        [Command("SetGame")]
        public async Task SetGame(string game) {
            if (Context.Channel.Id.ToString() != _config["bot_channel"]) return;

            _config["activity_game"] = game;

            _activity.UpdateStatus();

            await ReplyAsync(":metal:");
        }

    }
}
