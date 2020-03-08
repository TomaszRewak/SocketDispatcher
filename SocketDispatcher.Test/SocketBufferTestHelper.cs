using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDispatcher.Test
{
	public partial class SocketBufferTest
	{
		private SocketBuffer _buffer;

		[SetUp]
		public void SetUp()
		{
			_buffer = new SocketBuffer();
		}

		public void Write(params byte[] data)
		{
			data.CopyTo(_buffer.Write(data.Length));
		}

		public void AssertData(params byte[] data)
		{
			CollectionAssert.AreEqual(data, _buffer.Read().ToArray());
		}

		public void Pop(int length)
		{
			_buffer.Pop(length);
		}
	}
}
