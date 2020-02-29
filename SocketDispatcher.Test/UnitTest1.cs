using Moq;
using NUnit.Framework;

namespace SocketDispatcher.Test
{
	public class Tests
	{
		[Test]
		public void Test1()
		{
			var socket1 = MockSocket(9090);
			var socket2 = MockSocket(9090);

			
		}

		private Mock<SocketConnection> MockSocket(int port)
		{
			return new Mock<SocketConnection>("127.0.0.1", port);
		}
	}
}