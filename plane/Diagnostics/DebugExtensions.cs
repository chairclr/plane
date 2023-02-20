using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace plane.Diagnostics;

public static class DebugExtensions
{
    private static readonly HashSet<(ComPtr<ID3D11InfoQueue>, Action<D3DDebugMessage>, object)> PinnedInfoQueues = new HashSet<(ComPtr<ID3D11InfoQueue>, Action<D3DDebugMessage>, object)>();

    private static Guid _D3DDebugObjectName = new Guid(0x429b8c22, 0x9188, 0x4b0c, 0x87, 0x42, 0xac, 0xb0, 0xbf, 0x85, 0xc2, 0x00);

    public static ref Guid D3DDebugObjectName => ref _D3DDebugObjectName;

    public static unsafe Task SetInfoQueueCallback<T>(this ComPtr<T> device, Action<D3DDebugMessage> callback, CancellationToken cancellationToken = default)
        where T : unmanaged, IComVtbl<ID3D11Device>, IComVtbl<T>
    {
        Debug.Assert(callback is not null, "Callback cannot be null");

        SilkMarshal.ThrowHResult(((ID3D11Device*)device.Handle)->QueryInterface(out ComPtr<ID3D11InfoQueue> infoQueue));

        infoQueue.ClearStorageFilter();

        object infoQueueLock = new object();

        PinnedInfoQueues.Add((infoQueue, callback, infoQueueLock));

        return Task.Run
        (
            () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {

                    if (infoQueue.GetNumStoredMessages() == 0)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    lock (infoQueueLock)
                    {
                        for (ulong i = 0; i < infoQueue.GetNumStoredMessages(); i++)
                        {
                            nuint msgByteLength = 0;
                            SilkMarshal.ThrowHResult(infoQueue.GetMessageA(i, null, ref msgByteLength));

                            byte[] msgBytes = new byte[msgByteLength];
                            ref Message msg = ref Unsafe.As<byte, Message>(ref msgBytes[0]);
                            SilkMarshal.ThrowHResult(infoQueue.GetMessageA(i, ref msg, ref msgByteLength));

                            callback(new D3DDebugMessage(msg));
                        }

                        infoQueue.ClearStoredMessages();
                    }
                }

                PinnedInfoQueues.Remove((infoQueue, callback, infoQueueLock));

                infoQueue.Dispose();
            }
        , cancellationToken);
    }

    private unsafe delegate int SetPrivateData<T>(ref T pThis, ref Guid guid, uint DataSize, void* pData)
        where T : unmanaged, IComVtbl<T>;

    public static unsafe void SetObjectName<T>(this ComPtr<T> obj, string name)
        where T : unmanaged, IComVtbl<T>
    {
        MethodInfo setPrivateDataMethod = typeof(T).GetMethod("SetPrivateData", new Type[] { typeof(Guid).MakeByRefType(), typeof(uint), typeof(void*) })!;

        nint stringData = SilkMarshal.StringToPtr(name);

        setPrivateDataMethod.CreateDelegate<SetPrivateData<T>>().Invoke(ref obj.Get(), ref D3DDebugObjectName, (uint)name.Length, (void*)stringData);

        SilkMarshal.FreeString(stringData);
    }

    static unsafe DebugExtensions()
    {
        // Ensure all info queues are flushed when an exception occurs
        AppDomain.CurrentDomain.UnhandledException += (e, x) =>
        {
            foreach ((ComPtr<ID3D11InfoQueue> infoQueue, Action<D3DDebugMessage> callback, object infoQueueLock) in PinnedInfoQueues)
            {
                if (infoQueue.GetNumStoredMessages() == 0)
                {
                    continue;
                }

                lock (infoQueueLock)
                {
                    for (ulong i = 0; i < infoQueue.GetNumStoredMessages(); i++)
                    {
                        nuint msgByteLength = 0;
                        SilkMarshal.ThrowHResult(infoQueue.Get().GetMessageA(i, null, ref msgByteLength));

                        byte[] msgBytes = new byte[msgByteLength];
                        ref Message msg = ref Unsafe.As<byte, Message>(ref msgBytes[0]);
                        SilkMarshal.ThrowHResult(infoQueue.GetMessageA(i, ref msg, ref msgByteLength));

                        callback(new D3DDebugMessage(msg));
                    }

                    infoQueue.ClearStoredMessages();
                }
            }
        };
    }
}