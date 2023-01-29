using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace plane.Graphics;

public static class DebugExtensions
{
    public static unsafe Task SetInfoQueueCallback(this ComPtr<ID3D11Device> device, Action<Message> callback, CancellationToken cancellationToken = default)
    {
        Debug.Assert(callback is not null, "Callback cannot be null");

        ComPtr<ID3D11InfoQueue> infoQueue = default;

        SilkMarshal.ThrowHResult(device.Get().QueryInterface(ref SilkMarshal.GuidOf<ID3D11InfoQueue>(), (void**)infoQueue.GetAddressOf()));

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

                    for (ulong i = 0; i < numMessages; i++)
                    {
                        nuint msgByteLength = default;
                        SilkMarshal.ThrowHResult(infoQueue.Get().GetMessageA(i, null, ref msgByteLength));

                        byte[] msgBytes = new byte[msgByteLength];
                        ref Message msg = ref Unsafe.As<byte, Message>(ref msgBytes[0]);
                        SilkMarshal.ThrowHResult(infoQueue.Get().GetMessageA(i, ref msg, ref msgByteLength));

                        callback(msg);
                    }

                    infoQueue.Get().ClearStoredMessages();
                }
            }
        , cancellationToken);
    }
}
