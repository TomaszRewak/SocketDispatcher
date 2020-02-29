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
		private readonly Socket _socket;
		private readonly WinSock _winSock;
		private readonly ReadBuffer _readBuffer;
		private readonly WriteBuffer _writeBuffer;

		internal IntPtr Handle => _socket.Handle;

		public SocketConnection(string host, int port)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
			{
				Blocking = false
			};
			_socket.Connect(host, port);

			_winSock = WinSock.Current;
			_winSock.Add(this);

			_readBuffer = new ReadBuffer();
			_writeBuffer = new WriteBuffer();
		}

		protected abstract int Read(ReadOnlySpan<byte> data);
		protected abstract int OnConnected();
		protected abstract int OnDisconnected();

		internal void Read()
		{
			var buffer = _readBuffer.Write(_socket.Available);
			_socket.Receive(buffer);

			var bytes = Read(buffer);
			_readBuffer.Pop(bytes);
		}

		internal void Flush()
		{
			int bytes = 0;

			foreach(var chunk in _writeBuffer)
			{
				int sentBytes = _socket.Send(chunk);

				if (sentBytes == 0)
					break;
				else
					bytes += sentBytes;
			}

			_writeBuffer.Pop(bytes);
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
