using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SocketDispatcher
{
	internal sealed class WinSock
	{
		[ThreadStatic]
		private static WinSock _current;
		public static WinSock Current => _current ?? (_current = new WinSock());

		private WindowInteropHelper _messageHendler;
		private IDictionary<IntPtr, SocketConnection> _sockets = new Dictionary<IntPtr, SocketConnection>();

		private WinSock()
		{
			ComponentDispatcher.ThreadFilterMessage += ThreadFilterMessage;

			_messageHendler = new WindowInteropHelper(new Window());
			_messageHendler.EnsureHandle();
		}

		private void ThreadFilterMessage(ref MSG msg, ref bool handled)
		{
			if (msg.hwnd != _messageHendler.Handle) return;
			if (msg.message != MessageMagic) return;
			if (!_sockets.TryGetValue(msg.wParam, out var socket)) return;

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
				case NetworkEvents.Close:
					socket.OnDisconnected();
					break;
				default: break;
			}

			handled = true;
		}

		public void Add(SocketConnection socket)
		{
			_sockets.Add(socket.Handle, socket);
			var result = SelectAsync(socket.Handle, NetworkEvents.Read | NetworkEvents.Write | NetworkEvents.Accept);

			if (result != 0)
			{
				throw new SocketException(WSAGetLastError());
			}
		}

		private const int MessageMagic = 12345;

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
			AddressListChange = 0b10_0000_0000
		}


		[DllImport("wsock32.dll")]
		private static extern int WSAGetLastError();
		[DllImport("wsock32.dll")]
		private static extern int WSAAsyncSelect(IntPtr socket, IntPtr hWnd, int wMsg, int lEvent);

		private int SelectAsync(IntPtr socketHandle, NetworkEvents events) => WSAAsyncSelect(socketHandle, _messageHendler.Handle, MessageMagic, (int)events);
	}
}
