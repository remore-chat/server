using TTalk.Client.Core;

var ttalkClient = new TTalkClient("127.0.0.1", 9831, username: "roxxelroxx");
await ttalkClient.ConnectAsync();
await Task.Delay(-1);