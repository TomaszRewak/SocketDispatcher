using Moq;
using NUnit.Framework;
using SocketDispatcher.Test.Connections;
using SocketDispatcher.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace SocketDispatcher.Test
{
	[TestFixture]
	public partial class SocketConnectionTest
	{
		[Test]
		public void ClientConnectsToAValidServer()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090);

			AssertClientsConnected();
		}

		[Test]
		public void ClientDoesntConnectsToUnexistentServer()
		{
			StartClient(9090);

			AssertClientsFailedToConnect();
		}

		[Test]
		public void ClientDisconnectsFromDisconnectedServer()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090);

			AssertClientsConnected();
			StopServers();
			AssertClientsNotConnected();
		}
	}
}