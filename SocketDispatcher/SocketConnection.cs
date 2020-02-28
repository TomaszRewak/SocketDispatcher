using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SocketDispatcher
{
	public class SocketConnection
	{
		private readonly WinSock _winSock;
		private readonly Socket _socket;
		private readonly int _handle;

		public SocketConnection(string host, int port)
		{
			_winSock = WinSock.Current;

			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.Connect(host, port);

			_handle = 0;

			WinSock.SelectAsync(_socket, _handle, 0, WinSock.NetworkEvents.Read | WinSock.NetworkEvents.Write);

			_socket.Send()

			WSAAsyncSelect(_socket.Handle.ToInt32(), _handle, )


			ComponentDispatcher.ThreadFilterMessage += ThreadFilterMessage;




			var result = socket.ReceiveAsync((new int[10]).AsMemory<int>(), SocketFlags.None);

			if (socket.Poll(100, SelectMode.SelectRead))
				socket.

			ArraySegment<byte> array;

			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.ConnectSocket = socket;
			args.
			args.Completed += Args_Completed;

			socket.ReceiveAsync(args);
		}

		internal IntPtr Handle => _socket.Handle;

		private void ThreadFilterMessage(ref MSG msg, ref bool handled)
		{
			if (msg.)
		}

		private void Args_Completed(object sender, SocketAsyncEventArgs e)
		{
			e.
		}
	}
}
