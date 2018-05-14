using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke.Logging
{
	/// <summary>
	/// Formats a log event into a single-line string. The resulting string must not have any EOL characters.
	/// </summary>
	/// <param name="loggerName">The name (category) of the logger. May not be <c>null</c>.</param>
	/// <param name="logLevel">The importance of the event.</param>
	/// <param name="eventId">The id of the event, or <c>0</c> if there is no id.</param>
	/// <param name="message">The message. May not be <c>null</c>, but may be the empty string.</param>
	/// <param name="exception">The exception, if any. May be <c>null</c>.</param>
	/// <param name="state">The structured state for the message, if any. May be <c>null</c>.</param>
	/// <param name="scope">The structured scope for the message, if any. May be an empty sequence.</param>
	/// <param name="scopeMessages">The scope for the message (as strings), if any. May be an empty sequence.</param>
	public delegate string LoggerFormatter(string loggerName, LogLevel logLevel, EventId eventId, string message,
		Exception exception,
		IEnumerable<KeyValuePair<string, object>> state, IEnumerable<IEnumerable<KeyValuePair<string, object>>> scope,
		IEnumerable<string> scopeMessages);
}
