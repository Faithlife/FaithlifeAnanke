using System;
using System.Threading;
using System.Threading.Tasks;
using Faithlife.Ananke.Tests.Util;
using NUnit.Framework;

namespace Faithlife.Ananke.Tests
{
	public class MaximumRuntimeTests
	{
		[Test]
		public void ApplicationCode_ExitsWithinMaximumRuntime_ShutdownIsNotRequested()
		{
			var settings = new StubbedSettings
			{
				StubExitTimeout = Timeout.InfiniteTimeSpan,
				StubMaximumRuntime = TimeSpan.FromSeconds(1),
			};

			CancellationToken exitRequested;
			AnankeRunner.Main(settings, async context =>
			{
				exitRequested = context.ExitRequested;
				await Task.Delay(settings.StubMaximumRuntime / 2, context.ExitRequested);
			});

			AnankeSettings.Create(maximumRuntime: TimeSpan.FromHours(2));

			Assert.That(exitRequested.IsCancellationRequested, Is.False);
			Assert.That(settings.StubExitProcessService.ExitCode, Is.Zero);
		}

		[Test]
		public void ApplicationCode_ExceedsMaximumRuntime_ShutdownIsRequested()
		{
			var settings = new StubbedSettings
			{
				StubExitTimeout = Timeout.InfiniteTimeSpan,
				StubMaximumRuntime = TimeSpan.FromSeconds(1),
			};

			CancellationToken exitRequested;
			AnankeRunner.Main(settings, async context =>
			{
				exitRequested = context.ExitRequested;
				await Task.Delay(settings.StubMaximumRuntime * 2, context.ExitRequested);
			});

			Assert.That(exitRequested.IsCancellationRequested, Is.True);
			Assert.That(settings.StubExitProcessService.ExitCode, Is.Zero);
		}
	}
}
