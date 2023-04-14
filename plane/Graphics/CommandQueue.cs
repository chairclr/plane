using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D12;

namespace plane.Graphics;

public class CommandQueue
{
    public ComPtr<ID3D12CommandQueue> NativeCommandQueue;

    public CommandQueue(ComPtr<ID3D12Device> device)
    {
        CommandQueueDesc commandQueueDesc = new CommandQueueDesc()
        {
            Type = CommandListType.Direct
        };

        device.CreateCommandQueue(commandQueueDesc, out NativeCommandQueue);
    }
}
