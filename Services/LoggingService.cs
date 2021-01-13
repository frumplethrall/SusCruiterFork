using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Suscruiter.Services {

    public class LoggingService {

        private readonly DiscordSocketClient _discord;
        private readonly CommandService      _commands;
        private readonly ILoggerFactory      _loggerFactory;
        private readonly ILogger             _discordLogger;
        private readonly ILogger             _commandsLogger;

        public LoggingService(DiscordSocketClient discord, CommandService commands) {
            _discord  = discord;
            _commands = commands;

            _loggerFactory  = BuildLogger();
            _discordLogger  = _loggerFactory.CreateLogger("discord");
            _commandsLogger = _loggerFactory.CreateLogger("commands");

            _discord.Log  += LogDiscordAsync;
            _commands.Log += LogCommandAsync;
        }

        private ILoggerFactory BuildLogger() {
            return LoggerFactory.Create(builder => builder.AddConsole());
        }

        private Task LogDiscordAsync(LogMessage message) {
            _discordLogger.Log(
                               LogLevelFromSeverity(message.Severity),
                               0,
                               message,
                               message.Exception,
                               (_1, _2) => message.ToString(prependTimestamp: false)
                              );

            return Task.CompletedTask;
        }

        private Task LogCommandAsync(LogMessage message) {
            if (message.Exception is CommandException command) {
                var _ = command.Context.Channel.SendMessageAsync($"Error: {command.Message}");
            }

            _commandsLogger.Log(LogLevelFromSeverity(message.Severity),
                                0,
                                message,
                                message.Exception,
                                (_1, _2) => message.ToString(prependTimestamp: false));

            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity) {
            return (LogLevel) (Math.Abs((int) severity - 5));
        }

    }

}
