using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SocketDispatcher.Test.Helpers
{
	internal static class ThreadHelper
	{
		public static void Await<T>(ref T value) => Await(ref value, TimeSpan.FromSeconds(2));
		public static void Await<T>(ref T value, TimeSpan timeout)
		{
			var stopwatch = new Stopwatch();

			while (value == default)
			{
				if (stopwatch.Elapsed > timeout) throw new TimeoutException();
				Thread.Sleep(1);
			}
		}
	}
}
