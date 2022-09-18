namespace Gl;

using System;

public abstract class OpenglObject:IDisposable {

    public int Id { get; protected init; }

    protected abstract Action<int> Delete { get; }

    public static implicit operator int (OpenglObject ob) => !ob.Disposed ? ob.Id : throw new ObjectDisposedException(ob.GetType().Name);

    protected bool Disposed { get; private set; }

    protected void NotDisposed () {
        if (Disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose () {
        if (!Disposed) {
            Delete(Id);
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
