using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;

namespace Suscruiter.Services {
    public class NagService {

        public const string EMOTE_ACKYES = "emote_ackyes";
        public const string EMOTE_ACKNO  = "emote_ackno";

        public const string SUSFAM_MENTION = "<@&773566538468950038>";

        public const int LAST_LEG = 5;

        private readonly IServiceProvider    _provider;
        private readonly DiscordSocketClient _discord;
        private readonly IConfigurationRoot  _config;

        private Timer _timer;

        private Emote _ackYes;
        private Emote _ackNo;

        private IUserMessage _lastNag;

        private List<IUser> _ackUsers;

        private List<IUser> _ackLame;

        public IUserMessage LastNagMessage => _lastNag;

        public NagService(IServiceProvider provider, DiscordSocketClient discord, IConfigurationRoot config) {
            _provider = provider;
            _config   = config;
            _discord  = discord;

            UpdateEmotes();
            ConfigureTimer();
        }

        private void UpdateEmotes() {
            if (_config[EMOTE_ACKYES] != null && Emote.TryParse(_config[EMOTE_ACKYES], out var ackYes)) _ackYes = ackYes;
            if (_config[EMOTE_ACKNO]  != null && Emote.TryParse(_config[EMOTE_ACKNO],  out var ackNo)) _ackNo   = ackNo;
        }

        private void ConfigureTimer() {
            _timer         =  new Timer(24 * 60 * 60 * 1000);
            // _timer.Elapsed += HandleTick;
        }

        public async Task AddAckUser(IUser user) {
            bool changed = _ackLame.RemoveAll(u => u.Id == user.Id) > 0;
            if (_ackUsers.All(u => u.Id != user.Id)) {
                _ackUsers.Add(user);
                changed = true;
            }

            if (changed) await UpdateNag();
        }

        public async Task RemoveAckUser(IUser user) {
            bool changed = _ackUsers.RemoveAll(u => u.Id == user.Id) > 0;
            if (_ackLame.All(u => u.Id != user.Id)) {
                _ackLame.Add(user);
                changed = true;
            }

            if (changed) await UpdateNag();
        }

        public async Task SendPlayNag() {
            StringBuilder userList = new StringBuilder();
            userList.AppendLine("Yo");
            userList.AppendLine();

            foreach (var user in _ackUsers) {
                userList.AppendLine(user.Mention);
            }

            userList.AppendLine();
            userList.AppendLine("📢 Join voice!  We playing! 📢");

            if (_ackUsers.Count < 10) {
                userList.AppendLine();
                userList.AppendLine("(@here consider joining us!)");
            }

            var channel = _discord.GetChannel(ulong.Parse(_config["sus_channel"])) as IMessageChannel;
            _lastNag = await channel.SendMessageAsync(userList.ToString());
        }

        public async Task SendStartRecruit() {
            _ackUsers = new List<IUser>();
            _ackLame  = new List<IUser>();

            var channel = _discord.GetChannel(ulong.Parse(_config["sus_channel"])) as IMessageChannel;
            _lastNag = await channel.SendMessageAsync($"If you're available to play tonight, emote below.  If you would like to be included on all sus related messaging (like pings when we're gonna play), ask a queue captain to add you to the {SUSFAM_MENTION} role.");

            await _lastNag.AddReactionAsync(_ackYes);
            await _lastNag.AddReactionAsync(_ackNo);
        }

        public async Task RefreshNag() {
            await _lastNag.DeleteAsync();

            var channel = _discord.GetChannel(ulong.Parse(_config["sus_channel"])) as IMessageChannel;
            _lastNag = await channel.SendMessageAsync($"Refreshing...");

            await _lastNag.AddReactionAsync(_ackYes);
            await _lastNag.AddReactionAsync(_ackNo);

            await UpdateNag();
        }

        private async Task UpdateNag() {
            if (_lastNag != null) {
                StringBuilder userList = new StringBuilder();

                userList.AppendLine($"If you're available to play tonight, emote below.  If you would like to be included on all sus related messaging (like pings when we're gonna play), ask a queue captain to add you to the {SUSFAM_MENTION} role.");
                userList.AppendLine();

                userList.AppendLine("RSVP:");
                foreach (var user in _ackUsers) {
                    userList.AppendLine(user.Mention);
                }

                userList.AppendLine();

                if (_ackLame.Count > 0) {
                    userList.AppendLine("Lame (not available):");

                    foreach (var user in _ackLame) {
                        userList.AppendLine(user.Username);
                    }

                    userList.AppendLine();
                }

                if (_ackUsers.Count > LAST_LEG) {
                    userList.AppendLine();
                    userList.AppendLine($"@here we're at {_ackUsers.Count} users.  We just need a bit more for a full lobby!");
                }

                userList.AppendLine();

                userList.AppendLine($"<:{_ackYes.Name}:768959238688079902> = YES");
                userList.AppendLine($"<:{_ackNo.Name}:768959506411028550> = NO");

                await _lastNag.ModifyAsync((m) => {
                                               m.Content = userList.ToString();
                                           });
            }
        }

        private bool _alreadyReady = false;

        public Task StartAsync() {
            _discord.Ready += async () => {
                if (_alreadyReady) return;

                _alreadyReady = true;

                _timer.Start();

                await Task.Delay(1);

                //await SendStartRecruit();
            };

            return Task.CompletedTask;
        }

        private async void HandleTick(object sender, EventArgs e) {
            await SendStartRecruit();
        }

    }
}
