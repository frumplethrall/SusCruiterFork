using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Suscruiter.TypeReaders;

namespace Suscruiter.Services {
    public class CommandHandlerService {

        private readonly DiscordSocketClient _discord;
        private readonly CommandService      _commands;
        private readonly IConfigurationRoot  _config;
        private readonly IServiceProvider    _provider;

        public CommandHandlerService(
            DiscordSocketClient discord,
            CommandService      commands,
            IConfigurationRoot  config,
            IServiceProvider    provider) {
            _discord  = discord;
            _commands = commands;
            _config   = config;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;

            _commands.AddTypeReader<Emote>(new EmoteTypeReader());
        }

        private async Task OnMessageReceivedAsync(SocketMessage s) {
            var msg = s as SocketUserMessage;
            if (msg           == null) return;
            if (msg.Author.Id == _discord.CurrentUser.Id) return;

            var context = new SocketCommandContext(_discord, msg);

            int argPos = 0;
            if (msg.HasStringPrefix(_config["command_prefix"], ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)) {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess) {
                    await context.Channel.SendMessageAsync(result.ToString());
                }
            }
        }

    }
}
