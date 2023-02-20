using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Diagnostics;

public class D3DDebugMessage
{
    public readonly string? Description;

    public readonly MessageSeverity Severity;

    public readonly LogSeverity LogSeverity;

    public readonly MessageCategory Category;

    public readonly MessageID ID;

    public unsafe D3DDebugMessage(in Message message)
    {
        Description = SilkMarshal.PtrToString((nint)message.PDescription);
        Severity = message.Severity;
        Category = message.Category;
        ID = message.ID;

        LogSeverity = Severity switch
        {
            MessageSeverity.None => LogSeverity.Info,
            MessageSeverity.Info => LogSeverity.Info,
            MessageSeverity.Message => LogSeverity.Info,
            MessageSeverity.Warning => LogSeverity.Warning,
            _ => LogSeverity.Error
        };
    }
}