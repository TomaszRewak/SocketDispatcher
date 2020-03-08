using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDispatcher.Test
{
	[TestFixture]
	public partial class SocketBufferTest
	{
		[Test]
		public void DataIsWrittenIntoBuffer()
		{
			Write(1, 2, 3);

			AssertData(1, 2, 3);
		}

		[Test]
		public void NewDataIsAppendedToExistingData()
		{
			Write(1, 2, 3);
			Write(4, 5, 6);

			AssertData(1, 2, 3, 4, 5, 6);
		}

		[Test]
		public void DataIsPopped()
		{
			Write(1, 2, 3, 4, 5);
			Pop(3);

			AssertData(4, 5);
		}

		[Test]
		public void NewDataIsAppendedToRemainingDataAfterPopping()
		{
			Write(1, 2, 3, 4, 5);
			Pop(3);
			Write(6, 7);

			AssertData(4, 5, 6, 7);
		}

		[Test]
		public void BufferCanBeCleared()
		{
			Write(1, 2, 3, 4);
			Pop(4);

			AssertData();
		}

		[Test]
		public void DataCanBeAddedToEmptiedBuffer()
		{
			Write(1, 2, 3, 4);
			Pop(4);
			Write(5, 6);

			AssertData(5, 6);
		}
	}
}
