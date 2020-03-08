using Moq;
using NUnit.Framework;
using SocketDispatcher.Test.Connections;
using SocketDispatcher.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace SocketDispatcher.Test
{
	public partial class SocketConnectionTest
	{
		private Dispatcher _commonDispatcher;
		private List<(ServerConnection Connection, Dispatcher Dispatcher)> _serverConnections;
		private List<(RemoteConnection Connection, Dispatcher Dispatcher)> _remoteConnections;

		[SetUp]
		public void SetUp()
		{
			_commonDispatcher = DispatcherHelper.Spawn();
			_serverConnections = new List<(ServerConnection, Dispatcher)>();
			_remoteConnections = new List<(RemoteConnection, Dispatcher)>();
		}

		[TearDown]
		public void TearDown()
		{
			StopClients();
			StopServers();

			_commonDispatcher.InvokeShutdown();
		}

		private ServerConnection StartServer(int port, bool onCommonDispatcher = false)
		{
			var dispatcher = onCommonDispatcher ? _commonDispatcher : DispatcherHelper.Spawn();
			var serverConnection = dispatcher.Invoke(() => new ServerConnection(port));

			_serverConnections.Add((serverConnection, dispatcher));

			return serverConnection;
		}

		private RemoteConnection StartClient(int port, bool onCommonDispatcher = false)
		{
			var dispatcher = onCommonDispatcher ? _commonDispatcher : DispatcherHelper.Spawn();
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

		private void SendFromClient(byte[] data, int client = 0)
		{
			_remoteConnections[client].Connection.Send(data);
		}

		private void SendFromServer(byte[] data, int client = 0)
		{
			_serverConnections.First().Connection.Clients[client].Send(data);
		}

		public void AwaitClientsConnected()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.Connected, true);
		}

		public void AwaitClientsNotConnected()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.Connected, false);
		}

		public void AwaitClientsFailedToConnect()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.ConnectionFailed, true);
		}

		public void AwaitServerBuffered(byte[] data, int client = 0, int server = 0)
		{
			ThreadHelper.Await(ref _serverConnections[server].Connection.Clients[client].Buffer, data);
		}

		public void AwaitServerReceived(byte[] data)
		{
			ThreadHelper.Await(ref _serverConnections.First().Connection.Clients.First().LastMessage, data);
		}

		public void AwaitClientBuffered(byte[] data, int client = 0)
		{
			ThreadHelper.Await(ref _remoteConnections[client].Connection.Buffer, data);
		}

		public void AwaitClientReceived(byte[] data, int client = 0)
		{
			ThreadHelper.Await(ref _remoteConnections[client].Connection.LastMessage, data);
		}
	}
}