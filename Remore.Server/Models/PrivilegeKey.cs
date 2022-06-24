using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remore.Server.Models
{
    public class PrivilegeKey
    {
        [Key]
        public string Id { get; set; }
        public string Key { get; set; }

        public PrivilegeKey()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
