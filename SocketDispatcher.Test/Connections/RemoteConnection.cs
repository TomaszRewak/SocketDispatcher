using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDispatcher.Test.Connections
{
	internal sealed class RemoteConnection : BaseConnection
	{
		public RemoteConnection(string host, int port)
		{
			Connect(host, port);
		}
	}
}
