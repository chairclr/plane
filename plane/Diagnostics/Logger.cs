using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace plane.Diagnostics;

public abstract class Logger
{
    public static Logger Log { get; set; } = new ConsoleLogger();

    public abstract void WriteLine(string? message, LogSeverity severity, DateTime time);
}
