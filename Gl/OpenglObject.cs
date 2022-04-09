namespace Gl;

using System;

public abstract class OpenglObject:IDisposable {

    public abstract int Id { get; }
    
    protected abstract Action<int> Delete { get; }

    protected bool Disposed { get; private set; }
    
    public void Dispose (bool dispose) {
        if (dispose) {
            if (!Disposed) {
                Delete(Id);
                Disposed = true;
            }
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
