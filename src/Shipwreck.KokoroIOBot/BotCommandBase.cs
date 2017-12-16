using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KokoroIO;

namespace Shipwreck.KokoroIOBot
{
    internal abstract class BotCommandBase : IBotCommand
    {
        private readonly Regex _NamesPattern;

        protected BotCommandBase(params string[] names)
        {
            _NamesPattern = new Regex("^(" + string.Join("|", names.Select(Regex.Escape)) + ")$", RegexOptions.IgnoreCase);
        }

        public bool TryHandle(Message message, IReadOnlyList<string> args)
        {
            if (!(args.Count > 0)
                || !_NamesPattern.IsMatch(args[0]))
            {
                return false;
            }

            HandleCore(message, args.Skip(1).ToList());

            return true;
        }

        protected abstract void HandleCore(Message message, IReadOnlyList<string> args);
    }
}