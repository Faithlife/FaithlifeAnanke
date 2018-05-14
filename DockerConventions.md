# Doker Conventions

## Exit Codes

An Ananke process will return one of the following exit codes:

* `0` - If the application logic returns without exception.
* `64` - If the application logic directly threw an unhandled exception. Unhandled exceptions are logged before the process exits.
* `65` - If the application logic indirectly threw an unhandled exception (e.g., from a thread pool thread). Unhandled exceptions are logged before the process exits.
* `66` - If the application logic was requested to shutdown, but did not do so within the exit timeout (see `AnankeSettings.ExitTimeout`).
* (other) - If the application logic returns an `int`, then that value is used as the process exit code.

Exit codes are returned by Ananke even if you use `static void Main` as your entrypoint.

## Signals

Ananke responds to `Ctrl-C` (if your container is run interactively) as well as `docker stop` on all platforms and container types. Note that for `docker stop` to work with Windows containers, they must have a base image *and* host running Windows Version `1709` or higher.

### Signal Specifics

Ananke listens to various signals based on OS:
* Windows: `CTRL_C_EVENT`, `CTRL_CLOSE_EVENT`, and `CTRL_SHUTDOWN_EVENT`
* Other: `SIGINT` (`Ctrl-C`) and `SIGTERM`

These are all treated the same: as a graceful stop request. When one of these signals is received, the `AnankeContext.ExitRequested` cancellation token is cancelled. When this token is cancelled, your code should stop taking on new work. It should complete the work it already has and then exit.

When a signal comes in, Ananke will start a kill timer (see `AnankeSettings.ExitTimeout`). If the application code has not returned within that timeout, Ananke will exit the process with exit code `66`.

## Logs

Docker expects logs to be written to stdout (or stderr), with *one line per log message*.

Ananke has a core logging factory, exposed at `AnankeContext.LoggingFactory`. All of Ananke's logs go through this factory (using the `"Ananke"` category/logger name), and this same factory can be used to create application logs.

By default, all log messages sent to `AnankeContext.LoggingFactory` are formatted on a single line using backslash-escaping. These lines are then written to stdout.

### Redirecting Ananke Logs

Docker applications that deliberately do their own logging directly will want to redirect Ananke's logging. This is done by setting `AnankeSettings.LoggingFactory` before calling into Ananke. Ananke will then use the provided `ILoggerFactory` instead of its own factory and provider.

```C#
static void Main(string[] args)
{
	var myLoggerFactory = new LoggerFactory();
	myLoggerFactory.AddMyOwnProvider(); // log4net, seq, gelf, whatever...

	var anankeSettings = AnankeSettings.Create(maximumRuntime: TimeSpan.FromHours(2), loggerFactory: myLoggerFactory);
 	AnankeRunner.Main(anankeSettings, context =>
	{
		var loggerFactory = context.LoggerFactory; // Same instance as `myLoggerFactory` that we passed into the settings.
	});
}
```

This way you can send Ananke's own logs to your customized logging provider instead of as stdout to Docker.
