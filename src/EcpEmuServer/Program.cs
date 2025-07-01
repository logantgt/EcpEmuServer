using System.Net;
using Microsoft.Extensions.FileProviders;

namespace EcpEmuServer
{
    public class Program
    {
        public static string UserProvidedEndpoint = "";
        public static void Main(string[] args)
        {
            Console.Title = "EcpEmuServer";

            Logger.Log(Logger.LogSeverity.info, "EcpEmuServer started (press CTRL+C twice to terminate)");
            RuleManager ruleManager = new RuleManager();

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            if (args.Length > 0)
            {
                IPAddress? addr;
                if (IPAddress.TryParse(args[0], out addr))
                {
                    builder.WebHost.UseUrls($"http://{args[0]}:8060/");
                    UserProvidedEndpoint = args[0];
                }
            }
            else
            {
                builder.WebHost.UseUrls("http://*:8060/");
            }

            WebApplication app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true, });

            app.MapGet("/", () =>
            {
                Logger.Log(Logger.LogSeverity.info, "Client requested Root XML Response");

                return Results.Content(SSDPMessages.ecpDeviceRootResponse, "application/xml");
            });

            app.MapGet("/query/apps", () =>
            {
                Logger.Log(Logger.LogSeverity.info, $"Client requested /query/apps");

                return Results.Content("<apps></apps>", "application/xml");
            });

            app.MapPost("/keypress/{btn:alpha}", (string btn) =>
            {
                Logger.Log(Logger.LogSeverity.info, $"Got button press {btn}");

                ruleManager.Execute(btn);

                return HttpStatusCode.OK;
            });

            Thread apiThread = new Thread(new ThreadStart(app.Run));
            Thread ssdpThread = new Thread(new ThreadStart(SSDPManager.StartSSDP));

            apiThread.Start();
            Logger.Log(Logger.LogSeverity.info, "Button API running");
            ssdpThread.Start();
        }
    }
}