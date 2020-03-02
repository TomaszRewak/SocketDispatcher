using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDispatcher.Test.Connections
{
	internal sealed class RemoteConnection : SocketConnection
	{
		public bool Connected;
		public byte[] LastMessage;

		public RemoteConnection(string host, int port)
		{
			Connect(host, port);
		}

		protected override int Read(ReadOnlySpan<byte> data)
		{
			LastMessage = data.ToArray();
			return data.Length;
		}

		protected override void OnConnected() => Connected = true;
		protected override void OnDisconnected() => Connected = false;
	}
}
