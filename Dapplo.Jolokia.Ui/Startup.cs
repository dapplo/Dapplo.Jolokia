using System;
using System.Windows;
using Dapplo.CaliburnMicro;
using Dapplo.Log;
using Dapplo.Log.Loggers;

namespace Dapplo.Jolokia.Ui
{
	/// <summary>
	/// Class for the application start
	/// </summary>
	public static class Startup
	{
		/// <summary>
		/// Entry point for the application
		/// </summary>
		[STAThread]
		public static void Main()
		{
			var dapplication = new Dapplication("Jolokia", "1392C220-45DA-468D-BA32-53B93D9F6E70")
			{
				ShutdownMode = ShutdownMode.OnMainWindowClose,
				// Don't allow the application to run multiple times
				OnAlreadyRunning = () =>
				{
					MessageBox.Show("Already running, this process exits", "Jolokia", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					Dapplication.Current.Shutdown();
				},
				ObserveUnhandledTaskException = true,
				OnUnhandledTaskException = exception => new LogSource().Error().WriteLine(exception.Message)
			};
			LogSettings.RegisterDefaultLogger<DebugLogger>(LogLevels.Verbose);

			// Add some DLL's we need
			dapplication.Bootstrapper.FindAndLoadAssemblies("Dapplo*");

			// Let Dapplo initialize everything, including the web-app
			dapplication.Run();
		}
	}
}
