using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDispatcher
{
	internal sealed class ReadBuffer
	{
		private byte[] _buffer = new byte[0];
		private int _length = 0;

		public Span<byte> Push(int bytes)
		{
			Reserve(bytes);

			_length += bytes;

			return _buffer.AsSpan(_length - bytes, bytes);
		}

		public ReadOnlySpan<byte> Read()
		{
			return _buffer.AsSpan(0, _length);
		}

		public void Pop(int bytes)
		{
			if (bytes < _length)
				Buffer.BlockCopy(_buffer, bytes, _buffer, 0, _length - bytes);

			_length -= bytes;
		}

		private void Reserve(int bytes)
		{
			if (_buffer.Length >= _length + bytes) return;

			var newBuffer = new byte[(_length + bytes) * 2];
			Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _length);

			_buffer = newBuffer;
		}
	}
}
