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

		protected void Write(int bytes) => _writeBuffer.Write(bytes);

		protected abstract int Read(ReadOnlySpan<byte> data);
		protected abstract int OnConnected();
		protected abstract int OnDisconnected();

		protected void Flush()
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

		internal void Read()
		{
			var buffer = _readBuffer.Write(_socket.Available);
			_socket.Receive(buffer);

			var bytes = Read(buffer);
			_readBuffer.Pop(bytes);
		}
	}
}
