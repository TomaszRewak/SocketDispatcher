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

		protected override int Read(ReadOnlySpan<byte> data)
		{
			return data.Length;
		}

		public void Send(byte[] message)
		{
			var buffer = Write(message.Length);
			message.CopyTo(buffer);
			Flush();
		}
	}
}
