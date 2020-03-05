﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SocketDispatcher.Test.Connections
{
	internal abstract class BaseConnection : SocketConnection
	{
		protected byte? MessageTerminator;

		public bool Connected;
		public bool ConnectionFailed;
		public byte[] Buffer = new byte[0];
		public byte[] LastMessage = new byte[0];

		protected BaseConnection() : base() { }
		protected BaseConnection(Socket socket) : base(socket) { }

		public void Dispose()
		{
			if (!Connected) return;

			Disconnect();
		}

		public BaseConnection ThatAcceptsMessages(byte messageTerminator)
		{
			MessageTerminator = messageTerminator;
			return this;
		}

		public void Send(params byte[] data)
		{
			var buffer = Write(data.Length);
			data.CopyTo(buffer);
			Flush();
		}

		protected override int Read(ReadOnlySpan<byte> data)
		{
			var buffer = data;
			if (MessageTerminator.HasValue)
			{
				while (buffer.Contains(MessageTerminator.Value))
				{
					int length = buffer.IndexOf(MessageTerminator.Value) + 1;
					LastMessage = buffer.Slice(0, length).ToArray();
					buffer = buffer.Slice(length);
				}
			}
			Buffer = buffer.ToArray();
			return data.Length - buffer.Length;
		}

		protected override void OnConnected() => Connected = true;
		protected override void OnDisconnected() => Connected = false;
		protected override void OnConnectionFailed() => ConnectionFailed = true;
	}
}
