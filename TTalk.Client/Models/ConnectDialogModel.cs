using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Client.Models
{
    public class ConnectDialogModel : BaseReactiveModel
    {
        private string address;

        public string Address
        {
            get { return address; }
            set { this.RaiseAndSetIfChanged(ref address, value); }
        }
    }
}
