using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Maya.TypeReaders
{
    public class NullableTypeReader<T> : TypeReader
        where T : struct
    {
        public delegate bool TryParse<W>(string str, out T value);
        public TryParse<T> tryParseFunc;
        public NullableTypeReader(TryParse<T> parseFunc)
        {
            tryParseFunc = parseFunc;
        }

        public override Task<TypeReaderResult> Read(ICommandContext context, string input)
        {
            T value;
            if (tryParseFunc(input, out value))
                return Task.FromResult(TypeReaderResult.FromSuccess(new Nullable<T>(value)));
            return Task.FromResult(TypeReaderResult.FromSuccess(new Nullable<T>()));
        }
    }
}
