using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KokoroIO;
using Newtonsoft.Json;

namespace Shipwreck.KokoroIOBot
{
    internal sealed class PriparaCommand : BotCommandBase
    {
        public static string AnimeDBAccessToken { get; set; }

        private string[] _ErrorImages;

        public PriparaCommand()
            : base("pripara", "puripara", "puri", "pp", "p")
        {
        }

        protected override async void HandleCore(Message message, IReadOnlyList<string> args)
        {
            using (var hc = new HttpClient())
            {
                var urls = await GetUrlAsync(args, hc).ConfigureAwait(false);
                if (urls.Any())
                {
                    using (var bc = new BotClient())
                    {
                        await bc.PostMessageAsync(message.Channel.Id, string.Join(" ", urls.Select(u => $"[]({u})")), isNsfw: false).ConfigureAwait(false);
                    }
                }
                else
                {
                    if (_ErrorImages == null)
                    {
                        var mt1 = GetUrlAsync(new[] { "ep:63", "‚µ‚Î‚ç‚­‚±‚Ì‚Ü‚Ü", "ch:mirei" }, hc);
                        var mt2 = GetUrlAsync(new[] { "ep:87", "“š‚¦‚ªo‚È‚¢‚í", "ch:mirei" }, hc);
                        var tasks = new[] { mt1, mt2 };

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        _ErrorImages = tasks.SelectMany(t => t.Result).ToArray();
                    }

                    using (var bc = new BotClient())
                    {
                        await bc.PostMessageAsync
                            (
                                message.Channel.Id,
                                _ErrorImages.Length == 0 ? "Not found" : $"Not Found[]({_ErrorImages[new Random().Next(_ErrorImages.Length)]})",
                                isNsfw: false
                            )
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<string[]> GetUrlAsync(IReadOnlyList<string> args, HttpClient hc)
        {
            var sb = new StringBuilder("http://shipwreck.jp/anime/api/captures?s=3&o=Random&q=+series:pripara");

            foreach (var a in args)
            {
                sb.Append(" +");
                sb.Append(a);
            }

            var req = new HttpRequestMessage(HttpMethod.Get, sb.ToString());

            req.Headers.Add("X-Access-Token", AnimeDBAccessToken);

            var res = await hc.SendAsync(req).ConfigureAwait(false);

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                var iir = JsonConvert.DeserializeObject<PriparaResult>(json);

                if (iir.Captures?.Length > 0)
                {
                    var tasks = iir.Captures.Select(c => ImageSanitizer.GetSafeUrlAsync(c.RawUrl)).ToList();
                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    return tasks.Select(t => t.Result).ToArray();
                }
            }

            return new string[0];
        }
    }
}