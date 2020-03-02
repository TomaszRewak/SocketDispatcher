using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace SocketDispatcher.Test
{
	internal class ListenerSocketConnection : SocketConnection
	{
		public byte[] LastMessage { get; private set; }

		public ListenerSocketConnection(string host, int port)
		{
			Connect(host, port);
		}

		protected override int Read(ReadOnlySpan<byte> data)
		{
			LastMessage = data.ToArray();
			return data.Length;
		}
	}

	internal class ClientSocketConnection : SocketConnection
	{
		public ClientSocketConnection(Socket socket) : base(socket)
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

	internal class ServerSocketConnection : SocketConnection
	{
		public List<ClientSocketConnection> Clients { get; } = new List<ClientSocketConnection>();

		public ServerSocketConnection(int port)
		{
			Listen(port);
		}

		protected override int Read(ReadOnlySpan<byte> data)
		{
			return data.Length;
		}

		protected override void OnAccepted(Socket socket)
		{
			Clients.Add(new ClientSocketConnection(socket));
		}
	}

	[TestFixture]
	public class Tests
	{
		[Test]
		public void ConnectsToAValidPort()
		{
			Dispatcher dispatcher = null;
			ListenerSocketConnection listenerSocket = null;
			ServerSocketConnection serverSocket = null;

			var thread = new Thread(() =>
			{
				dispatcher = Dispatcher.CurrentDispatcher;
				Dispatcher.Run();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

			Thread.Sleep(1000);

			dispatcher.Invoke(() =>
			{
				serverSocket = new ServerSocketConnection(9090);
			});

			Thread.Sleep(2000);

			dispatcher.Invoke(() =>
			{
				listenerSocket = new ListenerSocketConnection("localhost", 9090);
			});

			Thread.Sleep(2000);

			dispatcher.Invoke(() =>
			{
				foreach (var client in serverSocket.Clients)
					client.Send(new byte[] { 1, 2, 3 });
			});

			Thread.Sleep(2000);

			CollectionAssert.IsNotEmpty(serverSocket.Clients);
			Assert.IsTrue(listenerSocket.Connected);
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, listenerSocket.LastMessage);
		}
	}
}