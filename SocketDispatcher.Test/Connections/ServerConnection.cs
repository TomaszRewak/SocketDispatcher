using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketDispatcher.Test.Connections
{
	internal sealed class ServerConnection : BaseConnection
	{
		private bool _accpetsClients = false;

		public List<ClientConnection> Clients { get; } = new List<ClientConnection>();

		public ServerConnection(int port)
		{
			Listen(port);
		}

		public new void Dispose()
		{
			foreach (var client in Clients)
				client.Dispose();

			Close();
		}

		public ServerConnection ThatAccpetsClients()
		{
			_accpetsClients = true;
			return this;
		}

		protected override void OnAccepted(Socket socket)
		{
			if (!_accpetsClients) socket.Close();
			else
			{
				var client = new ClientConnection(socket);
				if (MessageTerminator.HasValue) client.ThatAcceptsMessages(MessageTerminator.Value);
				Clients.Add(client);
			}
		}
	}
}
