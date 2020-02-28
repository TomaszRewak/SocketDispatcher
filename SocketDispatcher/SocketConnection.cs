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
		private readonly Socket _socket;
		private readonly int _handle;


		public SocketConnection(string host, int port)
		{
			handle = 

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(host, port);

			WSAAsyncSelect(socket.Handle.ToInt32(), )


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

		private void ThreadFilterMessage(ref MSG msg, ref bool handled)
		{
			if (msg)
		}



		private void Args_Completed(object sender, SocketAsyncEventArgs e)
		{
			e.
		}

		[DllImport("wsock32.dll")]
		public static extern int WSAAsyncSelect(int socket, int hWnd, int wMsg, int lEvent);
	}
}
