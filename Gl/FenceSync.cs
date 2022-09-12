namespace Gl;

using System;

public class FenceSync:IDisposable {

    internal readonly nint sync;
    private bool disposed;

    public FenceSync () {
        sync = Opengl.FenceSync();
        Opengl.Flush();
    }

    public bool Signaled =>
        !disposed ? IsSignaled() : throw new ObjectDisposedException(nameof(FenceSync));

    private bool IsSignaled () =>
        Opengl.GL_SIGNALED == Opengl.GetSynci(sync, Opengl.GL_SYNC_STATUS);

    public void Dispose () {
        if (!disposed) {
            disposed = true;
            Opengl.DeleteSync(sync);
            GC.SuppressFinalize(this);
        }
    }
}
