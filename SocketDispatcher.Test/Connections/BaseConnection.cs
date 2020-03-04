using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketDispatcher.Test.Connections
{
	internal abstract class BaseConnection : SocketConnection
	{
		public bool Connected;
		public bool ConnectionFailed;

		protected BaseConnection() : base() { }
		protected BaseConnection(Socket socket) : base(socket) { }

		public void Dispose()
		{
			if (!Connected) return;

			Disconnect();
		}

		protected override void OnConnected() => Connected = true;
		protected override void OnDisconnected() => Connected = false;
		protected override void OnConnectionFailed() => ConnectionFailed = true;
	}
}
