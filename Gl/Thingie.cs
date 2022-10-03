namespace Gl;

using System.Diagnostics;
using System;

public class FragOut {
    internal int location;
    internal FragOut (int i) => location = i;
    public static implicit operator int (FragOut self) => self.location;
    public static implicit operator FragOut (int i) => new(i);
}

public class Attrib<T> where T : unmanaged {
    internal int location;
    internal Attrib (int i) => location = i;
    public static implicit operator int (Attrib<T> self) => self.location;
    public static implicit operator Attrib<T> (int i) => new(i);
}

public class Thingie<T>:IDisposable where T : Program, new() {
    public T Program { get; }
    public VertexArray VertexArray { get; }

    public Thingie () : this(new()) { }

    public Thingie (T program) {
        Program = program;
        VertexArray = new();
    }

    public void Assign<U> (System.Linq.Expressions.Expression<Func<T, Attrib<U>>> f, BufferObject<U> buffer, int divisor = 0) where U : unmanaged {
        Debug.Assert(f is not null);
        var F = f.Compile();
        VertexArray.Assign(buffer, F(Program), divisor);
    }

    bool disposed = false;
    public void Dispose () {
        if (!disposed) {
            disposed = true;
            Program.Dispose();
            VertexArray.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
