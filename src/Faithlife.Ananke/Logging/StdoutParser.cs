using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke.Logging
{
	/// <summary>
	/// A delegate that parses <paramref name="message"/> and logs it to <paramref name="loggerFactory"/>.
	/// </summary>
	/// <param name="message">The message; this is a text that has been written to the console.</param>
	/// <param name="loggerFactory">The logger factory to log to.</param>
	public delegate void StdoutParser(string message, ILoggerFactory loggerFactory);
}
