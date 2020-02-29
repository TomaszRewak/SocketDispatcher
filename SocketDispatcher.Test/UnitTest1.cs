using Moq;
using NUnit.Framework;

namespace SocketDispatcher.Test
{
	public class Tests
	{
		[Test]
		public void ConnectsToAValidPort()
		{
			var publisherSocket = new Mock<SocketConnection>().Object;

			SocketConnection acceptedSocket;
			publisherSocket.Accepted += (p, s) => acceptedSocket = new Mock<SocketConnection>(s).Object;
			publisherSocket.Listen(9090);

			var listenerSocket = new Mock<SocketConnection>().Object;
			listenerSocket.Connect("127.0.0.1", 9090);

			Assert.IsTrue(listenerSocket.Connected);
		}
	}
}