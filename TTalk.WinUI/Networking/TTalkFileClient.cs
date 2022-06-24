using Microsoft.Extensions.Logging;
using NetCoreServer;
using System;
using System.Buffers.Binary;
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
    public class TTalkFileClient : TcpClient
    {
        public TTalkFileClient(string address, int port, Action<int> progressCallback = null) : base(IPAddress.Parse(address), port)
        {
            _slim = new(0);
            _progressCallback = progressCallback;
        }

        private SemaphoreSlim _slim;
        private Action<int> _progressCallback;
        private FileResponsePacket _response;
        private int _previousProgress;
        private object _lock = new();
        private int _packetLength = -1;
        private List<byte> _buffer = new();
        protected override void OnConnected()
        {
            ReceiveAsync();
        }

        public async Task<FileResponsePacket> DownloadFile(string id)
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
                this.Send(new RequestFilePacket() { FileId = id });
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
            lock (_lock)
            {

                if (_packetLength == -1)
                {
                    _packetLength = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(0, 4));
                    _buffer.AddRange(buffer.Slice(4, size - 4));
                }
                else
                {
                    _buffer.AddRange(buffer.Slice(0, size));
                }
                if (_progressCallback != null)
                {
                    var progress = (int)((float)_buffer.Count / (float)_packetLength * 100f);
                    if (_previousProgress != progress)
                        _progressCallback.Invoke(progress);
                }
                if (_buffer.Count == _packetLength)
                {
                    HandlePacket(_buffer.ToArray());
                }
                Thread.Sleep(100);
            }
        }

        private void HandlePacket(byte[] buffer)
        {
            var packet = IPacket.FromByteArray(buffer, out var ex);
            if (ex != null)
                App.GetService<ILogger<TTalkFileClient>>().LogError($"Failed to read packet with ID {BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(4, 4))}:\n" + ex.ToString());
            if (packet is not FileResponsePacket response)
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
