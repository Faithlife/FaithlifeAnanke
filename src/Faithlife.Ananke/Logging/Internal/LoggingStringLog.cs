using Microsoft.Extensions.Logging;

namespace Faithlife.Ananke.Logging.Internal
{
	/// <summary>
	/// A string log that writes to an <see cref="ILogger"/>.
	/// </summary>
	internal sealed class LoggingStringLog : IStringLog
	{
		/// <summary>
		/// Creates a string log that writes to loggers provided by the given logger provider.
		/// </summary>
		/// <param name="loggerFactory">The logger provider.</param>
		/// <param name="stdoutParser">The parser to determine how to log messages.</param>
		public LoggingStringLog(ILoggerFactory loggerFactory, StdoutParser stdoutParser)
		{
			m_loggerFactory = loggerFactory;
			m_stdoutParser = stdoutParser;
		}

		/// <inheritdoc/>
		public void WriteLine(string message) => m_stdoutParser(message, m_loggerFactory);

		private readonly ILoggerFactory m_loggerFactory;
		private readonly StdoutParser m_stdoutParser;
	}
}
