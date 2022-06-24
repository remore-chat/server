using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Client.Core.Exceptions
{
    public class ConnectionFailedException : Exception
    {
        public SocketType SocketType { get; }

        public ConnectionFailedException()
        {
        }

        public ConnectionFailedException(SocketType socketType, string message) : base(message)
        {
            SocketType = socketType;
        }

        public ConnectionFailedException(string? message) : base(message)
        {
        }

        public ConnectionFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ConnectionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
