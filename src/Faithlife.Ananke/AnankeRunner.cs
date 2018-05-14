using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faithlife.Ananke.Logging.Internal;
using Faithlife.Ananke.Services;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke
{
	/// <summary>
	/// The Ananke wrapper around your code. Normally, you should call the static <see cref="O:Faithlife.Ananke.AnankeRunner.Main"/> method rather than create instances of this type directly.
	/// </summary>
	public sealed class AnankeRunner
	{
		/// <summary>
		/// Creates an Ananke wrapper and executes the application logic within that wrapper.
		/// </summary>
		/// <param name="settings">The settings to use for the Ananke wrapper.</param>
		/// <param name="action">The application logic to execute.</param>
		public static int Main(AnankeSettings settings, Func<AnankeContext, Task<int>> action)
		{
			var runner = new AnankeRunner(settings);
			return runner.Run(action);
		}

		/// <summary>
		/// Creates an Ananke wrapper and executes the application logic within that wrapper.
		/// </summary>
		/// <param name="settings">The settings to use for the Ananke wrapper.</param>
		/// <param name="action">The application logic to execute.</param>
		public static int Main(AnankeSettings settings, Func<AnankeContext, Task> action) => Main(settings, async context =>
		{
			await action(context).ConfigureAwait(false);
			return c_successExitCode;
		});

#pragma warning disable 1998
		/// <summary>
		/// Creates an Ananke wrapper and executes the application logic within that wrapper.
		/// </summary>
		/// <param name="settings">The settings to use for the Ananke wrapper.</param>
		/// <param name="action">The application logic to execute.</param>
		public static int Main(AnankeSettings settings, Func<AnankeContext, int> action) =>
			Main(settings, async context => action(context));
#pragma warning restore

		/// <summary>
		/// Creates an Ananke wrapper and executes the application logic within that wrapper.
		/// </summary>
		/// <param name="settings">The settings to use for the Ananke wrapper.</param>
		/// <param name="action">The application logic to execute.</param>
		public static int Main(AnankeSettings settings, Action<AnankeContext> action) => Main(settings, context =>
		{
			action(context);
			return c_successExitCode;
		});

		/// <summary>
		/// Creates an Ananke wrapper with the specified settings.
		/// </summary>
		/// <param name="settings">The settings to use.</param>
		private AnankeRunner(AnankeSettings settings)
		{
			m_settings = settings;
			m_log = m_settings.LoggerFactory.CreateLogger("Ananke");
			m_exitRequested = new CancellationTokenSource();
			var loggingConsoleStdout =
				new StringLogTextWriter(new LoggingStringLog(m_settings.LoggerFactory, m_settings.StdoutParser));
			m_context = new AnankeContext(m_settings.LoggerFactory, m_exitRequested.Token, loggingConsoleStdout);
			m_done = new ManualResetEventSlim();
			m_exitCodeMutex = new object();
		}

		/// <summary>
		/// Executes application logic within this wrapper.
		/// </summary>
		/// <param name="action">The application logic to execute.</param>
		private int Run(Func<AnankeContext, Task<int>> action)
		{
			try
			{
				var hostname = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
				m_log.Starting(hostname);

				// Hook signals.
				m_settings.SignalService.AddHandler(signalName =>
				{
					Shutdown(() => m_log.ShutdownSignalReceived(signalName));
					m_done.Wait();
				});

				// Log any unhandled exceptions from the thread pool.
				AppDomain.CurrentDomain.UnhandledException += (_, args) =>
				{
					m_log.UnhandledAppDomainException(args.ExceptionObject as Exception);
					SetExitCode(c_unhandledAppDomainExceptionExitCode);
				};

				// Exit after our maximum runtime.
				ShutdownAfterMaximumRuntime();

				int exitCode;
				using (m_log.BeginScope("Host {hostname}", hostname))
					exitCode = action(m_context).GetAwaiter().GetResult();
				SetExitCode(exitCode);
				return exitCode;
			}
			catch (OperationCanceledException) when (m_exitRequested.IsCancellationRequested)
			{
				m_log.IgnoringOperationCanceledException();
				return c_successExitCode;
			}
			catch (Exception ex)
			{
				m_log.UnhandledApplicationException(ex);
				SetExitCode(c_unhandledApplicationExceptionExitCode);
				return c_unhandledApplicationExceptionExitCode;
			}
			finally
			{
				m_done.Set();
			}
		}

		/// <summary>
		/// Sets the exit code for the process if it has not already been set. If the exit code has already been set, then this method does nothing.
		/// </summary>
		/// <param name="exitCode">The exit code.</param>
		private void SetExitCode(int exitCode)
		{
			lock (m_exitCodeMutex)
			{
				if (m_settings.ExitProcessService.ExitCode == 0)
					m_settings.ExitProcessService.ExitCode = exitCode;
			}
		}

		/// <summary>
		/// Initiates an automatic shutdown after the application has been running for its maximum runtime.
		/// </summary>
		private async void ShutdownAfterMaximumRuntime()
		{
			if (m_settings.MaximumRuntime == Timeout.InfiniteTimeSpan)
				return;

			// Determine the actual maximum runtime
			var delta = m_settings.RandomMaximumRuntimeRelativeDelta * m_settings.MaximumRuntime.Ticks;
			var maxValue = m_settings.MaximumRuntime.Ticks + delta;
			var minValue = m_settings.MaximumRuntime.Ticks - delta;
			var randomizedValue = new Random().NextDouble() * (maxValue - minValue) + minValue;
			var shutdownAfter = TimeSpan.FromTicks(unchecked((long) randomizedValue));

			m_log.MaximumRuntime(shutdownAfter);
			await Task.Delay(shutdownAfter);
			Shutdown(() => m_log.MaximumRuntimeReached(shutdownAfter));
		}

		/// <summary>
		/// Initiates a shutdown. Sets <see cref="AnankeContext.ExitRequested"/> and starts the exit timeout.
		/// </summary>
		/// <param name="log">A delegate that writes to the console log.</param>
		private void Shutdown(Action log)
		{
			if (!m_exitRequested.IsCancellationRequested)
				log();

			ExitAfterTimeout();
			m_exitRequested.Cancel();
		}

		/// <summary>
		/// Waits for <see cref="AnankeSettings.ExitTimeout"/> and then sets <see cref="m_done"/> and calls <see cref="IExitProcessService.Exit"/>.
		/// This method is always asynchronous.
		/// </summary>
		private async void ExitAfterTimeout()
		{
			// If ExitTimeout is zero, we want to force asynchrony so that ExitAfterTimeout returns before calling ExitPocessService.Exit.
			await Task.Yield();

			await Task.Delay(m_settings.ExitTimeout);
			m_done.Set();
			SetExitCode(c_exitTimeoutExitCode);
			m_settings.ExitProcessService.Exit();
		}

		private readonly AnankeSettings m_settings;
		private readonly ILogger m_log;
		private readonly AnankeContext m_context;
		private readonly CancellationTokenSource m_exitRequested;
		private readonly ManualResetEventSlim m_done;
		private readonly object m_exitCodeMutex;

		private const int c_successExitCode = 0;
		private const int c_unhandledApplicationExceptionExitCode = 64;
		private const int c_unhandledAppDomainExceptionExitCode = 65;
		private const int c_exitTimeoutExitCode = 66;
	}
}
