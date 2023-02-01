using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Diagnostics;

public static class DebugExtensions
{
    private static readonly HashSet<(ComPtr<ID3D11InfoQueue>, Action<Direct3D11Message>, object)> PinnedInfoQueues = new HashSet<(ComPtr<ID3D11InfoQueue>, Action<Direct3D11Message>, object)>();

    public static unsafe Task SetInfoQueueCallback(this ComPtr<ID3D11Device> device, Action<Direct3D11Message> callback, CancellationToken cancellationToken = default)
    {
        Debug.Assert(callback is not null, "Callback cannot be null");

        SilkMarshal.ThrowHResult(device.QueryInterface(out ComPtr<ID3D11InfoQueue> infoQueue));

        object infoQueueLock = new object();

        PinnedInfoQueues.Add((infoQueue, callback, infoQueueLock));

        return Task.Run
        (
            () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ulong numMessages = infoQueue.Get().GetNumStoredMessages();

                    if (numMessages == 0)
                    {
                        Thread.Sleep(25);
                        continue;
                    }

                    lock (infoQueueLock)
                    {
                        for (ulong i = 0; i < numMessages; i++)
                        {
                            nuint msgByteLength = 0;
                            SilkMarshal.ThrowHResult(infoQueue.GetMessageA(i, null, ref msgByteLength));

                            byte[] msgBytes = new byte[msgByteLength];
                            ref Message msg = ref Unsafe.As<byte, Message>(ref msgBytes[0]);
                            SilkMarshal.ThrowHResult(infoQueue.GetMessageA(i, ref msg, ref msgByteLength));

                            callback(new Direct3D11Message(msg));
                        }

                        infoQueue.ClearStoredMessages();
                    }
                }

                PinnedInfoQueues.Remove((infoQueue, callback, infoQueueLock));

                infoQueue.Dispose();
            }
        , cancellationToken);
    }

    static unsafe DebugExtensions()
    {
        // Ensure all info queues are flushed when an exception occurs
        AppDomain.CurrentDomain.UnhandledException += (e, x) =>
        {
            foreach ((ComPtr<ID3D11InfoQueue> infoQueue, Action<Direct3D11Message> callback, object infoQueueLock) in PinnedInfoQueues)
            {
                ulong numMessages = infoQueue.GetNumStoredMessages();

                if (numMessages == 0)
                {
                    continue;
                }

                lock (infoQueueLock)
                {
                    for (ulong i = 0; i < numMessages; i++)
                    {
                        nuint msgByteLength = 0;
                        SilkMarshal.ThrowHResult(infoQueue.GetMessageA(i, null, ref msgByteLength));

                        byte[] msgBytes = new byte[msgByteLength];
                        ref Message msg = ref Unsafe.As<byte, Message>(ref msgBytes[0]);
                        SilkMarshal.ThrowHResult(infoQueue.GetMessageA(i, ref msg, ref msgByteLength));

                        callback(new Direct3D11Message(msg));
                    }

                    infoQueue.ClearStoredMessages();
                }
            }
        };
    }
}