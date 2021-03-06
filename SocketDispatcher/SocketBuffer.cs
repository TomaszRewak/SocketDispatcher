﻿using System;

namespace SocketDispatcher
{
	internal sealed class SocketBuffer
	{
		private byte[] _buffer = new byte[0];
		private int _start = 0;
		private int _length = 0;

		internal void Write(int bytes, out ArraySegment<byte> segment)
		{
			Reserve(bytes);

			segment = new ArraySegment<byte>(_buffer, _start + _length, bytes);
			_length += bytes;
		}

		public Span<byte> Write(int bytes)
		{
			Write(bytes, out var segment);
			return segment.AsSpan();
		}

		internal void Read(out ArraySegment<byte> segment)
		{
			segment = new ArraySegment<byte>(_buffer, _start, _length);
		}

		public ReadOnlySpan<byte> Read()
		{
			Read(out var segment);
			return segment.AsSpan();
		}

		public void Pop(int bytes)
		{
			_start += bytes;
			_length -= bytes;
		}

		private void Reserve(int bytes)
		{
			if (_length == 0) _start = 0;
			if (_buffer.Length >= _start + _length + bytes) return;

			if (_buffer.Length >= _length + bytes)
				Buffer.BlockCopy(_buffer, _start, _buffer, 0, _length);
			else
			{
				var newBuffer = new byte[(_length + bytes) * 2];
				Buffer.BlockCopy(_buffer, _start, newBuffer, 0, _length);
				_buffer = newBuffer;
			}

			_start = 0;
		}
	}
}
