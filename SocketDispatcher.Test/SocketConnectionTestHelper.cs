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

		private void SendFromClient(params byte[] data)
		{
			_remoteConnections.First().Connection.Send(data);
		}

		public void AwaitClientsConnected()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.Connected, true);
		}

		public void AssertClientsNotConnected()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.Connected, false);
		}

		public void AwaitClientsFailedToConnect()
		{
			foreach (var remoteConnection in _remoteConnections)
				ThreadHelper.Await(ref remoteConnection.Connection.ConnectionFailed, true);
		}

		public void AwaitServerBuffered(params byte[] bytes)
		{
			ThreadHelper.Await(ref _serverConnections.First().Connection.Clients.First().Buffer, bytes);
		}

		public void AwaitServerReceived(params byte[] bytes)
		{
			ThreadHelper.Await(ref _serverConnections.First().Connection.Clients.First().LastMessage, bytes);
		}
	}
}