using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows.Threading;

namespace SocketDispatcher.Test
{
	internal class DummySocketConnection : SocketConnection
	{
		protected override int OnConnected() => 0;
		protected override int OnDisconnected() => 0;
		protected override int Read(ReadOnlySpan<byte> data) => data.Length;
	}

	[TestFixture]
	public class Tests
	{
		[Test]
		public void ConnectsToAValidPort()
		{
			Dispatcher dispatcher = null;
			SocketConnection listenerSocket = null;
			SocketConnection acceptedSocket = null;
			DummySocketConnection publisherSocket = null;

			var thread = new Thread(() =>
			{
				dispatcher = Dispatcher.CurrentDispatcher;

				Dispatcher.Run();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

			Thread.Sleep(1000);

			dispatcher.Invoke(() => {
				publisherSocket = new DummySocketConnection();
				publisherSocket.Accepted += (p, s) => acceptedSocket = new DummySocketConnection();
				publisherSocket.Listen(9090);

				listenerSocket = new DummySocketConnection();
				listenerSocket.Connect("127.0.0.1", 9090);
			});

			Thread.Sleep(1000);

			Assert.IsNotNull(acceptedSocket);
			Assert.IsTrue(listenerSocket.Connected);
		}
	}
}