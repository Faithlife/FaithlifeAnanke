using System;
using System.Collections.Generic;
using System.Linq;
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
			foreach (var scopeMessage in logEvent.Scope.Where(ScopeStateHasStringRepresentation))
				sb.Append(Escaping.BackslashEscape(scopeMessage.ToString()) + ": ");
			if (logEvent.Message != "")
				sb.Append(Escaping.BackslashEscape(logEvent.Message));
			if (logEvent.Exception != null)
			{
				sb.Append(": ");
				sb.Append(Escaping.BackslashEscape(logEvent.Exception.ToString()));
			}

			return sb.ToString();
		}

		private static bool ScopeStateHasStringRepresentation(object state)
		{
			// Follow the same logic as Seq: https://nblumhardt.com/2016/11/ilogger-beginscope/
			if (!(state is IEnumerable<KeyValuePair<string, object>> data))
				return true;
			return data.Any(x => x.Key == "{OriginalFormat}");
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
