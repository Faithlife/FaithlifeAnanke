using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke.Logging.Internal
{
	internal sealed partial class AnankeLoggerProvider
	{
		private sealed class AnankeLogger : ILogger
		{
			public AnankeLogger(Internal.AnankeLoggerProvider provider, string name)
			{
				m_provider = provider;
				m_name = name;
			}

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
				Func<TState, Exception, string> formatter)
			{
				if (!IsEnabled(logLevel))
					return;
				m_provider.Log(m_name, logLevel, eventId, formatter(state, exception) ?? "", exception,
					state as IEnumerable<KeyValuePair<string, object>>);
			}

			public bool IsEnabled(LogLevel logLevel) => m_provider.IsEnabled(m_name, logLevel);

			public IDisposable BeginScope<TState>(TState state) => m_provider.BeginScope(state);

			private readonly Internal.AnankeLoggerProvider m_provider;
			private readonly string m_name;
		}
	}
}
