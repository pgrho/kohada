using System;
using System.Collections.Generic;
using System.Text;
using KokoroIO;

namespace Shipwreck.KokoroIOBot
{
    internal sealed class ImasCommand : ImasCommandBase
    {
        public ImasCommand()
            : base("imascg", "imas")
        {
        }

        protected override void AppendQueryString(Message message, IReadOnlyList<string> args, StringBuilder sb)
        {
            foreach (var a in args)
            {
                if (!TryAppendRarity(sb, a))
                {
                    sb.Append("&headline=");
                    sb.Append(Uri.EscapeDataString(a));
                }
            }
        }
    }
}