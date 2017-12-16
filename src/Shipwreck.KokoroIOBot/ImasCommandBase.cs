using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using KokoroIO;
using Newtonsoft.Json;

namespace Shipwreck.KokoroIOBot
{
    internal abstract class ImasCommandBase : BotCommandBase
    {
        protected ImasCommandBase(params string[] names)
            : base(names)
        {
        }

        protected override async void HandleCore(Message message, IReadOnlyList<string> args)
        {
            using (var hc = new HttpClient())
            {
                var sb = new StringBuilder("http://shipwreck.jp/imascg/Image/Search?count=128");

                AppendQueryString(message, args, sb);

                var res = await hc.GetAsync(sb.ToString()).ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var iir = JsonConvert.DeserializeObject<IdolImageResult>(json);

                    if (iir.Items?.Count > 0)
                    {
                        var img = iir.Items[new Random().Next(iir.Items.Count)];

                        var newUrl = await ImageSanitizer.GetSafeUrlAsync(img.ImageUrl).ConfigureAwait(false);

                        using (var bc = new BotClient())
                        {
                            await bc.PostMessageAsync(message.Channel.Id, $"[]({newUrl})", isNsfw: false).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        using (var bc = new BotClient())
                        {
                            await bc.PostMessageAsync(message.Channel.Id, "Not Found", isNsfw: false).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        protected abstract void AppendQueryString(Message message, IReadOnlyList<string> args, StringBuilder sb);

        protected static bool TryAppendRarity(StringBuilder sb, string a)
        {
            var m = Regex.Match(a, "^(N|S?R)\\+?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return false;
            }
            switch (char.ToLower(m.Value[0]))
            {
                case 'n':
                    sb.Append("&rarity=Normal");
                    break;

                case 'r':
                    sb.Append("&rarity=Rare");
                    break;

                case 's':
                    sb.Append("&rarity=SRare");
                    break;
            }
            sb.Append("&isPlus=").Append(m.Value.Last() == '+');

            return true;
        }
    }
}