using System.Net;

namespace EcpEmuServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "EcpEmuServer";

                Logger.Log(Logger.LogSeverity.info, "EcpEmuServer started");
                RuleManager ruleManager = new RuleManager();

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                builder.WebHost.UseUrls("http://*:8060/");
                WebApplication app = builder.Build();

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
            catch (Exception ex)
            {
                Logger.Log(Logger.LogSeverity.error, ex.ToString());
            }
        }
    }
}