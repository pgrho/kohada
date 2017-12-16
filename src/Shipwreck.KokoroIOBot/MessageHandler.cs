using System;
using System.Linq;
using KokoroIO;

namespace Shipwreck.KokoroIOBot
{
    internal static class MessageHandler
    {
        private static readonly IBotCommand[] COMMANDS = {
            new SachikoCommand(),
            new ImasCommand(),
            new PriparaCommand()
        };

        public static void Handle(Message message)
        {
            if (string.IsNullOrEmpty(message.RawContent))
            {
                return;
            }

            var args = message.RawContent.Split(new[] { ' ', '\t', '　' }, StringSplitOptions.RemoveEmptyEntries);

            if (!"/kohada".Equals(args?.FirstOrDefault(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var na = args.Skip(1).ToList();

            foreach (var c in COMMANDS)
            {
                if (c.TryHandle(message, na))
                {
                    return;
                }
            }
        }
    }
}