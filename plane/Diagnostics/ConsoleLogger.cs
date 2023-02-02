using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Assimp;

namespace plane.Diagnostics;
public class ConsoleLogger : Logger
{
    public override void WriteLine(string? message, LogSeverity severity, DateTime time)
    {
        message ??= "[Empty Log]";

        switch (severity)
        {
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
        }

        Console.WriteLine($"[{time:HH:mm:ss:FFF}] {message}");

        Console.ResetColor();
    }
}
