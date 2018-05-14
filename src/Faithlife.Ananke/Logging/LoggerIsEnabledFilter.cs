using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke.Logging
{
	/// <summary>
	/// Determines whether a log event is enabled. Any events for which this method returns <c>false</c> are not logged.
	/// </summary>
	/// <param name="loggerName">The name (category) of the logger. May not be <c>null</c>.</param>
	/// <param name="logLevel">The importance of the event.</param>
	public delegate bool LoggerIsEnabledFilter(string loggerName, LogLevel logLevel);
}
