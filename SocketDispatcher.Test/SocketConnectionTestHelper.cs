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
	public partial class SocketConnectionTest
	{
		List<(ServerConnection Connection, Dispatcher Dispatcher)> _serverConnections;
		List<(RemoteConnection Connection, Dispatcher Dispatcher)> _remoteConnections;

		[SetUp]
		public void SetUp()
		{
			_serverConnections = new List<(ServerConnection, Dispatcher)>();
			_remoteConnections = new List<(RemoteConnection, Dispatcher)>();
		}

		[TearDown]
		public void TearDown()
		{
			StopClients();
			StopServers();
		}

		private ServerConnection StartServer(int port)
		{
			var dispatcher = DispatcherHelper.Spawn();
			var serverConnection = dispatcher.Invoke(() => new ServerConnection(port));

			_serverConnections.Add((serverConnection, dispatcher));

			return serverConnection;
		}

		private RemoteConnection StartClient(int port)
		{
			var dispatcher = DispatcherHelper.Spawn();
			var remoteConnection = dispatcher.Invoke(() => new RemoteConnection("localhost", port));

			_remoteConnections.Add((remoteConnection, dispatcher));

			return remoteConnection;
		}

		private void StopServers()
		{
			foreach (var serverConnection in _serverConnections) serverConnection.Connection.Dispose();
			foreach (var serverConnection in _serverConnections) ThreadHelper.Await(ref serverConnection.Connection.Connected, false);
			foreach (var serverConnection in _serverConnections) serverConnection.Dispatcher.InvokeShutdown();

			_serverConnections.Clear();
		}

		private void StopClients()
		{
			foreach (var remoteConnection in _remoteConnections) remoteConnection.Connection.Dispose();
			foreach (var remoteConnection in _remoteConnections) ThreadHelper.Await(ref remoteConnection.Connection.Connected, false);
			foreach (var remoteConnection in _remoteConnections) remoteConnection.Dispatcher.InvokeShutdown();

			_remoteConnections.Clear();
		}

		public void AssertClientsConnected()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.Connected, true);
		}

		public void AssertClientsNotConnected()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.Connected, false);
		}

		public void AssertClientsFailedToConnect()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.ConnectionFailed, true);
		}
	}
}