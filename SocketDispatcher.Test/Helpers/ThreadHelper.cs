using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SocketDispatcher.Test.Helpers
{
	internal static class ThreadHelper
	{
		public static void Await<T>(ref T value) => Await(ref value, TimeSpan.FromSeconds(10));
		public static void Await<T>(ref T value, TimeSpan timeout)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			while (value == default)
			{
				if (stopwatch.Elapsed > timeout) throw new TimeoutException();
				Thread.Sleep(1);
			}
		}

		public static void Await<T>(ref T value, T expected) => Await(ref value, expected, TimeSpan.FromSeconds(10));
		public static void Await<T>(ref T value, T expected, TimeSpan timeout)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			while (!value.Equals(expected))
			{
				if (stopwatch.Elapsed > timeout) throw new TimeoutException();
				Thread.Sleep(1);
			}
		}
	}
}
