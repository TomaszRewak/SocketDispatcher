using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDispatcher
{
	internal sealed class WriteBuffer
	{
		private byte[] _buffer = new byte[0];
		private int _length = 0;
		private int _start = 0;
		private int _end = 0;

		public ref struct Enumerator
		{
			private readonly WriteBuffer _buffer;
			private int _section;

			public Enumerator(WriteBuffer buffer)
			{
				_buffer = buffer;
				_section = -1;
			}

			public bool MoveNext()
			{
				_section++;

				if (_section == 0 && _buffer._end != _buffer._start) return true;
				if (_section == 1 && _buffer._end < _buffer._start) return true;
				return false;
			}

			public ReadOnlySpan<byte> Current
			{
				get
				{
					if (_section == 1) return _buffer._buffer.AsSpan(0, _buffer._end);

					return _buffer._end >= _buffer._start
						? _buffer._buffer.AsSpan(_buffer._start, _buffer._end - _buffer._start)
						: _buffer._buffer.AsSpan(_buffer._start, _buffer._length - _buffer._start);
				}
			}
		}

		public bool Empty => _start == _end;

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

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
}
