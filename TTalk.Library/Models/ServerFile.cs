using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Models
{
    public class ServerFile
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public ServerFile()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
