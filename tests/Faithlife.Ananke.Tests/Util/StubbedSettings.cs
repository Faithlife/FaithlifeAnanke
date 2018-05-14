using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Faithlife.Ananke.Logging;
using Faithlife.Ananke.Services;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke.Tests.Util
{
	public sealed class StubbedSettings
	{
		public TimeSpan StubMaximumRuntime { get; set; } = Timeout.InfiniteTimeSpan;

		public TimeSpan StubExitTimeout { get; set; } = TimeSpan.FromSeconds(1);

		public StubLoggerProvider StubLoggerProvider { get; set; } = null;

		public LoggerFormatter Formatter { get; set; } = null;

		public StubStringLog StubStringLog { get; } = new StubStringLog(new StringWriter());

		public StubExitProcessService StubExitProcessService { get; } = new StubExitProcessService();

		public StubSignalService StubSignalService { get; } = new StubSignalService();

		public static implicit operator AnankeSettings(StubbedSettings stubs)
		{
			ILoggerFactory loggerFactory = null;
			if (stubs.StubLoggerProvider != null)
			{
				loggerFactory = new LoggerFactory();
				loggerFactory.AddProvider(stubs.StubLoggerProvider);
			}

			return AnankeSettings.InternalCreate(maximumRuntime: stubs.StubMaximumRuntime, exitTimeout: stubs.StubExitTimeout,
				consoleLog: stubs.StubStringLog, loggerFormatter: stubs.Formatter, exitProcessService: stubs.StubExitProcessService,
				signalService: stubs.StubSignalService, loggerFactory: loggerFactory);
		}
	}
}
