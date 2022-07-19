using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Enums;

namespace Remore.Library.Packets.Server
{
    public class StateChangedPacket : IPacket
    {
        public int Id => 4;

        public int NewState { get; set; }
        //Present only if State == SessionState.Connected; otherwise empty string :)
        public string ClientId { get; set; }
        public string RequestId { get; set; }

        public StateChangedPacket()
        {

        }
        public StateChangedPacket(SessionState newState)
        {
            NewState = (int)newState;
            ClientId = "";
        }
        public StateChangedPacket(SessionState newState, string clientId)
        {
            NewState = (int)newState;
            ClientId = clientId ?? "";
        }
    }
}
