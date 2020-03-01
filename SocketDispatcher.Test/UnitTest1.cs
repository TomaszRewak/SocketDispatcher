using Moq;
using NUnit.Framework;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace SocketDispatcher.Test
{
	internal class DummySocketConnection : SocketConnection
	{
		public byte[] LastMessage { get; private set; }

		public DummySocketConnection()
		{ }
		public DummySocketConnection(Socket socket) : base(socket)
		{ }

		protected override int OnConnected() => 0;
		protected override int OnDisconnected() => 0;
		protected override int Read(ReadOnlySpan<byte> data)
		{
			LastMessage = data.ToArray();

			return data.Length;
		}
	}

	[TestFixture]
	public class Tests
	{
		[Test]
		public void ConnectsToAValidPort()
		{
			Dispatcher dispatcher = null;
			DummySocketConnection listenerSocket = null;
			DummySocketConnection acceptedSocket = null;
			DummySocketConnection publisherSocket = null;

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
				publisherSocket = new DummySocketConnection();
				publisherSocket.Accepted += (p, s) => acceptedSocket = new DummySocketConnection(s);
				publisherSocket.Listen(9090);
			});

			Thread.Sleep(2000);

			dispatcher.Invoke(() =>
			{
				listenerSocket = new DummySocketConnection();
				listenerSocket.Connect("localhost", 9090);
			});

			Thread.Sleep(2000);

			dispatcher.Invoke(() =>
			{
				var buffer = acceptedSocket.Write(3);
				buffer[0] = 1;
				buffer[1] = 2;
				buffer[2] = 3;
				acceptedSocket.Flush();
			});

			Thread.Sleep(2000);

			Assert.IsNotNull(acceptedSocket);
			Assert.IsTrue(listenerSocket.Connected);
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, listenerSocket.LastMessage);
		}
	}
}