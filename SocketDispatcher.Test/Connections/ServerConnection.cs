using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketDispatcher.Test.Connections
{
	internal sealed class ServerConnection : SocketConnection
	{
		public ServerConnection(int port)
		{
			Listen(port);
		}

		protected override void OnAccepted(Socket socket)
		{
			Accepted?.Invoke(new ClientConnection(socket));
		}

		public event Action<ClientConnection> Accepted;
	}
}
