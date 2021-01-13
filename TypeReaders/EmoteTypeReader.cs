using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Suscruiter.TypeReaders {
    public class EmoteTypeReader : TypeReader {

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            if (Emote.TryParse(input, out var result)) {
                return Task.FromResult(TypeReaderResult.FromSuccess(result));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as an emote"));
        }

    }
}
