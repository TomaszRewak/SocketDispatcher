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
	public class SocketConnectionTest
	{
		[Test]
		public void ConnectsToAValidPort()
		{
			var dispatcher = DispatcherHelper.Spawn();

			ServerConnection serverSocket = null;
			RemoteConnection remoteSocket = null;
			ClientConnection clientSocket = null;

			dispatcher.Invoke(() =>
			{
				serverSocket = new ServerConnection(9090);
			});
			serverSocket.Accepted += s => clientSocket = s;

			ThreadHelper.Await(ref serverSocket);

			dispatcher.Invoke(() =>
			{
				remoteSocket = new RemoteConnection("localhost", 9090);
			});

			ThreadHelper.Await(ref remoteSocket);
			ThreadHelper.Await(ref clientSocket);

			dispatcher.Invoke(() =>
			{
				clientSocket.Send(new byte[] { 1, 2, 3 });
			});

			ThreadHelper.Await(ref remoteSocket.LastMessage);

			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, remoteSocket.LastMessage);
		}
	}
}