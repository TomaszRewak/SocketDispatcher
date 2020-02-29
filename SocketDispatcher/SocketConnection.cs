using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SocketDispatcher
{
	public abstract class SocketConnection
	{
		private readonly WinSock _winSock;
		private readonly Socket _socket;

		internal IntPtr Handle => _socket.Handle;

		public SocketConnection(string host, int port)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.Connect(host, port);

			_winSock = WinSock.Current;
			_winSock.Add(this);
		}

		protected abstract int Read(ReadOnlySpan<byte> data);
		protected abstract int OnConnected();
		protected abstract int OnDisconnected();

		internal void Read()
		{
			_socket.Available
		}

		private void Flush()
		{

		}

		private void ThreadFilterMessage(ref MSG msg, ref bool handled)
		{
			if (msg.)
		}

		private void Args_Completed(object sender, SocketAsyncEventArgs e)
		{
			e.
		}

		public event int Readable()
	}
}
