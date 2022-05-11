using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using TTalk.Library.Packets;
using TTalk.Server;

int port = 9831;
if (args.Length > 0)
    port = int.Parse(args[0]);

bool isStopping = false;
var cts = new CancellationTokenSource();
Logger.LogInfo($"Starting TTalk server at port {port}");
PacketReader.Init();
var server = new TTalkServer(IPAddress.Any, port);
Logger.LogInfo("Server starting...");
server.Start();
Logger.LogInfo("Server started. Have fun!");
Console.CancelKeyPress += Console_CancelKeyPress;
try
{
await Task.Delay(-1, cts.Token);

}
catch (Exception)
{

}

void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    if (isStopping) return;
    isStopping = true;
    Logger.LogInfo("Server stopping...");
    server.Stop();
    Logger.LogInfo("Done!");
    cts.Cancel();
    Environment.Exit(0);
}