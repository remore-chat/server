using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;

namespace TTalk.WinUI.Networking
{
    public class TTalkQueryClient : TcpClient
    {
        public TTalkQueryClient(string address, int port) : base(IPAddress.Parse(address), port)
        {
            _slim = new(0);
        }

        private SemaphoreSlim _slim;
        private ServerQueryResponsePacket _response;

        protected override void OnConnected()
        {
            base.OnConnected();
        }

        public async Task<ServerQueryResponsePacket> GetServerInfo()
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
                _slim.Wait();
                return _response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            using var reader = new ByteReaderWriter(buffer);
            int lengthOfPacket = reader.ReadInt();
            int id = reader.ReadInt(false);
            if (lengthOfPacket == size - 4)
            {
                HandlePacket(buffer);
                return;
            }
            else
            {
                int _offset = 0;
                do
                {
                    var packet = buffer.Slice(_offset, lengthOfPacket + 4);
                    HandlePacket(packet);
                    _offset += lengthOfPacket + 4;
                    reader.Position = _offset;
                    lengthOfPacket = reader.ReadInt();
                    id = reader.ReadInt(false);

                } while (_offset + lengthOfPacket < size);
            }
        }

        private void HandlePacket(byte[] buffer)
        {
            var packet = IPacket.FromByteArray(buffer);
            if (packet is not ServerQueryResponsePacket response)
            {
                _slim.Release();
            }
            else
            {
                _response = response;
                _slim.Release();
            }
        }
    }
}
