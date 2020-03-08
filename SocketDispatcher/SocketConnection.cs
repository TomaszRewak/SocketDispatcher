using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

[assembly: InternalsVisibleTo("SocketDispatcher.Test")]

namespace SocketDispatcher
{
	public abstract class SocketConnection
	{
		private readonly Socket _socket;
		private readonly Dispatcher _dispatcher;
		private readonly WinSock _winSock;
		private readonly SocketBuffer _readBuffer;
		private readonly SocketBuffer _writeBuffer;

		internal IntPtr Handle => _socket.Handle;

		protected SocketConnection() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
		{ }

		protected SocketConnection(Socket socket)
		{
			_socket = socket;

			_dispatcher = Dispatcher.CurrentDispatcher;
			_winSock = WinSock.Current;
			_winSock.Add(this);

			_readBuffer = new SocketBuffer();
			_writeBuffer = new SocketBuffer();
		}

		protected void Listen(int port, int maxPendingConnections = 100)
		{
			_socket.Bind(new IPEndPoint(IPAddress.Any, port));
			_socket.Listen(maxPendingConnections);

			OnConnected();
		}

		protected void Close()
		{
			_socket.Close();

			OnDisconnected();
		}

		protected void Connect(string host, int port)
		{
			_socket.BeginConnect(host, port, r =>
			{
				if (_socket.Connected) _dispatcher.Invoke(new Action(OnConnected));
				else _dispatcher.Invoke(new Action(OnConnectionFailed));
			}, null);
		}

		protected void Disconnect()
		{
			_socket.Shutdown(SocketShutdown.Both);
			_socket.BeginDisconnect(false, r => _dispatcher.Invoke(new Action(OnDisconnected)), null);
		}

		protected internal virtual void OnConnected() { }
		protected internal virtual void OnDisconnected() { }
		protected internal virtual void OnConnectionFailed() { }
		protected virtual void OnAccepted(Socket socket)
		{
			socket.BeginDisconnect(false, null, null);
		}

		protected virtual int Read(ReadOnlySpan<byte> data) => data.Length;
		protected Span<byte> Write(int bytes) => _writeBuffer.Write(bytes);

		public void Flush()
		{
			_writeBuffer.Read(out var segment);
			int sentBytes = _socket.Send(segment.Array, segment.Offset, segment.Count, SocketFlags.None);
			_writeBuffer.Pop(sentBytes);
		}

		internal void Read()
		{
			_readBuffer.Write(_socket.Available, out var buffer);
			_socket.Receive(buffer.Array, buffer.Offset, buffer.Count, SocketFlags.None);

			var processedBytes = Read(_readBuffer.Read());
			_readBuffer.Pop(processedBytes);
		}

		internal void Accept()
		{
			OnAccepted(_socket.Accept());
		}
	}
}
