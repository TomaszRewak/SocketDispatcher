using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Interop;

namespace SocketDispatcher
{
	internal sealed class WinSock
	{
		[ThreadStatic]
		private static WinSock _current;
		public static WinSock Current => _current ?? (_current = new WinSock());

		private IDictionary<IntPtr, SocketConnection> _sockets = new Dictionary<IntPtr, SocketConnection>();

		private WinSock()
		{
			ComponentDispatcher.ThreadFilterMessage += ThreadFilterMessage;
		}

		private void ThreadFilterMessage(ref MSG msg, ref bool handled)
		{
			if (msg.hwnd != IntPtr.Zero) return;
			if (_sockets.TryGetValue(msg.wParam, out var socket)) return;

			var events = (NetworkEvents)(msg.lParam.ToInt32() & 0b1111_1111_1111_1111);
			var errors = msg.lParam.ToInt32() >> 16;

			switch (events)
			{
				case NetworkEvents.Read:
					socket.Read();
					break;
				case NetworkEvents.Write:
					socket.Flush();
					break;
				case NetworkEvents.Accept:
					socket.Accept();
					break;
				case NetworkEvents.Connect:
					socket.Connected = true;
					break;
				case NetworkEvents.Close:
					socket.Connected = false;
					break;
				default: break;
			}
		}

		public void Add(SocketConnection socket)
		{
			_sockets.Add(socket.Handle, socket);
			SelectAsync(socket.Handle, NetworkEvents.Read | NetworkEvents.Write | NetworkEvents.Accept);
		}

		[Flags]
		private enum NetworkEvents
		{
			Read = 0b00_0000_0001,
			Write = 0b00_0000_0010,
			Oob = 0b00_0000_0100,
			Accept = 0b00_0000_1000,
			Connect = 0b00_0001_0000,
			Close = 0b00_0010_0000,
			Qos = 0b00_0100_0000,
			GroupQos = 0b00_1000_0000,
			RoutingInterfaceChange = 0b01_0000_0000,
			AddressListChange = 0b10_0000_0000,
			MaxEvents = 0b11_1111_1111
		}

		[DllImport("wsock32.dll")]
		private static extern int WSAAsyncSelect(IntPtr socket, IntPtr hWnd, int wMsg, int lEvent);
		private static int SelectAsync(IntPtr socketHandle, NetworkEvents events) => WSAAsyncSelect(socketHandle, IntPtr.Zero, 0, (int)events);
	}
}
