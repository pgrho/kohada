using System.Collections.Generic;
using KokoroIO;

namespace Shipwreck.KokoroIOBot
{
    internal interface IBotCommand
    {
        bool TryHandle(Message message, IReadOnlyList<string> args);
    }
}