using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Server.Models.Dto
{
    public class ServerConfigurationUpdateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxClients { get; set; }
        public PrivilegeKey? PrivilegeKey { get; set; }
    }
}
