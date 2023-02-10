using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plane.Diagnostics;

public abstract class Logger
{
    public static Logger Log { get; set; } = new ConsoleLogger();

    public static void WriteLine(string? message) => Log.WriteLine(message, LogSeverity.Info, DateTime.Now);

    public static void WriteLine(string? message, LogSeverity severity) => Log.WriteLine(message, severity, DateTime.Now);

    public abstract void WriteLine(string? message, LogSeverity severity, DateTime time);
}