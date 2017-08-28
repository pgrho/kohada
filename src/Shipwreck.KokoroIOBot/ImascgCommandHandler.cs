using System;
using System.Net.Http;
using Newtonsoft.Json;
using Shipwreck.KokoroIO;

namespace Shipwreck.KokoroIOBot
{
    internal static class ImascgCommandHandler
    {
        public static async void HandleAsync(Message m)
        {
            if (m.RawContent != "!sachiko")
            {
                return;
            }

            using (var hc = new HttpClient())
            {
                var res = await hc.GetAsync($"http://shipwreck.jp/imascg/Image/Search?count=128&headline=" + Uri.EscapeDataString("輿水幸子")).ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var iir = JsonConvert.DeserializeObject<IdolImageResult>(json);

                    var img = iir.Items[new Random().Next(iir.Items.Count)];

                    using (var bc = new BotClient())
                    {
                        await bc.PostMessageAsync(m.Room.Id, img.ImageUrl, false).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}