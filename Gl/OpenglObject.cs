namespace Gl;

using System;

public abstract class OpenglObject:IDisposable {

    public int Id { get; protected init; }

    protected abstract Action<int> Delete { get; }

    public static implicit operator int (OpenglObject ob) => !ob.Disposed ? ob.Id : throw new ObjectDisposedException(ob.GetType().Name);

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
