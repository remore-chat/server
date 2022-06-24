using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remore.Server.EF;
using Remore.Server.Models;

namespace Remore.Server.Services
{
    public record ServerConfiguration
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxClients { get; set; }
        public PrivilegeKey? PrivilegeKey { get; set; }
        public ServerConfiguration()
        {
            Id = Guid.NewGuid().ToString();
        }

    }
}
