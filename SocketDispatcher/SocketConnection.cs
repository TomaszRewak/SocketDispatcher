using System;
using System.Net;
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
		public bool Connected { get; internal set; }

		public SocketConnection() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
		{ }

		public SocketConnection(Socket socket)
		{
			_socket = socket;
			_socket.Blocking = true;

			_winSock = WinSock.Current;
			_winSock.Add(this);

			_readBuffer = new ReadBuffer();
			_writeBuffer = new WriteBuffer();
		}

		public void Listen(int port) => _socket.Bind(new IPEndPoint(IPAddress.Any, port));
		public void Connect(string host, int port) => _socket.ConnectAsync(host, port);

		protected abstract int Read(ReadOnlySpan<byte> data);
		protected abstract int OnConnected();
		protected abstract int OnDisconnected();

		protected Span<byte> Write(int bytes) => _writeBuffer.Write(bytes);

		protected internal void Flush()
		{
			int sentBytes = _socket.Send(_writeBuffer.Read());
			_writeBuffer.Pop(sentBytes);
		}

		internal void Read()
		{
			var buffer = _readBuffer.Write(_socket.Available);
			_socket.Receive(buffer);

			var processedBytes = Read(_readBuffer.Read());
			_readBuffer.Pop(processedBytes);
		}

		internal void Accept()
		{
			var socket = _socket.Accept();

			if (Accepted == null)
				socket.Disconnect(false);
			else
				Accepted?.Invoke(this, socket);
		}

		public event Action<SocketConnection, Socket> Accepted;
	}
}
