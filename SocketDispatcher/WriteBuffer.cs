using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDispatcher
{
	internal sealed class WriteBuffer
	{
		private readonly List<ArraySegment<byte>> _segments = new List<ArraySegment<byte>> { new ArraySegment<byte>(), new ArraySegment<byte>() };

		private byte[] _buffer = new byte[0];
		private int _length = 0;
		private int _start = 0;
		private int _end = 0;

		public bool Empty => _start == _end;

		public List<ArraySegment<byte>> Read()
		{
			if (_end >= _start)
			{
				_segments[0] = new ArraySegment<byte>(_buffer, _start, _end - _start);
				_segments[1] = new ArraySegment<byte>();
			}
			else
			{
				_segments[0] = new ArraySegment<byte>(_buffer, _end, _length - _end);
				_segments[1] = new ArraySegment<byte>(_buffer, 0, _end);
			}

			return _segments;
		}

		public Span<byte> Write(int bytes)
		{
			if (_end >= _start && _end + bytes < _length)
			{
				var span = _buffer.AsSpan(_end, bytes);
				_end += bytes;
				return span;
			}

			if (_end >= _start && bytes < _start)
			{
				var span = _buffer.AsSpan(0, bytes);
				_length = _end;
				_end = bytes;
				return span;
			}

			if (_end < _start && _end + bytes < _start)
			{
				var span = _buffer.AsSpan(_end, bytes);
				_end += bytes;
				return span;
			}

			var newBuffer = new byte[(_buffer.Length + bytes) * 2];

			if (_end >= _start)
			{
				Buffer.BlockCopy(_buffer, _start, newBuffer, 0, _end - _start);
				_end = _end - _start + bytes;
			}
			else
			{
				Buffer.BlockCopy(_buffer, _start, newBuffer, 0, _length - _start);
				Buffer.BlockCopy(_buffer, 0, newBuffer, _length - _start, _end);
				_end = _end + _length - _start + bytes;
			}

			_start = 0;
			_length = newBuffer.Length;
			_buffer = newBuffer;

			return _buffer.AsSpan(0, bytes);
		}

		public void Pop(int bytes)
		{
			if (_start >= _end)
			{
				_start += bytes;
				return;
			}

			if (bytes < _length - _start)
			{
				_start += bytes;
				return;
			}

			_start = bytes - (_length - _start);
		}
	}
}
