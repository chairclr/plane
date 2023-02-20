namespace plane.Diagnostics;

public class ConsoleLogger : Logger
{
    private object ConsoleLock = new object();

    public override void WriteLine(string? message, LogSeverity severity, DateTime time)
    {
        message ??= "[Empty Log]";

        lock (ConsoleLock)
        {
            switch (severity)
            {
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine($"[{time:mm:ss:ffff}] {message}");

            Console.ResetColor();
        }
    }
}