using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using KokoroIO;
using Newtonsoft.Json;

namespace Shipwreck.KokoroIOBot
{
    internal sealed class SachikoCommand : ImasCommandBase
    {
        public SachikoCommand()
            : base("sachiko", "satiko", "koshimizu", "kosimizu")
        {
        }

        protected override void AppendQueryString(Message message, IReadOnlyList<string> args, StringBuilder sb)
        {
            foreach (var a in args)
            {
                TryAppendRarity(sb, a);
            }

            sb.Append("&headline=");
            sb.Append(Uri.EscapeDataString("輿水幸子"));
        }
    }

}