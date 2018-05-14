### Intercepting Console Stdout and Stderr

Ananke does *not* intercept any application-level logs by default. Any direct console output from application logic is passed straight through. It is possible to intercept console output as such:

```C#
static void Main(string[] args) => AnankeRunner.Main(AnankeSettings.Create(), context =>
{
	Console.SetOut(context.LoggingConsoleStdout);
	Console.SetError(context.LoggingConsoleStdout);

	Console.WriteLine("Hello\nWorld!"); // Formatted and written to stdout as one line, not two
});
```

Please note that intercepted console outputs *require* the use of `WriteLine`. Code such as `Console.Write("Hello World!\n")` will write `Hello World!\\n`, not `Hello World!\n`, and will be interpreted as a log message that has not yet completed.

Redirected console output using `AnankeContext.LoggingConsoleStdout` captures all writes, and when `WriteLine` is invoked, it sends the log string to `AnankeContext.LoggingFactory`. If you specify a custom `AnankeSettings.LoggingFactory`, then you can use this technique to redirect all `Console.WriteLine` calls to your own logging provider.
