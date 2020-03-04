using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace SocketDispatcher.Test.Helpers
{
	internal static class DispatcherHelper
	{
		public static Dispatcher Spawn()
		{
			Dispatcher dispatcher = null;

			var thread = new Thread(() =>
			{
				dispatcher = Dispatcher.CurrentDispatcher;
				Dispatcher.Run();
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

			ThreadHelper.Await(ref dispatcher);

			return dispatcher;
		}
	}
}
