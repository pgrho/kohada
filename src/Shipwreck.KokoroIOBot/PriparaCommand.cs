using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
            var verbose = false;
            List<string> skip = null;
            foreach (var a in args)
            {
                if (Regex.IsMatch(a, "^([-/]v|(--|/)verbose)$", RegexOptions.IgnoreCase))
                {
                    verbose = true;
                    (skip ?? (skip = new List<string>(1))).Add(a);
                }
            }

            if (skip != null)
            {
                args = args.Except(skip).ToList();
            }

            using (var hc = new HttpClient())
            {
                var cr = await SearchCapturesAsync(args, hc).ConfigureAwait(false);

                if (cr.Captures?.Length > 0)
                {
                    await PostCapturesAsync(message, verbose, cr);
                }
                else
                {
                    var sr = await SearchSubtitlesAsync(args, hc).ConfigureAwait(false);

                    if (sr.Subtitles?.Length > 0)
                    {
                        await PostSubtitlesAsync(message, sr);
                    }
                    else
                    {
                        if (_ErrorImages == null)
                        {
                            var mt1 = SearchCapturesAsync(new[] { "ep:63", "‚µ‚Î‚ç‚­‚±‚Ì‚Ü‚Ü", "ch:mirei" }, hc);
                            var mt2 = SearchCapturesAsync(new[] { "ep:87", "“š‚¦‚ªo‚È‚¢‚í", "ch:mirei" }, hc);
                            var tasks = new[] { mt1, mt2 };

                            await Task.WhenAll(tasks).ConfigureAwait(false);

                            _ErrorImages = tasks.SelectMany(t => t.Result.Captures).Select(c => c.RawUrl).ToArray();
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
        }

        private static async Task PostSubtitlesAsync(Message message, AnimeDBSubtitleResult sr)
        {
            var sb = new StringBuilder();
            sb.Append("Found ").Append(sr.TotalCount.ToString("#,0")).Append(" subtitles.");

            foreach (var c in sr.Subtitles)
            {
                sb.AppendLine();
                sb.Append("- ").Append(c.Episode.FullTitle);
                if (c.Position >= 0)
                {
                    sb.Append(" ");
                    sb.Append(TimeSpan.FromSeconds(c.Position.Value).ToString("mm\":\"ss"));
                }
                sb.Append(Regex.Replace(c.Text, "\\s+", " "));
            }

            using (var bc = new BotClient())
            {
                await bc.PostMessageAsync(message.Channel.Id, sb.ToString(), isNsfw: false).ConfigureAwait(false);
            }
        }

        private static async Task PostCapturesAsync(Message message, bool verbose, AnimeDBCaptureResult cr)
        {
            var sanitized = cr.Captures.Select(c => ImageSanitizer.GetSafeUrlAsync(c.RawUrl)).ToArray();

            await Task.WhenAll(sanitized).ConfigureAwait(false);

            var sb = new StringBuilder();
            sb.Append("Found ").Append(cr.TotalCount.ToString("#,0")).Append(" images.");

            var i = 0;
            foreach (var c in cr.Captures)
            {
                var u = sanitized[i++].Result;
                if (verbose)
                {
                    sb.AppendLine();
                    sb.Append("- [").Append(c.Episode.FullTitle);
                    if (c.Position >= 0)
                    {
                        sb.Append(" ");
                        sb.Append(TimeSpan.FromSeconds(c.Position.Value).ToString("mm\":\"ss"));
                    }
                    sb.Append("](").Append(u).Append(")");
                }
                else
                {
                    sb.Append("[](").Append(u).Append(")");
                }
            }

            using (var bc = new BotClient())
            {
                await bc.PostMessageAsync(message.Channel.Id, sb.ToString(), isNsfw: false).ConfigureAwait(false);
            }
        }

        private async Task<AnimeDBCaptureResult> SearchCapturesAsync(IReadOnlyList<string> args, HttpClient hc)
        {
            var sb = new StringBuilder("http://shipwreck.jp/anime/api/captures?s=3&o=Random&q=+series:pripara");

            foreach (var a in args)
            {
                sb.Append(" +");
                sb.Append(Uri.EscapeDataString(a));
            }

            var req = new HttpRequestMessage(HttpMethod.Get, sb.ToString());

            req.Headers.Add("X-Access-Token", AnimeDBAccessToken);

            var res = await hc.SendAsync(req).ConfigureAwait(false);
            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AnimeDBCaptureResult>(json);
        }

        private async Task<AnimeDBSubtitleResult> SearchSubtitlesAsync(IReadOnlyList<string> args, HttpClient hc)
        {
            var sb = new StringBuilder("http://shipwreck.jp/anime/api/subtitles?s=8&o=Random&q=+series:pripara");

            foreach (var a in args)
            {
                sb.Append(" +");
                sb.Append(Uri.EscapeDataString(a));
            }

            var req = new HttpRequestMessage(HttpMethod.Get, sb.ToString());

            req.Headers.Add("X-Access-Token", AnimeDBAccessToken);

            var res = await hc.SendAsync(req).ConfigureAwait(false);
            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AnimeDBSubtitleResult>(json);
        }
    }
}