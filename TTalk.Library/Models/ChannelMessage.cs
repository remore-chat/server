using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Models
{
    [Serializable]
    public class ChannelMessage
    {
        public string Id { get; set; }
        public string ChannelId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }

        public ChannelMessage()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
