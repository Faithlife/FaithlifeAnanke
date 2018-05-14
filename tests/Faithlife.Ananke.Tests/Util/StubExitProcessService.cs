using System;
using System.Collections.Generic;
using System.Text;
using Faithlife.Ananke.Services;

namespace Faithlife.Ananke.Tests.Util
{
	public sealed class StubExitProcessService : IExitProcessService
	{
		public int ExitCode { get; set; }

		public void Exit() { }
	}
}
