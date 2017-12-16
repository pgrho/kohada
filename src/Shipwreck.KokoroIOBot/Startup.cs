using System.IO;
using System.Text;
using KokoroIO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shipwreck.KokoroIOBot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var routeBuilder = new RouteBuilder(app);

            var handler = new IncomingWebhookHandler(m => MessageHandler.Handle(m))
            {
                CallbackSecret = Configuration["CallbackSecret"]
            };
            BotClient.DefaultAccessToken = Configuration["AccessToken"];
            ImageSanitizer.GyazoAccessToken = Configuration["GyazoAccessToken"];

            routeBuilder.MapGet("", async (c) =>
            {
                c.Response.ContentType = "text/plain; charset=utf-8";

                using (var w = new StreamWriter(c.Response.Body, new UTF8Encoding(false)))
                {
                    await w.WriteLineAsync("kohada").ConfigureAwait(false);
                    w.Flush();
                }
            });
            routeBuilder.MapPost("incoming", handler.HandleAsync);

            app.UseRouter(routeBuilder.Build());
        }
    }
}