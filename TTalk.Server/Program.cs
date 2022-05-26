using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using TTalk.Library.Packets;
using TTalk.Server;
using TTalk.Server.EF;
using TTalk.Server.Services;

var databaseExists = File.Exists("server_database.db");

ServiceContainer.GetService<ServerDbContext>().Database.Migrate();
Logger.Init();



int port = 9831;
if (args.Length > 0)
{
    if (args[0] == "--generate-privilege-key")
    {
        var configService = ServiceContainer.GetService<ConfigurationService>();
        var privilegeKey = await configService.GeneratePrivilegeKeyAsync();
        var configuration = await configService.GetServerConfigurationAsync();
        await configService.UpdateServerConfigurationAsync(configuration with { PrivilegeKey = privilegeKey });
        Logger.LogInfo("New privilege key is generated. Old key was revoked and will not be usable more.");
        Logger.LogInfo($"Your new privilege key is: {privilegeKey.Key}");
        Logger.LogInfo($"Save it, as it is shown only once");
        return;
    }
}
bool isStopping = false;
var cts = new CancellationTokenSource();
Logger.LogInfo($"Starting TTalk server at port {port}");
PacketReader.Init();
if (!databaseExists)
{
    Logger.LogInfo("Looks like it's your first startup, hence Privilege key isn't created yet.\n" +
                    $"\tTo do this you have to stop server and run it again with --generate-privilege-key argument");
}
var server = new TTalkServer(IPAddress.Any, port);
Logger.LogInfo("Server starting...");
server.Start();
Logger.LogInfo($"Server {server.Name} started. Maximum clients: {server.MaxClients}. Have fun!");
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