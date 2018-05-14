using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke.Logging.Internal
{
	/// <inheritdoc />
	/// <summary>
	/// The logger provider used by Ananke. Sends logs to an <see cref="IStringLog"/>.
	/// </summary>
	internal sealed partial class AnankeLoggerProvider : ILoggerProvider
	{
		/// <summary>
		/// Creates a new logger provider.
		/// </summary>
		/// <param name="stringLog">The underlying string log to which all logs are written. May not be <c>null</c>.</param>
		/// <param name="formatter">The formatter used to translate log events into single-line strings. May not be <c>null</c>.</param>
		/// <param name="filter">The filter for determining which log events to log. May not be <c>null</c>.</param>
		public AnankeLoggerProvider(IStringLog stringLog, LoggerFormatter formatter, LoggerIsEnabledFilter filter)
		{
			if (stringLog == null)
				throw new ArgumentNullException(nameof(stringLog));
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));
			if (filter == null)
				throw new ArgumentNullException(nameof(filter));

			m_stringLog = stringLog;
			m_formatter = formatter;
			m_filter = filter;
			m_loggers = new ConcurrentDictionary<string, ILogger>();
		}

		/// <inheritdoc/>
		public ILogger CreateLogger(string categoryName)
		{
			if (categoryName == null)
				throw new ArgumentNullException(nameof(categoryName));
			return m_loggers.GetOrAdd(categoryName, name => new AnankeLoggerProvider.AnankeLogger(this, name));
		}

		void IDisposable.Dispose() { }

		private void Log(string loggerName, LogLevel logLevel, EventId eventId, string message, Exception exception,
			IEnumerable<KeyValuePair<string, object>> state)
		{
			if (message == "" && exception == null)
				return;
			// TODO: collect scope information and pass along
			var text = m_formatter(loggerName, logLevel, eventId, message, exception, state,
				Enumerable.Empty<IEnumerable<KeyValuePair<string, object>>>(),
				Enumerable.Empty<string>());
			m_stringLog.WriteLine(text);
		}

		private bool IsEnabled(string loggerName, LogLevel logLevel) => m_filter(loggerName, logLevel);

		private IDisposable BeginScope<TState>(TState state)
		{
			// TODO: handle scopes
			return null;
		}

		private readonly IStringLog m_stringLog;
		private readonly LoggerFormatter m_formatter;
		private readonly LoggerIsEnabledFilter m_filter;
		private readonly ConcurrentDictionary<string, ILogger> m_loggers;
	}
}
