using System;
using System.Collections.Generic;
using System.Text;

namespace Faithlife.Ananke.Services
{
	/// <inheritdoc/>
	internal sealed class ExitProcessService : IExitProcessService
	{
		/// <inheritdoc/>
		public int ExitCode
		{
			get => Environment.ExitCode;
			set => Environment.ExitCode = value;
		}

		/// <inheritdoc/>
		public void Exit() => Environment.Exit(ExitCode);
	}
}
