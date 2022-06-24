using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

using Remore.WinUI.Core.Contracts.Services;

namespace Remore.WinUI.Core.Services
{
    public class FileService : IFileService
    {
        public T Read<T>(string folderPath, string fileName)
        {
            var path = Path.Combine(folderPath, fileName);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }

            return default;
        }

        public void Save<T>(string folderPath, string fileName, T content)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileContent = JsonConvert.SerializeObject(content);
            try
            {
                File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
            }
            catch
            {
                Thread.Sleep(1000);
                Save(folderPath, fileName, fileContent);
            }
        }

        public void Delete(string folderPath, string fileName)
        {
            if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
            {
                File.Delete(Path.Combine(folderPath, fileName));
            }
        }
    }
}
