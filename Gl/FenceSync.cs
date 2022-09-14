namespace Gl;

using System;
//using static RenderingContext;

//public class FenceSync:IDisposable {
//    private const uint GL_OBJECT_TYPE = 0x9112;
//    private const uint GL_SYNC_CONDITION = 0x9113;
//    private const uint GL_SYNC_STATUS = 0x9114;
//    private const uint GL_SYNC_FLAGS = 0x9115;
//    private const uint GL_SYNC_FENCE = 0x9116;
//    private const uint GL_SYNC_GPU_COMMANDS_COMPLETE = 0x9117;
//    private const uint GL_UNSIGNALED = 0x9118;
//    private const uint GL_SIGNALED = 0x9119;
//    private const uint GL_ALREADY_SIGNALED = 0x911A;
//    private const uint GL_TIMEOUT_EXPIRED = 0x911B;

//    internal readonly nint sync;
//    private bool disposed;
//    public FenceSync () {
//        sync = RenderingContext.FenceSync();
//        Flush();
//    }

//    public bool Signaled =>
//        !disposed ? IsSignaled() : throw new ObjectDisposedException(nameof(FenceSync));

//    private bool IsSignaled () =>
//        GL_SIGNALED == GetSynci(sync, GL_SYNC_STATUS);

//    public void Dispose () {
//        if (!disposed) {
//            disposed = true;
//            DeleteSync(sync);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
