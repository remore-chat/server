using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PInvoke.User32;

namespace Remore.WinUI.KeyBindings
{
    public class KeyBinding
    {
        public VirtualKey Key { get; set; } 
        public KeyBindingAction Action { get; set; }
    }
}
