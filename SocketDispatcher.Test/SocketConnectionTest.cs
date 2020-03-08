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

			SendFromClient(new byte[] { 1, 2, 3, 4 });

			AwaitServerBuffered(new byte[] { 1, 2, 3, 4 });
		}

		[Test]
		public void ServerAccpetsMessagesFromClient()
		{
			StartServer(9090).ThatAccpetsClients().ThatAcceptsMessages(0);
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(new byte[] { 1, 2, 0, 3, 4, 0 });

			AwaitServerReceived(new byte[] { 3, 4, 0 });
		}

		[Test]
		public void ServerBuffersIncompleteMessages()
		{
			StartServer(9090).ThatAccpetsClients().ThatAcceptsMessages(0);
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(new byte[] { 1, 2, 0, 3, 4 });

			AwaitServerReceived(new byte[] { 1, 2, 0 });
			AwaitServerBuffered(new byte[] { 3, 4 });
		}

		[Test]
		public void ServerReceivesMultipleMessagesIntoSingleBuffer()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(new byte[] { 1, 2 });
			SendFromClient(new byte[] { 3, 4 });

			AwaitServerBuffered(new byte[] { 1, 2, 3, 4 });
		}

		[Test]
		public void ClientDisconnectsOnRequestFromServer()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090).ThatTerminatesConnection(9);
			AwaitClientsConnected();

			SendFromServer(new byte[] { 1, 2, 3, 9 });

			AwaitClientsNotConnected();
		}

		[Test]
		public void ServerCanAcceptMultipleClients()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090);
			StartClient(9090);

			AwaitClientsConnected();
		}

		[Test]
		public void EachClientReceivesItsOwnMessage()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090).ThatAcceptsMessages(0);
			StartClient(9090).ThatAcceptsMessages(0);
			AwaitClientsConnected();

			SendFromServer(client: 0, data: new byte[] { 1, 2, 3, 0 });
			SendFromServer(client: 1, data: new byte[] { 4, 5, 6, 0 });

			AwaitClientReceived(client: 0, data: new byte[] { 1, 2, 3, 0 });
			AwaitClientReceived(client: 1, data: new byte[] { 4, 5, 6, 0 });
		}

		[Test]
		public void EachClientSendsItsOwnMessage()
		{
			StartServer(9090).ThatAccpetsClients();
			StartClient(9090);
			StartClient(9090);
			AwaitClientsConnected();

			SendFromClient(client: 0, data: new byte[] { 1, 2, 3 });
			SendFromClient(client: 1, data: new byte[] { 4, 5, 6 });

			AwaitServerBuffered(client: 0, data: new byte[] { 1, 2, 3 });
			AwaitServerBuffered(client: 1, data: new byte[] { 4, 5, 6 });
		}

		[Test]
		public void TwoServersCanRunOnASingleDispatcher()
		{
			StartServer(9090, onCommonDispatcher: true).ThatAccpetsClients();
			StartServer(9091, onCommonDispatcher: true).ThatAccpetsClients();

			StartClient(9090);
			StartClient(9091);
			AwaitClientsConnected();

			SendFromClient(client: 0, data: new byte[] { 1, 2, 3 });
			SendFromClient(client: 1, data: new byte[] { 4, 5, 6 });

			AwaitServerBuffered(server: 0, data: new byte[] { 1, 2, 3 });
			AwaitServerBuffered(server: 1, data: new byte[] { 4, 5, 6 });
		}
	}
}