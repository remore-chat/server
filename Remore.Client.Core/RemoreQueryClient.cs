using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Remore.Library.Packets;
using Remore.Library.Packets.Client;
using Remore.Library.Packets.Server;

namespace Remore.Client.Core
{
    public class RemoreQueryClient : TcpClient
    {
        private SemaphoreSlim _semaphoreSlim;
        private ServerQueryResponsePacket _response;

        public RemoreQueryClient(string address, int port) : base(IPAddress.Parse(address), port)
        {
            _semaphoreSlim = new(0);
        }
        protected override void OnConnected()
        {
            ReceiveAsync();
        }

        public async Task<ServerQueryResponsePacket?> GetServerInfo()
        {
            try
            {
                bool success = false;
                if (!this.IsConnected)
                {
                    success = this.ConnectAsync();
                }
                if (!success)
                    return null;
                await Task.Delay(200);
                if (this.IsConnecting)
                {
                    await Task.Delay(5000);
                    if (!this.IsConnected)
                        return null;
                }
                this.Send(new ServerQueryPacket());
                _semaphoreSlim.Wait();
                return _response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            HandlePacket(buffer);
        }

        private void HandlePacket(byte[] buffer)
        {
            var packet = IPacket.FromByteArray(buffer);
            if (packet is not ServerQueryResponsePacket response)
            {
                _semaphoreSlim.Release();
            }
            else
            {
                _response = response;
                _semaphoreSlim.Release();
            }
        }
    }
}
