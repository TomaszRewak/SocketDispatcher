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

			AwaitClientsConnected();
		}

		[Test]
		public void ClientDoesntConnectsToUnexistentServer()
		{
			StartClient(9090);

			AwaitClientsFailedToConnect();
		}

		[Test]
		public void ServerReceivesDataFromClient()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(1, 2, 3, 4);

			AwaitServerBuffered(1, 2, 3, 4);
		}

		[Test]
		public void ServerAccpetsMessagesFromClient()
		{
			StartServer(9090).ThatAccpetsClients().ThatAcceptsMessages(0);
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(1, 2, 0, 3, 4, 0);

			AwaitServerReceived(3, 4, 0);
		}

		[Test]
		public void ServerBuffersIncompleteMessages()
		{
			StartServer(9090).ThatAccpetsClients().ThatAcceptsMessages(0);
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(1, 2, 0, 3, 4);

			AwaitServerReceived(1, 2, 0);
			AwaitServerBuffered(3, 4);
		}
		
		[Test]
		public void ServerReceivesMultipleMessagesIntoSingleBuffer()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(1, 2);
			SendFromClient(3, 4);

			AwaitServerBuffered(1, 2, 3, 4);
		}
	}
}