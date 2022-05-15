using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;

namespace TTalk.WinUI.Services
{
    public class SoundService
    {
        public async Task<byte[]> GetSoundBytes(string filename)
        {
            var sound = ResourceManager.Current.MainResourceMap[$"Files/Assets/sounds/{filename}.wav"];
            var stream = await sound.Resolve().GetValueAsStreamAsync();
            byte[] bytes = null;
            var reader = new Windows.Storage.Streams.DataReader(stream.GetInputStreamAt(0));
            bytes = new byte[stream.Size];
            await reader.LoadAsync((uint)stream.Size);
            reader.ReadBytes(bytes);
            return bytes;
        }
    }
}
