using System;
using System.Collections.Generic;
using System.Text;
using Faithlife.Ananke.Logging;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke
{
	/// <summary>
	/// Log formatters.
	/// </summary>
	internal static class AnankeFormatters
	{
		/// <summary>
		/// A formatter that formats logs messages as formatted plain-text.
		/// </summary>
		public static string FormattedText(LogEvent logEvent)
		{
			var sb = new StringBuilder();
			sb.Append(FormattedTextLogLevel(logEvent.LogLevel));
			sb.Append(Escaping.BackslashEscape(logEvent.LoggerName));
			if (logEvent.EventId.Id != 0)
				sb.Append("(" + logEvent.EventId.Id + ")");
			sb.Append(": ");
			foreach (var scopeMessage in logEvent.ScopeMessages)
				sb.Append(Escaping.BackslashEscape(scopeMessage) + ": ");
			if (logEvent.Message != "")
				sb.Append(Escaping.BackslashEscape(logEvent.Message));
			if (logEvent.Exception != null)
			{
				sb.Append(": ");
				sb.Append(Escaping.BackslashEscape(logEvent.Exception.ToString()));
			}

			return sb.ToString();
		}

		private static string FormattedTextLogLevel(LogLevel logLevel)
		{
			switch (logLevel)
			{
			case LogLevel.Trace:
				return "T ";
			case LogLevel.Debug:
				return "D ";
			case LogLevel.Information:
				return "I ";
			case LogLevel.Warning:
				return "W ";
			case LogLevel.Error:
				return "E ";
			case LogLevel.Critical:
				return "C ";
			}

			throw new InvalidOperationException($"Unknown LogLevel {logLevel}");
		}
	}
}
