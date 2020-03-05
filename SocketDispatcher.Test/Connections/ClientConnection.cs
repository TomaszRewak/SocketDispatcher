using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketDispatcher.Test.Connections
{
	internal sealed class ClientConnection : BaseConnection
	{
		public ClientConnection(Socket socket) : base(socket)
		{ }
	}
}
