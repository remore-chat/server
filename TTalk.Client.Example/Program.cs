using TTalk.Client.Core;

var client = new TTalkClient("127.0.0.1", 9831, "roxxelroxx");
client.Ready += OnReady;

async void OnReady(object? sender, object e)
{
    var res = await client.RequestChannelJoinAsync("c4ec4fbd-1c37-450e-a5af-3aa8bddcf482");
    ;
}

await client.ConnectAsync();
await Task.Delay(-1);